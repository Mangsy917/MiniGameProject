using UnityEngine;
using UnityEngine.UI;

// LAB_15 칠판의 수식을 지워라
// 지우개가 지나간 곳의 수식만 지워진다. 거의 다 지우면 나머지도 지워지며 성공.
public class EraseBoardMiniGame : TouchMiniGame
{
    [Header("LAB_15")]
    [SerializeField] float EraserRadius = 58f;
    [SerializeField, Range(0f, 1f)] float ClearRatio = 0.96f;   // 이만큼 지우면 성공
    [SerializeField] ErasableImage Formula;     // 1번 자리의 첫 번째 상태(수식). 뒤에 깨끗한 칠판이 깔려 있다
    [SerializeField] Image EraserIMG;

    protected override void Setup()
    {
        SpriteSlot slot = GetSlot(1);
        Sprite sprite = slot.GetSprite(0);
        Formula.SetPicture(sprite != null ? sprite : slot.Placeholder, sprite != null ? Color.white : slot.GetColor(0));
        Formula.ResetErase();
        SetGauge(0f);
    }

    protected override void OnDragBegin(Vector2 pos)
    {
        Erase(pos, pos);
    }

    protected override void OnDrag(Vector2 pos, Vector2 delta)
    {
        Erase(pos - delta, pos);
    }

    void Erase(Vector2 from, Vector2 to)
    {
        EraserIMG.rectTransform.anchoredPosition = to;
        Vector2 origin = Formula.rectTransform.anchoredPosition;
        Formula.Erase(from - origin, to - origin, EraserRadius);

        float progress = Formula.ErasedRatio / Mathf.Max(0.01f, ClearRatio);
        SetGauge(progress);
        if (progress >= 1f)
        {
            Formula.EraseAll();   // 조금 남은 수식도 성공하면 모두 지운다
            Finish(true);
        }
    }
}
