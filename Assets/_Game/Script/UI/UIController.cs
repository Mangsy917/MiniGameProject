using UnityEngine;

public class UIController : MonoBehaviour
{
    public GameObject SettingUI;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SettingUI.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PressSettingBT()
    {
        SettingUI.SetActive(true); 
    }
    public void PressBackTitleBT()
    {
        SettingUI.SetActive(false);
    }
}