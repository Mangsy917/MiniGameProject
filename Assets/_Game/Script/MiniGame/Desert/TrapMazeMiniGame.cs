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
    [SerializeField, Range(0.1f, 1f)] float BodyHitRatio = 0.5f; // 함정·적·출구 판정: 그려진 몸 중앙에서 이 비율만큼만
    [SerializeField] Image[] TrapIMGs;                          // 함정 바닥 (개수와 위치는 프리팹에서 정한다)
    [SerializeField] RectTransform[] Walls;
    [SerializeField] Patrol[] Patrols;                          // 적
    [SerializeField] Image ExitIMG;

    [Header("Walk")]                                            // 캐릭터 걷기 그림 (비어 있으면 슬롯 그림/임시 이미지 그대로)
    [SerializeField] Sprite[] WalkDownSprites;                  // 아래로 걸을 때 (앞모습). Player_Walk_Foward_1, _2
    [SerializeField] Sprite[] WalkUpSprites;                    // 위로 걸을 때 (뒷모습). Player_Walk_back_1, _2
    [SerializeField] float WalkFrameTime = 0.15f;               // 걷는 그림이 바뀌는 간격(초)

    float[] phases;
    float timer;
    bool facingUp;
    int walkFrame;
    float walkTimer;
    Vector2 lastPlayerPos;

    protected override void Setup()
    {
        base.Setup();
        lastPlayerPos = PlayerIMG.rectTransform.anchoredPosition;
        if (HasWalkSprites())
            SetLabel(PlayerIMG, "");
        RefreshWalk();
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
        UpdateWalk(deltaTime);
        RefreshTraps();
        MovePatrols();

        foreach (Patrol patrol in Patrols)
        {
            if (HitsPlayer(patrol.Image.rectTransform, HitShrink))
            {
                Finish(false);
                return;
            }
        }

        for (int i = 0; i < TrapIMGs.Length; i++)
        {
            if (IsSpearOut(i) && HitsPlayer(TrapIMGs[i].rectTransform, 1f))
            {
                Finish(false);
                return;
            }
        }
        if (HitsPlayer(ExitIMG.rectTransform, 1f))
            Finish(true);
    }

    // 캐릭터 판정 상자의 반 크기: 실제로 그려진 몸(그림 비율 반영)의 가운데 BodyHitRatio 만큼
    Vector2 PlayerHitHalf()
    {
        Vector2 size = PlayerIMG.rectTransform.rect.size;
        Sprite sprite = PlayerIMG.sprite;
        if (PlayerIMG.preserveAspect && sprite != null && size.y > 0f)
        {
            float aspect = sprite.rect.width / sprite.rect.height;
            if (size.x / size.y > aspect)
                size.x = size.y * aspect;
            else
                size.y = size.x / aspect;
        }
        return size * 0.5f * BodyHitRatio;
    }

    // other는 그림 크기에 otherShrink를 곱한 상자로 본다
    bool HitsPlayer(RectTransform other, float otherShrink)
    {
        Vector2 d = PlayerIMG.rectTransform.anchoredPosition - other.anchoredPosition;
        Vector2 half = PlayerHitHalf() + other.rect.size * 0.5f * otherShrink;
        return Mathf.Abs(d.x) < half.x && Mathf.Abs(d.y) < half.y;
    }

    // 움직이는 동안 두 그림을 번갈아 보여 주고, 멈추면 첫 그림으로 선다.
    // 위로 움직이면 뒷모습, 아래로 움직이면 앞모습. 옆으로만 움직일 때는 바라보던 방향을 유지한다
    void UpdateWalk(float deltaTime)
    {
        Vector2 pos = PlayerIMG.rectTransform.anchoredPosition;
        Vector2 moved = pos - lastPlayerPos;
        lastPlayerPos = pos;

        if (moved.sqrMagnitude > 0.01f)
        {
            if (Mathf.Abs(moved.y) > 0.1f)
                facingUp = moved.y > 0f;
            walkTimer += deltaTime;
            if (walkTimer >= WalkFrameTime)
            {
                walkTimer -= WalkFrameTime;
                walkFrame++;
            }
        }
        else
        {
            walkTimer = 0f;
            walkFrame = 0;
        }
        RefreshWalk();
    }

    void RefreshWalk()
    {
        Sprite[] sprites = facingUp ? WalkUpSprites : WalkDownSprites;
        if (sprites == null || sprites.Length == 0 || sprites[walkFrame % sprites.Length] == null)
            return;
        PlayerIMG.sprite = sprites[walkFrame % sprites.Length];
        PlayerIMG.color = Color.white;
        PlayerIMG.preserveAspect = true;
    }

    bool HasWalkSprites()
    {
        return (WalkDownSprites != null && WalkDownSprites.Length > 0) || (WalkUpSprites != null && WalkUpSprites.Length > 0);
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
