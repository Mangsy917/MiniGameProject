using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

// 스테이지 맵 카메라. 드래그로 맵 이동(PC: 마우스 좌클릭, 모바일: 터치), 탭으로 스테이지 선택, 스테이지 확대/복귀.
[RequireComponent(typeof(Camera))]
public class StageMapCamera : MonoBehaviour
{
    public SpriteRenderer MapBoundsSR;   // 카메라가 벗어나지 않을 맵 영역

    public float DragThreshold = 15f;    // 픽셀. 이보다 적게 움직이고 떼면 클릭으로 처리
    public float InertiaDamping = 6f;    // 드래그 후 미끄러짐이 멈추는 속도
    public float MaxInertiaSpeed = 30f;  // 프레임이 튀었을 때 맵이 날아가지 않도록 관성 속도 제한

    public float ZoomSize = 2.2f;        // 스테이지 확대 시 orthographicSize
    public float ZoomTime = 0.45f;

    // 휠(PC) / 핀치(모바일) 확대 축소 범위. MaxSize는 맵 전체가 화면에 들어오는 크기를 넘지 않는다
    public float MinSize = 3f;
    public float MaxSize = 9f;
    public float ScrollZoomStep = 0.1f;  // 휠 한 칸당 확대/축소 비율

    public bool InputEnabled = true;

    public event Action<StageNode> OnStageClicked;

    public float Size
    {
        get { return cam.orthographicSize; }
    }

    Camera cam;
    float defaultSize;

    bool pinching;
    float lastPinchDistance;

    bool pressing;
    bool dragging;
    Vector2 pressPos;
    Vector3 lastWorldPos;
    Vector3 velocity;

    Vector3 returnPos;
    float returnSize;
    Coroutine moveRoutine;

    static readonly List<RaycastResult> uiHits = new List<RaycastResult>();

    void Awake()
    {
        cam = GetComponent<Camera>();
        defaultSize = cam.orthographicSize;
    }

    void Update()
    {
        if (!InputEnabled)
        {
            pressing = false;
            pinching = false;
            velocity = Vector3.zero;
            return;
        }

        HandleScrollZoom();

        // 두 손가락 핀치 중에는 드래그/탭 처리를 하지 않는다
        if (!HandlePinchZoom())
        {
            // Pointer.current는 마우스와 터치스크린을 모두 포함한다
            Pointer pointer = Pointer.current;
            if (pointer != null)
                HandlePointer(pointer);
        }

        // 손을 뗀 뒤 관성 이동
        if (!pressing && velocity.sqrMagnitude > 0.0001f)
        {
            transform.position += velocity * Time.unscaledDeltaTime;
            velocity = Vector3.Lerp(velocity, Vector3.zero, InertiaDamping * Time.unscaledDeltaTime);
            ClampToMap();
        }
    }

    void HandlePointer(Pointer pointer)
    {
        Vector2 screenPos = pointer.position.ReadValue();

        if (pointer.press.wasPressedThisFrame)
        {
            // UI 버튼 위에서 누른 경우 맵 조작 무시
            if (IsOverUI(screenPos))
                return;

            pressing = true;
            dragging = false;
            pressPos = screenPos;
            lastWorldPos = ScreenToWorld(screenPos);
            velocity = Vector3.zero;
        }

        if (!pressing)
            return;

        if (pointer.press.isPressed)
        {
            if (!dragging && (screenPos - pressPos).magnitude > DragThreshold)
                dragging = true;

            if (dragging)
            {
                // 손가락 아래의 월드 좌표가 그대로 따라오도록 카메라 이동
                Vector3 delta = lastWorldPos - ScreenToWorld(screenPos);
                transform.position += delta;
                ClampToMap();

                Vector3 frameVelocity = delta / Mathf.Max(Time.unscaledDeltaTime, 0.0001f);
                velocity = Vector3.ClampMagnitude(Vector3.Lerp(velocity, frameVelocity, 0.5f), MaxInertiaSpeed);
                lastWorldPos = ScreenToWorld(screenPos);
            }
        }
        else
        {
            // 빠른 탭은 누름과 뗌이 같은 프레임에 들어올 수 있으므로 isPressed가 풀린 시점에 처리
            pressing = false;
            if (!dragging)
            {
                velocity = Vector3.zero;
                TryClickStage(screenPos);
            }
        }
    }

    void HandleScrollZoom()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null)
            return;

        // 휠 값의 크기는 OS마다 달라서 방향만 사용
        float scroll = mouse.scroll.ReadValue().y;
        if (scroll == 0f)
            return;

        Vector2 screenPos = mouse.position.ReadValue();
        if (IsOverUI(screenPos))
            return;

        ZoomAt(scroll > 0f ? 1f - ScrollZoomStep : 1f + ScrollZoomStep, screenPos);
    }

    bool HandlePinchZoom()
    {
        Touchscreen touchscreen = Touchscreen.current;
        if (touchscreen == null)
            return false;

        var touch0 = touchscreen.touches[0];
        var touch1 = touchscreen.touches[1];
        if (!touch0.press.isPressed || !touch1.press.isPressed)
        {
            pinching = false;
            return false;
        }

        Vector2 pos0 = touch0.position.ReadValue();
        Vector2 pos1 = touch1.position.ReadValue();
        float distance = Vector2.Distance(pos0, pos1);

        if (pinching && distance > 1f)
            ZoomAt(lastPinchDistance / distance, (pos0 + pos1) * 0.5f);

        lastPinchDistance = distance;
        pinching = true;

        // 핀치가 끝난 뒤 손가락을 떼도 탭이나 관성으로 처리되지 않도록
        pressing = false;
        dragging = false;
        velocity = Vector3.zero;
        return true;
    }

    // screenPos 아래의 월드 좌표가 고정된 채로 확대/축소
    void ZoomAt(float sizeMultiplier, Vector2 screenPos)
    {
        Vector3 before = ScreenToWorld(screenPos);
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize * sizeMultiplier, MinSize, GetMaxSize());
        Vector3 after = ScreenToWorld(screenPos);

        transform.position += before - after;
        ClampToMap();
    }

    float GetMaxSize()
    {
        if (MapBoundsSR == null)
            return MaxSize;

        Bounds bounds = MapBoundsSR.bounds;
        float fitSize = Mathf.Min(bounds.extents.y, bounds.extents.x / cam.aspect);
        return Mathf.Max(MinSize, Mathf.Min(MaxSize, fitSize));
    }

    void TryClickStage(Vector2 screenPos)
    {
        Collider2D hit = Physics2D.OverlapPoint(ScreenToWorld(screenPos));
        if (hit == null)
            return;

        StageNode stage = hit.GetComponentInParent<StageNode>();
        if (stage != null && OnStageClicked != null)
            OnStageClicked(stage);
    }

    // 맵 화면에서 해당 위치를 가운데로 (맵 밖으로는 나가지 않음)
    public void LookAt(Vector3 worldPos)
    {
        transform.position = new Vector3(worldPos.x, worldPos.y, transform.position.z);
        ClampToMap();
    }

    // 스테이지가 화면 왼쪽 절반의 가운데에 오도록 확대
    public void ZoomIn(Vector3 stagePos, Action onDone)
    {
        returnPos = transform.position;
        returnSize = cam.orthographicSize;

        float halfWidth = ZoomSize * cam.aspect;
        Vector3 target = new Vector3(stagePos.x + halfWidth * 0.5f, stagePos.y, transform.position.z);
        MoveTo(target, ZoomSize, onDone);
    }

    public void ZoomOut(Action onDone)
    {
        MoveTo(returnPos, returnSize, onDone);
    }

    void MoveTo(Vector3 pos, float size, Action onDone)
    {
        if (moveRoutine != null)
            StopCoroutine(moveRoutine);
        moveRoutine = StartCoroutine(MoveRoutine(pos, size, onDone));
    }

    IEnumerator MoveRoutine(Vector3 pos, float size, Action onDone)
    {
        Vector3 startPos = transform.position;
        float startSize = cam.orthographicSize;
        float timer = 0f;

        while (timer < ZoomTime)
        {
            timer += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(timer / ZoomTime));
            transform.position = Vector3.Lerp(startPos, pos, t);
            cam.orthographicSize = Mathf.Lerp(startSize, size, t);
            yield return null;
        }

        transform.position = pos;
        cam.orthographicSize = size;
        moveRoutine = null;

        if (onDone != null)
            onDone();
    }

    void ClampToMap()
    {
        if (MapBoundsSR == null)
            return;

        Bounds bounds = MapBoundsSR.bounds;
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;
        Vector3 p = transform.position;

        p.x = bounds.size.x > halfWidth * 2f ? Mathf.Clamp(p.x, bounds.min.x + halfWidth, bounds.max.x - halfWidth) : bounds.center.x;
        p.y = bounds.size.y > halfHeight * 2f ? Mathf.Clamp(p.y, bounds.min.y + halfHeight, bounds.max.y - halfHeight) : bounds.center.y;
        transform.position = p;
    }

    Vector3 ScreenToWorld(Vector2 screenPos)
    {
        Vector3 world = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -transform.position.z));
        world.z = 0f;
        return world;
    }

    bool IsOverUI(Vector2 screenPos)
    {
        if (EventSystem.current == null)
            return false;

        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = screenPos;
        uiHits.Clear();
        EventSystem.current.RaycastAll(eventData, uiHits);
        return uiHits.Count > 0;
    }
}
