using UnityEngine;
using UnityEngine.UI;

// FOR_14 냄비가 눌어붙지 않게 저어라 (회전 손잡이의 속도 유지형)
// 적정 속도로 끝까지 저으면 성공. 범위를 벗어난 상태가 일정 시간 이어지면 실패.
public class PotStirMiniGame : RotateMiniGame
{
    [Header("FOR_14")]
    [SerializeField] float MinTurnsPerSecond = 0.5f;
    [SerializeField] float MaxTurnsPerSecond = 3.5f;
    [SerializeField] float AllowedTime = 0.6f;       // 범위 밖이 이 시간보다 오래 이어지면 실패
    [SerializeField] float StartGrace = 1f;          // 젓기 시작하기 전까지 봐주는 시간 (젓기 시작하면 바로 판정)
    [SerializeField] float SpeedSmoothTime = 0.15f;  // 속도를 재는 구간. 짧으면 손 떨림에 민감하다
    [SerializeField] Image FoodIMG;                  // 이 그림의 중심을 기준으로 젓는다
    [SerializeField] float LadleRadius = 120f;       // 국자가 움직일 수 있는 범위 (냄비 중심에서)

    float turnsThisFrame;
    float speed;
    float outTimer;
    float timer;
    bool stirring;

    protected override void Setup() { }

    // 국자는 제자리에서 돌지 않고 손가락을 따라 냄비 안을 돌아다닌다
    protected override Vector2 Center
    {
        get { return FoodIMG.rectTransform.anchoredPosition; }
    }

    protected override void TurnHandle(float angle) { }

    protected override void OnDragBegin(Vector2 pos)
    {
        MoveLadle(pos);
    }

    protected override void OnDrag(Vector2 pos, Vector2 delta)
    {
        MoveLadle(pos);
        base.OnDrag(pos, delta);
    }

    void MoveLadle(Vector2 pos)
    {
        HandleIMG.rectTransform.anchoredPosition = Center + Vector2.ClampMagnitude(pos - Center, LadleRadius);
    }

    protected override void OnRotate(float turns)
    {
        turnsThisFrame += turns;
        stirring = true;
    }

    protected override void Tick(float deltaTime)
    {
        if (deltaTime <= 0f)
            return;

        // 부호를 살려 평균을 내므로 제자리에서 왔다 갔다 하는 것은 느린 것으로 본다
        speed = Mathf.Lerp(speed, turnsThisFrame / deltaTime, 1f - Mathf.Exp(-deltaTime / Mathf.Max(0.01f, SpeedSmoothTime)));
        turnsThisFrame = 0f;
        timer += deltaTime;

        float now = Mathf.Abs(speed);
        if (!stirring && timer < StartGrace)
            return;

        bool slow = now < MinTurnsPerSecond;
        bool fast = now > MaxTurnsPerSecond;
        outTimer = slow || fast ? outTimer + deltaTime : 0f;
        if (outTimer > AllowedTime)
        {
            Show(FoodIMG, 3, slow ? 1 : 2);
            Finish(false);
        }
    }
}
