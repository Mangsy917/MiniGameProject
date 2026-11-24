using UnityEngine;
using UnityEngine.UI;

// PRE_16 손에 묻은 초크가루를 털어내라
// 탭할 때마다 박수를 쳐서 손에 묻은 초크가루를 털어낸다.
public class ChalkClapMiniGame : TapGaugeMiniGame
{
    [Header("PRE_16")]
    [SerializeField] int MinTaps = 9;            // 필요한 박수 횟수는 매번 MinTaps~MaxTaps에서 랜덤 (Required Taps는 쓰지 않는다)
    [SerializeField] int MaxTaps = 15;
    [SerializeField] float ClapDistance = 70f;   // 박수 칠 때 손이 가운데로 모이는 거리
    [SerializeField] float ClapTime = 0.12f;
    [SerializeField] Image LeftHandIMG;
    [SerializeField] Image RightHandIMG;
    [SerializeField] Image LeftChalkIMG;         // 손에 묻은 가루 (손 이미지의 자식). 상태가 많음 -> 적음 순서로 바뀌고 마지막에 사라진다
    [SerializeField] Image RightChalkIMG;
    [SerializeField] Image DustIMG;              // 박수 칠 때 날리는 가루

    Vector2 leftPos, rightPos;
    float clapTimer;
    int requiredTaps;

    protected override int TotalTaps
    {
        get { return Mathf.Max(1, requiredTaps); }
    }

    protected override void Setup()
    {
        requiredTaps = Random.Range(MinTaps, MaxTaps + 1);
        base.Setup();
        leftPos = LeftHandIMG.rectTransform.anchoredPosition;
        rightPos = RightHandIMG.rectTransform.anchoredPosition;
        SetActive(DustIMG, false);
        RefreshChalk();
    }

    protected override void OnTapped()
    {
        clapTimer = ClapTime;
        RefreshChalk();
        ShowClap();
    }

    protected override void Tick(float deltaTime)
    {
        clapTimer = Mathf.Max(0f, clapTimer - deltaTime);
        ShowClap();
    }

    void ShowClap()
    {
        float t = ClapTime > 0f ? clapTimer / ClapTime : 0f;
        LeftHandIMG.rectTransform.anchoredPosition = leftPos + Vector2.right * (ClapDistance * t);
        RightHandIMG.rectTransform.anchoredPosition = rightPos + Vector2.left * (ClapDistance * t);
        SetActive(DustIMG, t > 0f);
        DustIMG.rectTransform.localScale = Vector3.one * (1.4f - 0.6f * t);
    }

    void RefreshChalk()
    {
        int count = GetSlot(2).StateCount;
        int state = Mathf.Min((int)(Progress * count), count - 1);
        Show(LeftChalkIMG, 2, state);
        Show(RightChalkIMG, 2, state);
        SetAlpha(LeftChalkIMG, 1f - Progress);
        SetAlpha(RightChalkIMG, 1f - Progress);
    }
}
