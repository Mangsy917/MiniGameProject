using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SoundUISliderTXTBinding : MonoBehaviour
{
    public Slider AllSLD;
    public Slider EffectSLD;
    public Slider BGMSLD;
    public TMP_Text AllTXT;
    public TMP_Text EffectTXT;
    public TMP_Text BGMTXT;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AllSLD.value = 0.7f;
        EffectSLD.value = 1f;
        BGMSLD.value = 1f;

        AllSLD.onValueChanged.AddListener(AllSLDValue);
        AllSLDValue(AllSLD.value);

        EffectSLD.onValueChanged.AddListener(EffectSLDValue);
        EffectSLDValue(EffectSLD.value);

        BGMSLD.onValueChanged.AddListener(BGMSLDValue);
        BGMSLDValue(BGMSLD.value);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AllSLDValue(float value)
    {
        AllTXT.text = value.ToString("F2");
    }
    public void EffectSLDValue(float value)
    {
        EffectTXT.text = value.ToString("F2");
    }
    public void BGMSLDValue(float value)
    {
        BGMTXT.text = value.ToString("F2");
    }
}
