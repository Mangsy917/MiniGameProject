using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

// DES_04 벽화의 틀린 부분을 찾아라 (공통 템플릿: 틀린그림)
// 오른쪽 그림을 탭하면 그 자리에 빨간 원이 생기고, 원 안에 틀린 부분이 겹치면 성공, 아니면 실패.
// 오른쪽 그림에서 골라야 한다는 것을 알 수 있도록 오른쪽 그림 테두리가 깜빡이고, 위에서 화살표가 가리킨다.
public class MuralDifferenceMiniGame : TouchMiniGame
{
    // 벽화 한 세트의 정답 영역 (틀린 곳은 항상 1곳). 그림 왼쪽 아래를 (0,0), 오른쪽 위를 (1,1)로 본 비율
    [Serializable]
    public class MuralSet
    {
        public Rect AnswerArea = new Rect(0.4f, 0.4f, 0.2f, 0.2f);
    }

    [Header("DES_04")]
    [SerializeField] MuralSet[] Sets;     // 1번 자리의 스프라이트는 원본1, 틀림1, 원본2, 틀림2 ... 순서
    [SerializeField] Image OriginalIMG;
    [SerializeField] Image WrongIMG;
    [SerializeField] Image MarkIMG;       // 탭한 곳에 생기는 빨간 원
    [SerializeField] float MarkRadius = 45f;

    [Header("Highlight")]                         // 오른쪽 그림 강조
    [SerializeField] Image HighlightIMG;          // 오른쪽 그림 뒤의 테두리 (그림보다 조금 크게)
    [SerializeField] float HighlightPadding = 10f;
    [SerializeField] float PulseSpeed = 2.5f;     // 초당 깜빡임 횟수
    [SerializeField] bool ShowArrow = true;       // 오른쪽 그림 위에서 아래를 가리키는 화살표
    [SerializeField] Color ArrowColor = new Color(1f, 0.85f, 0.2f, 1f);
    [SerializeField] float ArrowBounce = 10f;

    UILine arrow;
    Vector2 arrowPos;
    float pulseTimer;

    Rect area;

    protected override void Setup()
    {
        int set = Random.Range(0, Sets.Length);
        area = Sets[set].AnswerArea;
        Show(OriginalIMG, 1, set * 2);
        Show(WrongIMG, 1, set * 2 + 1);
        // 정답 영역을 그림 전체 비율로 계산하므로 칸에 꽉 채운다
        OriginalIMG.preserveAspect = false;
        WrongIMG.preserveAspect = false;
        SetActive(MarkIMG, false);
        SetupHighlight();
    }

    void SetupHighlight()
    {
        RectTransform panel = WrongIMG.rectTransform;
        if (HighlightIMG != null)
        {
            HighlightIMG.rectTransform.anchoredPosition = panel.anchoredPosition;
            HighlightIMG.rectTransform.sizeDelta = panel.rect.size + Vector2.one * (HighlightPadding * 2f);
            HighlightIMG.raycastTarget = false;
        }
        if (!ShowArrow)
            return;

        // 아래를 가리키는 꺾쇠 화살표
        GameObject go = new GameObject("Arrow", typeof(RectTransform));
        RectTransform rt = (RectTransform)go.transform;
        rt.SetParent(panel.parent, false);
        rt.sizeDelta = Vector2.zero;
        arrowPos = panel.anchoredPosition + new Vector2(0f, panel.rect.height * 0.5f + HighlightPadding + 34f);
        rt.anchoredPosition = arrowPos;
        arrow = go.AddComponent<UILine>();
        arrow.raycastTarget = false;
        arrow.color = ArrowColor;
        arrow.SetWidth(10f);
        arrow.SetPoints(new List<Vector2> { new Vector2(-22f, 18f), new Vector2(0f, -6f), new Vector2(22f, 18f) });
    }

    protected override void Tick(float deltaTime)
    {
        pulseTimer += deltaTime;
        float wave = 0.5f + 0.5f * Mathf.Sin(pulseTimer * PulseSpeed * Mathf.PI * 2f);
        if (HighlightIMG != null)
            SetAlpha(HighlightIMG, Mathf.Lerp(0.35f, 1f, wave));
        if (arrow != null)
            arrow.rectTransform.anchoredPosition = arrowPos + Vector2.down * (ArrowBounce * wave);
    }

    protected override void OnTap(Vector2 pos)
    {
        RectTransform panel = WrongIMG.rectTransform;
        if (!Hit(panel, pos, 0f))
            return;

        MarkIMG.rectTransform.anchoredPosition = pos;
        MarkIMG.rectTransform.sizeDelta = Vector2.one * (MarkRadius * 2f);
        SetActive(MarkIMG, true);

        // 틀린 부분 영역에서 원의 중심과 가장 가까운 점이 원 안에 있으면 겹친 것
        Vector2 size = panel.rect.size;
        Vector2 min = panel.anchoredPosition + Vector2.Scale(area.min - Vector2.one * 0.5f, size);
        Vector2 max = panel.anchoredPosition + Vector2.Scale(area.max - Vector2.one * 0.5f, size);
        Vector2 nearest = new Vector2(Mathf.Clamp(pos.x, min.x, max.x), Mathf.Clamp(pos.y, min.y, max.y));
        Finish(Vector2.Distance(nearest, pos) <= MarkRadius);
    }
}
