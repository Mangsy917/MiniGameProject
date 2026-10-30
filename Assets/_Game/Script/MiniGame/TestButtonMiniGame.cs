using TMPro;
using UnityEngine;

// 테스트용 미니게임. 성공/실패 버튼으로 결과를 바로 정한다.
public class TestButtonMiniGame : MiniGame
{
    public TMP_Text TitleTXT;

    protected override void OnBegin()
    {
        if (TitleTXT != null)
            TitleTXT.text = Title;
    }

    public void PressSuccessBT()
    {
        Finish(true);
    }
    public void PressFailBT()
    {
        Finish(false);
    }
}
