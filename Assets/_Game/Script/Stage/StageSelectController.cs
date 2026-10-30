using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class StageSelectController : MonoBehaviour
{
    public StageMapCamera MapCamera;
    public StageNode[] StageNodes;

    public GameObject MapUI;      // 맵을 둘러볼 때만 보이는 UI (타이틀, 뒤로가기)
    public GameObject DetailUI;   // 스테이지 확대 시 오른쪽에 나오는 설명 UI
    public TMP_Text StageNameTXT;
    public TMP_Text DescriptionTXT;

    [SerializeField] private string Title;

    // 카메라 orthographicSize가 이 값 이상이면 (맵이 넓게 보이면) 스테이지 이름을 숨기고 아이콘만 표시
    public float LabelHideSize = 6.5f;

    [Header("Clear Animation")]
    public float ClearZoomSize = 2.5f;    // 연출 중 아이콘을 확대해 보여줄 카메라 orthographicSize
    public float ClearDelay = 0.4f;       // 씬 진입 후 연출 시작까지
    public float ClearStampTime = 0.3f;   // CLEAR 글자가 크게 나타났다가 제자리에 찍히는 시간
    public float ClearStampScale = 3f;
    public float ClearPunchTime = 0.35f;  // 스테이지 아이콘이 통통 튀는 시간
    public float ClearPunchScale = 1.3f;
    public float ClearHoldTime = 0.6f;    // 연출이 끝나고 조작을 풀기 전까지

    [Header("Unlock Animation")]          // 1~5 스테이지를 모두 클리어해 연구소가 열릴 때
    public float UnlockDelay = 0.3f;      // 연구소로 이동한 뒤 연출 시작까지
    public float UnlockShakeTime = 0.5f;  // 자물쇠가 흔들리는 시간
    public float UnlockShakeAngle = 12f;
    public int UnlockShakeCount = 4;
    public float UnlockFadeTime = 0.4f;   // 자물쇠가 커지면서 사라지는 시간
    public float UnlockFadeScale = 1.5f;
    public float UnlockHoldTime = 0.6f;   // 연출이 끝나고 맵으로 돌아가기 전까지

    StageNode selectedStage;
    bool labelsVisible = true;

    void Start()
    {
        DetailUI.SetActive(false);
        MapUI.SetActive(true);
        MapCamera.OnStageClicked += SelectStage;

        StageNode clearedStage = FindStage(StageProgress.JustClearedStage);
        StageProgress.JustClearedStage = 0;

        if (clearedStage != null)
        {
            StartCoroutine(PlayClearAnimation(clearedStage));
            return;
        }

        // 아직 클리어하지 않은 첫 스테이지를 화면 가운데로
        foreach (StageNode stage in StageNodes)
        {
            if (!StageProgress.IsCleared(stage.StageNumber) || stage == StageNodes[StageNodes.Length - 1])
            {
                MapCamera.LookAt(stage.transform.position);
                break;
            }
        }
    }

    void Update()
    {
        UpdateLabels();
        HandleDetailOutsideClick();
    }

    void UpdateLabels()
    {
        bool visible = MapCamera.Size < LabelHideSize;
        if (visible == labelsVisible)
            return;

        labelsVisible = visible;
        foreach (StageNode stage in StageNodes)
            stage.SetLabelsVisible(visible);
    }

    // 스테이지 확대 중 인포 패널/버튼, 스테이지 아이콘 이외의 곳을 누르면 맵으로 돌아간다
    void HandleDetailOutsideClick()
    {
        if (!DetailUI.activeSelf)
            return;

        Pointer pointer = Pointer.current;
        if (pointer == null || !pointer.press.wasPressedThisFrame)
            return;

        Vector2 screenPos = pointer.position.ReadValue();
        if (MapCamera.IsOverUI(screenPos))
            return;

        StageNode stage = MapCamera.GetStageAt(screenPos);
        if (stage != null)
        {
            // 다른 스테이지 아이콘을 누르면 그 스테이지로 옮겨간다
            if (stage != selectedStage)
                SelectStage(stage);
            return;
        }

        PressCloseBT_Detail();
    }

    StageNode FindStage(int stageNumber)
    {
        foreach (StageNode stage in StageNodes)
        {
            if (stage.StageNumber == stageNumber)
                return stage;
        }
        return null;
    }

    void SelectStage(StageNode stage)
    {
        if (stage.IsLocked)
            return;

        selectedStage = stage;
        StageNameTXT.text = stage.StageName;
        DescriptionTXT.text = stage.Description;

        MapCamera.InputEnabled = false;
        MapUI.SetActive(false);
        DetailUI.SetActive(false);
        MapCamera.ZoomIn(stage.transform.position, () => DetailUI.SetActive(true));
    }

    IEnumerator PlayClearAnimation(StageNode stage)
    {
        // 연출 중에는 맵과 맵 UI 버튼을 조작할 수 없다
        MapCamera.InputEnabled = false;
        CanvasGroup mapUIGroup = MapUI.GetComponent<CanvasGroup>();
        if (mapUIGroup == null)
            mapUIGroup = MapUI.AddComponent<CanvasGroup>();
        mapUIGroup.interactable = false;
        mapUIGroup.blocksRaycasts = false;

        // 연출이 끝나면 돌아갈 맵 화면을 기억해 두고, 아이콘을 확대한 상태에서 시작한다
        Vector3 stagePos = stage.transform.position;
        MapCamera.LookAt(stagePos);
        Vector3 mapViewPos = MapCamera.transform.position;
        float mapViewSize = MapCamera.Size;
        MapCamera.SetView(stagePos, ClearZoomSize);

        // 이번 클리어로 연구소가 열렸는지 (클리어 연출은 첫 클리어에만 오므로 지금 막 열린 것이다)
        StageNode labStage = null;
        if (stage.StageNumber != StageProgress.LabStage && StageProgress.IsLabStageUnlocked())
            labStage = FindStage(StageProgress.LabStage);

        // StageNode.Start에서 켜지는 CLEAR 표시는 연출 전까지 숨기고,
        // 꺼지는 연구소 자물쇠는 잠금 해제 연출 전까지 보이게 둔다
        yield return null;
        GameObject mark = stage.ClearedMark;
        if (mark != null)
            mark.SetActive(false);
        if (labStage != null && labStage.LockedMark != null)
            labStage.LockedMark.SetActive(true);

        yield return new WaitForSecondsRealtime(ClearDelay);

        // CLEAR 글자가 크게 나타나며 제자리에 찍힌다
        if (mark != null)
        {
            mark.SetActive(true);
            TMP_Text markTXT = mark.GetComponent<TMP_Text>();
            Vector3 markScale = mark.transform.localScale;
            float startTime = Time.realtimeSinceStartup;
            float t = 0f;
            while (t < 1f)
            {
                t = Mathf.Clamp01((Time.realtimeSinceStartup - startTime) / ClearStampTime);
                mark.transform.localScale = markScale * Mathf.Lerp(ClearStampScale, 1f, t * t);
                if (markTXT != null)
                    markTXT.alpha = t;
                yield return null;
            }
        }

        // 찍히는 순간 스테이지 아이콘이 통통 튄다
        yield return Punch(stage.transform);
        yield return new WaitForSecondsRealtime(ClearHoldTime);

        // 이번 클리어로 1~5 스테이지를 모두 깼다면 연구소로 이동해 잠금 해제 연출
        if (labStage != null)
        {
            yield return MoveCamera(labStage.transform.position, ClearZoomSize);
            yield return new WaitForSecondsRealtime(UnlockDelay);
            yield return Unlock(labStage.LockedMark);
            yield return Punch(labStage.transform);
            yield return new WaitForSecondsRealtime(UnlockHoldTime);
        }

        // 맵 화면으로 축소한 뒤 조작을 풀어준다
        yield return MoveCamera(mapViewPos, mapViewSize);

        mapUIGroup.interactable = true;
        mapUIGroup.blocksRaycasts = true;
        MapCamera.InputEnabled = true;
    }

    IEnumerator MoveCamera(Vector3 pos, float size)
    {
        bool done = false;
        MapCamera.MoveView(pos, size, () => done = true);
        while (!done)
            yield return null;
    }

    IEnumerator Punch(Transform target)
    {
        Vector3 scale = target.localScale;
        float startTime = Time.realtimeSinceStartup;
        float t = 0f;
        while (t < 1f)
        {
            t = Mathf.Clamp01((Time.realtimeSinceStartup - startTime) / ClearPunchTime);
            target.localScale = scale * Mathf.Lerp(1f, ClearPunchScale, Mathf.Sin(t * Mathf.PI));
            yield return null;
        }
        target.localScale = scale;
    }

    // 자물쇠 표시가 흔들리다가 커지면서 사라진다
    IEnumerator Unlock(GameObject lockedMark)
    {
        if (lockedMark == null)
            yield break;

        Transform mark = lockedMark.transform;
        Vector3 scale = mark.localScale;
        Quaternion rotation = mark.localRotation;
        SpriteRenderer[] sprites = lockedMark.GetComponentsInChildren<SpriteRenderer>(true);
        TMP_Text[] texts = lockedMark.GetComponentsInChildren<TMP_Text>(true);
        float[] spriteAlphas = new float[sprites.Length];
        for (int i = 0; i < sprites.Length; i++)
            spriteAlphas[i] = sprites[i].color.a;
        float[] textAlphas = new float[texts.Length];
        for (int i = 0; i < texts.Length; i++)
            textAlphas[i] = texts[i].alpha;

        // 흔들림 (점점 약해짐)
        float startTime = Time.realtimeSinceStartup;
        float t = 0f;
        while (t < 1f)
        {
            t = Mathf.Clamp01((Time.realtimeSinceStartup - startTime) / UnlockShakeTime);
            float angle = Mathf.Sin(t * Mathf.PI * 2f * UnlockShakeCount) * UnlockShakeAngle * (1f - t);
            mark.localRotation = rotation * Quaternion.Euler(0f, 0f, angle);
            yield return null;
        }
        mark.localRotation = rotation;

        // 커지면서 사라짐
        startTime = Time.realtimeSinceStartup;
        t = 0f;
        while (t < 1f)
        {
            t = Mathf.Clamp01((Time.realtimeSinceStartup - startTime) / UnlockFadeTime);
            mark.localScale = scale * Mathf.Lerp(1f, UnlockFadeScale, t);
            for (int i = 0; i < sprites.Length; i++)
            {
                Color color = sprites[i].color;
                color.a = spriteAlphas[i] * (1f - t);
                sprites[i].color = color;
            }
            for (int i = 0; i < texts.Length; i++)
                texts[i].alpha = textAlphas[i] * (1f - t);
            yield return null;
        }

        // 숨긴 뒤 원래 상태로 되돌려 둔다
        lockedMark.SetActive(false);
        mark.localScale = scale;
        for (int i = 0; i < sprites.Length; i++)
        {
            Color color = sprites[i].color;
            color.a = spriteAlphas[i];
            sprites[i].color = color;
        }
        for (int i = 0; i < texts.Length; i++)
            texts[i].alpha = textAlphas[i];
    }

    public void PressEnterBT()
    {
        if (selectedStage == null)
            return;

        SceneLoader.LoadScene(selectedStage.SceneName);
    }
    public void PressCloseBT_Detail()
    {
        DetailUI.SetActive(false);
        MapCamera.ZoomOut(() =>
        {
            selectedStage = null;
            MapUI.SetActive(true);
            MapCamera.InputEnabled = true;
        });
    }
    public void PressBackTitleBT()
    {
        SceneLoader.LoadScene(Title);
    }
}
