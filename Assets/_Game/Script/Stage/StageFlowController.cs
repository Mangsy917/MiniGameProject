using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 스테이지 씬의 진행을 담당한다.
// 카운트다운 -> 대기 -> 랜덤 미니게임 -> 결과 -> (보스 등장) -> 대기 -> ... -> 최종보스 클리어
// 라운드는 1부터 시작하며 MidBossRound에 중간보스, FinalBossRound에 최종보스 미니게임이 고정으로 나온다.
public class StageFlowController : MonoBehaviour
{
    public int StageNumber;               // StageNode.StageNumber와 같은 값
    [SerializeField] private string SelectStage = "SelectStage";

    public StageCountdown Countdown;

    [Header("Round")]
    public int MidBossRound = 10;
    public int FinalBossRound = 20;

    [Header("Time")]
    public float TimeLimit = 5f;                // 미니게임 기본 제한 시간
    public float TimeLimitAfterMidBoss = 4f;    // 중간보스 클리어 후 제한 시간
    public float WaitTime = 2f;                 // 미니게임 시작 전 대기 화면
    public float ResultTime = 2f;               // 결과 확인 화면
    public float BossIntroTime = 2f;            // 보스 등장 연출

    [Header("Life")]
    public int MaxLife = 4;
    public int StartLife = 4;

    [Header("MiniGame")]
    public MiniGame[] NormalGames;   // 일반 라운드에서 랜덤으로 나오는 미니게임 프리팹
    public MiniGame MidBossGame;
    public MiniGame FinalBossGame;
    public Transform MiniGameRoot;   // 미니게임 프리팹이 생성될 위치

    [Header("Waiting UI")]
    public GameObject WaitingUI;
    public TMP_Text RoundTXT;
    public TMP_Text ResultTXT;
    public Image[] LifeIMGs;
    public Color LifeOnColor = new Color(0.95f, 0.25f, 0.3f);
    public Color LifeOffColor = new Color(0.3f, 0.3f, 0.3f, 0.6f);

    // 캐릭터 스프라이트. 비어 있으면 기존 스프라이트를 그대로 둔다
    public Image CharacterIMG;
    public Sprite CharacterIdleSprite;
    public Sprite CharacterSuccessSprite;
    public Sprite CharacterFailSprite;

    [Header("Door")]
    // 대기 화면 뒤를 가리는 좌우 문. 미니게임 시작 전에 가운데에서 양옆으로 열리고, 끝나면 닫힌다
    public GameObject DoorUI;
    public RectTransform LeftDoor;
    public RectTransform RightDoor;
    public float DoorTime = 0.5f;

    // 왼쪽 문 기준으로 그린 문 스프라이트 하나를 양쪽에 쓴다. 오른쪽 문은 좌우 반전(scale x -1)되어 있다
    // 비어 있으면 문 Image에 들어 있는 임시 스프라이트를 그대로 쓴다
    public Sprite DoorSprite;

    [Header("Play UI")]
    public GameObject TimerUI;
    public Image TimerFillIMG;
    public TMP_Text TimerTXT;   // 남은 시간 (초, 올림)

    [Header("Boss Intro UI")]
    public GameObject BossIntroUI;
    public TMP_Text BossIntroTXT;
    public string MidBossMessage = "MID BOSS!";
    public string FinalBossMessage = "FINAL BOSS!";
    public float PopScale = 1.6f;
    public float PopTime = 0.25f;

    [Header("Speed Up UI")]                 // 중간보스 라운드가 끝난 뒤 (성공/실패 모두)
    public GameObject SpeedUpUI;
    public RectTransform SpeedUpBand;      // 화면을 가로지르는 띠. 들어오고 나갈 때 좌우로 미끄러진다
    public TMP_Text SpeedUpTXT;
    public string SpeedUpMessage = "SPEED UP!";
    public float SpeedUpTime = 2f;         // 연출 전체 시간
    public float SpeedUpSlideTime = 0.2f;  // 띠가 들어오고 나가는 시간

    [Header("End UI")]
    public GameObject ClearUI;
    public GameObject GameOverUI;

    public int Round { get; private set; }
    public int Life { get; private set; }

    int lastGameIndex = -1;

    void Start()
    {
        WaitingUI.SetActive(false);
        TimerUI.SetActive(false);
        BossIntroUI.SetActive(false);
        SpeedUpUI.SetActive(false);
        ClearUI.SetActive(false);
        GameOverUI.SetActive(false);
        DoorUI.SetActive(false);
        ApplyDoorSprite();

        Life = Mathf.Clamp(StartLife, 1, MaxLife);
        StartCoroutine(Flow());
    }

    IEnumerator Flow()
    {
        if (Countdown != null)
            yield return new WaitUntil(() => Countdown.IsEnded);

        SetDoors(0f);
        DoorUI.SetActive(true);
        WaitingUI.SetActive(true);
        Round = 1;

        while (true)
        {
            // 대기 화면: 라운드 숫자와 체력
            ShowWaiting();
            yield return new WaitForSecondsRealtime(WaitTime);

            // 문이 열리며 미니게임 시작, 끝나면 문이 닫힌다
            MiniGame game = Instantiate(PickGame(), MiniGameRoot);
            WaitingUI.SetActive(false);
            yield return MoveDoors(0f, 1f);
            yield return PlayGame(game);
            yield return MoveDoors(1f, 0f);
            bool success = game.IsSuccess;
            Destroy(game.gameObject);

            if (!success)
                Life--;

            // 결과 화면
            WaitingUI.SetActive(true);
            ShowResult(success);
            yield return new WaitForSecondsRealtime(ResultTime);

            if (Life <= 0)
            {
                GameOverUI.SetActive(true);
                yield break;
            }

            if (success && Round == FinalBossRound)
            {
                // 클리어 연출은 처음 클리어했을 때만 보여준다
                if (!StageProgress.IsCleared(StageNumber))
                    StageProgress.JustClearedStage = StageNumber;
                StageProgress.SetCleared(StageNumber);
                ClearUI.SetActive(true);
                yield break;
            }

            // 최종보스만 실패하면 다시 도전한다. 중간보스는 성공/실패와 관계없이 다음 라운드로
            bool passedMidBoss = Round == MidBossRound;
            if (success || Round != FinalBossRound)
                Round++;

            // 중간보스 이후로는 제한 시간이 줄어든다
            if (passedMidBoss)
                yield return SpeedUp();

            if (IsBossRound(Round))
                yield return BossIntro();
        }
    }

    IEnumerator PlayGame(MiniGame game)
    {
        float timeLimit = Round > MidBossRound ? TimeLimitAfterMidBoss : TimeLimit;

        TimerUI.SetActive(true);
        game.Begin(timeLimit);

        float startTime = Time.realtimeSinceStartup;
        while (!game.IsFinished)
        {
            float remain = timeLimit - (Time.realtimeSinceStartup - startTime);
            if (remain <= 0f)
            {
                game.TimeUp();
                break;
            }

            TimerFillIMG.fillAmount = remain / timeLimit;
            TimerTXT.text = Mathf.CeilToInt(remain).ToString();
            yield return null;
        }

        TimerUI.SetActive(false);
    }

    IEnumerator MoveDoors(float from, float to)
    {
        float startTime = Time.realtimeSinceStartup;
        float t = 0f;
        while (t < 1f)
        {
            t = DoorTime > 0f ? Mathf.Clamp01((Time.realtimeSinceStartup - startTime) / DoorTime) : 1f;
            SetDoors(Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t)));
            yield return null;
        }
    }

    void ApplyDoorSprite()
    {
        if (DoorSprite == null)
            return;

        foreach (RectTransform door in new[] { LeftDoor, RightDoor })
        {
            Image image = door.GetComponent<Image>();
            image.sprite = DoorSprite;
            image.color = Color.white;   // 임시 문 색을 지우고 스프라이트 원래 색으로
        }
    }

    // open 0: 가운데에서 닫힘, 1: 양옆으로 화면 밖까지 열림
    void SetDoors(float open)
    {
        LeftDoor.anchoredPosition = new Vector2(-LeftDoor.rect.width * open, 0f);
        RightDoor.anchoredPosition = new Vector2(RightDoor.rect.width * open, 0f);
    }

    MiniGame PickGame()
    {
        if (Round == MidBossRound && MidBossGame != null)
            return MidBossGame;
        if (Round == FinalBossRound && FinalBossGame != null)
            return FinalBossGame;

        // 같은 미니게임이 연속으로 나오지 않도록
        int index = Random.Range(0, NormalGames.Length);
        if (NormalGames.Length > 1 && index == lastGameIndex)
            index = (index + Random.Range(1, NormalGames.Length)) % NormalGames.Length;

        lastGameIndex = index;
        return NormalGames[index];
    }

    bool IsBossRound(int round)
    {
        return round == MidBossRound || round == FinalBossRound;
    }

    void ShowWaiting()
    {
        RoundTXT.text = Round.ToString();
        ResultTXT.gameObject.SetActive(false);
        SetCharacter(CharacterIdleSprite);
        RefreshLife();
    }

    void ShowResult(bool success)
    {
        ResultTXT.gameObject.SetActive(true);
        ResultTXT.text = success ? "SUCCESS!" : "FAIL...";
        SetCharacter(success ? CharacterSuccessSprite : CharacterFailSprite);
        RefreshLife();
    }

    void SetCharacter(Sprite sprite)
    {
        if (CharacterIMG != null && sprite != null)
            CharacterIMG.sprite = sprite;
    }

    void RefreshLife()
    {
        for (int i = 0; i < LifeIMGs.Length; i++)
        {
            LifeIMGs[i].gameObject.SetActive(i < MaxLife);
            LifeIMGs[i].color = i < Life ? LifeOnColor : LifeOffColor;
        }
    }

    IEnumerator BossIntro()
    {
        WaitingUI.SetActive(false);
        BossIntroTXT.text = Round == FinalBossRound ? FinalBossMessage : MidBossMessage;
        BossIntroUI.SetActive(true);

        yield return PopText(BossIntroTXT.transform, BossIntroTime);

        BossIntroUI.SetActive(false);
        WaitingUI.SetActive(true);
    }

    // 띠가 오른쪽에서 들어와 SPEED UP! 이 튀어나오고, 끝날 때 왼쪽으로 빠져나간다
    IEnumerator SpeedUp()
    {
        WaitingUI.SetActive(false);
        SpeedUpTXT.text = SpeedUpMessage;
        SpeedUpUI.SetActive(true);

        float width = ((RectTransform)SpeedUpUI.transform).rect.width;
        yield return SlideBand(width, 0f);
        yield return PopText(SpeedUpTXT.transform, Mathf.Max(0f, SpeedUpTime - SpeedUpSlideTime * 2f));
        yield return SlideBand(0f, -width);

        SpeedUpUI.SetActive(false);
        SpeedUpBand.anchoredPosition = Vector2.zero;
        WaitingUI.SetActive(true);
    }

    IEnumerator SlideBand(float fromX, float toX)
    {
        float startTime = Time.realtimeSinceStartup;
        float t = 0f;
        while (t < 1f)
        {
            t = SpeedUpSlideTime > 0f ? Mathf.Clamp01((Time.realtimeSinceStartup - startTime) / SpeedUpSlideTime) : 1f;
            SpeedUpBand.anchoredPosition = new Vector2(Mathf.Lerp(fromX, toX, t), 0f);
            yield return null;
        }
    }

    // duration 동안 표시하며, 텍스트가 커졌다가 원래 크기로 줄어드는 연출
    IEnumerator PopText(Transform target, float duration)
    {
        float startTime = Time.realtimeSinceStartup;
        float timer = 0f;
        while (timer < duration)
        {
            timer = Time.realtimeSinceStartup - startTime;
            float t = Mathf.Clamp01(timer / PopTime);
            target.localScale = Vector3.one * Mathf.Lerp(PopScale, 1f, 1f - (1f - t) * (1f - t));
            yield return null;
        }
        target.localScale = Vector3.one;
    }

    public void PressConfirmBT_Clear()
    {
        // 클리어 시점에 이미 저장했지만, 확인을 누를 때도 한 번 더 기록해 둔다
        StageProgress.SetCleared(StageNumber);
        SceneLoader.LoadScene(SelectStage);
    }
    public void PressConfirmBT_GameOver()
    {
        SceneLoader.LoadScene(SelectStage);
    }
}
