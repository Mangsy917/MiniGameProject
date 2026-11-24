using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// JUN_09 과일의 개수를 맞춰라
// 통통 튀는 과일 중 지시문에 나온 과일의 개수를 숫자 버튼 3개 중에서 고른다.
public class FruitCountMiniGame : PickOneMiniGame
{
    [Header("JUN_09")]
    [SerializeField] int MinTotal = 6;
    [SerializeField] int MaxTotal = 9;
    [SerializeField] int MinAnswer = 2;
    [SerializeField] int MaxAnswer = 4;
    [SerializeField] float MoveSpeed = 260f;          // 옆으로 움직이는 속도
    [SerializeField] float Gravity = 1600f;           // 클수록 빠르게 튄다
    [SerializeField, Range(0f, 1f)] float MinBounce = 0.5f;   // 튀어 오르는 높이 (영역 높이 대비). 과일마다 MinBounce~1
    [SerializeField] float SquashTime = 0.12f;        // 바닥에 닿았을 때 찌그러지는 시간
    [SerializeField] float ButtonAreaHeight = 130f;   // 아래 버튼 자리. 과일이 들어오지 않는다
    [SerializeField] Image FruitIMG;                  // 복제해서 쓰는 원본
    [SerializeField] Image[] ButtonIMGs;

    class Fruit
    {
        public RectTransform Transform;
        public Vector2 Velocity;
        public float JumpSpeed;
        public float Squash;
    }

    readonly List<Fruit> fruits = new List<Fruit>();

    protected override void Setup()
    {
        SpriteSlot slot = GetSlot(1);
        int types = slot.StateCount;   // 과일 종류 수
        int type = Random.Range(0, types);
        int answer = Random.Range(MinAnswer, MaxAnswer + 1);
        int total = Mathf.Max(answer, Random.Range(MinTotal, MaxTotal + 1));
        SetTitle(slot.StateNames[type] + "의 수는?");

        Rect area = FruitArea();
        List<Vector2> positions = RandomPositions(total, area, 90f);
        for (int i = 0; i < total; i++)
        {
            Image image = Spawn(FruitIMG, positions[i]);
            int fruitType = i < answer ? type : (type + Random.Range(1, types)) % types;
            Show(image, 1, fruitType);
            if (slot.GetSprite(fruitType) == null)
                SetLabel(image, slot.StateNames[fruitType]);

            Fruit fruit = new Fruit { Transform = image.rectTransform };
            fruit.JumpSpeed = Mathf.Sqrt(2f * Gravity * area.height * Random.Range(MinBounce, 1f));
            fruit.Velocity = new Vector2((Random.value < 0.5f ? -1f : 1f) * MoveSpeed * Random.Range(0.7f, 1.2f), 0f);
            fruits.Add(fruit);
        }

        // 정답을 포함한 연속된 숫자. 정답이 놓이는 자리는 랜덤
        int lowest = Mathf.Max(1, answer - Random.Range(0, ButtonIMGs.Length));
        for (int i = 0; i < ButtonIMGs.Length; i++)
        {
            int number = lowest + i;
            SetLabel(ButtonIMGs[i], number.ToString()).resizeTextMaxSize = 60;
            AddChoice(ButtonIMGs[i], number == answer);
        }
    }

    Rect FruitArea()
    {
        return InnerArea(FruitIMG.rectTransform, ButtonAreaHeight);
    }

    protected override void Tick(float deltaTime)
    {
        deltaTime *= SpeedScale;
        Rect area = FruitArea();
        foreach (Fruit fruit in fruits)
        {
            fruit.Velocity.y -= Gravity * deltaTime;
            Vector2 pos = fruit.Transform.anchoredPosition + fruit.Velocity * deltaTime;

            if (pos.x < area.xMin || pos.x > area.xMax)
            {
                fruit.Velocity.x = Mathf.Abs(fruit.Velocity.x) * (pos.x < area.xMin ? 1f : -1f);
                pos.x = Mathf.Clamp(pos.x, area.xMin, area.xMax);
            }
            if (pos.y <= area.yMin)
            {
                pos.y = area.yMin;
                fruit.Velocity.y = fruit.JumpSpeed;
                fruit.Squash = SquashTime;
            }
            fruit.Transform.anchoredPosition = pos;

            fruit.Squash = Mathf.Max(0f, fruit.Squash - deltaTime);
            float squash = SquashTime > 0f ? fruit.Squash / SquashTime : 0f;
            fruit.Transform.localScale = new Vector3(1f + 0.25f * squash, 1f - 0.3f * squash, 1f);
        }
    }
}
