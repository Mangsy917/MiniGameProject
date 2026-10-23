using UnityEngine;

public class UIController : MonoBehaviour
{
    public GameObject SettingUI;
    public GameObject LeaderboardUI;
    public GameObject InfinityLockPopupUI;

    [SerializeField] private string Title;
    [SerializeField] private string InfinityMode;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SettingUI.SetActive(false);
        LeaderboardUI.SetActive(false);
        InfinityLockPopupUI.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PressStartBT()
    {
        SceneLoader.LoadScene(Title);
    }
    public void PressInfinityModeBT()
    {
        // 연구소 스테이지(6)는 1~5 스테이지를 모두 클리어해야 열리므로 연구소 클리어만 확인하면 된다
        if (StageProgress.IsCleared(StageProgress.LabStage))
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
        UnityEditor.EditorApplication.isPlaying = false;
    }
}