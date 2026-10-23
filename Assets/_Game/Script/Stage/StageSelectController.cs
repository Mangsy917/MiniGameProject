using TMPro;
using UnityEngine;

public class StageSelectController : MonoBehaviour
{
    public StageMapCamera MapCamera;
    public StageNode[] StageNodes;

    public GameObject MapUI;      // 맵을 둘러볼 때만 보이는 UI (타이틀, 뒤로가기)
    public GameObject DetailUI;   // 스테이지 확대 시 오른쪽에 나오는 설명 UI
    public TMP_Text StageNameTXT;
    public TMP_Text DescriptionTXT;

    [SerializeField] private string Title;

    // 카메라 orthographicSize가 이 값 이상이면 (맵이 넓게 보이면) 스테이지 이름을 숨기고 아이콘만 표시
    public float LabelHideSize = 6.5f;

    StageNode selectedStage;
    bool labelsVisible = true;

    void Start()
    {
        DetailUI.SetActive(false);
        MapUI.SetActive(true);
        MapCamera.OnStageClicked += SelectStage;

        // 아직 클리어하지 않은 첫 스테이지를 화면 가운데로
        foreach (StageNode stage in StageNodes)
        {
            if (!StageProgress.IsCleared(stage.StageNumber) || stage == StageNodes[StageNodes.Length - 1])
            {
                MapCamera.LookAt(stage.transform.position);
                break;
            }
        }
    }

    void Update()
    {
        bool visible = MapCamera.Size < LabelHideSize;
        if (visible == labelsVisible)
            return;

        labelsVisible = visible;
        foreach (StageNode stage in StageNodes)
            stage.SetLabelsVisible(visible);
    }

    void SelectStage(StageNode stage)
    {
        if (stage.IsLocked)
            return;

        selectedStage = stage;
        StageNameTXT.text = stage.StageName;
        DescriptionTXT.text = stage.Description;

        MapCamera.InputEnabled = false;
        MapUI.SetActive(false);
        MapCamera.ZoomIn(stage.transform.position, () => DetailUI.SetActive(true));
    }

    public void PressEnterBT()
    {
        if (selectedStage == null)
            return;

        SceneLoader.LoadScene(selectedStage.SceneName);
    }
    public void PressCloseBT_Detail()
    {
        DetailUI.SetActive(false);
        MapCamera.ZoomOut(() =>
        {
            selectedStage = null;
            MapUI.SetActive(true);
            MapCamera.InputEnabled = true;
        });
    }
    public void PressBackTitleBT()
    {
        SceneLoader.LoadScene(Title);
    }
}
