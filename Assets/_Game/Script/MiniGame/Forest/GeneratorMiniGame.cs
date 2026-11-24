using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// FOR_19 발전기를 돌려라
// 가운데 발전기의 크랭크 손잡이를 돌려 전력 게이지를 채우면, 왼쪽 위에 전구가 반짝 켜진다.
public class GeneratorMiniGame : RotateGaugeMiniGame
{
    [Header("FOR_19")]
    [SerializeField] Image BulbIMG;
    [SerializeField] Image[] RayIMGs;             // 전구가 켜질 때 퍼지는 빛줄기
    [SerializeField] float PopTime = 0.25f;
    [SerializeField] float PopScale = 1.5f;

    protected override void Setup()
    {
        base.Setup();
        SetLit(false);
    }

    protected override void OnProgress(float value) { }

    protected override void OnEnd(bool success)
    {
        if (success)
            StartCoroutine(LightUp());
    }

    void SetLit(bool lit)
    {
        Show(BulbIMG, 2, lit ? 1 : 0);
        SetActive(BulbIMG, lit);
        foreach (Image ray in RayIMGs)
            SetActive(ray, lit);
    }

    // 전구가 커졌다가 제 크기로 돌아오고 빛줄기가 퍼진다
    IEnumerator LightUp()
    {
        SetLit(true);
        float t = 0f;
        while (t < 1f)
        {
            t = Mathf.Clamp01(t + Time.unscaledDeltaTime / Mathf.Max(0.01f, PopTime));
            BulbIMG.rectTransform.localScale = Vector3.one * Mathf.Lerp(PopScale, 1f, t);
            foreach (Image ray in RayIMGs)
                ray.rectTransform.localScale = new Vector3(t, 1f, 1f);
            yield return null;
        }
    }
}
