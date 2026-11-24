using UnityEngine;
using UnityEngine.UI;

// 공통 템플릿: 속도 제한 긁기 (LAB_01)
// 물건을 잡고 위에서 아래로 최소 길이만큼 긁은 순간의 평균 속도를 판정한다. 방향이 바뀌면 새로 긁는 것으로 본다.
public abstract class ScratchMiniGame : TouchMiniGame
{
    [Header("Scratch")]
    [SerializeField] protected Image ItemIMG;                // 끌고 다니는 물건
    [SerializeField] RectTransform LengthReference;          // 최소 길이의 기준 (리더기)
    [SerializeField, Range(0f, 1f)] float MinLengthRatio = 0.8f;
    [SerializeField] float MoveRange = 190f;                 // 가운데에서 위아래로 움직일 수 있는 거리

    float strokeStart;
    float strokeStartTime;
    bool movingDown;
    bool judged;
    bool grabbed;

    // speed: 초당 화면 높이의 몇 배를 움직였는지 (화면 높이/0.2초 = 5)
    protected abstract void OnStroke(float speed);

    // 최소 길이를 채우지 못하고 손을 뗐을 때
    protected abstract void OnShortStroke();

    protected override void OnDragBegin(Vector2 pos)
    {
        grabbed = Hit(ItemIMG.rectTransform, pos, 30f);
        movingDown = false;
        judged = false;
    }

    protected override void OnDrag(Vector2 pos, Vector2 delta)
    {
        if (!grabbed)
            return;

        RectTransform item = ItemIMG.rectTransform;
        Vector2 itemPos = item.anchoredPosition;
        float before = itemPos.y;
        itemPos.y = Mathf.Clamp(itemPos.y + delta.y, -MoveRange, MoveRange);
        item.anchoredPosition = itemPos;

        if (itemPos.y > before)
            movingDown = false;
        if (itemPos.y >= before)
            return;

        if (!movingDown)
        {
            // 움직이기 시작한 순간부터 잰다 (누른 채 가만히 있던 시간은 빼고)
            movingDown = true;
            strokeStart = before;
            strokeStartTime = Time.unscaledTime - Time.unscaledDeltaTime;
            judged = false;
        }

        float length = strokeStart - itemPos.y;
        if (judged || length < LengthReference.rect.height * MinLengthRatio)
            return;

        judged = true;
        float time = Mathf.Max(Time.unscaledTime - strokeStartTime, 0.001f);
        OnStroke(length / ScreenHeight / time);
    }

    protected override void OnDragEnd(Vector2 pos)
    {
        if (grabbed && !judged && movingDown)
            OnShortStroke();
        grabbed = false;
    }

    // 손을 떼고 다시 잡게 한다
    protected void Release()
    {
        grabbed = false;
    }
}
