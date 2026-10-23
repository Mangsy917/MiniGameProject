using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingSceneController : MonoBehaviour
{
    public Slider ProgressSLD;
    public TMP_Text ProgressTXT;

    // 로딩이 너무 빨라도 로딩 화면을 최소 이 시간만큼 보여준다
    public float MinLoadingTime = 1f;

    // 로딩 완료 후 99%에서 멈춰 있는 시간
    public float HoldTimeAt99 = 1.5f;

    const float HoldProgress = 0.99f;

    void Start()
    {
        if (string.IsNullOrEmpty(SceneLoader.NextSceneName))
        {
            Debug.LogWarning("다음 씬이 지정되지 않았습니다. SceneLoader.LoadScene()으로 진입하세요.");
            return;
        }

        StartCoroutine(LoadNextScene());
    }

    IEnumerator LoadNextScene()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(SceneLoader.NextSceneName);
        if (op == null)
        {
            // 씬 이름이 틀렸거나 Build Profiles의 씬 목록에 없는 경우
            Debug.LogError("씬을 로드할 수 없습니다: " + SceneLoader.NextSceneName);
            yield break;
        }
        op.allowSceneActivation = false;

        // 첫 프레임의 deltaTime은 씬 로드 지연까지 포함해 커질 수 있어 시작 시각 기준으로 잰다
        float startTime = Time.realtimeSinceStartup;
        float progress = 0f;

        // 실제 로딩이 끝나도 99%까지만 표시
        while (progress < HoldProgress)
        {
            float timer = Time.realtimeSinceStartup - startTime;

            // allowSceneActivation이 false면 progress는 0.9에서 멈추므로 0~1로 환산
            float loadProgress = Mathf.Clamp01(op.progress / 0.9f);
            float timeProgress = MinLoadingTime > 0f ? Mathf.Clamp01(timer / MinLoadingTime) : 1f;
            progress = Mathf.Min(loadProgress, timeProgress) * HoldProgress;

            SetProgress(progress);
            yield return null;
        }

        // 99%에서 잠시 멈춘 뒤 100%
        yield return new WaitForSecondsRealtime(HoldTimeAt99);
        SetProgress(1f);

        // 100%가 화면에 그려진 다음 프레임에 씬 전환
        yield return null;
        op.allowSceneActivation = true;
    }

    void SetProgress(float value)
    {
        ProgressSLD.value = value;
        // 반올림하면 99.5%가 100%로 보이므로 내림 처리
        ProgressTXT.text = Mathf.FloorToInt(value * 100f + 0.001f) + "%";
    }
}
