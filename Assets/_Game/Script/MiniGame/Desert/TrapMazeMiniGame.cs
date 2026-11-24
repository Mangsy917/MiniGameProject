using System;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

// DES_01 피라미드의 함정을 피해라
// 위에서 내려다본 미로에서 캐릭터를 끌어, 주기적으로 창이 튀어나오는 바닥을 지나 출구까지 간다. 벽은 통과할 수 없다.
// 정해진 구간을 왔다 갔다 하는 적이 있고, 닿으면 실패.
// 출구에 닿으면 바로 성공, 찔리거나 시간 안에 도착하지 못하면 실패.
public class TrapMazeMiniGame : DodgeMiniGame
{
    // 처음 자리와 처음 자리 + Offset 사이를 반복해서 움직이는 적
    [Serializable]
    public class Patrol
    {
        public Image Image;
        public Vector2 Offset;
        public float Speed = 260f;

        [NonSerialized] public Vector2 Start;
        [NonSerialized] public float Phase;
    }

    const float MoveStep = 8f;   // 프레임이 튀어도 벽을 뚫지 않도록 나눠 움직이는 거리

    [Header("DES_01")]
    [SerializeField] float TrapPeriod = 1.6f;                     // 창이 나왔다 들어가는 주기(초)
    [SerializeField, Range(0f, 1f)] float SpearRatio = 0.3f;    // 주기 중 창이 나와 있는 비율
    [SerializeField] float BodyRadius = 20f;                    // 벽에 부딪히는 몸 크기
    [SerializeField] Image[] TrapIMGs;                          // 함정 바닥 (개수와 위치는 프리팹에서 정한다)
    [SerializeField] RectTransform[] Walls;
    [SerializeField] Patrol[] Patrols;                          // 적
    [SerializeField] Image ExitIMG;

    float[] phases;
    float timer;

    protected override void Setup()
    {
        base.Setup();
        phases = new float[TrapIMGs.Length];
        for (int i = 0; i < phases.Length; i++)
            phases[i] = Random.value;
        RefreshTraps();

        foreach (Patrol patrol in Patrols)
        {
            patrol.Start = patrol.Image.rectTransform.anchoredPosition;
            patrol.Phase = Random.Range(0f, 2f);
        }
        MovePatrols();
    }

    void MovePatrols()
    {
        foreach (Patrol patrol in Patrols)
        {
            float distance = Mathf.Max(1f, patrol.Offset.magnitude);
            float t = Mathf.PingPong(timer * patrol.Speed / distance + patrol.Phase, 1f);
            patrol.Image.rectTransform.anchoredPosition = patrol.Start + patrol.Offset * t;
        }
    }

    protected override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);
        timer += deltaTime;
        RefreshTraps();
        MovePatrols();

        foreach (Patrol patrol in Patrols)
        {
            if (Overlap(PlayerIMG, patrol.Image, HitShrink))
            {
                Finish(false);
                return;
            }
        }

        Vector2 pos = PlayerIMG.rectTransform.anchoredPosition;
        for (int i = 0; i < TrapIMGs.Length; i++)
        {
            if (IsSpearOut(i) && Hit(TrapIMGs[i].rectTransform, pos, 0f))
            {
                Finish(false);
                return;
            }
        }
        if (Overlap(PlayerIMG, ExitIMG, HitShrink))
            Finish(true);
    }

    bool IsSpearOut(int index)
    {
        return Mathf.Repeat(timer / Mathf.Max(0.01f, TrapPeriod) + phases[index], 1f) < SpearRatio;
    }

    void RefreshTraps()
    {
        for (int i = 0; i < TrapIMGs.Length; i++)
            Show(TrapIMGs[i], 2, IsSpearOut(i) ? 1 : 0);
    }

    // 벽에 막히면 벽을 따라 미끄러진다
    protected override Vector2 Move(Vector2 from, Vector2 to)
    {
        Vector2 pos = from;
        int steps = Mathf.CeilToInt(Vector2.Distance(from, to) / MoveStep);
        if (steps == 0)
            return pos;
        Vector2 step = (to - from) / steps;
        for (int i = 0; i < steps; i++)
        {
            Vector2 x = pos + new Vector2(step.x, 0f);
            if (!InWall(x))
                pos = x;
            Vector2 y = pos + new Vector2(0f, step.y);
            if (!InWall(y))
                pos = y;
        }
        return pos;
    }

    bool InWall(Vector2 pos)
    {
        foreach (RectTransform wall in Walls)
        {
            if (Hit(wall, pos, BodyRadius))
                return true;
        }
        return false;
    }
}
