using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIController : MonoBehaviour
{
    public GameObject SettingUI;
    public GameObject LeaderboardUI;
    public GameObject InfinityLockPopupUI;

    [SerializeField] private string Title;
    [SerializeField] private string InfinityMode;

    [Header("Infinity Mode Lock")]
    public CanvasGroup MenuGroup;              // 타이틀 메뉴 버튼들. 연출 중에는 누를 수 없다
    public Transform InfinityModeBT;
    public CanvasGroup InfinityLockMark;       // 무한 모드 버튼 위의 잠금 표시
    public float UnlockDelay = 0.5f;           // 타이틀 진입 후 연출 시작까지
    public float UnlockShakeTime = 0.5f;       // 잠금 표시가 흔들리는 시간
    public float UnlockShakeAngle = 8f;
    public int UnlockShakeCount = 4;
    public float UnlockFadeTime = 0.4f;        // 잠금 표시가 커지면서 사라지는 시간
    public float UnlockFadeScale = 1.3f;
    public float UnlockPunchTime = 0.3f;       // 버튼이 통통 튀는 시간
    public float UnlockPunchScale = 1.15f;
    public float UnlockHoldTime = 0.3f;        // 연출이 끝나고 조작을 풀기 전까지

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SettingUI.SetActive(false);
        LeaderboardUI.SetActive(false);
        InfinityLockPopupUI.SetActive(false);

        bool unlocked = StageProgress.IsInfinityModeUnlocked();
        if (unlocked && !StageProgress.IsInfinityUnlockShown())
        {
            // 연구소를 처음 클리어하고 돌아왔을 때 한 번만 잠금 해제 연출
            StageProgress.SetInfinityUnlockShown();
            StartCoroutine(PlayInfinityUnlock());
        }
        else
        {
            InfinityLockMark.gameObject.SetActive(!unlocked);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator PlayInfinityUnlock()
    {
        SetMenuInteractable(false);
        InfinityLockMark.gameObject.SetActive(true);

        Transform mark = InfinityLockMark.transform;
        Vector3 scale = mark.localScale;
        Quaternion rotation = mark.localRotation;

        yield return new WaitForSecondsRealtime(UnlockDelay);

        // 흔들림 (점점 약해짐)
        float startTime = Time.realtimeSinceStartup;
        float t = 0f;
        while (t < 1f)
        {
            t = Mathf.Clamp01((Time.realtimeSinceStartup - startTime) / UnlockShakeTime);
            float angle = Mathf.Sin(t * Mathf.PI * 2f * UnlockShakeCount) * UnlockShakeAngle * (1f - t);
            mark.localRotation = rotation * Quaternion.Euler(0f, 0f, angle);
            yield return null;
        }
        mark.localRotation = rotation;

        // 커지면서 사라짐
        startTime = Time.realtimeSinceStartup;
        t = 0f;
        while (t < 1f)
        {
            t = Mathf.Clamp01((Time.realtimeSinceStartup - startTime) / UnlockFadeTime);
            mark.localScale = scale * Mathf.Lerp(1f, UnlockFadeScale, t);
            InfinityLockMark.alpha = 1f - t;
            yield return null;
        }
        InfinityLockMark.gameObject.SetActive(false);
        mark.localScale = scale;
        InfinityLockMark.alpha = 1f;

        // 버튼이 통통 튄다
        Vector3 buttonScale = InfinityModeBT.localScale;
        startTime = Time.realtimeSinceStartup;
        t = 0f;
        while (t < 1f)
        {
            t = Mathf.Clamp01((Time.realtimeSinceStartup - startTime) / UnlockPunchTime);
            InfinityModeBT.localScale = buttonScale * Mathf.Lerp(1f, UnlockPunchScale, Mathf.Sin(t * Mathf.PI));
            yield return null;
        }
        InfinityModeBT.localScale = buttonScale;

        yield return new WaitForSecondsRealtime(UnlockHoldTime);
        SetMenuInteractable(true);
    }

    // interactable을 끄면 버튼이 비활성 색으로 바뀌므로, 클릭과 키보드/패드 입력만 막는다
    void SetMenuInteractable(bool interactable)
    {
        MenuGroup.blocksRaycasts = interactable;
        if (EventSystem.current != null)
            EventSystem.current.sendNavigationEvents = interactable;
    }

    public void PressStartBT()
    {
        SceneLoader.LoadScene(Title);
    }
    public void PressInfinityModeBT()
    {
        if (StageProgress.IsInfinityModeUnlocked())
            SceneLoader.LoadScene(InfinityMode);
        else
            InfinityLockPopupUI.SetActive(true);
    }
    public void PressCloseBT_InfinityLockPopup()
    {
        InfinityLockPopupUI.SetActive(false);
    }
    public void PressSettingBT()
    {
        SettingUI.SetActive(true);
    }
    public void PressLeaderboardgBT()
    {
        LeaderboardUI.SetActive(true);
    }
    public void PressBackTitleBT_Setting()
    {
        SettingUI.SetActive(false);
    }
    public void PressBackTitleBT_Leaderboard()
    {
        LeaderboardUI.SetActive(false);
    }
    public void PressExitBT()
    {
        Application.Quit();
#if UNITY_EDITOR
        // UnityEditor는 빌드에 포함되지 않으므로 에디터에서만 호출
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
