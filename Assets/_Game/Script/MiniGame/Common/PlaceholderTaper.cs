using UnityEngine;
using UnityEngine.UI;

// 임시 이미지에 원근감을 주려고 한쪽 끝을 좁힌다. 실제 그림이 들어오면(preserveAspect) 아무것도 하지 않는다.
public class PlaceholderTaper : BaseMeshEffect
{
    [SerializeField] bool Vertical;           // false: 왼쪽->오른쪽, true: 아래->위
    [SerializeField] float StartScale = 0.3f; // 왼쪽/아래 끝의 두께 배율
    [SerializeField] float EndScale = 1f;     // 오른쪽/위 끝의 두께 배율

    public override void ModifyMesh(VertexHelper vh)
    {
        Image image = graphic as Image;
        if (!IsActive() || (image != null && image.preserveAspect))
            return;

        Rect rect = graphic.rectTransform.rect;
        UIVertex vertex = default(UIVertex);
        for (int i = 0; i < vh.currentVertCount; i++)
        {
            vh.PopulateUIVertex(ref vertex, i);
            Vector3 p = vertex.position;
            if (Vertical)
                p.x = rect.center.x + (p.x - rect.center.x) * Mathf.Lerp(StartScale, EndScale, Mathf.InverseLerp(rect.yMin, rect.yMax, p.y));
            else
                p.y = rect.center.y + (p.y - rect.center.y) * Mathf.Lerp(StartScale, EndScale, Mathf.InverseLerp(rect.xMin, rect.xMax, p.x));
            vertex.position = p;
            vh.SetUIVertex(vertex, i);
        }
    }
}
