using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public DropWindow OptionWindow;            // 위에서 내려오는 옵션 창 (프리팹)
    public DropWindow LeaderboardWindow;       // 위에서 내려오는 리더보드 창

    [SerializeField] private string Title;
    [SerializeField] private string InfinityMode;

    [Header("Title Intro")]
    public RectTransform TitleText;            // 게임 제목. 처음엔 정중앙, 메뉴가 나오면 위로 올라간다
    public CanvasGroup TapToStart;             // 제목 아래 "Tap to Start"
    public Vector2 TitleCenterPos = new Vector2(0f, 30f);
    public Vector2 TitleMenuPos = new Vector2(0f, 185f);
    public float TitleMenuScale = 0.6f;
    public float IntroDelay = 0.2f;            // 씬 진입 후 제목이 나오기 전까지
    public float TitlePopTime = 0.7f;          // 제목이 커지면서 나타나는 시간
    public float TitlePopOvershoot = 1.15f;    // 나타날 때 잠깐 더 커지는 배율
    public float TapFadeTime = 0.4f;           // Tap to Start가 나타나는 시간
    public float TapBlinkSpeed = 1.2f;         // Tap to Start 깜빡임 속도 (초당 왕복)
    public float TapBlinkMinAlpha = 0.25f;
    public float MenuRevealTime = 0.45f;       // 제목이 올라가고 메뉴가 나오는 시간
    public float MenuButtonStagger = 0.08f;    // 메뉴 버튼이 하나씩 튀어나오는 간격
    public float MenuButtonOvershoot = 1.7f;   // 버튼이 튀어나올 때 살짝 커졌다 돌아오는 정도
    public RectTransform[] MenuButtons;        // 튀어나오는 순서대로

    [Header("Mode Detail (Adventure / Infinity)")]
    public RectTransform AdventureBT;
    public CanvasGroup SelectDim;             // 버튼을 고르면 뒤를 어둡게 가리는 배경 (누르면 닫힘)
    public RectTransform DetailPanel;          // 오른쪽에서 들어오는 설명창
    public TMP_Text DetailTitleText;
    public TMP_Text DetailDescText;
    public Vector2 SelectedButtonPos = new Vector2(-240f, -20f);  // 고른 버튼이 옮겨 가는 자리
    public float SelectedButtonScale = 1.1f;
    public Vector2 DetailPanelPos = new Vector2(150f, -20f);      // 설명창이 멈추는 자리
    public float SelectMoveTime = 0.35f;       // 버튼이 왼쪽으로 가고 배경이 어두워지는 시간
    public float DetailSlideTime = 0.4f;       // 설명창이 들어오는 시간
    public float DetailBounce = 1.2f;          // 설명창이 멈출 때 살짝 지나쳤다 돌아오는 정도
    public float DetailCloseTime = 0.25f;      // 닫을 때 걸리는 시간 (설명창이 나가고 버튼이 돌아오는 각각)
    public string AdventureTitle = "모험";
    [TextArea] public string AdventureDesc = "스테이지를 돌며\n미니게임을 클리어하자!";
    public string InfinityTitle = "무한";
    [TextArea] public string InfinityDesc = "모든 스테이지의 미니게임이 무작위로!\n얼마나 버틸 수 있을까?";

    [Header("Infinity Locked Shake")]
    public float LockedShakeTime = 0.4f;       // 잠긴 무한 버튼을 눌렀을 때 흔들리는 시간
    public float LockedShakeDistance = 12f;    // 좌우로 흔들리는 폭 (캔버스 픽셀)
    public int LockedShakeCount = 4;           // 왕복 횟수

    // 한 번 인트로를 본 뒤 다른 씬에서 돌아오면 바로 메뉴를 보여 준다
    static bool introShown;
    bool lockedShaking;

    enum Mode { None, Adventure, Infinity }
    Mode selectedMode = Mode.None;
    bool detailMoving;
    RectTransform selectedButton;
    Vector2 selectedButtonHome;
    Vector3 selectedButtonHomeScale;
    int selectedButtonSibling;

    [Header("Infinity Mode Lock")]
    public CanvasGroup MenuGroup;              // 타이틀 메뉴 버튼들. 연출 중에는 누를 수 없다
    public Transform InfinityModeBT;
    public CanvasGroup InfinityLockMark;       // 무한 모드 버튼 위의 잠금 표시
    public float UnlockDelay = 0.5f;           // 타이틀 진입 후 연출 시작까지
    public float UnlockShakeTime = 0.5f;       // 잠금 표시가 흔들리는 시간
    public float UnlockShakeAngle = 8f;
    public int UnlockShakeCount = 4;
    public float UnlockFadeTime = 0.4f;        // 잠금 표시가 커지면서 사라지는 시간
    public float UnlockFadeScale = 1.3f;
    public float UnlockPunchTime = 0.3f;       // 버튼이 통통 튀는 시간
    public float UnlockPunchScale = 1.15f;
    public float UnlockHoldTime = 0.3f;        // 연출이 끝나고 조작을 풀기 전까지

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        OptionWindow.gameObject.SetActive(false);
        LeaderboardWindow.gameObject.SetActive(false);
        SelectDim.gameObject.SetActive(false);
        DetailPanel.gameObject.SetActive(false);

        if (introShown)
        {
            ShowMenuImmediately();
            OnMenuShown();
        }
        else
        {
            introShown = true;
            StartCoroutine(PlayIntro());
        }
    }

    // 에디터에서 도메인 리로드 없이 플레이해도 매번 인트로가 나오도록 초기화
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        introShown = false;
    }

    // 메뉴가 다 나온 뒤: 무한 모드 잠금 상태 표시, 처음 해금됐으면 해금 연출
    void OnMenuShown()
    {
        bool unlocked = StageProgress.IsInfinityModeUnlocked();
        if (unlocked && !StageProgress.IsInfinityUnlockShown())
        {
            // 연구소를 처음 클리어하고 돌아왔을 때 한 번만 잠금 해제 연출
            StageProgress.SetInfinityUnlockShown();
            StartCoroutine(PlayInfinityUnlock());
        }
        else
        {
            InfinityLockMark.gameObject.SetActive(!unlocked);
            SetMenuInteractable(true);
        }
    }

    // ---------- 타이틀 인트로 ----------

    // 제목이 정중앙에 튀어나오고 Tap to Start가 깜빡인다. 연출 중 탭하면 연출을 끝 상태로 건너뛴다.
    // 연출이 끝난 뒤 탭하면 제목이 위로 올라가며 메뉴가 나온다.
    IEnumerator PlayIntro()
    {
        Vector3 titleScale = TitleText.localScale;
        TitleText.anchoredPosition = TitleCenterPos;
        TitleText.localScale = Vector3.zero;
        TapToStart.gameObject.SetActive(true);
        TapToStart.alpha = 0f;
        MenuGroup.alpha = 0f;
        SetMenuInteractable(false);

        bool skip = false;
        float startTime = Time.realtimeSinceStartup;

        // 대기
        while (!skip && Time.realtimeSinceStartup - startTime < IntroDelay)
        {
            yield return null;
            skip = TappedThisFrame();
        }

        // 제목이 커지면서 나타난다 (살짝 지나쳤다 돌아옴)
        startTime = Time.realtimeSinceStartup;
        float t = 0f;
        while (!skip && t < 1f)
        {
            t = Progress(startTime, TitlePopTime);
            float k = t < 0.6f
                ? Mathf.Lerp(0f, TitlePopOvershoot, EaseOutCubic(t / 0.6f))
                : Mathf.Lerp(TitlePopOvershoot, 1f, EaseInOutSine((t - 0.6f) / 0.4f));
            TitleText.localScale = titleScale * k;
            yield return null;
            skip = TappedThisFrame();
        }
        TitleText.localScale = titleScale;

        // Tap to Start가 나타난다
        startTime = Time.realtimeSinceStartup;
        t = 0f;
        while (!skip && t < 1f)
        {
            t = Progress(startTime, TapFadeTime);
            TapToStart.alpha = t;
            yield return null;
            skip = TappedThisFrame();
        }
        TapToStart.alpha = 1f;

        // 건너뛴 탭이 그대로 "시작" 탭으로 이어지지 않도록 다음 프레임부터 탭을 받는다
        yield return null;

        // 탭할 때까지 깜빡인다
        float blinkStart = Time.realtimeSinceStartup;
        while (!TappedThisFrame())
        {
            float wave = 0.5f + 0.5f * Mathf.Cos((Time.realtimeSinceStartup - blinkStart) * Mathf.PI * 2f * TapBlinkSpeed);
            TapToStart.alpha = Mathf.Lerp(TapBlinkMinAlpha, 1f, wave);
            yield return null;
        }

        yield return RevealMenu(titleScale);
        OnMenuShown();
    }

    // 제목이 위로 올라가며 작아지고, Tap to Start가 사라지고, 메뉴 버튼이 하나씩 튀어나온다.
    // 이 연출 중에 탭해도 끝 상태로 건너뛴다.
    IEnumerator RevealMenu(Vector3 titleScale)
    {
        Vector3[] buttonScales = new Vector3[MenuButtons.Length];
        for (int i = 0; i < MenuButtons.Length; i++)
        {
            buttonScales[i] = MenuButtons[i].localScale;
            MenuButtons[i].localScale = Vector3.zero;
        }
        float tapAlpha = TapToStart.alpha;
        float total = MenuRevealTime + MenuButtonStagger * Mathf.Max(0, MenuButtons.Length - 1);

        yield return null;
        bool skip = false;
        float startTime = Time.realtimeSinceStartup;
        float elapsed = 0f;
        while (!skip && elapsed < total)
        {
            elapsed = Time.realtimeSinceStartup - startTime;
            float t = Mathf.Clamp01(elapsed / MenuRevealTime);
            float e = EaseOutCubic(t);
            TitleText.anchoredPosition = Vector2.Lerp(TitleCenterPos, TitleMenuPos, e);
            TitleText.localScale = titleScale * Mathf.Lerp(1f, TitleMenuScale, e);
            TapToStart.alpha = tapAlpha * (1f - Mathf.Clamp01(t * 2f));
            MenuGroup.alpha = e;
            for (int i = 0; i < MenuButtons.Length; i++)
            {
                float bt = Mathf.Clamp01((elapsed - MenuButtonStagger * i) / MenuRevealTime);
                MenuButtons[i].localScale = buttonScales[i] * EaseOutBack(bt, MenuButtonOvershoot);
            }
            yield return null;
            skip = TappedThisFrame();
        }

        for (int i = 0; i < MenuButtons.Length; i++)
            MenuButtons[i].localScale = buttonScales[i];
        TitleText.localScale = titleScale * TitleMenuScale;
        ShowMenuImmediately(false);
    }

    // 연출 없이 메뉴가 다 나온 상태로 맞춘다
    void ShowMenuImmediately(bool setTitleScale = true)
    {
        TitleText.anchoredPosition = TitleMenuPos;
        if (setTitleScale)
            TitleText.localScale = TitleText.localScale * TitleMenuScale;
        TapToStart.alpha = 0f;
        TapToStart.gameObject.SetActive(false);
        MenuGroup.alpha = 1f;
    }

    static bool TappedThisFrame()
    {
        Vector2 position;
        return TouchInput.TryGetPress(out position);
    }

    static float Progress(float startTime, float duration)
    {
        return duration > 0f ? Mathf.Clamp01((Time.realtimeSinceStartup - startTime) / duration) : 1f;
    }

    static float EaseOutCubic(float t)
    {
        float u = 1f - t;
        return 1f - u * u * u;
    }

    static float EaseInOutSine(float t)
    {
        return 0.5f - 0.5f * Mathf.Cos(t * Mathf.PI);
    }

    // 끝에서 살짝 지나쳤다가 돌아오는 곡선. overshoot = 0이면 지나치지 않는다
    static float EaseOutBack(float t, float overshoot)
    {
        float u = t - 1f;
        return 1f + (overshoot + 1f) * u * u * u + overshoot * u * u;
    }

    // ---------- 모험 / 무한 설명창 ----------

    // 고른 버튼이 왼쪽으로 옮겨 가고 뒤가 어두워진 뒤, 오른쪽에서 설명창이 들어온다
    void OpenDetail(Mode mode, RectTransform button)
    {
        if (detailMoving || AnyWindowOpen() || lockedShaking || selectedMode != Mode.None)
            return;
        StartCoroutine(OpenDetailRoutine(mode, button));
    }

    IEnumerator OpenDetailRoutine(Mode mode, RectTransform button)
    {
        detailMoving = true;
        selectedMode = mode;
        SetMenuInteractable(false);

        // 설명 내용
        DetailTitleText.text = mode == Mode.Adventure ? AdventureTitle : InfinityTitle;
        DetailDescText.text = mode == Mode.Adventure ? AdventureDesc : InfinityDesc;

        // 고른 버튼을 어두운 배경 위로 올린다
        selectedButton = button;
        selectedButtonHome = button.anchoredPosition;
        selectedButtonHomeScale = button.localScale;
        selectedButtonSibling = button.GetSiblingIndex();
        SelectDim.gameObject.SetActive(true);
        SelectDim.alpha = 0f;
        SelectDim.transform.SetAsLastSibling();
        button.SetAsLastSibling();
        DetailPanel.SetAsLastSibling();

        // 버튼 이동 + 배경 어두워짐
        float startTime = Time.realtimeSinceStartup;
        float t = 0f;
        while (t < 1f)
        {
            t = Progress(startTime, SelectMoveTime);
            float e = EaseOutCubic(t);
            button.anchoredPosition = Vector2.Lerp(selectedButtonHome, SelectedButtonPos, e);
            button.localScale = selectedButtonHomeScale * Mathf.Lerp(1f, SelectedButtonScale, e);
            SelectDim.alpha = t;
            yield return null;
        }

        // 설명창이 오른쪽 밖에서 들어온다
        DetailPanel.gameObject.SetActive(true);
        Vector2 from = new Vector2(OffscreenDetailX(), DetailPanelPos.y);
        startTime = Time.realtimeSinceStartup;
        t = 0f;
        while (t < 1f)
        {
            t = Progress(startTime, DetailSlideTime);
            DetailPanel.anchoredPosition = Vector2.LerpUnclamped(from, DetailPanelPos, EaseOutBack(t, DetailBounce));
            yield return null;
        }
        DetailPanel.anchoredPosition = DetailPanelPos;

        SetMenuInteractable(true);
        detailMoving = false;
    }

    // 설명창이 오른쪽으로 나가고, 버튼이 제자리로 돌아오며 배경이 밝아진다
    IEnumerator CloseDetailRoutine()
    {
        detailMoving = true;
        SetMenuInteractable(false);

        Vector2 to = new Vector2(OffscreenDetailX(), DetailPanelPos.y);
        float startTime = Time.realtimeSinceStartup;
        float t = 0f;
        while (t < 1f)
        {
            t = Progress(startTime, DetailCloseTime);
            DetailPanel.anchoredPosition = Vector2.Lerp(DetailPanelPos, to, t * t);
            yield return null;
        }
        DetailPanel.gameObject.SetActive(false);

        Vector3 bigScale = selectedButton.localScale;
        startTime = Time.realtimeSinceStartup;
        t = 0f;
        while (t < 1f)
        {
            t = Progress(startTime, DetailCloseTime);
            float e = EaseOutCubic(t);
            selectedButton.anchoredPosition = Vector2.Lerp(SelectedButtonPos, selectedButtonHome, e);
            selectedButton.localScale = Vector3.Lerp(bigScale, selectedButtonHomeScale, e);
            SelectDim.alpha = 1f - t;
            yield return null;
        }
        selectedButton.anchoredPosition = selectedButtonHome;
        selectedButton.localScale = selectedButtonHomeScale;
        selectedButton.SetSiblingIndex(selectedButtonSibling);
        SelectDim.gameObject.SetActive(false);
        selectedButton = null;
        selectedMode = Mode.None;

        SetMenuInteractable(true);
        detailMoving = false;
    }

    // 설명창이 화면 오른쪽으로 완전히 벗어나는 x
    float OffscreenDetailX()
    {
        RectTransform parent = (RectTransform)DetailPanel.parent;
        return parent.rect.width * 0.5f + DetailPanel.rect.width * 0.5f + 20f;
    }

    // 옵션/리더보드 창이 열려 있거나 움직이는 중
    bool AnyWindowOpen()
    {
        return OptionWindow.IsOpen || LeaderboardWindow.IsOpen;
    }

    // ---------- 잠긴 무한 모드 ----------

    // 잠긴 버튼을 누르면 좌우로 흔들린다 (점점 약해짐)
    IEnumerator ShakeLockedButton()
    {
        lockedShaking = true;
        RectTransform button = (RectTransform)InfinityModeBT;
        Vector2 home = button.anchoredPosition;
        float startTime = Time.realtimeSinceStartup;
        float t = 0f;
        while (t < 1f)
        {
            t = Progress(startTime, LockedShakeTime);
            float offset = Mathf.Sin(t * Mathf.PI * 2f * LockedShakeCount) * LockedShakeDistance * (1f - t);
            button.anchoredPosition = home + new Vector2(offset, 0f);
            yield return null;
        }
        button.anchoredPosition = home;
        lockedShaking = false;
    }

    IEnumerator PlayInfinityUnlock()
    {
        SetMenuInteractable(false);
        InfinityLockMark.gameObject.SetActive(true);

        Transform mark = InfinityLockMark.transform;
        Vector3 scale = mark.localScale;
        Quaternion rotation = mark.localRotation;

        yield return new WaitForSecondsRealtime(UnlockDelay);

        float startTime;
        float t;

        // 사슬 잠금이면 사슬이 한 줄씩 끊어지며 사라진 뒤 버튼이 통통 튄다
        ChainLock chainLock = InfinityLockMark.GetComponent<ChainLock>();
        if (chainLock != null)
        {
            yield return chainLock.PlayUnlock();
            yield return PunchButton();
            yield return new WaitForSecondsRealtime(UnlockHoldTime);
            SetMenuInteractable(true);
            yield break;
        }

        // 흔들림 (점점 약해짐)
        startTime = Time.realtimeSinceStartup;
        t = 0f;
        while (t < 1f)
        {
            t = Mathf.Clamp01((Time.realtimeSinceStartup - startTime) / UnlockShakeTime);
            float angle = Mathf.Sin(t * Mathf.PI * 2f * UnlockShakeCount) * UnlockShakeAngle * (1f - t);
            mark.localRotation = rotation * Quaternion.Euler(0f, 0f, angle);
            yield return null;
        }
        mark.localRotation = rotation;

        // 커지면서 사라짐
        startTime = Time.realtimeSinceStartup;
        t = 0f;
        while (t < 1f)
        {
            t = Mathf.Clamp01((Time.realtimeSinceStartup - startTime) / UnlockFadeTime);
            mark.localScale = scale * Mathf.Lerp(1f, UnlockFadeScale, t);
            InfinityLockMark.alpha = 1f - t;
            yield return null;
        }
        InfinityLockMark.gameObject.SetActive(false);
        mark.localScale = scale;
        InfinityLockMark.alpha = 1f;

        yield return PunchButton();
        yield return new WaitForSecondsRealtime(UnlockHoldTime);
        SetMenuInteractable(true);
    }

    // 버튼이 통통 튄다
    IEnumerator PunchButton()
    {
        Vector3 buttonScale = InfinityModeBT.localScale;
        float startTime = Time.realtimeSinceStartup;
        float t = 0f;
        while (t < 1f)
        {
            t = Mathf.Clamp01((Time.realtimeSinceStartup - startTime) / UnlockPunchTime);
            InfinityModeBT.localScale = buttonScale * Mathf.Lerp(1f, UnlockPunchScale, Mathf.Sin(t * Mathf.PI));
            yield return null;
        }
        InfinityModeBT.localScale = buttonScale;
    }

    // interactable을 끄면 버튼이 비활성 색으로 바뀌므로, 터치만 막는다 (키보드/패드 입력은 MobileInputSetup에서 항상 꺼 둔다)
    void SetMenuInteractable(bool interactable)
    {
        MenuGroup.blocksRaycasts = interactable;
    }

    public void PressStartBT()
    {
        OpenDetail(Mode.Adventure, AdventureBT);
    }
    public void PressInfinityModeBT()
    {
        // 잠겨 있으면 흔들리기만 한다
        if (!StageProgress.IsInfinityModeUnlocked())
        {
            if (!lockedShaking && !detailMoving && selectedMode == Mode.None)
                StartCoroutine(ShakeLockedButton());
            return;
        }
        OpenDetail(Mode.Infinity, (RectTransform)InfinityModeBT);
    }
    // 설명창의 들어가기
    public void PressEnterBT()
    {
        if (detailMoving)
            return;
        if (selectedMode == Mode.Adventure)
            SceneLoader.LoadScene(Title);
        else if (selectedMode == Mode.Infinity)
        {
            if (string.IsNullOrEmpty(InfinityMode))
                Debug.LogWarning("UIController: 무한 모드 씬 이름(InfinityMode)이 비어 있다");
            else
                SceneLoader.LoadScene(InfinityMode);
        }
    }
    // 설명창의 뒤로 / 어두운 배경 누르기
    public void PressDetailBackBT()
    {
        if (!detailMoving && selectedMode != Mode.None)
            StartCoroutine(CloseDetailRoutine());
    }
    public void PressSettingBT()
    {
        if (!AnyWindowOpen() && !detailMoving && selectedMode == Mode.None)
            OptionWindow.Open();
    }
    public void PressLeaderboardgBT()
    {
        if (!AnyWindowOpen() && !detailMoving && selectedMode == Mode.None)
            LeaderboardWindow.Open();
    }
    public void PressBackTitleBT_Leaderboard()
    {
        LeaderboardWindow.Close();
    }
}
