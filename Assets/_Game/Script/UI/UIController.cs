using UnityEngine;

public class UIController : MonoBehaviour
{
    public GameObject SettingUI;
    public GameObject LeaderboardUI;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SettingUI.SetActive(false);
        LeaderboardUI.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
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