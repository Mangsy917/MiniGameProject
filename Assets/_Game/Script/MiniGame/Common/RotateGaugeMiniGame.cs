using UnityEngine;
using UnityEngine.UI;

// 공통 템플릿: 회전 손잡이 (DES_10, JUN_08, FOR_19)
// 손잡이를 돌린 만큼 게이지가 차고, 멈추면 되돌아간다. 다 차면 성공.
public abstract class RotateGaugeMiniGame : RotateMiniGame
{
    [SerializeField] float MinTurns = 2.5f;          // 필요한 바퀴 수는 매번 MinTurns~MaxTurns에서 랜덤
    [SerializeField] float MaxTurns = 4f;
    [SerializeField] float RewindPerSecond = 0.2f;   // 1 = 게이지 100%
    [SerializeField] float IdleTime = 0.1f;          // 이 시간 동안 돌리지 않으면 되감기 시작

    [SerializeField] Image KnobIMG;                  // 임시 손잡이 끝. 손잡이 그림이 들어오면 숨긴다 (선택)

    float requiredTurns;
    float progress;
    float idle;
    float direction;   // 처음 돌린 방향. 반대로 돌리면 줄어들어서 왔다 갔다 흔들기만으로는 차지 않는다

    protected abstract void OnProgress(float value);

    protected override void Setup()
    {
        requiredTurns = Random.Range(MinTurns, MaxTurns);
        if (KnobIMG != null && HandleIMG.preserveAspect)
            SetActive(KnobIMG, false);
        SetProgress(0f);
    }

    protected override void OnDragBegin(Vector2 pos)
    {
        direction = 0f;
    }

    protected override void OnRotate(float turns)
    {
        if (turns == 0f)
            return;
        if (direction == 0f)
            direction = Mathf.Sign(turns);

        float forward = turns * direction;
        if (forward > 0f)
            idle = 0f;
        SetProgress(progress + forward / Mathf.Max(0.01f, requiredTurns));
        if (progress >= 1f)
            Finish(true);
    }

    protected override void Tick(float deltaTime)
    {
        idle += deltaTime;
        if (idle > IdleTime && progress > 0f)
            SetProgress(progress - RewindPerSecond * deltaTime);
    }

    void SetProgress(float value)
    {
        progress = Mathf.Clamp01(value);
        SetGauge(progress);
        OnProgress(progress);
    }
}
