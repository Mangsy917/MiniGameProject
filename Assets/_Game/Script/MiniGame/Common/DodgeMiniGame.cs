using UnityEngine;
using UnityEngine.UI;

// 공통 템플릿: 장애물 피하기 (DES_01, CAV_09)의 캐릭터 이동 부분.
// 손가락을 일정 속도로 따라간다. 장애물과 실패 판정은 각 미니게임에서 한다.
public abstract class DodgeMiniGame : TouchMiniGame
{
    [Header("Dodge")]
    [SerializeField] protected Image PlayerIMG;
    [SerializeField] float MoveSpeed = 350f;
    [SerializeField] bool RelativeSwipe;        // true: 스와이프한 만큼 좌우로만 이동 / false: 손가락 위치로 이동
    [SerializeField] protected Vector2 MoveHalfSize = new Vector2(360f, 50f);
    [SerializeField] protected float HitShrink = 0.75f;   // 장애물 판정을 그림보다 작게

    Vector2 target;

    protected override void Setup()
    {
        target = PlayerIMG.rectTransform.anchoredPosition;
    }

    protected override void OnDragBegin(Vector2 pos)
    {
        if (!RelativeSwipe)
            target = ClampMove(pos);
    }

    protected override void OnDrag(Vector2 pos, Vector2 delta)
    {
        if (RelativeSwipe)
            target.x = Mathf.Clamp(target.x + delta.x, -MoveHalfSize.x, MoveHalfSize.x);
        else
            target = ClampMove(pos);
    }

    protected override void OnDragEnd(Vector2 pos)
    {
        // 손을 떼면 그 자리에 멈춘다 (스와이프한 거리는 끝까지 간다)
        if (!RelativeSwipe)
            target = PlayerIMG.rectTransform.anchoredPosition;
    }

    protected override void Tick(float deltaTime)
    {
        RectTransform player = PlayerIMG.rectTransform;
        player.anchoredPosition = Move(player.anchoredPosition, Vector2.MoveTowards(player.anchoredPosition, target, MoveSpeed * deltaTime));
    }

    // from에서 to로 움직인 결과 위치. 벽이 있는 미니게임에서 바꾼다
    protected virtual Vector2 Move(Vector2 from, Vector2 to)
    {
        return to;
    }

    Vector2 ClampMove(Vector2 pos)
    {
        return new Vector2(Mathf.Clamp(pos.x, -MoveHalfSize.x, MoveHalfSize.x),
            Mathf.Clamp(pos.y, -MoveHalfSize.y, MoveHalfSize.y));
    }
}
