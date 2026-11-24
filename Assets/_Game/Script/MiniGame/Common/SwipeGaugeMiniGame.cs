using UnityEngine;
using UnityEngine.UI;

// 공통 템플릿: 좌우 스와이프 게이지 (JUN_11, PRE_10)
// 좌우로 문지른 이동량이 다 차면 성공. 일정 단계를 넘을 때마다 때가 한 번에 줄어든다. 위아래 이동은 세지 않는다.
public class SwipeGaugeMiniGame : TouchMiniGame
{
    [Header("Swipe Gauge")]
    [SerializeField] float RequiredScreenWidths = 6f;  // 필요 이동량 (화면 너비의 몇 배)
    [SerializeField] int StageCount = 4;               // 때가 줄어드는 단계 수
    [SerializeField] Image TargetIMG;                  // 상태가 순서대로 바뀌는 대상 (더러움 -> 깨끗함)
    [SerializeField] int TargetSlot = 1;
    [SerializeField] Image OverlayIMG;                 // 이끼처럼 단계마다 옅어지는 것 (선택)
    [SerializeField] Image ToolIMG;                    // 손가락을 따라다니는 도구

    Image blendIMG;   // 다음 상태를 겹쳐 보여주는 이미지
    float moved;

    protected override void Setup()
    {
        blendIMG = Instantiate(TargetIMG, TargetIMG.transform.parent);
        blendIMG.transform.SetSiblingIndex(TargetIMG.transform.GetSiblingIndex() + 1);
        Refresh(0f);
    }

    protected override void OnDragBegin(Vector2 pos)
    {
        ToolIMG.rectTransform.anchoredPosition = pos;
    }

    protected override void OnDrag(Vector2 pos, Vector2 delta)
    {
        ToolIMG.rectTransform.anchoredPosition = pos;
        moved += Mathf.Abs(delta.x);

        float progress = Mathf.Clamp01(moved / Mathf.Max(1f, ScreenWidth * RequiredScreenWidths));
        Refresh(progress);
        if (progress >= 1f)
            Finish(true);
    }

    void Refresh(float progress)
    {
        int stages = Mathf.Max(1, StageCount);
        float look = Mathf.Floor(progress * stages) / stages;
        SetAlpha(OverlayIMG, 1f - look);

        float step = look * (GetSlot(TargetSlot).StateCount - 1);
        int state = (int)step;
        Show(TargetIMG, TargetSlot, state);
        Show(blendIMG, TargetSlot, state + 1);
        SetAlpha(blendIMG, step - state);
    }
}
