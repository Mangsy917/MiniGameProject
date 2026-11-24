using UnityEngine;
using UnityEngine.UI;

// LAB_19 타자기로 보고서를 작성해라
// 컴퓨터 문서 작성 프로그램에 탭할 때마다 글자가 입력된다. 한 장을 다 채우면 성공.
public class DocTypingMiniGame : TapGaugeMiniGame
{
    [Header("LAB_19")]
    [SerializeField, TextArea(4, 10)] string Report;
    [SerializeField] float CursorBlinkTime = 0.4f;
    [SerializeField] Image PageIMG;      // 2번 자리(글자 채워짐 단계) 그림이 있으면 글자 대신 그림을 보여준다
    [SerializeField] Text BodyTXT;

    float cursorTimer;

    protected override void Setup()
    {
        base.Setup();
        Refresh();
    }

    protected override void OnTapped()
    {
        cursorTimer = 0f;
        Refresh();
    }

    protected override void Tick(float deltaTime)
    {
        cursorTimer += deltaTime;
        Refresh();
    }

    void Refresh()
    {
        SpriteSlot slot = GetSlot(2);
        int state = Mathf.FloorToInt(Progress * (slot.StateCount - 1) + 0.001f);
        Show(PageIMG, 2, state);
        SetLabel(PageIMG, "");
        bool useText = slot.GetSprite(state) == null;
        SetActive(BodyTXT, useText);
        if (!useText)
            return;

        bool cursor = Mathf.Repeat(cursorTimer, CursorBlinkTime * 2f) < CursorBlinkTime;
        BodyTXT.text = Report.Substring(0, Mathf.RoundToInt(Report.Length * Progress)) + (cursor ? "|" : "");
    }
}
