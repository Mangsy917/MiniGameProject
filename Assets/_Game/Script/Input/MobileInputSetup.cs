using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

// 씬이 열릴 때마다 UI를 터치 기준으로 맞춘다. 씬에 따로 배치할 필요 없다.
public static class MobileInputSetup
{
    // 손가락은 마우스보다 많이 흔들리므로 이 거리(인치)까지는 드래그가 아닌 탭으로 본다
    const float TapSlopInch = 0.08f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EventSystem eventSystem = Object.FindFirstObjectByType<EventSystem>();
        if (eventSystem == null)
            return;

        // 키보드/패드로 버튼을 옮겨 다니거나 누르는 조작은 쓰지 않는다
        eventSystem.sendNavigationEvents = false;
        eventSystem.pixelDragThreshold = Mathf.RoundToInt(TapSlopPixels(eventSystem.pixelDragThreshold));
    }

    // 화면 dpi에 맞춘 탭 허용 거리(픽셀). minPixels보다 작아지지 않는다
    public static float TapSlopPixels(float minPixels)
    {
        return Mathf.Max(minPixels, Screen.dpi * TapSlopInch);
    }
}
