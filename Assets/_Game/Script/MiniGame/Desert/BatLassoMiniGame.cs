using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// DES_15 박쥐를 잡아라
// 날아다니는 박쥐 주변에 선을 그어 감싸면 성공. 선이 완전히 이어지지 않아도 CloseGap 안이면 이어진 것으로 본다.
// 선의 길이는 정해져 있고, 다 쓰거나 손을 떼면 지워져서 다시 그어야 한다.
public class BatLassoMiniGame : TouchMiniGame
{
    const float PointSpacing = 12f;

    [Header("DES_15")]
    [SerializeField] int BatCount = 1;
    [SerializeField] float BatSpeed = 470f;
    [SerializeField] float ZigzagTime = 0.35f;    // 이 시간마다 위아래 방향을 꺾는다
    [SerializeField] float FlapTime = 0.1f;       // 날갯짓 한 프레임 시간
    [SerializeField] float MaxLineLength = 840f; // 한 번에 그을 수 있는 선 길이 (놀이 영역 너비가 800)
    [SerializeField] float CloseGap = 70f;        // 선과 선이 이만큼 떨어져 있어도 이어진 것으로 본다
    [SerializeField] float MinLoopLength = 250f;  // 이보다 짧은 고리는 원으로 보지 않는다
    [SerializeField] Image BatIMG;                // 복제해서 쓰는 원본
    [SerializeField] Image CageIMG;               // 잡은 박쥐 위에 나타나는 철장
    [SerializeField] UILine Line;

    class Bat
    {
        public Image Image;
        public Vector2 Velocity;
        public float TurnTimer;
        public bool Caught;
    }

    readonly List<Bat> bats = new List<Bat>();
    readonly List<Vector2> points = new List<Vector2>();
    float length;
    float flapTimer;
    bool drawing;

    protected override void Setup()
    {
        foreach (Vector2 pos in RandomPositions(BatCount, InnerArea(BatIMG.rectTransform, 60f), 120f))
        {
            Bat bat = new Bat { Image = Spawn(BatIMG, pos), TurnTimer = ZigzagTime };
            bat.Velocity = new Vector2(Random.value < 0.5f ? -0.8f : 0.8f, 0.6f) * BatSpeed;
            bats.Add(bat);
        }
        SetActive(CageIMG, false);
        EraseLine();
    }

    protected override void OnDragBegin(Vector2 pos)
    {
        EraseLine();
        drawing = true;
        points.Add(pos);
    }

    protected override void OnDrag(Vector2 pos, Vector2 delta)
    {
        if (!drawing)
            return;

        // 빠르게 그어도 촘촘히 검사하도록 일정 간격으로 점을 찍는다
        while (drawing && IsPlaying && Vector2.Distance(points[points.Count - 1], pos) >= PointSpacing)
        {
            points.Add(Vector2.MoveTowards(points[points.Count - 1], pos, PointSpacing));
            length += PointSpacing;
            CheckLoops();
            if (IsPlaying && length >= MaxLineLength)
                EraseLine();
        }
        RefreshLine();
    }

    protected override void OnDragEnd(Vector2 pos)
    {
        EraseLine();
    }

    // 방금 그은 끝점과 가까운 이전 점을 찾아, 그 사이의 고리가 박쥐를 감싸는지 본다
    void CheckLoops()
    {
        int last = points.Count - 1;
        int minCount = Mathf.CeilToInt(MinLoopLength / PointSpacing);
        for (int i = 0; i <= last - minCount; i++)
        {
            if (Vector2.Distance(points[i], points[last]) > CloseGap)
                continue;

            bool all = true;
            foreach (Bat bat in bats)
            {
                if (!bat.Caught && Inside(bat.Image.rectTransform.anchoredPosition, i, last))
                    Catch(bat);
                all &= bat.Caught;
            }
            if (all)
            {
                RefreshLine();
                Finish(true);
                return;
            }
        }
    }

    bool Inside(Vector2 p, int first, int last)
    {
        bool inside = false;
        for (int i = first, j = last; i <= last; j = i++)
        {
            Vector2 a = points[i];
            Vector2 b = points[j];
            if (a.y > p.y != b.y > p.y && p.x < (b.x - a.x) * (p.y - a.y) / (b.y - a.y) + a.x)
                inside = !inside;
        }
        return inside;
    }

    void Catch(Bat bat)
    {
        bat.Caught = true;
        CageIMG.rectTransform.anchoredPosition = bat.Image.rectTransform.anchoredPosition;
        SetActive(CageIMG, true);
    }

    void EraseLine()
    {
        drawing = false;
        length = 0f;
        points.Clear();
        RefreshLine();
    }

    void RefreshLine()
    {
        Line.SetPoints(points);
        SetGauge(1f - length / MaxLineLength);   // 남은 선 길이
    }

    protected override void Tick(float deltaTime)
    {
        flapTimer += deltaTime;
        int frame = (int)(flapTimer / Mathf.Max(0.01f, FlapTime)) % 2;
        Rect area = InnerArea(BatIMG.rectTransform, 60f);

        foreach (Bat bat in bats)
        {
            Show(bat.Image, 1, frame);
            if (bat.Caught)
                continue;

            bat.TurnTimer -= deltaTime;
            if (bat.TurnTimer <= 0f)
            {
                bat.TurnTimer = ZigzagTime;
                bat.Velocity.y = -bat.Velocity.y;
            }
            Bounce(bat.Image.rectTransform, ref bat.Velocity, area, deltaTime);
        }
    }
}
