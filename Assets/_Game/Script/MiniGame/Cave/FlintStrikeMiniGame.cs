using UnityEngine;
using UnityEngine.UI;

// CAV_01 부싯돌로 불을 붙여라
// 장작을 내려다보는 시점. 장작 위에 고정된 돌에 손에 든 돌을 위아래로 여러 번 비벼 불을 붙인다. 너무 빨리 비비면 부서져 실패.
// 위로 비빈 것과 아래로 비빈 것을 각각 한 번으로 센다.
public class FlintStrikeMiniGame : TouchMiniGame
{
    [Header("CAV_01")]
    [SerializeField] int MinStrikes = 6;                // 필요한 비비기 횟수는 매번 MinStrikes~MaxStrikes에서 랜덤
    [SerializeField] int MaxStrikes = 10;
    [SerializeField] float FastestScreenTime = 0.12f;   // 화면 높이를 이 시간보다 빨리 지나가는 속도면 부서진다
    [SerializeField] float MinStroke = 120f;            // 한 방향으로 이만큼은 움직여야 한 번으로 인정
    [SerializeField] float MoveRange = 100f;            // 처음 자리에서 위아래로 움직일 수 있는 거리
    [SerializeField] float SparkTime = 0.15f;
    [SerializeField] Image HeldIMG;                     // 손에 든 돌
    [SerializeField] Image WoodIMG;
    [SerializeField] Image SparkIMG;

    Vector2 restPos;
    float moved;             // 처음 자리에서 위로 움직인 거리 (-MoveRange ~ MoveRange)
    float direction;         // 지금 비비는 방향 (위 1, 아래 -1)
    float strokeStart;
    float strokeStartTime;
    float sparkTimer;
    int requiredStrikes;
    int strikes;
    bool grabbed;
    bool struck;             // 이번 방향에서 이미 한 번으로 셌는지

    protected override void Setup()
    {
        requiredStrikes = Random.Range(MinStrikes, MaxStrikes + 1);
        restPos = HeldIMG.rectTransform.anchoredPosition;
        SetGauge(0f);
        SetActive(SparkIMG, false);
    }

    protected override void OnDragBegin(Vector2 pos)
    {
        grabbed = Hit(HeldIMG.rectTransform, pos, 60f);
        direction = 0f;
    }

    protected override void OnDrag(Vector2 pos, Vector2 delta)
    {
        if (!grabbed)
            return;

        float before = moved;
        moved = Mathf.Clamp(moved + delta.y, -MoveRange, MoveRange);
        HeldIMG.rectTransform.anchoredPosition = restPos + Vector2.up * moved;
        if (moved == before)
            return;

        // 방향이 바뀌면 새로 비비기 시작한 것으로 본다
        float now = Mathf.Sign(moved - before);
        if (now != direction)
        {
            direction = now;
            struck = false;
            strokeStart = before;
            strokeStartTime = Time.unscaledTime - Time.unscaledDeltaTime;
        }

        float length = Mathf.Abs(moved - strokeStart);
        if (!struck && length >= MinStroke)
        {
            struck = true;
            float time = Mathf.Max(Time.unscaledTime - strokeStartTime, 0.001f);
            Strike(length / ScreenHeight / time);
        }
    }

    protected override void OnDragEnd(Vector2 pos)
    {
        grabbed = false;
    }

    void Strike(float speed)
    {
        if (speed > 1f / FastestScreenTime)
        {
            Show(HeldIMG, 1, 1);
            Finish(false);
            return;
        }

        strikes++;
        sparkTimer = SparkTime;
        SetActive(SparkIMG, true);
        SetGauge((float)strikes / Mathf.Max(1, requiredStrikes));
        if (strikes >= requiredStrikes)
        {
            Show(WoodIMG, 2, 1);
            Finish(true);
        }
    }

    protected override void Tick(float deltaTime)
    {
        sparkTimer -= deltaTime;
        if (sparkTimer <= 0f)
            SetActive(SparkIMG, false);
    }
}
