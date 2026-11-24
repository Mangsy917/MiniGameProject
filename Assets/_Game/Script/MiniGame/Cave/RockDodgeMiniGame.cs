using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// CAV_09 굴러오는 돌들을 피해라
// 좌우로 스와이프해 화면 위쪽 밖에서 굴러오는 돌을 끝까지 피한다. 닿으면 즉시 실패.
public class RockDodgeMiniGame : DodgeMiniGame
{
    [Header("CAV_09")]
    [SerializeField] int RockCount = 3;
    [SerializeField] float RockSpeed = 620f;
    [SerializeField] float RockInterval = 0.5f;                  // 돌마다 출발하는 시간 차이
    [SerializeField, Range(0f, 1f)] float AimRatio = 0.5f;       // 캐릭터가 있는 쪽으로 굴러올 확률
    [SerializeField] float SpinSpeed = 360f;
    [SerializeField] Image RockIMG;                              // 복제해서 쓰는 원본

    readonly List<Image> rocks = new List<Image>();
    float timer;
    float topY, bottomY;   // 돌이 완전히 화면 밖에 있게 되는 높이

    protected override void Setup()
    {
        base.Setup();
        Rect screen = FullScreen;
        float size = RockIMG.rectTransform.rect.size.magnitude;   // 돌아가도 화면에 걸치지 않도록 대각선 길이
        topY = screen.yMax + size * 0.5f;
        bottomY = screen.yMin - size * 0.5f;
        for (int i = 0; i < RockCount; i++)
            rocks.Add(Spawn(RockIMG, new Vector2(NextX(), topY)));
    }

    protected override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);
        float rockDelta = deltaTime * SpeedScale;
        timer += rockDelta;

        for (int i = 0; i < rocks.Count; i++)
        {
            if (timer < RockInterval * i)
                continue;

            RectTransform rock = rocks[i].rectTransform;
            rock.anchoredPosition += Vector2.down * (RockSpeed * rockDelta);
            rock.Rotate(0f, 0f, SpinSpeed * rockDelta);

            if (Overlap(rocks[i], PlayerIMG, HitShrink))
            {
                Finish(false);
                return;
            }
            // 화면 아래로 완전히 나가면 화면 위쪽 밖에서 다시 굴러온다
            if (rock.anchoredPosition.y < bottomY)
                rock.anchoredPosition = new Vector2(NextX(), topY);
        }
    }

    float NextX()
    {
        if (Random.value < AimRatio)
            return PlayerIMG.rectTransform.anchoredPosition.x + Random.Range(-60f, 60f);
        return Random.Range(-MoveHalfSize.x, MoveHalfSize.x);
    }
}
