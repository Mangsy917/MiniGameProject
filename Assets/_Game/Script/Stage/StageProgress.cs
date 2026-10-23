using UnityEngine;

// 스테이지 클리어 기록 저장/조회. 스테이지 번호는 1부터 시작한다.
public static class StageProgress
{
    const string ClearKeyPrefix = "StageClear_";

    public const int StageCount = 6;
    public const int LabStage = 6;  // 연구소 스테이지. 1~5 스테이지를 모두 클리어해야 열린다

    public static bool IsLabStageUnlocked()
    {
        return IsAllCleared(LabStage - 1);
    }

    public static bool IsCleared(int stage)
    {
        return PlayerPrefs.GetInt(ClearKeyPrefix + stage, 0) == 1;
    }

    // 스테이지 클리어 시 호출
    public static void SetCleared(int stage)
    {
        PlayerPrefs.SetInt(ClearKeyPrefix + stage, 1);
        PlayerPrefs.Save();
    }

    public static bool IsAllCleared(int stageCount)
    {
        if (stageCount <= 0)
            return false;

        for (int stage = 1; stage <= stageCount; stage++)
        {
            if (!IsCleared(stage))
                return false;
        }
        return true;
    }

    public static void ResetAll(int stageCount)
    {
        for (int stage = 1; stage <= stageCount; stage++)
            PlayerPrefs.DeleteKey(ClearKeyPrefix + stage);
        PlayerPrefs.Save();
    }
}
