using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// FOR_14 냄비가 눌어붙지 않게 저어라 (회전 손잡이의 속도 유지형)
// 적정 속도로 끝까지 저으면 성공. 범위를 벗어난 상태가 일정 시간 이어지면 실패.
// 너무 빨라서 실패하면 음식이 넘치고, 넘친 음식(슬롯 4)이 냄비 가장자리에서 튀어 나와 냄비 밖 바닥에 떨어진다.
public class PotStirMiniGame : RotateMiniGame
{
    [Header("FOR_14")]
    [SerializeField] float MinTurnsPerSecond = 0.5f;
    [SerializeField] float MaxTurnsPerSecond = 3.5f;
    [SerializeField] float AllowedTime = 0.6f;       // 범위 밖이 이 시간보다 오래 이어지면 실패
    [SerializeField] float StartGrace = 1f;          // 젓기 시작하기 전까지 봐주는 시간 (젓기 시작하면 바로 판정)
    [SerializeField] float SpeedSmoothTime = 0.15f;  // 속도를 재는 구간. 짧으면 손 떨림에 민감하다
    [SerializeField] Image FoodIMG;                  // 이 그림의 중심을 기준으로 젓는다
    [SerializeField] float LadleRadius = 120f;       // 국자가 움직일 수 있는 범위 (냄비 중심에서)

    [Header("Spill")]                                // 넘쳤을 때 냄비 밖 바닥에 떨어지는 음식
    [SerializeField] Image SpillIMG;                 // 복제해서 쓰는 원본 (숨겨 둔다). 냄비보다 뒤에 두어 바닥에 깔린다
    [SerializeField] int SpillCount = 5;
    [SerializeField] float SpillStartRadius = 170f;  // 튀어 나오는 곳 (냄비 가장자리 안쪽)
    [SerializeField] Vector2 SpillLandRadius = new Vector2(300f, 200f);   // 떨어지는 곳 (냄비 중심에서 가로, 세로 거리)
    [SerializeField] float SpillFlyTime = 0.35f;     // 하나가 날아가 떨어지기까지
    [SerializeField] float SpillHop = 45f;           // 날아가며 튀어 오르는 높이
    [SerializeField] float SpillInterval = 0.06f;    // 하나씩 튀어 나오는 간격

    float turnsThisFrame;
    float speed;
    float outTimer;
    float timer;
    bool stirring;

    protected override void Setup()
    {
        SetActive(SpillIMG, false);
    }

    // 국자는 제자리에서 돌지 않고 손가락을 따라 냄비 안을 돌아다닌다
    protected override Vector2 Center
    {
        get { return FoodIMG.rectTransform.anchoredPosition; }
    }

    protected override void TurnHandle(float angle) { }

    protected override void OnDragBegin(Vector2 pos)
    {
        MoveLadle(pos);
    }

    protected override void OnDrag(Vector2 pos, Vector2 delta)
    {
        MoveLadle(pos);
        base.OnDrag(pos, delta);
    }

    void MoveLadle(Vector2 pos)
    {
        HandleIMG.rectTransform.anchoredPosition = Center + Vector2.ClampMagnitude(pos - Center, LadleRadius);
    }

    protected override void OnRotate(float turns)
    {
        turnsThisFrame += turns;
        stirring = true;
    }

    protected override void Tick(float deltaTime)
    {
        if (deltaTime <= 0f)
            return;

        // 부호를 살려 평균을 내므로 제자리에서 왔다 갔다 하는 것은 느린 것으로 본다
        speed = Mathf.Lerp(speed, turnsThisFrame / deltaTime, 1f - Mathf.Exp(-deltaTime / Mathf.Max(0.01f, SpeedSmoothTime)));
        turnsThisFrame = 0f;
        timer += deltaTime;

        float now = Mathf.Abs(speed);
        if (!stirring && timer < StartGrace)
            return;

        bool slow = now < MinTurnsPerSecond;
        bool fast = now > MaxTurnsPerSecond;
        outTimer = slow || fast ? outTimer + deltaTime : 0f;
        if (outTimer > AllowedTime)
        {
            Show(FoodIMG, 3, slow ? 1 : 2);
            Finish(false);
            if (fast)
                StartCoroutine(Spill());
        }
    }

    // 냄비 둘레 랜덤 각도(고르게 나눈 뒤 흔듦)로 넘친 음식이 하나씩 튀어 나와 냄비 밖 바닥에 떨어진다
    IEnumerator Spill()
    {
        if (SpillIMG == null || SpillCount <= 0)
            yield break;

        Vector2 center = FoodIMG.rectTransform.anchoredPosition;
        float step = 360f / SpillCount;
        float offset = Random.Range(0f, 360f);
        Image[] spills = new Image[SpillCount];
        Vector2[] from = new Vector2[SpillCount];
        Vector2[] to = new Vector2[SpillCount];
        for (int i = 0; i < SpillCount; i++)
        {
            float angle = (offset + step * i + Random.Range(-0.3f, 0.3f) * step) * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            from[i] = center + dir * SpillStartRadius;
            to[i] = center + Vector2.Scale(dir, SpillLandRadius) * Random.Range(0.92f, 1.08f);
            Image spill = Spawn(SpillIMG, from[i]);
            Show(spill, 4, 0);
            spill.rectTransform.localEulerAngles = new Vector3(0f, 0f, Random.Range(0f, 360f));
            spill.rectTransform.localScale = Vector3.zero;
            spills[i] = spill;
        }

        float startTime = Time.unscaledTime;
        float end = SpillInterval * (SpillCount - 1) + SpillFlyTime;
        float elapsed = 0f;
        while (elapsed < end)
        {
            elapsed = Time.unscaledTime - startTime;
            for (int i = 0; i < spills.Length; i++)
            {
                float t = Mathf.Clamp01((elapsed - SpillInterval * i) / Mathf.Max(0.01f, SpillFlyTime));
                float move = 1f - (1f - t) * (1f - t);
                Vector2 pos = Vector2.Lerp(from[i], to[i], move) + Vector2.up * (Mathf.Sin(t * Mathf.PI) * SpillHop);
                spills[i].rectTransform.anchoredPosition = pos;
                // 날아가는 동안 작게, 바닥에 닿으며 납작하게 퍼진다
                float land = Mathf.Clamp01((t - 0.85f) / 0.15f);
                spills[i].rectTransform.localScale = t <= 0f ? Vector3.zero : new Vector3(Mathf.Lerp(0.6f, 1.1f, land), Mathf.Lerp(0.6f, 0.9f, land), 1f);
            }
            yield return null;
        }
        for (int i = 0; i < spills.Length; i++)
        {
            spills[i].rectTransform.anchoredPosition = to[i];
            spills[i].rectTransform.localScale = new Vector3(1.1f, 0.9f, 1f);
        }
    }
}
