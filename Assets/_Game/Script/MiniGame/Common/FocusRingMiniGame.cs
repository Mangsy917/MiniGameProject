using UnityEngine;
using UnityEngine.UI;

// 공통 템플릿: 초점 링 (PRE_02, LAB_08)
// 오른쪽의 초점 링을 렌즈처럼 돌려 초점을 맞춘다. 선명한 위치(허용 오차 안)에서 손을 떼면 성공. 틀린 위치에서 떼면 다시 시도.
// 링에는 숫자나 눈금을 표시하지 않는다. 돌아가는 것이 보이도록 테두리에 손잡이 홈(Grips)만 있다.
// 시계 방향으로 돌리면 초점 값이 커지고, 끝에 닿으면 더 돌아가지 않는다.
public class FocusRingMiniGame : RotateMiniGame
{
    [Header("Focus Ring")]
    [SerializeField, Range(0f, 1f)] float MinAnswer = 0.1f;     // 정답 위치는 매번 랜덤
    [SerializeField, Range(0f, 1f)] float MaxAnswer = 0.9f;
    [SerializeField] float Tolerance = 0.08f;
    [SerializeField] float MinStartGap = 0.3f;        // 링 시작 위치(랜덤)와 정답 사이의 최소 거리
    [SerializeField] float TurnsForFullRange = 0.75f; // 이만큼(바퀴) 돌리면 초점이 끝에서 끝까지 바뀐다
    [SerializeField] float GrabPadding = 60f;         // 링 바깥 이 거리 안에서 잡아도 돌릴 수 있다
    [SerializeField] float BlurDistance = 90f;        // 초점이 완전히 어긋났을 때 상이 갈라지는 거리
    [SerializeField] GameObject RingGrips;            // 임시 손잡이 홈. 링 그림이 들어오면 숨긴다 (선택)
    [SerializeField] Image SubjectIMG;                // 초점을 맞출 대상 (선명)
    [SerializeField] Image BlurIMG;                   // 흐린 그림. 정답에 가까울수록 사라진다

    Image ghostA, ghostB;
    float answer;
    float value;
    bool dragging;

    protected override void Setup()
    {
        answer = Random.Range(MinAnswer, MaxAnswer);
        if (RingGrips != null && HandleIMG.preserveAspect)
            RingGrips.SetActive(false);
        ghostA = Spawn(SubjectIMG, SubjectIMG.rectTransform.anchoredPosition);
        ghostB = Spawn(SubjectIMG, SubjectIMG.rectTransform.anchoredPosition);
        SetActive(SubjectIMG, true);
        SetValue(RandomStart());
    }

    // 정답에서 MinStartGap 이상 떨어진 랜덤 위치
    float RandomStart()
    {
        float below = Mathf.Max(0f, answer - MinStartGap);
        float above = Mathf.Max(0f, 1f - MinStartGap - answer);
        float pick = Random.Range(0f, below + above);
        return pick < below ? pick : answer + MinStartGap + pick - below;
    }

    protected override void OnDragBegin(Vector2 pos)
    {
        dragging = Hit(HandleIMG.rectTransform, pos, GrabPadding);
    }

    protected override void OnDrag(Vector2 pos, Vector2 delta)
    {
        if (dragging)
            base.OnDrag(pos, delta);
    }

    // 링의 각도는 초점 값으로 정해진다 (끝에 닿으면 멈춤)
    protected override void TurnHandle(float angle) { }

    protected override void OnRotate(float turns)
    {
        SetValue(Mathf.Clamp01(value - turns / Mathf.Max(0.01f, TurnsForFullRange)));
    }

    protected override void OnDragEnd(Vector2 pos)
    {
        if (dragging && Mathf.Abs(value - answer) <= Tolerance)
        {
            SetValue(answer);
            Finish(true);
        }
        dragging = false;
    }

    void SetValue(float newValue)
    {
        value = newValue;
        HandleIMG.rectTransform.localEulerAngles = new Vector3(0f, 0f, -value * TurnsForFullRange * 360f);

        // 블러 대신 상이 갈라지고 흐린 그림이 덮인다. 허용 오차 안이면 완전히 선명
        float blur = Mathf.Clamp01((Mathf.Abs(value - answer) - Tolerance) / 0.6f);
        bool sharp = blur <= 0f;
        Vector2 center = SubjectIMG.rectTransform.anchoredPosition;
        Vector2 offset = new Vector2(1f, 0.3f) * (blur * BlurDistance);
        ghostA.rectTransform.anchoredPosition = center + offset;
        ghostB.rectTransform.anchoredPosition = center - offset;
        SetAlpha(ghostA, sharp ? 0f : 0.4f);
        SetAlpha(ghostB, sharp ? 0f : 0.4f);
        SetAlpha(SubjectIMG, sharp ? 1f : 0.4f);
        SetAlpha(BlurIMG, sharp ? 0f : Mathf.Clamp01(0.25f + blur * 0.6f));
    }
}
