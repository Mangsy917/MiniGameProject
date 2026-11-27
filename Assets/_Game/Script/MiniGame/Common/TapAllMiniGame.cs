using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 공통 템플릿: 정답 탭 (DES_03)
// 랜덤 위치의 대상을 모두 탭하면 성공. 탭한 대상은 두 번째 상태로 바뀌고 다시 탭되지 않는다.
// 대상은 움직이는 방향을 바라보도록 돈다 (FaceMoveDirection).
// 잡힌 대상의 몸은 그 자리에 남아 서서히 흑백이 되고, 흑백 영혼이 몸에서 빠져나와 일렁이며 떠오르다가 사라진다.
// (CaughtMaterial이 비어 있으면 몸은 색이 그대로이고 영혼도 원래 색으로 나온다)
public class TapAllMiniGame : TouchMiniGame
{
    [Header("Tap All")]
    [SerializeField] int TargetCount = 5;
    [SerializeField] float MoveSpeed = 320f;
    [SerializeField] float Spacing = 140f;     // 처음 배치할 때 대상끼리 최소 간격
    [SerializeField] Image TargetIMG;          // 복제해서 쓰는 원본. 상태: 탭하기 전 / 탭한 뒤
    [SerializeField] int TargetSlot = 1;
    [SerializeField] bool FaceMoveDirection = true;   // 움직이는 방향으로 그림을 돌린다
    [SerializeField] float SpriteFacingAngle = -90f;  // 그림이 바라보는 방향 (오른쪽 0, 위 90, 아래 -90). 전갈 그림은 아래를 본다

    [Header("Caught")]                          // 잡힌 대상: 몸은 남아 흑백이 되고, 영혼이 떠오르며 사라진다
    [SerializeField] Material CaughtMaterial;   // 흑백 머티리얼 (UIGrayscale)
    [SerializeField] float BodyGrayTime = 0.5f; // 몸이 완전히 흑백이 되기까지
    [SerializeField, Range(0f, 1f)] float SoulAlpha = 0.6f;   // 영혼이 처음 보이는 진하기
    [SerializeField] float CaughtTime = 0.8f;   // 영혼이 사라지기까지
    [SerializeField] float CaughtRise = 80f;    // 떠오르는 높이
    [SerializeField] float CaughtWobble = 12f;  // 좌우로 일렁이는 폭
    [SerializeField] float CaughtWobbleSpeed = 3f;   // 초당 일렁이는 횟수

    class Target
    {
        public Image Image;
        public Vector2 Velocity;
        public bool Done;
    }

    readonly List<Target> targets = new List<Target>();
    int doneCount;

    protected override void Setup()
    {
        foreach (Vector2 pos in RandomPositions(TargetCount, InnerArea(TargetIMG.rectTransform), Spacing))
        {
            Image image = Spawn(TargetIMG, pos);
            Show(image, TargetSlot, 0);
            Target target = new Target { Image = image, Velocity = RandomDirection() };
            targets.Add(target);
            Face(target);
        }
    }

    protected override void OnBegin()
    {
        foreach (Target target in targets)
            target.Velocity *= MoveSpeed * SpeedScale;
    }

    protected override void OnTap(Vector2 pos)
    {
        // 겹쳐 있으면 한 번에 하나만
        foreach (Target target in targets)
        {
            if (target.Done || !Hit(target.Image, pos))
                continue;

            target.Done = true;
            doneCount++;
            Show(target.Image, TargetSlot, 1);
            StartCoroutine(TurnGray(target.Image));
            StartCoroutine(FloatAway(Clone(target.Image)));
            if (doneCount >= targets.Count)
                Finish(true);
            return;
        }
    }

    protected override void Tick(float deltaTime)
    {
        Rect area = InnerArea(TargetIMG.rectTransform);
        foreach (Target target in targets)
        {
            if (target.Done)
                continue;
            Bounce(target.Image.rectTransform, ref target.Velocity, area, deltaTime);
            Face(target);
        }
    }

    void Face(Target target)
    {
        if (!FaceMoveDirection || target.Velocity.sqrMagnitude < 0.0001f)
            return;
        float angle = Mathf.Atan2(target.Velocity.y, target.Velocity.x) * Mathf.Rad2Deg - SpriteFacingAngle;
        target.Image.rectTransform.localEulerAngles = new Vector3(0f, 0f, angle);
    }

    // 같은 자리, 같은 그림, 같은 각도의 복제본 (원본은 그대로 둔다)
    static Image Clone(Image source)
    {
        Image image = Instantiate(source, source.transform.parent);
        image.raycastTarget = false;
        return image;
    }

    // 죽은 몸 위에 흑백 그림을 겹쳐 점점 진하게 해서, 몸이 서서히 흑백이 되어 가게 한다
    IEnumerator TurnGray(Image body)
    {
        if (CaughtMaterial == null)
            yield break;

        Image gray = Clone(body);
        gray.material = CaughtMaterial;
        gray.transform.SetSiblingIndex(body.transform.GetSiblingIndex() + 1);
        float startTime = Time.unscaledTime;
        float t = 0f;
        while (t < 1f)
        {
            t = Mathf.Clamp01((Time.unscaledTime - startTime) / Mathf.Max(0.01f, BodyGrayTime));
            SetAlpha(gray, t);
            yield return null;
        }
    }

    // 몸에서 빠져나온 흑백 영혼이 좌우로 일렁이며 떠오르다가 사라진다
    IEnumerator FloatAway(Image image)
    {
        if (CaughtMaterial != null)
            image.material = CaughtMaterial;
        image.transform.SetAsLastSibling();   // 다른 대상 위로

        RectTransform rt = image.rectTransform;
        Vector2 start = rt.anchoredPosition;
        float baseAngle = rt.localEulerAngles.z;
        float startTime = Time.unscaledTime;
        float t = 0f;
        while (t < 1f)
        {
            t = Mathf.Clamp01((Time.unscaledTime - startTime) / Mathf.Max(0.01f, CaughtTime));
            float wave = Mathf.Sin(t * CaughtTime * CaughtWobbleSpeed * Mathf.PI * 2f);
            rt.anchoredPosition = start + new Vector2(wave * CaughtWobble, CaughtRise * (1f - (1f - t) * (1f - t)));
            rt.localEulerAngles = new Vector3(0f, 0f, baseAngle + wave * 8f);
            SetAlpha(image, SoulAlpha * (1f - t * t));
            yield return null;
        }
        image.gameObject.SetActive(false);
    }
}
