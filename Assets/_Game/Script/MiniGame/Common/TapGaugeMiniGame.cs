using UnityEngine;

// 공통 템플릿: 연타 게이지 (PRE_16, FOR_01, LAB_19)
// 화면 어디든 탭할 때마다 게이지가 차고 목표 횟수에 도달하면 성공.
public abstract class TapGaugeMiniGame : TouchMiniGame
{
    [Header("Tap Gauge")]
    [SerializeField] int RequiredTaps = 12;

    protected int Taps { get; private set; }

    protected virtual int TotalTaps
    {
        get { return Mathf.Max(1, RequiredTaps); }
    }

    protected float Progress
    {
        get { return Mathf.Clamp01((float)Taps / TotalTaps); }
    }

    protected abstract void OnTapped();

    protected override void Setup()
    {
        SetGauge(0f);
    }

    // 지금 탭을 받을 수 있는지
    protected virtual bool CanTap
    {
        get { return true; }
    }

    protected override void OnTap(Vector2 pos)
    {
        if (!CanTap)
            return;

        Taps++;
        SetGauge(Progress);
        OnTapped();
        if (Taps >= TotalTaps)
            Finish(true);
    }
}
