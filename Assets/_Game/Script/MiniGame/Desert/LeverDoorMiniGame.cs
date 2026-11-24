using UnityEngine;
using UnityEngine.UI;

// DES_10 레버를 돌려라
// 레버를 돌린 만큼 문이 좌우로 열린다. 돌리지 않으면 다시 닫힌다.
public class LeverDoorMiniGame : RotateGaugeMiniGame
{
    [Header("DES_10")]
    [SerializeField] Image LeftDoorIMG;
    [SerializeField] Image RightDoorIMG;
    [SerializeField] float OpenDistance = 110f;   // 문 한 짝이 열리는 거리

    Vector2 leftPos, rightPos;

    protected override void Setup()
    {
        leftPos = LeftDoorIMG.rectTransform.anchoredPosition;
        rightPos = RightDoorIMG.rectTransform.anchoredPosition;
        base.Setup();
    }

    protected override void OnProgress(float value)
    {
        LeftDoorIMG.rectTransform.anchoredPosition = leftPos + Vector2.left * (OpenDistance * value);
        RightDoorIMG.rectTransform.anchoredPosition = rightPos + Vector2.right * (OpenDistance * value);
    }
}
