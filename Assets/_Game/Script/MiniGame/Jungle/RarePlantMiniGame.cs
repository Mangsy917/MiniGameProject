using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// JUN_06 희귀 식물을 채집해라
// 화면 왼쪽 위에 보이는 희귀 식물을 화면 곳곳의 식물 중에서 찾아 탭하면 성공, 다른 식물을 탭하면 실패.
public class RarePlantMiniGame : PickOneMiniGame
{
    [Header("JUN_06")]
    [SerializeField] int ChoiceCount = 4;      // 희귀 식물 1 + 일반 식물
    [SerializeField] float Spacing = 150f;
    [SerializeField] float CardMargin = 10f;   // 카드와 화면 왼쪽 위 모서리 사이 간격
    [SerializeField] Image CardIMG;            // 왼쪽 위 카드
    [SerializeField] Image CardPlantIMG;       // 카드에 보이는 희귀 식물
    [SerializeField] Image PlantIMG;           // 복제해서 쓰는 원본

    protected override void Setup()
    {
        // 카드를 화면 왼쪽 위에 붙인다
        Rect cardArea = ScreenArea(CardIMG.rectTransform);
        Vector2 cardPos = new Vector2(cardArea.xMin + CardMargin, cardArea.yMax - CardMargin);
        CardIMG.rectTransform.anchoredPosition = cardPos;
        CardPlantIMG.rectTransform.anchoredPosition = cardPos;

        int rareKind = Random.Range(0, GetSlot(2).StateCount);
        Show(CardPlantIMG, 2, rareKind);

        // 식물은 화면 전체에 나오되 카드와는 겹치지 않는다
        Vector2 avoidSize = CardIMG.rectTransform.rect.size + PlantIMG.rectTransform.rect.size + Vector2.one * HitPadding * 2f;
        Rect avoid = new Rect(cardPos - avoidSize * 0.5f, avoidSize);
        List<Vector2> positions = RandomPositions(ChoiceCount, ScreenArea(PlantIMG.rectTransform), Spacing, avoid);
        for (int i = 0; i < positions.Count; i++)
        {
            Image plant = Spawn(PlantIMG, positions[i]);
            bool rare = i == 0;
            if (rare)
                Show(plant, 2, rareKind);
            else
                Show(plant, 3, (i - 1) % GetSlot(3).StateCount);
            AddChoice(plant, rare);
        }
    }
}
