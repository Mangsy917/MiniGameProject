using UnityEngine;
using UnityEngine.UI;

// CAV_03 동굴의 출구를 찾아라 (공통 템플릿: 손전등 비추기)
// 횃불을 끌어 어둠을 비추고 출구를 찾아 탭한다. 빛 밖에 있는 출구는 탭되지 않는다.
public class TorchExitMiniGame : TouchMiniGame
{
    const float HoleRatio = 0.25f;   // 어둠 마스크 그림에서 구멍 반지름 / 그림 너비
    const float CoverSize = 2500f;
    const int TextureSize = 256;

    [Header("CAV_03")]
    [SerializeField, Range(0.03f, 0.5f)] float LightRadiusRatio = 0.1f;   // 빛 반경 (화면 너비 대비)
    [SerializeField] Image TorchIMG;
    [SerializeField] Image ExitIMG;
    [SerializeField] Image DarkIMG;     // 그림을 넣을 때는 가운데에 반지름 25%(너비 대비) 구멍이 뚫린 정사각형 그림

    Texture2D holeTexture;
    Sprite holeSprite;
    float radius;
    bool grabbed;

    protected override void Setup()
    {
        radius = ScreenWidth * LightRadiusRatio;
        DarkIMG.rectTransform.sizeDelta = Vector2.one * (radius / HoleRatio);
        if (GetSlot(2).GetSprite(0) == null)
        {
            MakeHoleSprite();
            DarkIMG.sprite = holeSprite;
            SetLabel(DarkIMG, "");
        }

        // 어둠 그림 바깥쪽은 검은 판 4장으로 덮는다
        float size = DarkIMG.rectTransform.sizeDelta.x;
        float far = size * 0.5f + CoverSize * 0.5f;
        foreach (Vector2 side in new[] { Vector2.up, Vector2.down, Vector2.left, Vector2.right })
            CreateImage(DarkIMG.rectTransform, GetSlot(2).Placeholder, GetSlot(2).GetColor(0), side * far,
                side.x == 0f ? new Vector2(CoverSize * 2f, CoverSize) : new Vector2(CoverSize, size));

        RectTransform torch = TorchIMG.rectTransform;
        if (Random.value < 0.5f)
            torch.anchoredPosition = new Vector2(-torch.anchoredPosition.x, torch.anchoredPosition.y);

        // 출구는 처음 빛이 닿지 않는 곳에. 자리가 없으면 가장 먼 곳
        Rect area = InnerArea(ExitIMG.rectTransform);
        float farthest = -1f;
        for (int i = 0; i < 60 && farthest < radius + 60f; i++)
        {
            Vector2 pos = new Vector2(Random.Range(area.xMin, area.xMax), Random.Range(area.yMin, area.yMax));
            float distance = Vector2.Distance(pos, torch.anchoredPosition);
            if (distance > farthest)
            {
                farthest = distance;
                ExitIMG.rectTransform.anchoredPosition = pos;
            }
        }
        MoveLight();
    }

    void MakeHoleSprite()
    {
        holeTexture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false);
        holeTexture.wrapMode = TextureWrapMode.Clamp;
        Color32[] pixels = new Color32[TextureSize * TextureSize];
        float half = TextureSize * 0.5f;
        float hole = TextureSize * HoleRatio;
        for (int y = 0; y < TextureSize; y++)
        {
            for (int x = 0; x < TextureSize; x++)
            {
                float d = Mathf.Sqrt((x + 0.5f - half) * (x + 0.5f - half) + (y + 0.5f - half) * (y + 0.5f - half));
                float alpha = Mathf.Clamp01((d - hole) / 3f + 1f);
                pixels[y * TextureSize + x] = new Color(1f, 1f, 1f, alpha);
            }
        }
        holeTexture.SetPixels32(pixels);
        holeTexture.Apply();
        holeSprite = Sprite.Create(holeTexture, new Rect(0f, 0f, TextureSize, TextureSize), Vector2.one * 0.5f);
    }

    void OnDestroy()
    {
        if (holeSprite != null)
            Destroy(holeSprite);
        if (holeTexture != null)
            Destroy(holeTexture);
    }

    protected override void OnTap(Vector2 pos)
    {
        bool lit = Vector2.Distance(ExitIMG.rectTransform.anchoredPosition, TorchIMG.rectTransform.anchoredPosition) < radius;
        if (lit && Hit(ExitIMG, pos))
            Finish(true);
    }

    protected override void OnDragBegin(Vector2 pos)
    {
        grabbed = Hit(TorchIMG.rectTransform, pos, 40f);
    }

    protected override void OnDrag(Vector2 pos, Vector2 delta)
    {
        if (!grabbed)
            return;
        Vector2 next = TorchIMG.rectTransform.anchoredPosition + delta;
        Vector2 half = HalfArea;
        TorchIMG.rectTransform.anchoredPosition = new Vector2(Mathf.Clamp(next.x, -half.x, half.x), Mathf.Clamp(next.y, -half.y, half.y));
        MoveLight();
    }

    void MoveLight()
    {
        DarkIMG.rectTransform.anchoredPosition = TorchIMG.rectTransform.anchoredPosition;
    }
}
