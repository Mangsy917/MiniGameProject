using UnityEngine;

// 스테이지 아이콘 확대 반응. 마우스를 올리면 확대된 채로 있고, 누르면(클릭/터치) 순간적으로 더 커졌다가 돌아온다.
// 보간 없이 크기가 뚝 바뀌어 IconWobble의 스톱모션 느낌과 맞춘다.
public class IconPop : MonoBehaviour
{
    public float HoverScale = 1.12f;   // 마우스를 올렸을 때
    public float PressScale = 1.25f;   // 클릭/터치한 순간
    public float PressTime = 0.12f;    // 누른 크기로 머무는 시간

    Vector3 baseScale;
    bool hovered;
    float pressTimer;

    void Awake()
    {
        baseScale = transform.localScale;
    }

    void OnDisable()
    {
        hovered = false;
        pressTimer = 0f;
        transform.localScale = baseScale;
    }

    public void SetHovered(bool value)
    {
        hovered = value;
        Apply();
    }

    public void Press()
    {
        pressTimer = PressTime;
        Apply();
    }

    void Update()
    {
        if (pressTimer <= 0f)
            return;

        // 연출 중 Time.timeScale이 바뀌어도 일정하게
        pressTimer -= Time.unscaledDeltaTime;
        if (pressTimer <= 0f)
            Apply();
    }

    void Apply()
    {
        float scale = pressTimer > 0f ? PressScale : hovered ? HoverScale : 1f;
        transform.localScale = baseScale * scale;
    }
}
