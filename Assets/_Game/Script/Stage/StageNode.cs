using UnityEngine;

// 스테이지 선택 맵에 배치되는 스테이지 오브젝트. 클릭 판정용 Collider2D가 필요하다.
[RequireComponent(typeof(Collider2D))]
public class StageNode : MonoBehaviour
{
    public int StageNumber;
    public string StageName;
    [TextArea(3, 10)] public string Description;
    public string SceneName;

    public GameObject LockedMark;

    // 맵을 넓게 볼 때 숨길 텍스트 (스테이지 이름 등)
    public GameObject[] Labels;

    public bool IsLocked
    {
        // 연구소 스테이지는 1~5 스테이지를 모두 클리어해야 열린다
        get { return StageNumber == StageProgress.LabStage && !StageProgress.IsLabStageUnlocked(); }
    }

    void Start()
    {
        RefreshLock();
    }

    public void RefreshLock()
    {
        if (LockedMark != null)
            LockedMark.SetActive(IsLocked);
    }

    public void SetLabelsVisible(bool visible)
    {
        foreach (GameObject label in Labels)
        {
            if (label != null)
                label.SetActive(visible);
        }
    }
}
