using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// PRE_15 흔들다리를 건너라 (공통 템플릿: 순서 탭)
// 캐릭터의 시야에서 앞에 놓인 판자를 순서대로 탭해 건넌다. 부서진 판자는 건너뛰고, 탭하면 떨어져 실패.
// 순서를 건너뛴 탭은 무시한다.
public class BridgeCrossMiniGame : TouchMiniGame
{
    [Header("PRE_15")]
    [SerializeField] int PlankCount = 10;
    [SerializeField] int MinBroken = 2;
    [SerializeField] int MaxBroken = 3;
    [SerializeField] float WalkSpeed = 25f;       // 초당 지나가는 판자 수 (앞으로 나아가는 연출 속도)
    [SerializeField] float HorizonY = 190f;       // 판자가 멀어지며 모이는 높이
    [SerializeField] float NearDepth = 370f;      // 바로 앞 판자가 지평선에서 내려온 거리
    [SerializeField] float Perspective = 0.4f;    // 클수록 멀리 있는 판자가 빨리 작아진다
    [SerializeField] Image PlankIMG;              // 복제해서 쓰는 원본 (바로 앞 판자의 크기). 상태: 멀쩡함 / 부서짐
    [SerializeField] Image FallIMG;               // 추락 이펙트

    readonly List<Image> planks = new List<Image>();
    Vector2 plankSize;
    bool[] broken;
    int next;
    int standing = -1;      // 지금 밟고 있는 판자. -1이면 다리 앞
    float viewPos = -1f;    // 화면에 그리는 위치. standing을 부드럽게 따라간다

    protected override void Setup()
    {
        ChooseBroken();
        plankSize = PlankIMG.rectTransform.sizeDelta;
        // Spawn은 원본 바로 위에 끼워 넣으므로, 앞 판자부터 만들면 먼 판자가 위에 온다. 먼 것부터 만들어 가까운 판자를 위로
        for (int i = 0; i < PlankCount; i++)
        {
            Image plank = Spawn(PlankIMG, Vector2.zero);
            Show(plank, 2, broken[i] ? 1 : 0);
            planks.Add(plank);
        }
        for (int i = PlankCount - 1; i >= 0; i--)
            planks[i].transform.SetSiblingIndex(PlankIMG.transform.GetSiblingIndex() + PlankCount - i);

        SetActive(FallIMG, false);
        Place();
    }

    // 첫 판자와 마지막 판자는 멀쩡하고, 부서진 판자는 2개 연속으로 나오지 않는다
    void ChooseBroken()
    {
        broken = new bool[PlankCount];
        List<int> candidates = new List<int>();
        for (int i = 1; i < PlankCount - 1; i++)
            candidates.Add(i);
        Shuffle(candidates);

        int remain = Random.Range(MinBroken, MaxBroken + 1);
        foreach (int i in candidates)
        {
            if (remain > 0 && !broken[i - 1] && !broken[i + 1])
            {
                broken[i] = true;
                remain--;
            }
        }
    }

    void Place()
    {
        for (int i = 0; i < planks.Count; i++)
        {
            float depth = i - viewPos - 1f;   // 0이면 바로 앞
            bool visible = depth > -0.9f;     // 발밑을 지나간 판자는 숨긴다
            SetActive(planks[i], visible);
            if (!visible)
                continue;

            float scale = 1f / (1f + Perspective * depth);
            RectTransform rt = planks[i].rectTransform;
            rt.anchoredPosition = new Vector2(0f, HorizonY - NearDepth * scale);
            rt.sizeDelta = plankSize * scale;
        }
    }

    protected override void OnTap(Vector2 pos)
    {
        for (int i = 0; i < planks.Count; i++)
        {
            if (!planks[i].gameObject.activeSelf || !Hit(planks[i].rectTransform, pos, 2f))
                continue;

            // 앞으로 나아가는 중에는 판자가 움직이므로 잘못 눌린 것으로 실패시키지 않는다
            if (broken[i] && viewPos != standing)
                return;
            if (broken[i])
                Fall(i);
            else if (i == next)
                Step(i);
            return;
        }
    }

    void Step(int index)
    {
        standing = index;
        next = index + 1;
        while (next < PlankCount && broken[next])
            next++;
        if (next >= PlankCount)
            Finish(true);
    }

    void Fall(int index)
    {
        FallIMG.rectTransform.anchoredPosition = planks[index].rectTransform.anchoredPosition;
        SetActive(FallIMG, true);
        Finish(false);
    }

    protected override void Tick(float deltaTime)
    {
        viewPos = Mathf.MoveTowards(viewPos, standing, WalkSpeed * deltaTime);
        Place();
    }
}
