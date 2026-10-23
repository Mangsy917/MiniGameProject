using UnityEngine.SceneManagement;

// 씬 전환 진입점. 로딩 씬을 먼저 띄운 뒤 목표 씬을 비동기로 로드한다.
public static class SceneLoader
{
    public const string LoadingSceneName = "Loading";

    public static string NextSceneName { get; private set; }

    public static void LoadScene(string sceneName)
    {
        NextSceneName = sceneName;
        SceneManager.LoadScene(LoadingSceneName);
    }
}
