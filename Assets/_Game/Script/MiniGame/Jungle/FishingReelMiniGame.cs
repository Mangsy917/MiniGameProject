using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// JUN_08 물고기를 낚아라
// 낚싯대의 릴을 돌린 만큼 멀리 있던 물고기가 가까이 끌려온다. 멈추면 다시 끌려간다.
public class FishingReelMiniGame : RotateGaugeMiniGame
{
    [Header("JUN_08")]
    [SerializeField] Image FishIMG;
    [SerializeField] Vector2 FishMove = new Vector2(150f, -130f);   // 다 감았을 때까지 움직이는 거리
    [SerializeField] float FarScale = 0.5f;                         // 멀리 있을 때의 크기
    [SerializeField] RectTransform RodTip;                          // 낚싯줄이 시작되는 낚싯대 끝
    [SerializeField] UILine FishingLine;

    Vector2 fishStart;

    protected override void Setup()
    {
        fishStart = FishIMG.rectTransform.anchoredPosition;
        base.Setup();
    }

    protected override void OnProgress(float value)
    {
        Vector2 pos = fishStart + FishMove * value;
        FishIMG.rectTransform.anchoredPosition = pos;
        FishIMG.rectTransform.localScale = Vector3.one * Mathf.Lerp(FarScale, 1f, value);
        FishingLine.SetPoints(new List<Vector2> { RodTip.anchoredPosition, pos });
    }
}
