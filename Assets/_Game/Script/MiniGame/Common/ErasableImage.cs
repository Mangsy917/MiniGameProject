using UnityEngine;
using UnityEngine.UI;

// 문지른 곳만 지워지는 UI 그림. 격자로 나눠 칸마다 투명도를 바꾼다
[RequireComponent(typeof(CanvasRenderer))]
public class ErasableImage : MaskableGraphic
{
    [SerializeField] Sprite Picture;
    [SerializeField] int Columns = 48;
    [SerializeField] int Rows = 30;

    float[] alphas;   // 격자점마다의 투명도
    int erased;

    public override Texture mainTexture
    {
        get { return Picture != null ? Picture.texture : s_WhiteTexture; }
    }

    // 지운 비율 (0~1)
    public float ErasedRatio
    {
        get { return alphas == null ? 0f : (float)erased / alphas.Length; }
    }

    public void SetPicture(Sprite sprite, Color tint)
    {
        Picture = sprite;
        color = tint;
        SetMaterialDirty();
        SetVerticesDirty();
    }

    public void ResetErase()
    {
        alphas = new float[(Columns + 1) * (Rows + 1)];
        for (int i = 0; i < alphas.Length; i++)
            alphas[i] = 1f;
        erased = 0;
        SetVerticesDirty();
    }

    public void EraseAll()
    {
        for (int i = 0; i < alphas.Length; i++)
            alphas[i] = 0f;
        erased = alphas.Length;
        SetVerticesDirty();
    }

    // from에서 to까지 두께 radius로 지운다. 좌표는 자기 RectTransform 기준
    public void Erase(Vector2 from, Vector2 to, float radius)
    {
        Rect rect = rectTransform.rect;
        for (int i = 0; i < alphas.Length; i++)
        {
            if (alphas[i] <= 0f)
                continue;
            if (DistanceToSegment(GridPoint(rect, i), from, to) > radius)
                continue;
            alphas[i] = 0f;
            erased++;
        }
        SetVerticesDirty();
    }

    public static float DistanceToSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        float t = ab.sqrMagnitude > 0f ? Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude) : 0f;
        return Vector2.Distance(p, a + ab * t);
    }

    Vector2 GridPoint(Rect rect, int index)
    {
        float x = (float)(index % (Columns + 1)) / Columns;
        float y = (float)(index / (Columns + 1)) / Rows;
        return new Vector2(rect.xMin + rect.width * x, rect.yMin + rect.height * y);
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        Rect rect = rectTransform.rect;
        Vector4 uv = Picture != null ? UnityEngine.Sprites.DataUtility.GetOuterUV(Picture) : new Vector4(0f, 0f, 1f, 1f);
        int count = (Columns + 1) * (Rows + 1);
        for (int i = 0; i < count; i++)
        {
            Vector2 pos = GridPoint(rect, i);
            float u = (pos.x - rect.xMin) / rect.width;
            float v = (pos.y - rect.yMin) / rect.height;
            Color c = color;
            c.a *= alphas == null ? 1f : alphas[i];
            vh.AddVert(pos, c, new Vector2(Mathf.Lerp(uv.x, uv.z, u), Mathf.Lerp(uv.y, uv.w, v)));
        }
        for (int y = 0; y < Rows; y++)
        {
            for (int x = 0; x < Columns; x++)
            {
                int i = y * (Columns + 1) + x;
                vh.AddTriangle(i, i + Columns + 1, i + Columns + 2);
                vh.AddTriangle(i, i + Columns + 2, i + 1);
            }
        }
    }
}
