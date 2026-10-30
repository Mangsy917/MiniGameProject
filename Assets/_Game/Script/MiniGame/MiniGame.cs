using UnityEngine;

// 모든 미니게임의 기본 클래스. 프리팹 루트에 붙이고 StageFlowController의 미니게임 목록에 등록한다.
// 제한 시간은 StageFlowController가 재고, 시간이 다 되면 TimeUp()을 호출한다.
public abstract class MiniGame : MonoBehaviour
{
    public string Title;

    // 제한 시간이 끝났을 때의 결과. 버티기형 미니게임처럼 시간을 넘기면 성공인 경우 true로
    public bool SucceedOnTimeUp = false;

    public bool IsPlaying { get; private set; }
    public bool IsFinished { get; private set; }
    public bool IsSuccess { get; private set; }

    protected float TimeLimit { get; private set; }

    // 문이 다 열린 뒤 호출된다. 그 전의 입력은 결과에 반영되지 않는다
    public void Begin(float timeLimit)
    {
        TimeLimit = timeLimit;
        IsPlaying = true;
        IsFinished = false;
        IsSuccess = false;
        OnBegin();
    }

    public void TimeUp()
    {
        Finish(SucceedOnTimeUp);
    }

    // 미니게임 쪽에서 결과가 정해지면 호출. 처음 한 번만 반영된다
    protected void Finish(bool success)
    {
        if (!IsPlaying || IsFinished)
            return;

        IsPlaying = false;
        IsFinished = true;
        IsSuccess = success;
        OnEnd(success);
    }

    protected abstract void OnBegin();

    protected virtual void OnEnd(bool success)
    {
    }
}
