using UnityEngine;
using UnityEngine.UI;

// 공통 템플릿: 회전 손잡이의 입력 부분. 손가락이 중심을 도는 각도를 잰다.
public abstract class RotateMiniGame : TouchMiniGame
{
    [Header("Rotate")]
    [SerializeField] protected Image HandleIMG;
    [SerializeField] float DeadRadius = 25f;   // 중심에 너무 가까우면 각도가 튀므로 무시

    protected virtual Vector2 Center
    {
        get { return HandleIMG.rectTransform.anchoredPosition; }
    }

    // turns: 이번에 돌린 바퀴 수 (반시계 방향 +, 시계 방향 -)
    protected abstract void OnRotate(float turns);

    protected override void OnDrag(Vector2 pos, Vector2 delta)
    {
        Vector2 now = pos - Center;
        Vector2 before = now - delta;
        if (now.magnitude < DeadRadius || before.magnitude < DeadRadius)
            return;

        float angle = Vector2.SignedAngle(before, now);
        if (!CanTurn(angle))
            return;
        TurnHandle(angle);
        OnRotate(angle / 360f);
    }

    // false면 이번 움직임은 무시한다 (손잡이도 안 돌아감). angle: 반시계 +, 시계 -
    protected virtual bool CanTurn(float angle)
    {
        return true;
    }

    protected virtual void TurnHandle(float angle)
    {
        HandleIMG.rectTransform.Rotate(0f, 0f, angle);
    }
}
