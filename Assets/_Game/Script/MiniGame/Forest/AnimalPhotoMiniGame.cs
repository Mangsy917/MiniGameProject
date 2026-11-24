using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// FOR_07 숲 동물을 촬영해라 (공통 템플릿: 카메라 촬영)
// 프레임을 끌어 동물에 맞추고 촬영 버튼을 누른다. 빈 곳을 찍어도 실패가 아니다.
public class AnimalPhotoMiniGame : TouchMiniGame
{
    [Header("FOR_07")]
    [SerializeField] int TargetCount = 3;
    [SerializeField] float BottomHeight = 150f;   // 아래 촬영 버튼 자리. 동물이 나오지 않는다
    [SerializeField] float Spacing = 170f;
    [SerializeField] float FlashTime = 0.12f;
    [SerializeField] float ListHeight = 90f;      // 위쪽 동물 리스트 자리. 동물이 나오지 않는다
    [SerializeField] float ListSpacing = 80f;
    [SerializeField] Image ListIMG;               // 왼쪽 위 동물 리스트 칸. 복제해서 쓰는 원본
    [SerializeField] Image CheckIMG;              // 찍은 동물의 체크표시. 복제해서 쓰는 원본
    [SerializeField] Image AnimalIMG;             // 복제해서 쓰는 원본
    [SerializeField] Image FrameIMG;
    [SerializeField] Image ShutterIMG;
    [SerializeField] Image FlashIMG;

    readonly List<Image> remain = new List<Image>();
    readonly List<Image> checks = new List<Image>();   // remain과 같은 순서
    float flashTimer;
    bool grabbed;

    protected override void Setup()
    {
        SetActive(FlashIMG, false);

        int kinds = GetSlot(1).StateCount;
        List<int> order = new List<int>();
        for (int i = 0; i < Mathf.Max(kinds, TargetCount); i++)
            order.Add(i % kinds);
        Shuffle(order);

        Rect area = InnerArea(AnimalIMG.rectTransform, BottomHeight);
        area.yMax -= ListHeight;
        List<Vector2> positions = RandomPositions(TargetCount, area, Spacing);
        for (int i = 0; i < TargetCount; i++)
        {
            Image animal = Spawn(AnimalIMG, positions[i]);
            Show(animal, 1, order[i]);
            remain.Add(animal);

            Vector2 listPos = ListIMG.rectTransform.anchoredPosition + new Vector2(ListSpacing * i, 0f);
            Show(Spawn(ListIMG, listPos), 1, order[i]);
            Image check = Spawn(CheckIMG, listPos);
            SetActive(check, false);
            checks.Add(check);
        }
    }

    protected override void OnTap(Vector2 pos)
    {
        // 프레임을 잡은 손가락과 다른 손가락으로 눌러도 된다
        if (Hit(ShutterIMG, pos))
            Shoot();
    }

    protected override void OnDragBegin(Vector2 pos)
    {
        grabbed = !Hit(ShutterIMG, pos) && Hit(FrameIMG.rectTransform, pos, 30f);
    }

    protected override void OnDrag(Vector2 pos, Vector2 delta)
    {
        if (!grabbed)
            return;
        Vector2 next = FrameIMG.rectTransform.anchoredPosition + delta;
        Vector2 half = HalfArea;
        FrameIMG.rectTransform.anchoredPosition = new Vector2(Mathf.Clamp(next.x, -half.x, half.x), Mathf.Clamp(next.y, -half.y, half.y));
    }

    void Shoot()
    {
        flashTimer = FlashTime;
        SetActive(FlashIMG, true);

        // 프레임 중심이 동물 위에 있으면 찍힌다
        for (int i = remain.Count - 1; i >= 0; i--)
        {
            if (!Hit(remain[i].rectTransform, FrameIMG.rectTransform.anchoredPosition, HitPadding))
                continue;
            SetAlpha(remain[i], 0.25f);
            remain.RemoveAt(i);
            SetActive(checks[i], true);
            checks.RemoveAt(i);
        }
        if (remain.Count == 0)
            Finish(true);
    }

    protected override void Tick(float deltaTime)
    {
        flashTimer -= deltaTime;
        if (flashTimer <= 0f)
            SetActive(FlashIMG, false);
    }
}
