using System.Collections;
using UnityEngine;

// 위에서 내려오다가 가운데에서 멈추는 창 (옵션, 도움말, 리더보드).
// Open()을 부르면 이 오브젝트가 켜지면서 판이 떨어지고(살짝 지나쳤다 돌아옴) 뒤가 어두워진다.
// Close()를 부르면 판이 위로 올라간 뒤 이 오브젝트가 꺼진다.
// 어두운 배경(Dim)이 화면 전체를 덮어 뒤쪽 버튼과 맵 조작을 막는다.
public class DropWindow : MonoBehaviour
{
    public CanvasGroup Dim;              // 뒤를 어둡게 가리는 배경
    public RectTransform Board;          // 위에서 내려오는 판
    public float DropTime = 0.55f;       // 내려와서 멈추기까지
    public float Bounce = 1.6f;          // 멈출 때 살짝 지나쳤다 돌아오는 정도 (0 = 그냥 멈춤)
    public float RiseTime = 0.3f;        // 닫을 때 위로 올라가는 시간

    public bool IsOpen { get; private set; }
    public bool IsMoving { get; private set; }

    public void Open()
    {
        if (IsOpen || IsMoving)
            return;
        gameObject.SetActive(true);
        StartCoroutine(DropRoutine());
    }

    public void Close()
    {
        if (!IsOpen || IsMoving)
            return;
        StartCoroutine(RiseRoutine());
    }

    IEnumerator DropRoutine()
    {
        IsMoving = true;
        IsOpen = true;
        Board.anchoredPosition = new Vector2(0f, OffscreenY());
        Dim.alpha = 0f;
        // 판 크기가 앵커로 정해지는 경우를 위해 한 프레임 뒤에 출발 위치를 다시 잡는다
        yield return null;

        Vector2 from = new Vector2(0f, OffscreenY());
        float startTime = Time.realtimeSinceStartup;
        float t = 0f;
        while (t < 1f)
        {
            t = Progress(startTime, DropTime);
            Board.anchoredPosition = Vector2.LerpUnclamped(from, Vector2.zero, EaseOutBack(t, Bounce));
            Dim.alpha = Mathf.Clamp01(t * 2f);
            yield return null;
        }
        Board.anchoredPosition = Vector2.zero;
        Dim.alpha = 1f;
        IsMoving = false;
    }

    IEnumerator RiseRoutine()
    {
        IsMoving = true;
        Vector2 to = new Vector2(0f, OffscreenY());
        float startTime = Time.realtimeSinceStartup;
        float t = 0f;
        while (t < 1f)
        {
            t = Progress(startTime, RiseTime);
            Board.anchoredPosition = Vector2.Lerp(Vector2.zero, to, t * t);
            Dim.alpha = 1f - t;
            yield return null;
        }
        Board.anchoredPosition = Vector2.zero;
        Dim.alpha = 1f;
        IsMoving = false;
        IsOpen = false;
        gameObject.SetActive(false);
    }

    // 판이 화면 위로 완전히 벗어나는 높이 (화면 방향·비율이 바뀌어도 맞도록 매번 계산)
    float OffscreenY()
    {
        RectTransform parent = (RectTransform)Board.parent;
        // 판 바깥으로 튀어나온 버튼(도움말 확인 버튼 등)까지 가려지도록 여유를 둔다
        return parent.rect.height * 0.5f + Board.rect.height * 0.5f + 120f;
    }

    static float Progress(float startTime, float duration)
    {
        return duration > 0f ? Mathf.Clamp01((Time.realtimeSinceStartup - startTime) / duration) : 1f;
    }

    // 끝에서 살짝 지나쳤다가 돌아오는 곡선. overshoot = 0이면 지나치지 않는다
    static float EaseOutBack(float t, float overshoot)
    {
        float u = t - 1f;
        return 1f + (overshoot + 1f) * u * u * u + overshoot * u * u;
    }
}
