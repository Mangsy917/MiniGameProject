using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 공통 템플릿: 정답 탭 (DES_03)
// 랜덤 위치의 대상을 모두 탭하면 성공. 탭한 대상은 두 번째 상태로 바뀌고 다시 탭되지 않는다.
public class TapAllMiniGame : TouchMiniGame
{
    [Header("Tap All")]
    [SerializeField] int TargetCount = 5;
    [SerializeField] float MoveSpeed = 320f;
    [SerializeField] float Spacing = 140f;     // 처음 배치할 때 대상끼리 최소 간격
    [SerializeField] Image TargetIMG;          // 복제해서 쓰는 원본. 상태: 탭하기 전 / 탭한 뒤
    [SerializeField] int TargetSlot = 1;

    class Target
    {
        public Image Image;
        public Vector2 Velocity;
        public bool Done;
    }

    readonly List<Target> targets = new List<Target>();
    int doneCount;

    protected override void Setup()
    {
        foreach (Vector2 pos in RandomPositions(TargetCount, InnerArea(TargetIMG.rectTransform), Spacing))
        {
            Image image = Spawn(TargetIMG, pos);
            Show(image, TargetSlot, 0);
            targets.Add(new Target { Image = image, Velocity = RandomDirection() });
        }
    }

    protected override void OnBegin()
    {
        foreach (Target target in targets)
            target.Velocity *= MoveSpeed * SpeedScale;
    }

    protected override void OnTap(Vector2 pos)
    {
        // 겹쳐 있으면 한 번에 하나만
        foreach (Target target in targets)
        {
            if (target.Done || !Hit(target.Image, pos))
                continue;

            target.Done = true;
            doneCount++;
            Show(target.Image, TargetSlot, 1);
            if (doneCount >= targets.Count)
                Finish(true);
            return;
        }
    }

    protected override void Tick(float deltaTime)
    {
        Rect area = InnerArea(TargetIMG.rectTransform);
        foreach (Target target in targets)
        {
            if (!target.Done)
                Bounce(target.Image.rectTransform, ref target.Velocity, area, deltaTime);
        }
    }
}
