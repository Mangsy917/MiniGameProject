using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// 미니게임 테스트 씬(Assets/Scenes/Test/MiniGameTest)용. 목록에서 하나를 골라 바로 해 본다.
// 실제 게임 진행(StageFlowController)과는 관계없다. UI는 실행할 때 코드로 만든다.
public class MiniGameTester : MonoBehaviour
{
    [Serializable]
    public class Entry
    {
        public string Stage;
        public string Id;
        public string Name;
        public MiniGame Prefab;
    }

    public Entry[] Games;
    public float[] TimeLimits = { 5f, 4.5f, 4f };
    public float StartDelay = 0.5f;          // 고른 뒤 시작까지 (문이 열리는 시간 대신)
    public RectTransform CanvasRoot;

    RectTransform gameRoot, menu, playUI, resultUI;
    Image timerFill;
    Text infoTXT, resultTXT;
    Button timeBT;
    int timeIndex;                           // TimeLimits 길이와 같으면 무제한
    int current;
    MiniGame game;
    Coroutine routine;

    bool Unlimited
    {
        get { return timeIndex >= TimeLimits.Length; }
    }

    void Start()
    {
        gameRoot = Panel("MiniGameRoot", CanvasRoot, Color.clear);
        BuildPlayUI();
        BuildMenu();
        ShowMenu();
    }

    // ---- 화면 만들기 ----

    void BuildMenu()
    {
        menu = Panel("Menu", CanvasRoot, new Color(0.15f, 0.16f, 0.2f));
        VerticalLayoutGroup rows = menu.gameObject.AddComponent<VerticalLayoutGroup>();
        rows.padding = new RectOffset(12, 12, 12, 12);
        rows.spacing = 8f;

        RectTransform header = Row(menu);
        Label(header, "미니게임 테스트", Color.white, 30);
        timeBT = MakeButton(header, "", new Color(1f, 0.85f, 0.4f), NextTimeLimit);
        RefreshTimeButton();

        RectTransform row = null;
        string stage = null;
        for (int i = 0; i < Games.Length; i++)
        {
            Entry entry = Games[i];
            if (entry.Stage != stage)
            {
                stage = entry.Stage;
                row = Row(menu);
                Label(row, stage, Color.white, 24);
            }
            int index = i;
            MakeButton(row, entry.Id + "\n" + entry.Name, Color.white, () => Play(index));
        }
    }

    void BuildPlayUI()
    {
        playUI = Panel("PlayUI", CanvasRoot, Color.clear);
        RectTransform back = Panel("TimerBack", playUI, new Color(0f, 0f, 0f, 0.5f));
        Anchor(back, new Vector2(0.25f, 1f), new Vector2(0.75f, 1f), new Vector2(0f, -40f), new Vector2(0f, -20f));
        timerFill = Panel("TimerFill", back, new Color(1f, 0.4f, 0.3f)).GetComponent<Image>();

        infoTXT = Label(playUI, "", Color.white, 18);
        Anchor(infoTXT.rectTransform, new Vector2(0.76f, 1f), Vector2.one, new Vector2(0f, -50f), new Vector2(-8f, -10f));
        Button listBT = MakeButton(playUI, "목록", Color.white, ShowMenu);
        Anchor((RectTransform)listBT.transform, new Vector2(0f, 1f), new Vector2(0.15f, 1f), new Vector2(8f, -55f), new Vector2(0f, -8f));

        resultUI = Panel("ResultUI", playUI, new Color(0f, 0f, 0f, 0.45f));
        resultTXT = Label(resultUI, "", Color.white, 64);
        Anchor(resultTXT.rectTransform, new Vector2(0f, 0.5f), new Vector2(1f, 0.8f), Vector2.zero, Vector2.zero);
        RectTransform buttons = Row(resultUI);
        Anchor(buttons, new Vector2(0.1f, 0.3f), new Vector2(0.9f, 0.45f), Vector2.zero, Vector2.zero);
        MakeButton(buttons, "이전", Color.white, () => Play((current + Games.Length - 1) % Games.Length));
        MakeButton(buttons, "다시 하기", new Color(1f, 0.85f, 0.4f), () => Play(current));
        MakeButton(buttons, "다음", Color.white, () => Play((current + 1) % Games.Length));
        MakeButton(buttons, "목록", Color.white, ShowMenu);
    }

    static RectTransform Panel(string name, RectTransform parent, Color color)
    {
        RectTransform rt = (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
        rt.SetParent(parent, false);
        Anchor(rt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        Image image = rt.gameObject.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return rt;
    }

    static void Anchor(RectTransform rt, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax)
    {
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = offsetMin;
        rt.offsetMax = offsetMax;
    }

    static RectTransform Row(RectTransform parent)
    {
        RectTransform rt = (RectTransform)new GameObject("Row", typeof(RectTransform)).transform;
        rt.SetParent(parent, false);
        HorizontalLayoutGroup group = rt.gameObject.AddComponent<HorizontalLayoutGroup>();
        group.spacing = 8f;
        Fill(rt);
        return rt;
    }

    // 글자 길이와 관계없이 칸을 똑같이 나눈다
    static void Fill(RectTransform rt)
    {
        LayoutElement element = rt.gameObject.AddComponent<LayoutElement>();
        element.minWidth = element.minHeight = 0f;
        element.preferredWidth = element.preferredHeight = 0f;
        element.flexibleWidth = element.flexibleHeight = 1f;
        element.layoutPriority = 10;
    }

    static Text Label(RectTransform parent, string text, Color color, int size)
    {
        RectTransform rt = (RectTransform)new GameObject("TXT", typeof(RectTransform)).transform;
        rt.SetParent(parent, false);
        Fill(rt);
        Text label = rt.gameObject.AddComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.alignment = TextAnchor.MiddleCenter;
        label.color = color;
        label.raycastTarget = false;
        label.resizeTextForBestFit = true;
        label.resizeTextMinSize = 6;
        label.resizeTextMaxSize = size;
        label.text = text;
        return label;
    }

    static Button MakeButton(RectTransform parent, string text, Color color, Action onClick)
    {
        RectTransform rt = Panel("BT", parent, color);
        Fill(rt);
        rt.GetComponent<Image>().raycastTarget = true;
        Button button = rt.gameObject.AddComponent<Button>();
        button.onClick.AddListener(() => onClick());
        Text label = Label(rt, text, Color.black, 18);
        Anchor(label.rectTransform, Vector2.zero, Vector2.one, Vector2.one * 4f, Vector2.one * -4f);
        return button;
    }

    // ---- 진행 ----

    void NextTimeLimit()
    {
        timeIndex = (timeIndex + 1) % (TimeLimits.Length + 1);
        RefreshTimeButton();
    }

    void RefreshTimeButton()
    {
        string time = Unlimited ? "무제한" : TimeLimits[timeIndex].ToString("F1") + "초";
        timeBT.GetComponentInChildren<Text>().text = "제한시간: " + time;
    }

    void ShowMenu()
    {
        Clear();
        menu.gameObject.SetActive(true);
        playUI.gameObject.SetActive(false);
    }

    void Clear()
    {
        if (routine != null)
            StopCoroutine(routine);
        routine = null;
        if (game != null)
            Destroy(game.gameObject);
        game = null;
    }

    void Play(int index)
    {
        Clear();
        current = index;
        menu.gameObject.SetActive(false);
        playUI.gameObject.SetActive(true);
        resultUI.gameObject.SetActive(false);
        infoTXT.text = Games[index].Id + " " + Games[index].Name;
        routine = StartCoroutine(Run(Games[index].Prefab));
    }

    IEnumerator Run(MiniGame prefab)
    {
        SetTimer(1f);
        game = Instantiate(prefab, gameRoot);
        yield return new WaitForSecondsRealtime(StartDelay);

        // 무제한이어도 낙하 간격 등의 계산에는 기본 제한시간을 쓴다
        float limit = Unlimited ? TimeLimits[0] : TimeLimits[timeIndex];
        game.Begin(limit);

        float startTime = Time.realtimeSinceStartup;
        while (!game.IsFinished)
        {
            float elapsed = Time.realtimeSinceStartup - startTime;
            if (!Unlimited)
            {
                SetTimer(1f - elapsed / limit);
                if (elapsed >= limit)
                    game.TimeUp();
            }
            yield return null;
        }

        // 끝난 화면(성공/실패 그림)을 남겨둔 채 결과를 보여준다
        float time = Time.realtimeSinceStartup - startTime;
        resultTXT.text = (game.IsSuccess ? "성공!" : "실패...") + "\n" + time.ToString("F2") + "초";
        resultTXT.color = game.IsSuccess ? new Color(0.5f, 1f, 0.5f) : new Color(1f, 0.5f, 0.5f);

        // 연타하던 손이 결과 버튼을 바로 누르지 않도록 잠깐 기다린다
        yield return new WaitForSecondsRealtime(0.4f);
        resultUI.gameObject.SetActive(true);
    }

    void SetTimer(float value)
    {
        timerFill.rectTransform.anchorMax = new Vector2(Mathf.Clamp01(value), 1f);
    }
}
