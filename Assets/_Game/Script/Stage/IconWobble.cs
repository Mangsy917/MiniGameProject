using UnityEngine;

// 아이콘을 좌우로 기울인 두 각도 사이에서 뚝뚝 끊기듯 번갈아 바꾼다 (보간 없음, 스톱모션 느낌).
public class IconWobble : MonoBehaviour
{
    public float Angle = 6f;          // 좌우로 기울어지는 각도
    public float Interval = 0.5f;     // 한 포즈에 머무는 시간
    public bool RandomStart = true;   // 아이콘마다 시작 타이밍/방향을 다르게 해서 덜 기계적으로 보이게

    Quaternion baseRotation;
    float timer;
    bool tiltLeft;

    void Awake()
    {
        baseRotation = transform.localRotation;
    }

    void OnEnable()
    {
        tiltLeft = RandomStart && Random.value < 0.5f;
        timer = RandomStart ? Random.Range(0f, Interval) : 0f;
        Apply();
    }

    void OnDisable()
    {
        transform.localRotation = baseRotation;
    }

    void Update()
    {
        // 클리어 연출 등에서 Time.timeScale이 바뀌어도 일정하게 흔들리도록 unscaled 시간 사용
        timer += Time.unscaledDeltaTime;
        if (timer < Interval)
            return;

        timer -= Interval;
        tiltLeft = !tiltLeft;
        Apply();
    }

    void Apply()
    {
        transform.localRotation = baseRotation * Quaternion.Euler(0f, 0f, tiltLeft ? Angle : -Angle);
    }
}
