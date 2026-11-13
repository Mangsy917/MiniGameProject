using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 사슬 잠금 표시. 잠금 해제 시 전체가 잠깐 흔들린 뒤, 사슬이 한 줄씩 팽팽해졌다가 끊어져 떨어지며 사라진다.
// 월드(SpriteRenderer)와 UI(Image) 모두 사용할 수 있다.
// Chains의 각 항목이 사슬 한 줄이며, 자식으로 사슬 조각 여러 개를 이어 붙여도 한 줄로 함께 움직인다.
// 사슬이 아닌 나머지 표시(덮개, 힌트 글자 등)는 마지막 사슬과 함께 사라진다.
public class ChainLock : MonoBehaviour
{
    public Transform[] Chains;            // 끊어지는 순서대로

    public float ShakeTime = 0.4f;        // 전체가 흔들리는 시간
    public float ShakeAngle = 6f;
    public int ShakeCount = 4;
    public float TensionTime = 0.12f;     // 끊어지기 직전 사슬이 팽팽해지는(살짝 커지는) 시간
    public float TensionScale = 1.08f;
    public float BreakTime = 0.45f;       // 끊어진 사슬이 떨어지며 사라지는 시간
    public float FallDistance = 1.5f;     // 떨어지는 거리 (부모 기준 로컬 단위. 월드면 유닛, UI면 캔버스 픽셀)
    public float BreakSpin = 30f;         // 떨어지면서 기울어진 방향으로 더 도는 각도
    public float ChainInterval = 0.15f;   // 한 줄이 끊어지고 다음 줄이 끊어지기까지

    // 색(알파)을 바꿀 대상 묶음. 끝나면 원래 값으로 되돌린다
    class FadeGroup
    {
        readonly List<SpriteRenderer> sprites = new List<SpriteRenderer>();
        readonly List<Graphic> graphics = new List<Graphic>();
        readonly List<TMP_Text> texts = new List<TMP_Text>();
        readonly List<float> spriteAlphas = new List<float>();
        readonly List<float> graphicAlphas = new List<float>();
        readonly List<float> textAlphas = new List<float>();

        public void Add(Component c)
        {
            TMP_Text text = c as TMP_Text;
            if (text != null)
            {
                texts.Add(text);
                textAlphas.Add(text.alpha);
                return;
            }
            SpriteRenderer sprite = c as SpriteRenderer;
            if (sprite != null)
            {
                sprites.Add(sprite);
                spriteAlphas.Add(sprite.color.a);
                return;
            }
            Graphic graphic = c as Graphic;
            if (graphic != null)
            {
                graphics.Add(graphic);
                graphicAlphas.Add(graphic.color.a);
            }
        }

        // k: 원래 알파에 곱할 값 (1 = 원래대로, 0 = 투명)
        public void SetAlpha(float k)
        {
            for (int i = 0; i < sprites.Count; i++)
            {
                Color color = sprites[i].color;
                color.a = spriteAlphas[i] * k;
                sprites[i].color = color;
            }
            for (int i = 0; i < graphics.Count; i++)
            {
                Color color = graphics[i].color;
                color.a = graphicAlphas[i] * k;
                graphics[i].color = color;
            }
            for (int i = 0; i < texts.Count; i++)
                texts[i].alpha = textAlphas[i] * k;
        }
    }

    static void CollectVisuals(Transform root, FadeGroup group, System.Predicate<Transform> include)
    {
        foreach (Component c in root.GetComponentsInChildren<Component>(false))
        {
            if (!(c is SpriteRenderer || c is Graphic))
                continue;
            if (include != null && !include(c.transform))
                continue;
            group.Add(c);
        }
    }

    bool IsUnderChain(Transform t)
    {
        foreach (Transform chain in Chains)
        {
            if (chain != null && (t == chain || t.IsChildOf(chain)))
                return true;
        }
        return false;
    }

    // 잠금 해제 연출. 끝나면 이 오브젝트를 끄고 모든 값을 원래대로 되돌려 둔다
    public IEnumerator PlayUnlock()
    {
        gameObject.SetActive(true);

        Transform root = transform;
        Quaternion rootRotation = root.localRotation;

        int count = Chains.Length;
        Vector3[] positions = new Vector3[count];
        Quaternion[] rotations = new Quaternion[count];
        Vector3[] scales = new Vector3[count];
        FadeGroup[] chainFades = new FadeGroup[count];
        for (int i = 0; i < count; i++)
        {
            if (Chains[i] == null)
                continue;
            positions[i] = Chains[i].localPosition;
            rotations[i] = Chains[i].localRotation;
            scales[i] = Chains[i].localScale;
            chainFades[i] = new FadeGroup();
            CollectVisuals(Chains[i], chainFades[i], null);
        }
        FadeGroup others = new FadeGroup();
        CollectVisuals(root, others, t => !IsUnderChain(t));

        // 전체가 흔들린다 (점점 약해짐)
        float startTime = Time.realtimeSinceStartup;
        float t = 0f;
        while (t < 1f)
        {
            t = ShakeTime > 0f ? Mathf.Clamp01((Time.realtimeSinceStartup - startTime) / ShakeTime) : 1f;
            float angle = Mathf.Sin(t * Mathf.PI * 2f * ShakeCount) * ShakeAngle * (1f - t);
            root.localRotation = rootRotation * Quaternion.Euler(0f, 0f, angle);
            yield return null;
        }
        root.localRotation = rootRotation;

        // 사슬이 한 줄씩 끊어진다
        for (int i = 0; i < count; i++)
        {
            Transform chain = Chains[i];
            if (chain == null)
                continue;
            bool last = i == count - 1;

            // 팽팽해짐
            startTime = Time.realtimeSinceStartup;
            t = 0f;
            while (t < 1f)
            {
                t = TensionTime > 0f ? Mathf.Clamp01((Time.realtimeSinceStartup - startTime) / TensionTime) : 1f;
                chain.localScale = scales[i] * Mathf.Lerp(1f, TensionScale, t);
                yield return null;
            }

            // 끊어져서 기울어진 쪽으로 돌며 떨어지고 사라진다 (중력처럼 점점 빨라짐)
            float tilt = Mathf.DeltaAngle(0f, rotations[i].eulerAngles.z);
            float spin = (tilt >= 0f ? 1f : -1f) * BreakSpin;
            startTime = Time.realtimeSinceStartup;
            t = 0f;
            while (t < 1f)
            {
                t = BreakTime > 0f ? Mathf.Clamp01((Time.realtimeSinceStartup - startTime) / BreakTime) : 1f;
                chain.localScale = scales[i] * Mathf.Lerp(TensionScale, 1f, t);
                chain.localPosition = positions[i] + Vector3.down * FallDistance * t * t;
                chain.localRotation = rotations[i] * Quaternion.Euler(0f, 0f, spin * t);
                chainFades[i].SetAlpha(1f - t);
                if (last)
                    others.SetAlpha(1f - t);
                yield return null;
            }

            if (!last)
                yield return new WaitForSecondsRealtime(ChainInterval);
        }

        // 숨긴 뒤 원래 상태로 되돌려 둔다
        gameObject.SetActive(false);
        for (int i = 0; i < count; i++)
        {
            if (Chains[i] == null)
                continue;
            Chains[i].localPosition = positions[i];
            Chains[i].localRotation = rotations[i];
            Chains[i].localScale = scales[i];
            chainFades[i].SetAlpha(1f);
        }
        others.SetAlpha(1f);
    }
}
