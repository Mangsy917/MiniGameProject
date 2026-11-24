using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 점들을 잇는 선을 UI에 그린다. 좌표는 자기 RectTransform 기준
[RequireComponent(typeof(CanvasRenderer))]
public class UILine : MaskableGraphic
{
    [SerializeField] float Width = 12f;

    readonly List<Vector2> points = new List<Vector2>();

    public void SetPoints(List<Vector2> newPoints)
    {
        points.Clear();
        points.AddRange(newPoints);
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        for (int i = 1; i < points.Count; i++)
        {
            Vector2 a = points[i - 1];
            Vector2 b = points[i];
            Vector2 dir = (b - a).normalized * (Width * 0.5f);
            Vector2 side = new Vector2(-dir.y, dir.x);
            a -= dir * 0.5f;   // 꺾이는 곳이 끊겨 보이지 않게 조금 겹친다
            b += dir * 0.5f;

            int start = vh.currentVertCount;
            vh.AddVert(a - side, color, Vector2.zero);
            vh.AddVert(a + side, color, Vector2.zero);
            vh.AddVert(b + side, color, Vector2.zero);
            vh.AddVert(b - side, color, Vector2.zero);
            vh.AddTriangle(start, start + 1, start + 2);
            vh.AddTriangle(start, start + 2, start + 3);
        }
    }
}
