using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// FOR_01 텐트의 못을 박아라
// 못을 비스듬히 내려다보는 시점. 탭할 때마다 망치로 화면 가운데의 못을 박고, 다 박으면 옆으로 넘어가 다음 못이 나온다.
public class HammerNailsMiniGame : TapGaugeMiniGame
{
    const string HeadName = "Head";   // 못 머리 임시 이미지 (못 이미지의 자식)

    [Header("FOR_01")]
    [SerializeField] int NailCount = 4;
    [SerializeField] int TapsPerNail = 3;          // 전체 탭 수 = 못 수 x 이 값 (Required Taps는 쓰지 않는다)
    [SerializeField] float SinkDepth = 120f;       // 다 박혔을 때 땅속으로 들어가는 길이
    [SerializeField] float NailSpacing = 450f;
    [SerializeField] float SlideSpeed = 3200f;     // 다음 못으로 넘어가는 속도
    [SerializeField] float HammerAngle = 40f;
    [SerializeField] float HammerTime = 0.1f;
    [SerializeField] Image HoleIMG;                // 못이 박히는 땅 구멍. 복제해서 쓰는 원본
    [SerializeField] Image NailIMG;                // 복제해서 쓰는 원본. 상태: 박힘 단계. 아래쪽부터 땅에 가려진다
    [SerializeField] Image HammerIMG;

    readonly List<Image> nails = new List<Image>();
    readonly List<Image> holes = new List<Image>();
    Vector2 nailCenter, holeCenter;
    Quaternion hammerRotation;
    float slide;
    float hammerTimer;

    protected override int TotalTaps
    {
        get { return Mathf.Max(1, NailCount * TapsPerNail); }
    }

    int CurrentNail
    {
        get { return Mathf.Min(Taps / Mathf.Max(1, TapsPerNail), NailCount - 1); }
    }

    // 다음 못으로 완전히 넘어간 뒤에만 박을 수 있다
    protected override bool CanTap
    {
        get { return Mathf.Approximately(slide, CurrentNail * NailSpacing); }
    }

    protected override void Setup()
    {
        base.Setup();
        nailCenter = NailIMG.rectTransform.anchoredPosition;
        holeCenter = HoleIMG.rectTransform.anchoredPosition;
        hammerRotation = HammerIMG.rectTransform.localRotation;
        for (int i = 0; i < NailCount; i++)
        {
            holes.Add(Spawn(HoleIMG, holeCenter));
            nails.Add(Spawn(NailIMG, nailCenter));
        }
        Place();
    }

    protected override void OnTapped()
    {
        hammerTimer = HammerTime;
        Place();
    }

    protected override void Tick(float deltaTime)
    {
        hammerTimer = Mathf.Max(0f, hammerTimer - deltaTime);
        float t = HammerTime > 0f ? hammerTimer / HammerTime : 0f;
        HammerIMG.rectTransform.localRotation = hammerRotation * Quaternion.Euler(0f, 0f, HammerAngle * t);

        // 망치질이 끝난 뒤 넘어가서 못이 다 박힌 모습이 보인다
        if (hammerTimer <= 0f)
            slide = Mathf.MoveTowards(slide, CurrentNail * NailSpacing, SlideSpeed * SpeedScale * deltaTime);
        Place();
    }

    void Place()
    {
        int states = GetSlot(1).StateCount;
        for (int i = 0; i < nails.Count; i++)
        {
            float progress = Mathf.Clamp01((float)(Taps - i * TapsPerNail) / Mathf.Max(1, TapsPerNail));
            float x = i * NailSpacing - slide;
            float sink = SinkDepth * progress;
            Image nail = nails[i];
            holes[i].rectTransform.anchoredPosition = holeCenter + new Vector2(x, 0f);
            nail.rectTransform.anchoredPosition = nailCenter + new Vector2(x, -sink);
            nail.fillAmount = 1f - sink / nail.rectTransform.rect.height;   // 땅속에 들어간 만큼 아래를 가린다
            Show(nail, 1, Mathf.FloorToInt(progress * (states - 1) + 0.001f));
            SetLabel(nail, "");

            Transform head = nail.transform.Find(HeadName);
            if (head != null)
                head.gameObject.SetActive(!nail.preserveAspect);
        }
    }
}
