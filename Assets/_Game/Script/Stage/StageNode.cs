using UnityEngine;

// 스테이지 선택 맵에 배치되는 스테이지 오브젝트. 클릭 판정용 Collider2D가 필요하다.
[RequireComponent(typeof(Collider2D))]
public class StageNode : MonoBehaviour
{
    // 개발용: 인스펙터에서 잠김 상태를 강제로 지정 (에디터에서만 적용, 빌드에서는 무시된다)
    public enum DebugLockMode
    {
        Default,    // 진행도에 따름
        Locked,     // 강제 잠금
        Unlocked,   // 강제 해제
    }

    public int StageNumber;
    public string StageName;
    [TextArea(3, 10)] public string Description;
    public string SceneName;

    public GameObject LockedMark;
    public GameObject ClearedMark;   // 클리어한 스테이지에 표시

    // 맵을 넓게 볼 때 숨길 텍스트 (스테이지 이름 등)
    public GameObject[] Labels;

    [Header("Icon")]
    public SpriteRenderer IconSR;
    public Material LockedMaterial;  // 잠겼을 때 아이콘을 흑백으로 표시할 머티리얼

    [Header("Dev Only (Editor)")]
    [Tooltip("게임 제작용. 에디터에서만 적용되고 빌드에서는 무시된다.")]
    public DebugLockMode DebugLock = DebugLockMode.Default;

    public bool IsLocked
    {
        get
        {
#if UNITY_EDITOR
            if (DebugLock != DebugLockMode.Default)
                return DebugLock == DebugLockMode.Locked;
#endif
            // 연구소 스테이지는 1~5 스테이지를 모두 클리어해야 열린다
            return StageNumber == StageProgress.LabStage && !StageProgress.IsLabStageUnlocked();
        }
    }

    IconPop iconPop;
    IconWobble iconWobble;
    Material normalMaterial;
    bool lockedVisual;

    void Awake()
    {
        CacheIcon();
    }

    void Start()
    {
        RefreshLock();
    }

    void CacheIcon()
    {
        if (IconSR == null)
            return;
        if (iconPop == null)
            iconPop = IconSR.GetComponent<IconPop>();
        if (iconWobble == null)
            iconWobble = IconSR.GetComponent<IconWobble>();
        if (normalMaterial == null && IconSR.sharedMaterial != LockedMaterial)
            normalMaterial = IconSR.sharedMaterial;
    }

    // 마우스를 올렸을 때 아이콘 확대 (잠긴 스테이지는 반응하지 않음)
    public void SetHovered(bool hovered)
    {
        if (iconPop != null && !lockedVisual)
            iconPop.SetHovered(hovered);
    }

    // 클릭/터치한 순간 아이콘이 톡 커진다 (잠긴 스테이지는 반응하지 않음)
    public void PressIcon()
    {
        if (iconPop != null && !lockedVisual)
            iconPop.Press();
    }

    public void RefreshLock()
    {
        bool locked = IsLocked;
        if (LockedMark != null)
            LockedMark.SetActive(locked);
        if (ClearedMark != null)
            ClearedMark.SetActive(StageProgress.IsCleared(StageNumber));
        SetLockedVisual(locked);
    }

    // 잠긴 모습: 흑백 아이콘, 흔들림/확대 없음
    // (연구소 잠금 해제 연출처럼 실제 잠김 상태와 별개로 보여줘야 할 때도 사용)
    public void SetLockedVisual(bool locked)
    {
        CacheIcon();
        lockedVisual = locked;

        if (IconSR != null)
        {
            Material material = locked ? LockedMaterial : normalMaterial;
            if (material != null)
                IconSR.sharedMaterial = material;
        }

        // 컴포넌트를 끄면 OnDisable에서 원래 각도/크기로 돌아간다
        if (iconWobble != null)
            iconWobble.enabled = !locked;
        if (iconPop != null)
            iconPop.enabled = !locked;
    }

    public void SetLabelsVisible(bool visible)
    {
        foreach (GameObject label in Labels)
        {
            if (label != null)
                label.SetActive(visible);
        }
    }

#if UNITY_EDITOR
    // 플레이 중 인스펙터에서 DebugLock을 바꾸면 바로 반영 (OnValidate 안에서는 SetActive를 못 써서 한 프레임 미룬다)
    // 편집 모드에서는 씬 데이터를 건드리지 않고, 플레이를 시작할 때 적용된다
    void OnValidate()
    {
        if (!Application.isPlaying)
            return;

        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (this != null && Application.isPlaying)
                RefreshLock();
        };
    }
#endif
}
