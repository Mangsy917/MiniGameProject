using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// CAV_15 횃불에 불을 붙여라
// 한 줄로 늘어선 횃불을 하나씩 누르고 있으면 불이 붙는다. 모두 켜면 성공. 다 붙기 전에 떼면 처음부터.
public class TorchHoldMiniGame : TouchMiniGame
{
    [Header("CAV_15")]
    [SerializeField] int TorchCount = 4;
    [SerializeField] float HoldTime = 0.5f;    // 불이 붙을 때까지 누르고 있는 시간
    [SerializeField] float Spacing = 180f;
    [SerializeField] Image TorchIMG;           // 복제해서 쓰는 원본. 상태: 꺼짐 / 켜짐

    readonly List<Image> torches = new List<Image>();
    Vector2 finger;
    bool pressing;
    Image holding;
    float holdTimer;

    protected override void Setup()
    {
        Vector2 center = TorchIMG.rectTransform.anchoredPosition;
        for (int i = 0; i < TorchCount; i++)
        {
            Image torch = Spawn(TorchIMG, center + new Vector2((i - (TorchCount - 1) * 0.5f) * Spacing, 0f));
            Show(torch, 1, 0);
            torches.Add(torch);
        }
    }

    protected override void OnDragBegin(Vector2 pos)
    {
        pressing = true;
        finger = pos;
    }

    protected override void OnDrag(Vector2 pos, Vector2 delta)
    {
        finger = pos;
    }

    protected override void OnDragEnd(Vector2 pos)
    {
        pressing = false;
    }

    protected override void Tick(float deltaTime)
    {
        // 꺼진 횃불만 torches에 남아 있다
        Image now = pressing ? torches.Find(torch => Hit(torch, finger)) : null;
        if (now != holding)
        {
            holding = now;
            holdTimer = 0f;
        }
        if (holding == null)
            return;

        holdTimer += deltaTime;
        if (holdTimer < HoldTime)
            return;

        Show(holding, 1, 1);
        torches.Remove(holding);
        holding = null;
        if (torches.Count == 0)
            Finish(true);
    }
}
