using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

// 스테이지 씬 진입 시 3, 2, 1 카운트다운 후 Start! 메시지를 띄운다.
public class StageCountdown : MonoBehaviour
{
    public TMP_Text CountdownTXT;

    public int CountFrom = 3;
    public float CountInterval = 1f;      // 숫자 하나가 표시되는 시간
    public string StartMessage = "Start!";
    public float StartMessageTime = 1f;   // Start! 메시지가 떠 있는 시간

    // 숫자가 나타날 때 커졌다가 원래 크기로 줄어드는 연출
    public float PopScale = 1.6f;
    public float PopTime = 0.25f;

    // Start! 가 뜨는 순간 호출. 게임 시작(플레이어 조작 허용 등)을 여기에 연결한다
    public UnityEvent OnCountdownFinished;

    public bool IsFinished { get; private set; }

    // Start! 메시지까지 사라져 카운트다운 연출이 완전히 끝났는지
    public bool IsEnded { get; private set; }

    void Start()
    {
        StartCoroutine(Countdown());
    }

    IEnumerator Countdown()
    {
        CountdownTXT.gameObject.SetActive(true);

        for (int count = CountFrom; count > 0; count--)
        {
            CountdownTXT.text = count.ToString();
            yield return Pop(CountInterval);
        }

        CountdownTXT.text = StartMessage;
        IsFinished = true;
        OnCountdownFinished.Invoke();
        yield return Pop(StartMessageTime);

        CountdownTXT.gameObject.SetActive(false);
        IsEnded = true;
    }

    // duration 동안 표시. Time.timeScale의 영향을 받지 않고,
    // 씬 로드 직후 첫 프레임의 deltaTime이 커지는 문제를 피하려고 시작 시각 기준으로 잰다
    IEnumerator Pop(float duration)
    {
        Transform target = CountdownTXT.transform;
        float startTime = Time.realtimeSinceStartup;
        float timer = 0f;

        while (timer < duration)
        {
            timer = Time.realtimeSinceStartup - startTime;
            float t = Mathf.Clamp01(timer / PopTime);
            float scale = Mathf.Lerp(PopScale, 1f, 1f - (1f - t) * (1f - t));
            target.localScale = Vector3.one * scale;
            yield return null;
        }

        target.localScale = Vector3.one;
    }
}
