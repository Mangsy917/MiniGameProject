using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 공통 템플릿: 회전 손잡이 (DES_10, JUN_08, FOR_19)
// 손잡이를 돌린 만큼 게이지가 차고, 멈추면 되돌아간다. 다 차면 성공.
// 돌려야 하는 방향(Direction)과 반대로 돌렸을 때의 규칙(Reverse)은 미니게임마다 프리팹에서 고른다.
public abstract class RotateGaugeMiniGame : RotateMiniGame
{
    public enum TurnDirection
    {
        FirstTurn,          // 처음 돌린 방향이 정방향
        Clockwise,          // 시계 방향 고정
        CounterClockwise,   // 반시계 방향 고정
        Random,             // 매번 시계/반시계 중 랜덤
    }

    public enum ReverseRule
    {
        Rewind,   // 반대로 돌리면 돌린 만큼 게이지가 줄어든다 (문이 닫힘, 릴이 풀림)
        Block,    // 반대로는 손잡이가 돌아가지 않는다
    }

    [SerializeField] float MinTurns = 2.5f;          // 필요한 바퀴 수는 매번 MinTurns~MaxTurns에서 랜덤
    [SerializeField] float MaxTurns = 4f;
    [SerializeField] float RewindPerSecond = 0.2f;   // 1 = 게이지 100%
    [SerializeField] float IdleTime = 0.1f;          // 이 시간 동안 정방향으로 돌리지 않으면 되감기 시작

    [Header("Direction")]
    [SerializeField] TurnDirection Direction = TurnDirection.FirstTurn;
    [SerializeField] ReverseRule Reverse = ReverseRule.Rewind;
    [SerializeField] bool ShowArrow;                 // 손잡이 둘레에 돌릴 방향 화살표 (FirstTurn이면 표시 안 함)
    [SerializeField] float ArrowRadius = 150f;
    [SerializeField] float ArrowWidth = 12f;
    [SerializeField] Color ArrowColor = new Color(1f, 0.95f, 0.6f, 1f);
    [SerializeField] Image ArrowIMG;                 // 화살표 그림 (선택, 시계 방향 그림). 비어 있으면 선으로 그린다

    [SerializeField] Image KnobIMG;                  // 임시 손잡이 끝. 손잡이 그림이 들어오면 숨긴다 (선택)

    float requiredTurns;
    float progress;
    float idle;
    float direction;   // 정방향 (+1 반시계, -1 시계, 0 아직 모름)

    protected abstract void OnProgress(float value);

    protected override void Setup()
    {
        requiredTurns = Random.Range(MinTurns, MaxTurns);
        if (KnobIMG != null && HandleIMG.preserveAspect)
            SetActive(KnobIMG, false);

        direction = FixedDirection();
        SetupArrow();
        SetProgress(0f);
    }

    float FixedDirection()
    {
        switch (Direction)
        {
            case TurnDirection.Clockwise: return -1f;
            case TurnDirection.CounterClockwise: return 1f;
            case TurnDirection.Random: return Random.value < 0.5f ? -1f : 1f;
            default: return 0f;
        }
    }

    protected override void OnDragBegin(Vector2 pos)
    {
        if (Direction == TurnDirection.FirstTurn)
            direction = 0f;
    }

    protected override bool CanTurn(float angle)
    {
        // 막힘 규칙이면 반대 방향으로는 손잡이가 움직이지 않는다
        return !(Reverse == ReverseRule.Block && direction != 0f && Mathf.Sign(angle) != direction);
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

    // ---- 방향 화살표 ----

    void SetupArrow()
    {
        bool show = ShowArrow && direction != 0f;
        bool useArt = ArrowIMG != null && ArrowIMG.sprite != null;
        if (ArrowIMG != null)
        {
            SetActive(ArrowIMG, show && useArt);
            // 그림은 시계 방향 기준. 반시계면 좌우로 뒤집는다
            Vector3 scale = ArrowIMG.rectTransform.localScale;
            scale.x = Mathf.Abs(scale.x) * (direction > 0f ? -1f : 1f);
            ArrowIMG.rectTransform.localScale = scale;
        }
        if (show && !useArt)
            DrawArrow();
    }

    // 손잡이 둘레에 3/4 바퀴짜리 원호와 화살촉을 그린다 (임시 화살표)
    void DrawArrow()
    {
        GameObject go = new GameObject("Arrow", typeof(RectTransform));
        RectTransform rt = (RectTransform)go.transform;
        rt.SetParent(HandleIMG.rectTransform.parent, false);
        rt.SetSiblingIndex(HandleIMG.rectTransform.GetSiblingIndex());
        rt.anchoredPosition = Center;
        rt.sizeDelta = Vector2.zero;

        UILine line = go.AddComponent<UILine>();
        line.raycastTarget = false;
        line.color = ArrowColor;
        line.SetWidth(ArrowWidth);

        // 시계 방향: 위쪽(135°)에서 시작해 각도가 줄어드는 쪽으로 270° 진행
        const int steps = 36;
        const float sweep = 270f;
        float start = direction < 0f ? 135f : 45f;
        List<Vector2> points = new List<Vector2>();
        for (int i = 0; i <= steps; i++)
            points.Add(PointOnCircle(start + direction * sweep * i / steps));

        float endAngle = start + direction * sweep;
        Vector2 end = PointOnCircle(endAngle);
        float rad = endAngle * Mathf.Deg2Rad;
        Vector2 tangent = new Vector2(-Mathf.Sin(rad), Mathf.Cos(rad)) * direction;   // 진행 방향
        float head = ArrowWidth * 2.5f;
        points.Add(end - (Vector2)(Quaternion.Euler(0f, 0f, 35f) * tangent) * head);
        points.Add(end);
        points.Add(end - (Vector2)(Quaternion.Euler(0f, 0f, -35f) * tangent) * head);
        line.SetPoints(points);
    }

    Vector2 PointOnCircle(float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * ArrowRadius;
    }
}
