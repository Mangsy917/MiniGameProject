using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 옵션 창 버튼들 (프리팹 OptionWindow). 열고 닫는 연출은 DropWindow가 맡는다.
// 음량 슬라이더 표시는 SoundUISliderTXTBinding이 맡는다.
// 진동 토글과 언어 선택은 아직 화면 표시만 바뀌고 실제 기능은 없다.
public class OptionWindow : MonoBehaviour
{
    public DropWindow Window;

    [Header("Vibration (표시만)")]
    public Toggle VibrationToggle;
    public RectTransform VibrationKnob;        // 켜면 오른쪽, 끄면 왼쪽으로 가는 손잡이
    public float KnobOffX = -14f;
    public float KnobOnX = 14f;

    [Header("Language (표시만)")]
    public TMP_Text LanguageText;
    public string[] Languages = { "한국어", "English" };

    int languageIndex;

    void Awake()
    {
        VibrationToggle.onValueChanged.AddListener(OnVibrationChanged);
        OnVibrationChanged(VibrationToggle.isOn);
        ShowLanguage();
    }

    void OnVibrationChanged(bool on)
    {
        if (VibrationKnob != null)
            VibrationKnob.anchoredPosition = new Vector2(on ? KnobOnX : KnobOffX, VibrationKnob.anchoredPosition.y);
    }

    // 언어 스텝퍼: 좌우 버튼으로 돌아가며 바뀐다
    public void PressLanguagePrevBT()
    {
        languageIndex = (languageIndex + Languages.Length - 1) % Languages.Length;
        ShowLanguage();
    }

    public void PressLanguageNextBT()
    {
        languageIndex = (languageIndex + 1) % Languages.Length;
        ShowLanguage();
    }

    void ShowLanguage()
    {
        if (LanguageText != null && Languages.Length > 0)
            LanguageText.text = Languages[languageIndex];
    }

    public void PressBackBT()
    {
        Window.Close();
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
