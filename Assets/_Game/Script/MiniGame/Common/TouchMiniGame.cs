using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 터치로 하는 미니게임의 공통 부모. 터치를 PlayArea 좌표로 바꿔 전달하고 지시문과 에셋 자리를 관리한다.
// PlayArea는 800x500 크기로 배치하고, 화면 비율이 달라도 통째로 축소해 화면 안에 들어오게 한다 (화면 방향 미정).
public abstract class TouchMiniGame : MiniGame
{
    const string LabelName = "PlaceholderLabel";

    [Header("Common")]
    [SerializeField] protected RectTransform PlayArea;
    [SerializeField] protected Text TitleTXT;
    [SerializeField] protected Image GaugeIMG;          // 없으면 비워 둔다
    [SerializeField] protected float TopMargin = 100f;  // 타이머와 지시문 자리
    [SerializeField] protected float HitPadding = 12f;  // 손가락으로 누르기 쉽게 판정을 넓히는 폭

    [Header("Speed Up")]   // 제한시간이 짧아질수록 움직이는 것이 빨라진다 (SpeedScale을 쓰는 미니게임만)
    [SerializeField] float BaseTimeLimit = 5f;         // 이 제한시간일 때 1배
    [SerializeField] float SpeedUpPerSecond = 0.5f;    // 제한시간이 1초 짧아질 때마다 더해지는 배율

    [Header("Sprite")]
    [SerializeField] protected SpriteSlot[] Slots;
    [SerializeField] protected SpritePlacement[] Placements;

    Canvas canvas;
    int dragId = -1;
    Vector2 lastDragPos;

    // PlayArea 단위로 나타낸 화면 전체 너비 ("화면 너비 x 6" 같은 수치의 기준)
    protected float ScreenWidth
    {
        get { return ((RectTransform)transform).rect.width / Mathf.Max(PlayArea.localScale.x, 0.001f); }
    }

    // PlayArea 단위로 나타낸 화면 전체 높이
    protected float ScreenHeight
    {
        get { return ((RectTransform)transform).rect.height / Mathf.Max(PlayArea.localScale.y, 0.001f); }
    }

    // 움직이는 것의 속도에 곱하는 배율. 시작(OnBegin)한 뒤부터 쓸 수 있다
    protected float SpeedScale
    {
        get { return 1f + Mathf.Max(0f, BaseTimeLimit - TimeLimit) * SpeedUpPerSecond; }
    }

    // PlayArea 좌표로 나타낸 화면 전체
    protected Rect FullScreen
    {
        get
        {
            Vector2 size = new Vector2(ScreenWidth, ScreenHeight);
            Vector2 center = -PlayArea.anchoredPosition / Mathf.Max(PlayArea.localScale.x, 0.001f);
            return new Rect(center - size * 0.5f, size);
        }
    }

    protected Vector2 HalfArea
    {
        get { return PlayArea.rect.size * 0.5f; }
    }

    void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        FitPlayArea();
        foreach (SpritePlacement placement in Placements)
            Show(placement.Image, placement.Slot, placement.State);
        SetTitle(Title);
        Setup();
    }

    // 문이 열리기 전에 호출된다. 대상 배치를 여기서 해서 시작하자마자 모두 보이게 한다
    protected abstract void Setup();

    protected override void OnBegin() { }

    protected virtual void Tick(float deltaTime) { }

    // 손가락이 닿을 때마다 (연타할 때 여러 손가락을 써도 모두 인정)
    protected virtual void OnTap(Vector2 pos) { }

    // 드래그는 먼저 닿은 손가락 하나만 따라간다
    protected virtual void OnDragBegin(Vector2 pos) { }
    protected virtual void OnDrag(Vector2 pos, Vector2 delta) { }
    protected virtual void OnDragEnd(Vector2 pos) { }

    void OnRectTransformDimensionsChange()
    {
        if (PlayArea != null)
            FitPlayArea();
    }

    void FitPlayArea()
    {
        Rect rect = ((RectTransform)transform).rect;
        Vector2 size = PlayArea.rect.size;
        if (rect.width <= 0f || rect.height <= TopMargin || size.x <= 0f || size.y <= 0f)
            return;

        float scale = Mathf.Min(rect.width / size.x, (rect.height - TopMargin) / size.y, 1f);
        PlayArea.localScale = new Vector3(scale, scale, 1f);
        PlayArea.anchoredPosition = new Vector2(0f, -TopMargin * 0.5f);
    }

    void Update()
    {
        if (!IsPlaying)
        {
            dragId = -1;
            return;
        }

        ReadTouches();
        if (IsPlaying)
            Tick(Time.deltaTime);
    }

    void ReadTouches()
    {
        IReadOnlyList<TouchPoint> points = TouchInput.Points;
        bool dragFound = false;

        for (int i = 0; i < points.Count; i++)
        {
            TouchPoint point = points[i];
            Vector2 pos = ToArea(point.Position);

            if (point.Began)
            {
                OnTap(pos);
                if (!IsPlaying)
                    return;
            }

            if (dragId < 0 && (point.Began || point.Held))
            {
                dragId = point.Id;
                lastDragPos = pos;
                OnDragBegin(pos);
                if (!IsPlaying)
                    return;
            }

            if (dragId != point.Id)
                continue;

            dragFound = true;
            if (pos != lastDragPos)
            {
                Vector2 delta = pos - lastDragPos;
                lastDragPos = pos;
                OnDrag(pos, delta);
                if (!IsPlaying)
                    return;
            }
            if (!point.Held)
            {
                dragId = -1;
                OnDragEnd(pos);
                if (!IsPlaying)
                    return;
            }
        }

        // 앱 전환 등으로 뗀 프레임을 받지 못한 경우
        if (!dragFound && dragId >= 0)
        {
            dragId = -1;
            OnDragEnd(lastDragPos);
        }
    }

    Vector2 ToArea(Vector2 screenPos)
    {
        Camera cam = canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(PlayArea, screenPos, cam, out local);
        return local;
    }

    protected void SetTitle(string title)
    {
        Title = title;
        if (TitleTXT != null)
            TitleTXT.text = title;
    }

    protected void SetGauge(float value)
    {
        if (GaugeIMG != null)
            GaugeIMG.fillAmount = Mathf.Clamp01(value);
    }

    // ---- 에셋 자리 ----

    protected SpriteSlot GetSlot(int slot)
    {
        return slot >= 1 && slot <= Slots.Length ? Slots[slot - 1] : null;
    }

    // 에셋 자리의 그림을 보여준다. 그림이 없으면 임시 이미지와 이름표
    protected void Show(Image image, int slot, int state)
    {
        SpriteSlot data = GetSlot(slot);
        if (image == null || data == null)
            return;

        Sprite sprite = data.GetSprite(state);
        image.sprite = sprite != null ? sprite : data.Placeholder;
        image.color = sprite != null ? Color.white : data.GetColor(state);
        image.preserveAspect = sprite != null;
        SetLabel(image, sprite != null ? "" : data.GetLabel(state));
    }

    protected Text SetLabel(Image image, string label)
    {
        Transform found = image.transform.Find(LabelName);
        Text text = found != null ? found.GetComponent<Text>() : CreateText(image.rectTransform, LabelName, 20);
        text.text = label;
        return text;
    }

    // 한글 TMP 폰트가 프로젝트에 없어서, OS의 한글 글꼴로 대체되는 기본 Text를 쓴다
    static Text CreateText(RectTransform parent, string name, int maxSize)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        RectTransform rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Text text = go.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.black;
        text.raycastTarget = false;
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = 8;
        text.resizeTextMaxSize = maxSize;
        return text;
    }

    // 이름표까지 함께 흐려지도록 CanvasGroup으로 투명도를 바꾼다
    protected static void SetAlpha(Graphic graphic, float alpha)
    {
        if (graphic == null)
            return;
        CanvasGroup group = graphic.GetComponent<CanvasGroup>();
        if (group == null)
            group = graphic.gameObject.AddComponent<CanvasGroup>();
        group.alpha = alpha;
    }

    protected static void SetActive(Component target, bool active)
    {
        if (target != null)
            target.gameObject.SetActive(active);
    }

    // template을 복제해서 배치한다. template 자신은 숨긴다
    protected static Image Spawn(Image template, Vector2 pos)
    {
        Image image = Instantiate(template, template.transform.parent);
        image.transform.SetSiblingIndex(template.transform.GetSiblingIndex() + 1);
        image.gameObject.SetActive(true);
        image.rectTransform.anchoredPosition = pos;
        template.gameObject.SetActive(false);
        return image;
    }

    protected static Image CreateImage(RectTransform parent, Sprite sprite, Color color, Vector2 pos, Vector2 size)
    {
        GameObject go = new GameObject("IMG", typeof(RectTransform));
        RectTransform rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        Image image = go.AddComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    // ---- 판정 (PlayArea 바로 아래, 앵커와 피벗이 가운데인 이미지 기준) ----

    protected bool Hit(Image target, Vector2 pos)
    {
        return target.gameObject.activeInHierarchy && Hit(target.rectTransform, pos, HitPadding);
    }

    protected static bool Hit(RectTransform rt, Vector2 pos, float padding)
    {
        Vector2 local = pos - rt.anchoredPosition;
        Vector2 half = rt.rect.size * 0.5f + Vector2.one * padding;
        return Mathf.Abs(local.x) <= half.x && Mathf.Abs(local.y) <= half.y;
    }

    // shrink: 판정 크기 배율 (1보다 작으면 그림이 살짝 겹쳐도 닿지 않은 것으로 본다)
    protected static bool Overlap(Image a, Image b, float shrink)
    {
        Vector2 d = a.rectTransform.anchoredPosition - b.rectTransform.anchoredPosition;
        Vector2 half = (a.rectTransform.rect.size + b.rectTransform.rect.size) * 0.5f * shrink;
        return Mathf.Abs(d.x) < half.x && Mathf.Abs(d.y) < half.y;
    }

    // ---- 배치와 이동 ----

    // rt 크기만큼 안쪽으로 줄인 PlayArea 영역. bottomCut은 아래에 비워 둘 높이
    protected Rect InnerArea(RectTransform rt, float bottomCut = 0f)
    {
        Vector2 half = HalfArea - rt.rect.size * 0.5f;
        return Rect.MinMaxRect(-half.x, -half.y + bottomCut, half.x, half.y);
    }

    // rt 크기만큼 안쪽으로 줄인 화면 전체 영역 (위쪽 지시문 자리는 뺀다)
    protected Rect ScreenArea(RectTransform rt)
    {
        Rect screen = FullScreen;
        screen.yMax -= TopMargin / Mathf.Max(PlayArea.localScale.y, 0.001f);
        Vector2 half = rt.rect.size * 0.5f;
        return Rect.MinMaxRect(screen.xMin + half.x, screen.yMin + half.y, screen.xMax - half.x, screen.yMax - half.y);
    }

    // area 안에서 되도록 spacing 이상 떨어진 랜덤 위치들. avoid 안에는 놓지 않는다
    protected static List<Vector2> RandomPositions(int count, Rect area, float spacing, Rect avoid = default(Rect))
    {
        // 자리가 막히면 처음부터 다시 뽑고, 그래도 안 되면 가장 덜 겹친 배치를 쓴다
        List<Vector2> best = null;
        float bestNearest = -1f;
        for (int attempt = 0; attempt < 20 && bestNearest < spacing; attempt++)
        {
            List<Vector2> positions = new List<Vector2>();
            float worst = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                Vector2 pick = area.center;
                float pickNearest = -1f;
                for (int tries = 0; tries < 40 && pickNearest < spacing; tries++)
                {
                    Vector2 pos = new Vector2(Random.Range(area.xMin, area.xMax), Random.Range(area.yMin, area.yMax));
                    if (avoid.Contains(pos))
                        continue;
                    float nearest = float.MaxValue;
                    foreach (Vector2 other in positions)
                        nearest = Mathf.Min(nearest, Vector2.Distance(pos, other));
                    if (nearest > pickNearest)
                    {
                        pickNearest = nearest;
                        pick = pos;
                    }
                }
                positions.Add(pick);
                worst = Mathf.Min(worst, pickNearest);
            }
            if (worst > bestNearest)
            {
                bestNearest = worst;
                best = positions;
            }
        }
        return best;
    }

    // 벽에 튕기며 움직이는 대상
    protected static void Bounce(RectTransform rt, ref Vector2 velocity, Rect area, float deltaTime)
    {
        Vector2 pos = rt.anchoredPosition + velocity * deltaTime;
        if (pos.x < area.xMin) velocity.x = Mathf.Abs(velocity.x);
        if (pos.x > area.xMax) velocity.x = -Mathf.Abs(velocity.x);
        if (pos.y < area.yMin) velocity.y = Mathf.Abs(velocity.y);
        if (pos.y > area.yMax) velocity.y = -Mathf.Abs(velocity.y);
        pos.x = Mathf.Clamp(pos.x, area.xMin, area.xMax);
        pos.y = Mathf.Clamp(pos.y, area.yMin, area.yMax);
        rt.anchoredPosition = pos;
    }

    protected static Vector2 RandomDirection()
    {
        float angle = Random.Range(0f, Mathf.PI * 2f);
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }

    protected static void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
}
