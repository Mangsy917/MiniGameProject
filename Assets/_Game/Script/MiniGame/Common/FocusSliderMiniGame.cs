using UnityEngine;
using UnityEngine.UI;

// 공통 템플릿: 초점 슬라이더 (PRE_02, LAB_08)
// 슬라이더를 정답 위치(허용 오차 안)에 놓고 손을 떼면 성공. 틀린 위치에서 떼면 다시 시도.
public class FocusSliderMiniGame : TouchMiniGame
{
    [Header("Focus Slider")]
    [SerializeField, Range(0f, 1f)] float MinAnswer = 0.1f;     // 정답 위치는 매번 랜덤
    [SerializeField, Range(0f, 1f)] float MaxAnswer = 0.9f;
    [SerializeField] float Tolerance = 0.08f;
    [SerializeField] float MinStartGap = 0.3f;   // 슬라이더 시작 위치(랜덤)와 정답 사이의 최소 거리
    [SerializeField] float BlurDistance = 90f;   // 초점이 완전히 어긋났을 때 상이 갈라지는 거리
    [SerializeField] Image TrackIMG;             // 세로 슬라이더
    [SerializeField] Image HandleIMG;
    [SerializeField] Image SubjectIMG;           // 초점을 맞출 대상 (선명)
    [SerializeField] Image BlurIMG;              // 흐린 그림. 정답에 가까울수록 사라진다

    Image ghostA, ghostB;
    float answer;
    float value;
    bool dragging;

    protected override void Setup()
    {
        answer = Random.Range(MinAnswer, MaxAnswer);
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
        dragging = Hit(TrackIMG.rectTransform, pos, 50f) || Hit(HandleIMG.rectTransform, pos, 30f);
        if (dragging)
            Follow(pos);
    }

    protected override void OnDrag(Vector2 pos, Vector2 delta)
    {
        if (dragging)
            Follow(pos);
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

    void Follow(Vector2 pos)
    {
        RectTransform track = TrackIMG.rectTransform;
        SetValue(Mathf.Clamp01((pos.y - track.anchoredPosition.y) / track.rect.height + 0.5f));
    }

    void SetValue(float newValue)
    {
        value = newValue;
        RectTransform track = TrackIMG.rectTransform;
        HandleIMG.rectTransform.anchoredPosition = track.anchoredPosition + new Vector2(0f, (value - 0.5f) * track.rect.height);

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
