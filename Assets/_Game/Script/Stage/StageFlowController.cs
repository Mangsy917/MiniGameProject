using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 스테이지 씬의 진행을 담당한다.
// 카운트다운 -> 대기 -> 랜덤 미니게임 -> 결과 -> 대기 -> ... -> FinalBossRound를 마치면 클리어
// 라운드는 1부터 시작한다. 지금은 보스 없이 모든 라운드에 일반 미니게임이 나온다 (UseBossGames = false).
// MidBossRound가 끝나면 스피드업 연출 후 제한 시간이 줄어든다.
// UseBossGames를 켜면 MidBossRound/FinalBossRound에 보스 미니게임이 고정으로 나오고, 최종보스는 성공해야 클리어된다.
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
    public bool UseBossGames = false; // 보스 라운드에 보스 미니게임을 낼지. 끄면 그 라운드도 일반 미니게임
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

    // 캐릭터 얼굴 (화면 좌/우에 하나씩). 스프라이트가 비어 있으면 기존 스프라이트를 그대로 둔다
    public Image[] CharacterIMGs;
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

    // 스테이지별 Menu 일러스트 (화면 전체 크기 그림 한 장). 지정하면 DoorSprite 대신
    // 왼쪽 문에 그림의 왼쪽 절반, 오른쪽 문에 오른쪽 절반을 보여줘서 닫혔을 때 한 장의 그림이 되고 가운데서 갈라지며 열린다.
    // 화면 비율이 그림과 다르면 비율을 유지한 채 넘치는 부분을 잘라낸다
    public Sprite MenuSprite;

    // 문 그림(Menu 그림 또는 문 스프라이트)은 문이 열리는 동시에 이 배율까지 확대되고, 닫히는 동시에 1배로 축소된다
    // 문과 그림 모두 점점 빨라지며, 다 닫히는 순간 쾅 부딪힌 반동으로 그림이 튀고 흔들린다 (문 가운데 이음새 기준)
    public float MenuZoomScale = 1.8f;
    public float DoorImpactScale = 1.12f;  // 문이 닫히며 부딪히는 순간 그림이 튀어 오르는 배율
    public float DoorImpactTime = 0.3f;    // 반동이 가라앉는 시간
    public float DoorShake = 12f;          // 반동 때 위아래로 흔들리는 폭

    RawImage leftMenuIMG;     // Menu 그림이 있을 때만 (uv 계산용)
    RawImage rightMenuIMG;
    RectTransform leftDoorContent;    // 문 안을 채우는 그림. 확대/축소 대상
    RectTransform rightDoorContent;

    [Header("Play UI")]
    public GameObject TimerUI;
    public Image TimerFillIMG;
    public TMP_Text TimerTXT;   // 남은 시간 (초, 올림)

    [Header("Boss Intro UI")]
    public bool ShowBossIntro = false;     // 보스 라운드 전 "MID BOSS!" / "FINAL BOSS!" 연출을 보여 줄지 (현재 사용 안 함)
    public GameObject BossIntroUI;
    public TMP_Text BossIntroTXT;
    public string MidBossMessage = "중간 보스!";
    public string FinalBossMessage = "최종 보스!";
    public float PopScale = 1.6f;
    public float PopTime = 0.25f;

    [Header("Speed Up UI")]                 // 중간보스 라운드가 끝난 뒤 (성공/실패 모두)
    public GameObject SpeedUpUI;
    public RectTransform SpeedUpBand;      // 글자를 담은 띠 (배경은 투명). 아래에서 올라와 가운데 멈췄다가 위로 빠져나간다
    public TMP_Text SpeedUpTXT;
    public string SpeedUpMessage = "스피드 업!";
    public float SpeedUpTime = 2f;         // 연출 전체 시간
    public float SpeedUpMoveTime = 0.35f;  // 아래에서 가운데까지 올라오는 시간 (위로 나갈 때도 같은 시간)

    [Header("End UI")]
    public GameObject ClearUI;
    public GameObject GameOverUI;

    public int Round { get; private set; }
    public int Life { get; private set; }

    int lastGameIndex = -1;
    readonly System.Collections.Generic.List<int> gameBag = new System.Collections.Generic.List<int>();

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

        // 게임 시작: 열려 있던 문이 좌우에서 닫히고 대기 화면이 나온다
        SetDoors(1f);
        DoorUI.SetActive(true);
        UpdateMenuUV();
        yield return MoveDoors(1f, 0f);
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

            // 마지막 라운드를 마치면 클리어 (보스를 쓸 때는 최종보스를 이겨야 클리어)
            if (Round == FinalBossRound && (success || !UseBossGames))
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

            if (UseBossGames && ShowBossIntro && IsBossRound(Round))
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
        bool opening = to > from;
        float startTime = Time.realtimeSinceStartup;
        float t = 0f;
        while (t < 1f)
        {
            t = DoorTime > 0f ? Mathf.Clamp01((Time.realtimeSinceStartup - startTime) / DoorTime) : 1f;

            // 문과 그림이 함께 움직이고 둘 다 점점 빨라진다 (닫힐 때 더 세게 가속해서 쾅 닫히는 느낌)
            // 열릴 때: 문이 열리면서 동시에 확대 / 닫힐 때: 문이 닫히면서 동시에 축소
            float eased = opening ? t * t : t * t * t;
            float open = Mathf.Lerp(from, to, eased);
            SetDoors(open, Mathf.Lerp(1f, MenuZoomScale, open), 0f);
            yield return null;
        }

        if (!opening && to <= 0f)
            yield return DoorImpact();
    }

    // 문이 닫히며 쾅 부딪힌 반동: 그림이 순간 커졌다가 가라앉고, 위아래로 흔들린다
    IEnumerator DoorImpact()
    {
        float startTime = Time.realtimeSinceStartup;
        float t = 0f;
        while (t < 1f)
        {
            t = DoorImpactTime > 0f ? Mathf.Clamp01((Time.realtimeSinceStartup - startTime) / DoorImpactTime) : 1f;
            float decay = (1f - t) * (1f - t) * (1f - t);
            float zoom = 1f + (DoorImpactScale - 1f) * decay;
            // 그림이 커진 만큼 위아래로 넘쳐 있어서 흔들려도 빈틈이 보이지 않는다
            float shake = Mathf.Sin(t * Mathf.PI * 6f) * DoorShake * decay;
            SetDoors(0f, zoom, shake);
            yield return null;
        }
        SetDoors(0f, 1f, 0f);
    }

    void ApplyDoorSprite()
    {
        if (MenuSprite != null)
        {
            leftMenuIMG = CreateMenuHalf(LeftDoor);
            rightMenuIMG = CreateMenuHalf(RightDoor);
            leftDoorContent = leftMenuIMG.rectTransform;
            rightDoorContent = rightMenuIMG.rectTransform;
            return;
        }

        // Menu 그림이 없는 스테이지: 문 스프라이트(없으면 임시 문)를 같은 방식으로 확대/축소한다
        foreach (RectTransform door in new[] { LeftDoor, RightDoor })
        {
            Image image = door.GetComponent<Image>();
            if (DoorSprite != null)
            {
                image.sprite = DoorSprite;
                image.color = Color.white;   // 임시 문 색을 지우고 스프라이트 원래 색으로
            }
        }
        leftDoorContent = CreateDoorContent(LeftDoor);
        rightDoorContent = CreateDoorContent(RightDoor);
    }

    // 문 안을 꽉 채우는 그림 오브젝트. 확대/축소 기준점은 두 문이 맞닿는 이음새(화면 가운데 쪽 모서리).
    // 오른쪽 문은 scale x -1로 뒤집혀 있어서 문 기준 오른쪽 모서리가 화면 가운데 쪽이므로 양쪽 모두 pivot x=1
    static RectTransform CreateContent(RectTransform door, string name, System.Type graphicType)
    {
        door.GetComponent<Image>().enabled = false;   // 문 자체 Image는 숨기고 그림 오브젝트로 대신 보여준다

        GameObject go = new GameObject(name, typeof(RectTransform), graphicType);
        RectTransform rt = (RectTransform)go.transform;
        rt.SetParent(door, false);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(1f, 0.5f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return rt;
    }

    // Menu 그림 조각 (오른쪽 문 그림의 좌우 반전은 UpdateMenuUV에서 uv로 되돌린다)
    RawImage CreateMenuHalf(RectTransform door)
    {
        RawImage image = CreateContent(door, "MenuHalf", typeof(RawImage)).GetComponent<RawImage>();
        image.texture = MenuSprite.texture;
        return image;
    }

    // 문 Image를 그대로 옮긴 그림 (문 스프라이트는 원래처럼 오른쪽 문에서 좌우 반전되어 보인다)
    static RectTransform CreateDoorContent(RectTransform door)
    {
        Image source = door.GetComponent<Image>();
        RectTransform rt = CreateContent(door, "DoorContent", typeof(Image));
        Image image = rt.GetComponent<Image>();
        image.sprite = source.sprite;
        image.color = source.color;
        image.material = source.material;
        image.type = source.type;
        image.preserveAspect = source.preserveAspect;
        image.raycastTarget = source.raycastTarget;
        return rt;
    }

    // 화면(문 두 짝을 합친 영역) 비율에 맞춰 그림을 가운데 기준으로 잘라 좌우 절반씩 나눠준다
    void UpdateMenuUV()
    {
        if (leftMenuIMG == null || rightMenuIMG == null)
            return;

        Texture tex = MenuSprite.texture;
        Rect r = MenuSprite.textureRect;   // 스프라이트가 텍스처 일부만 쓰는 경우까지 고려
        Rect uv = new Rect(r.x / tex.width, r.y / tex.height, r.width / tex.width, r.height / tex.height);

        float screenAspect = LeftDoor.rect.width * 2f / Mathf.Max(LeftDoor.rect.height, 1f);
        float imageAspect = r.width / r.height;
        if (screenAspect > imageAspect)
        {
            // 화면이 더 넓다: 위아래를 잘라낸다
            float h = uv.height * imageAspect / screenAspect;
            uv.y += (uv.height - h) * 0.5f;
            uv.height = h;
        }
        else
        {
            // 화면이 더 좁다: 좌우를 잘라낸다
            float w = uv.width * screenAspect / imageAspect;
            uv.x += (uv.width - w) * 0.5f;
            uv.width = w;
        }

        float half = uv.width * 0.5f;
        leftMenuIMG.uvRect = new Rect(uv.x, uv.y, half, uv.height);
        // 오른쪽 문은 뒤집혀 있으므로 uv도 좌우 반전 (그림 오른쪽 끝 -> 가운데 순서)
        rightMenuIMG.uvRect = new Rect(uv.x + uv.width, uv.y, -half, uv.height);
    }

    // open 0: 가운데에서 닫힘, 1: 양옆으로 화면 밖까지 열림
    void SetDoors(float open)
    {
        // 열릴수록 그림이 확대되고, 닫힐수록 원래 크기로 축소된다
        SetDoors(open, Mathf.Lerp(1f, MenuZoomScale, open), 0f);
    }

    void SetDoors(float open, float zoom, float offsetY)
    {
        LeftDoor.anchoredPosition = new Vector2(-LeftDoor.rect.width * open, offsetY);
        RightDoor.anchoredPosition = new Vector2(RightDoor.rect.width * open, offsetY);

        Vector3 scale = new Vector3(zoom, zoom, 1f);
        if (leftDoorContent != null)
            leftDoorContent.localScale = scale;
        if (rightDoorContent != null)
            rightDoorContent.localScale = scale;
    }

    MiniGame PickGame()
    {
        if (UseBossGames && Round == MidBossRound && MidBossGame != null)
            return MidBossGame;
        if (UseBossGames && Round == FinalBossRound && FinalBossGame != null)
            return FinalBossGame;

        // 섞은 주머니에서 하나씩 꺼낸다: 등록된 미니게임이 한 바퀴에 한 번씩 골고루 나온다
        if (gameBag.Count == 0)
            RefillGameBag();

        int index = gameBag[gameBag.Count - 1];
        gameBag.RemoveAt(gameBag.Count - 1);
        lastGameIndex = index;
        return NormalGames[index];
    }

    // 미니게임 번호를 모두 넣고 섞는다. 새 바퀴의 첫 게임이 직전 게임과 같으면 다른 자리와 바꿔 연속으로 나오지 않게 한다
    void RefillGameBag()
    {
        gameBag.Clear();
        for (int i = 0; i < NormalGames.Length; i++)
            gameBag.Add(i);

        for (int i = gameBag.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (gameBag[i], gameBag[j]) = (gameBag[j], gameBag[i]);
        }

        // 맨 뒤가 다음에 꺼낼 게임
        int last = gameBag.Count - 1;
        if (gameBag.Count > 1 && gameBag[last] == lastGameIndex)
            (gameBag[last], gameBag[0]) = (gameBag[0], gameBag[last]);
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
        SetWaitingWobble(true);
        RefreshLife();
    }

    void ShowResult(bool success)
    {
        ResultTXT.gameObject.SetActive(true);
        ResultTXT.text = success ? "성공!" : "실패...";
        SetCharacter(success ? CharacterSuccessSprite : CharacterFailSprite);
        // 실패하면 대기 화면의 그림(캐릭터, 체력 등)이 흔들리지 않고 멈춰 있다. 다음 대기 화면에서 다시 흔들린다
        SetWaitingWobble(success);
        RefreshLife();
    }

    void SetCharacter(Sprite sprite)
    {
        if (sprite == null || CharacterIMGs == null)
            return;

        foreach (Image image in CharacterIMGs)
        {
            if (image != null)
                image.sprite = sprite;
        }
    }

    // 대기 화면 안의 흔들림(IconWobble)을 모두 켜거나 끈다. 끄면 원래 각도로 돌아간다
    void SetWaitingWobble(bool wobble)
    {
        foreach (IconWobble iconWobble in WaitingUI.GetComponentsInChildren<IconWobble>(true))
            iconWobble.enabled = wobble;
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

    // 스피드 업 글자가 화면 아래에서 올라와 가운데 잠시 멈췄다가, 계속 위로 올라가 빠져나간다
    IEnumerator SpeedUp()
    {
        WaitingUI.SetActive(false);
        SpeedUpTXT.text = SpeedUpMessage;
        SpeedUpUI.SetActive(true);

        float offY = ((RectTransform)SpeedUpUI.transform).rect.height * 0.5f + SpeedUpBand.rect.height * 0.5f;
        yield return MoveBandY(-offY, 0f, true);
        yield return new WaitForSecondsRealtime(Mathf.Max(0f, SpeedUpTime - SpeedUpMoveTime * 2f));
        yield return MoveBandY(0f, offY, false);

        SpeedUpUI.SetActive(false);
        SpeedUpBand.anchoredPosition = Vector2.zero;
        WaitingUI.SetActive(true);
    }

    // easeOut: 들어올 때 점점 느려지며 멈춤 / false: 나갈 때 점점 빨라짐
    IEnumerator MoveBandY(float fromY, float toY, bool easeOut)
    {
        float startTime = Time.realtimeSinceStartup;
        float t = 0f;
        while (t < 1f)
        {
            t = SpeedUpMoveTime > 0f ? Mathf.Clamp01((Time.realtimeSinceStartup - startTime) / SpeedUpMoveTime) : 1f;
            float e = easeOut ? 1f - (1f - t) * (1f - t) * (1f - t) : t * t * t;
            SpeedUpBand.anchoredPosition = new Vector2(0f, Mathf.Lerp(fromY, toY, e));
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
