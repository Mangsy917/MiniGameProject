using UnityEngine;
using UnityEngine.UI;

// LAB_01 출입 카드를 긁어라
// 카드를 위에서 아래로 적당한 속도로 한 번에 긁으면 성공. 너무 빠르거나 느리면 다시 긁는다.
public class CardSwipeMiniGame : ScratchMiniGame
{
    [Header("LAB_01")]
    [SerializeField] float MinTargetTime = 0.28f;      // 맞춰야 하는 속도 (화면 높이를 지나가는 시간). 매번 이 사이에서 랜덤
    [SerializeField] float MaxTargetTime = 0.5f;
    [SerializeField] float ToleranceTime = 0.13f;      // 이만큼 빠르거나 느려도 인정
    [SerializeField] float MessageTime = 0.6f;
    [SerializeField] Image SpeedIMG;                   // 상태: 빠름 / 느림
    [SerializeField] Text RetryTXT;

    Vector2 startPos;
    float targetTime;
    float messageTimer;

    protected override void Setup()
    {
        targetTime = Random.Range(MinTargetTime, MaxTargetTime);
        startPos = ItemIMG.rectTransform.anchoredPosition;
        HideMessage();
    }

    protected override void OnStroke(float speed)
    {
        float time = 1f / Mathf.Max(0.001f, speed);
        if (time < targetTime - ToleranceTime)
            Retry(0);
        else if (time > targetTime + ToleranceTime)
            Retry(1);
        else
            Finish(true);
    }

    protected override void OnShortStroke()
    {
        ItemIMG.rectTransform.anchoredPosition = startPos;
        SetActive(SpeedIMG, false);
        SetActive(RetryTXT, true);
        messageTimer = MessageTime;
    }

    void Retry(int state)
    {
        Release();
        ItemIMG.rectTransform.anchoredPosition = startPos;
        Show(SpeedIMG, 3, state);
        SetActive(SpeedIMG, true);
        SetActive(RetryTXT, true);
        messageTimer = MessageTime;
    }

    void HideMessage()
    {
        SetActive(SpeedIMG, false);
        SetActive(RetryTXT, false);
    }

    protected override void Tick(float deltaTime)
    {
        messageTimer -= deltaTime;
        if (messageTimer <= 0f)
            HideMessage();
    }
}
