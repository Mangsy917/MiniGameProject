using UnityEngine;
using UnityEngine.UI;

// 공통 템플릿: 연타 후 멈추기 (CAV_08 다이아를 발굴해라, PRE_03 암모나이트 화석을 발굴해라)
// 목표 횟수 N(±허용) 안에서 StopTime 동안 탭을 멈추면 성공, 허용 범위를 넘기면 즉시 실패.
public class TapThenStopMiniGame : TouchMiniGame
{
    [Header("Tap Then Stop")]
    [SerializeField] int MinTarget = 6;
    [SerializeField] int MaxTarget = 12;
    [SerializeField] int Tolerance = 1;
    [SerializeField] float StopTime = 0.5f;
    [SerializeField] Image FossilIMG;        // 상태: 묻힘 / 발굴 / 부서짐
    [SerializeField] int FossilSlot = 1;
    [SerializeField] Image ToolIMG;
    [SerializeField] float ToolAngle = 35f;
    [SerializeField] float ToolTime = 0.12f;

    int target;
    int taps;
    float idle;
    float toolTimer;
    Quaternion toolRotation;   // 프리팹에서 비스듬히 놓은 각도

    protected override void Setup()
    {
        toolRotation = ToolIMG.rectTransform.localRotation;
        target = Random.Range(MinTarget, MaxTarget + 1);
        Show(FossilIMG, FossilSlot, 0);
    }

    protected override void OnTap(Vector2 pos)
    {
        if (!Hit(FossilIMG, pos))
            return;

        taps++;
        idle = 0f;
        toolTimer = ToolTime;
        if (taps > target + Tolerance)
        {
            Show(FossilIMG, FossilSlot, 2);
            Finish(false);
        }
        else if (taps >= target - Tolerance)
        {
            // 멈춰도 되는 순간부터 대상이 드러나 보인다
            Show(FossilIMG, FossilSlot, 1);
        }
    }

    protected override void Tick(float deltaTime)
    {
        idle += deltaTime;
        toolTimer = Mathf.Max(0f, toolTimer - deltaTime);
        float t = ToolTime > 0f ? toolTimer / ToolTime : 0f;
        ToolIMG.rectTransform.localRotation = toolRotation * Quaternion.Euler(0f, 0f, ToolAngle * t);
        FossilIMG.rectTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * Mathf.PI * 4f) * 4f * t);

        if (taps >= target - Tolerance && idle >= StopTime)
            Finish(true);
    }
}
