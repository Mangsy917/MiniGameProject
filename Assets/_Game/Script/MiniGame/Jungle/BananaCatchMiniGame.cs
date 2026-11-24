using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// JUN_04 떨어지는 코코넛을 잡아라 (문서 가정에 따라 바나나로 구현, 공통 템플릿: 받기)
// 바구니를 좌우로 끌어 떨어지는 바나나를 모두 받는다. 하나라도 놓치면 실패, 다 받으면 바로 성공.
public class BananaCatchMiniGame : TouchMiniGame
{
    [Header("JUN_04")]
    [SerializeField] int BananaCount = 4;
    [SerializeField] float FallSpeed = 520f;
    [SerializeField] float FirstDropDelay = 0.3f;
    [SerializeField] float MinDropGap = 0.35f;       // 바나나끼리 최소 시간 차이 (못 받는 경우 방지)
    [SerializeField] float LastCatchMargin = 0.2f;   // 마지막 바나나는 제한시간보다 이만큼 먼저 바구니 높이에 온다
    [SerializeField] float MissY = -235f;            // 이보다 내려가면 놓친 것
    [SerializeField] Image BasketIMG;
    [SerializeField] Image BananaIMG;                // 복제해서 쓰는 원본 (놓인 높이에서 떨어진다)

    readonly List<Image> bananas = new List<Image>();
    float timer;
    int caught;
    float fallSpeed;
    float[] dropTimes;   // 바나나마다 떨어지기 시작하는 시간

    protected override void Setup()
    {
        Rect area = InnerArea(BananaIMG.rectTransform);
        for (int i = 0; i < BananaCount; i++)
            bananas.Add(Spawn(BananaIMG, new Vector2(Random.Range(area.xMin, area.xMax), BananaIMG.rectTransform.anchoredPosition.y)));
    }

    protected override void OnBegin()
    {
        fallSpeed = FallSpeed * SpeedScale;
        float fallTime = (BananaIMG.rectTransform.anchoredPosition.y - BasketIMG.rectTransform.anchoredPosition.y) / fallSpeed;
        float window = TimeLimit - fallTime - FirstDropDelay - LastCatchMargin;

        // 최소 간격을 뺀 나머지 시간 안에서 랜덤하게 나눈다
        float free = Mathf.Max(0f, window - MinDropGap * (BananaCount - 1));
        dropTimes = new float[BananaCount];
        for (int i = 0; i < BananaCount; i++)
            dropTimes[i] = Random.Range(0f, free);
        System.Array.Sort(dropTimes);
        for (int i = 0; i < BananaCount; i++)
            dropTimes[i] += FirstDropDelay + MinDropGap * i;
    }

    protected override void OnDragBegin(Vector2 pos)
    {
        MoveBasket(pos.x);
    }

    protected override void OnDrag(Vector2 pos, Vector2 delta)
    {
        MoveBasket(pos.x);
    }

    void MoveBasket(float x)
    {
        RectTransform basket = BasketIMG.rectTransform;
        Rect area = InnerArea(basket);
        basket.anchoredPosition = new Vector2(Mathf.Clamp(x, area.xMin, area.xMax), basket.anchoredPosition.y);
    }

    protected override void Tick(float deltaTime)
    {
        timer += deltaTime;
        for (int i = 0; i < bananas.Count; i++)
        {
            Image banana = bananas[i];
            if (!banana.gameObject.activeSelf || timer < dropTimes[i])
                continue;

            banana.rectTransform.anchoredPosition += Vector2.down * (fallSpeed * deltaTime);
            if (Overlap(banana, BasketIMG, 0.9f))
            {
                banana.gameObject.SetActive(false);
                caught++;
            }
            else if (banana.rectTransform.anchoredPosition.y < MissY)
            {
                Finish(false);
                return;
            }
        }
        if (caught >= bananas.Count)
            Finish(true);
    }
}
