using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// FOR_13 낙엽을 쓸어라
// 화면 전체를 덮은 낙엽을 빗자루로 쓸면 낙엽이 쓴 방향 바깥으로 날아간다. 모두 치우면 성공.
public class LeafSweepMiniGame : TouchMiniGame
{
    [Header("FOR_13")]
    [SerializeField] int LeafCount = 90;
    [SerializeField] float BroomRadius = 110f;                  // 빗자루에 쓸리는 범위
    [SerializeField] float FlySpeed = 1500f;
    [SerializeField, Range(0f, 1f)] float ClearRatio = 0.95f;   // 이만큼 치우면 성공
    [SerializeField] Image LeafIMG;                             // 복제해서 쓰는 원본. 상태: 낙엽 종류
    [SerializeField] Image BroomIMG;

    class Leaf
    {
        public RectTransform Transform;
        public Vector2 Velocity;
        public bool Swept;
    }

    readonly List<Leaf> leaves = new List<Leaf>();
    Vector2 half;
    int swept;

    protected override void Setup()
    {
        // 놀이 영역은 화면 가운데보다 조금 아래에 있으므로 화면 위쪽까지 덮도록 위로 옮긴다
        half = new Vector2(ScreenWidth, ScreenHeight) * 0.5f;
        float offsetY = -PlayArea.anchoredPosition.y / PlayArea.localScale.y;

        int columns = Mathf.Max(1, Mathf.RoundToInt(Mathf.Sqrt(LeafCount * half.x / half.y)));
        int rows = Mathf.Max(1, Mathf.CeilToInt((float)LeafCount / columns));
        Vector2 cell = new Vector2(half.x * 2f / columns, half.y * 2f / rows);
        Vector2 size = Vector2.one * (Mathf.Max(cell.x, cell.y) * 1.5f);   // 빈틈이 없도록 겹친다
        int kinds = GetSlot(1).StateCount;

        for (int i = 0; i < columns * rows; i++)
        {
            Vector2 pos = new Vector2(-half.x + cell.x * (i % columns + Random.Range(0.35f, 0.65f)),
                offsetY - half.y + cell.y * (i / columns + Random.Range(0.35f, 0.65f)));
            Image image = Spawn(LeafIMG, pos);
            int kind = Random.Range(0, kinds);
            Show(image, 1, kind);
            SetLabel(image, "");
            image.rectTransform.sizeDelta = size;
            image.rectTransform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
            leaves.Add(new Leaf { Transform = image.rectTransform });
        }
    }

    protected override void OnDragBegin(Vector2 pos)
    {
        BroomIMG.rectTransform.anchoredPosition = pos;
    }

    protected override void OnDrag(Vector2 pos, Vector2 delta)
    {
        BroomIMG.rectTransform.anchoredPosition = pos;
        foreach (Leaf leaf in leaves)
        {
            Vector2 leafPos = leaf.Transform.anchoredPosition;
            if (leaf.Swept || ErasableImage.DistanceToSegment(leafPos, pos - delta, pos) > BroomRadius)
                continue;

            // 쓴 방향에 화면 바깥 방향을 섞어 날려 보낸다
            leaf.Swept = true;
            Vector2 direction = delta.normalized + leafPos.normalized * 0.6f;
            if (direction.sqrMagnitude < 0.01f)
                direction = delta;
            leaf.Velocity = direction.normalized * (FlySpeed * Random.Range(0.8f, 1.2f));
            swept++;
        }
        if (swept >= leaves.Count * ClearRatio)
            Finish(true);
    }

    protected override void Tick(float deltaTime)
    {
        foreach (Leaf leaf in leaves)
        {
            if (!leaf.Swept || !leaf.Transform.gameObject.activeSelf)
                continue;

            leaf.Transform.anchoredPosition += leaf.Velocity * deltaTime;
            leaf.Transform.Rotate(0f, 0f, 540f * deltaTime);
            Vector2 pos = leaf.Transform.anchoredPosition;
            if (Mathf.Abs(pos.x) > half.x * 1.3f || Mathf.Abs(pos.y) > half.y * 1.5f)
                leaf.Transform.gameObject.SetActive(false);
        }
    }
}
