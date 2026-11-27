using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 도움말 창. 위에서 내려오고(DropWindow), 좌우 버튼으로 페이지를 넘기며, 아래 가운데 확인 버튼으로 닫는다.
// 페이지는 Pages에 등록한 오브젝트를 하나씩 켜서 보여 준다 (내용은 기획/아트가 페이지 오브젝트 안에 채운다).
public class HelpWindow : MonoBehaviour
{
    public DropWindow Window;
    public GameObject[] Pages;
    public Button PrevButton;
    public Button NextButton;
    public TMP_Text PageText;      // "1 / 3"

    int page;

    // 열 때마다 첫 페이지부터
    public void Open()
    {
        ShowPage(0);
        Window.Open();
    }

    public void PressPrevBT()
    {
        ShowPage(page - 1);
    }

    public void PressNextBT()
    {
        ShowPage(page + 1);
    }

    public void PressConfirmBT()
    {
        Window.Close();
    }

    void ShowPage(int index)
    {
        if (Pages == null || Pages.Length == 0)
            return;

        page = Mathf.Clamp(index, 0, Pages.Length - 1);
        for (int i = 0; i < Pages.Length; i++)
        {
            if (Pages[i] != null)
                Pages[i].SetActive(i == page);
        }
        // 첫 페이지에서는 왼쪽, 마지막 페이지에서는 오른쪽 버튼을 숨긴다
        PrevButton.gameObject.SetActive(page > 0);
        NextButton.gameObject.SetActive(page < Pages.Length - 1);
        if (PageText != null)
            PageText.text = (page + 1) + " / " + Pages.Length;
    }
}
