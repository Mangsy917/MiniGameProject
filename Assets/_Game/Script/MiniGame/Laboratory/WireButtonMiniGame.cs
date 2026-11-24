using System;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

// LAB_12 올바른 선을 찾아 버튼을 눌러라
// 얽힌 선을 따라가 가운데 기계와 이어진 버튼을 탭하면 성공, 다른 버튼이면 실패.
public class WireButtonMiniGame : PickOneMiniGame
{
    // 선 패턴 한 장. 3번 자리의 같은 번호 스프라이트와 짝이다
    [Serializable]
    public class WirePattern
    {
        public int AnswerButton;   // 가운데 기계와 이어진 버튼. 0: 왼쪽, 1: 가운데, 2: 오른쪽
        public int[] Crossings;    // 그림이 없을 때만 쓰는 임시 선. 아래에서 위로 이웃한 두 선이 교차하는 순서 (0: 왼쪽-가운데, 1: 가운데-오른쪽)
    }

    [Header("LAB_12")]
    [SerializeField] WirePattern[] Patterns;
    [SerializeField] bool RandomMirror = true;   // 패턴을 좌우로 뒤집어 경우의 수를 늘린다
    [SerializeField] Image MachineIMG;           // 항상 가운데
    [SerializeField] Image[] ButtonIMGs;         // 왼쪽부터
    [SerializeField] Image WireIMG;
    [SerializeField] Image SparkIMG;
    [SerializeField] Color WireColor = Color.black;
    [SerializeField] float WireWidth = 8f;

    protected override void Setup()
    {
        int index = Random.Range(0, Patterns.Length);
        WirePattern pattern = Patterns[index];
        bool mirror = RandomMirror && Random.value < 0.5f;
        int answer = mirror ? ButtonIMGs.Length - 1 - pattern.AnswerButton : pattern.AnswerButton;

        Show(WireIMG, 3, index);
        WireIMG.preserveAspect = false;
        WireIMG.rectTransform.localScale = new Vector3(mirror ? -1f : 1f, 1f, 1f);
        if (GetSlot(3).GetSprite(index) == null)
        {
            SetLabel(WireIMG, "");
            DrawWires(pattern.Crossings);
        }

        for (int i = 0; i < ButtonIMGs.Length; i++)
            AddChoice(ButtonIMGs[i], i == answer);
        SetActive(SparkIMG, false);
    }

    static int Cross(int lane, int crossing)
    {
        if (lane == crossing)
            return crossing + 1;
        if (lane == crossing + 1)
            return crossing;
        return lane;
    }

    void DrawWires(int[] crossings)
    {
        float height = WireIMG.rectTransform.rect.height;
        float center = WireIMG.rectTransform.anchoredPosition.x;
        float step = height / crossings.Length;

        for (int wire = 0; wire < ButtonIMGs.Length; wire++)
        {
            int lane = wire;
            Vector2 from = new Vector2(LaneX(lane) - center, -height * 0.5f);
            foreach (int crossing in crossings)
            {
                int nextLane = Cross(lane, crossing);
                Vector2 to = new Vector2(LaneX(nextLane) - center, from.y + step);
                DrawLine(from, to);
                from = to;
                lane = nextLane;
            }
        }
    }

    float LaneX(int lane)
    {
        return ButtonIMGs[lane].rectTransform.anchoredPosition.x;
    }

    void DrawLine(Vector2 from, Vector2 to)
    {
        Vector2 d = to - from;
        Image line = CreateImage(WireIMG.rectTransform, GetSlot(3).Placeholder, WireColor, (from + to) * 0.5f,
            new Vector2(d.magnitude + WireWidth, WireWidth));
        line.rectTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg);
    }

    protected override void OnPicked(Image choice, bool correct)
    {
        Show(choice, 2, 1);
        if (correct)
        {
            Show(MachineIMG, 1, 1);
            return;
        }
        SparkIMG.rectTransform.anchoredPosition = choice.rectTransform.anchoredPosition;
        SetActive(SparkIMG, true);
    }
}
