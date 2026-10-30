using System.Collections.Generic;
using UnityEngine;

// 스테이지 클리어 기록 저장/조회. 스테이지 번호는 1부터 시작한다.
// 빌드에서만 로컬(PlayerPrefs)에 저장한다. 에디터에서는 플레이 중에만 메모리에 기록하고,
// 플레이를 시작할 때마다 모든 스테이지가 클리어 안 된 상태로 돌아간다.
public static class StageProgress
{
    const string ClearKeyPrefix = "StageClear_";
    const string InfinityUnlockShownKey = "InfinityUnlockShown";

    public const int StageCount = 6;
    public const int LabStage = 6;  // 연구소 스테이지. 1~5 스테이지를 모두 클리어해야 열린다

    // 방금 클리어하고 돌아온 스테이지. 스테이지 선택 씬에서 클리어 연출을 보여준 뒤 0으로 되돌린다
    public static int JustClearedStage;

#if UNITY_EDITOR
    static readonly HashSet<string> editorFlags = new HashSet<string>();

    // Enter Play Mode 설정에서 도메인 리로드를 꺼도 플레이마다 초기화되도록
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetEditorProgress()
    {
        editorFlags.Clear();
        JustClearedStage = 0;
    }
#endif

    public static bool IsLabStageUnlocked()
    {
        return IsAllCleared(LabStage - 1);
    }

    // 무한 모드는 연구소 스테이지를 클리어하면 열린다
    public static bool IsInfinityModeUnlocked()
    {
        return IsCleared(LabStage);
    }

    public static bool IsCleared(int stage)
    {
        return GetFlag(ClearKeyPrefix + stage);
    }

    // 스테이지 클리어 시 호출
    public static void SetCleared(int stage)
    {
        SetFlag(ClearKeyPrefix + stage);
    }

    // 타이틀의 무한 모드 잠금 해제 연출을 이미 보여줬는지 (한 번만 재생)
    public static bool IsInfinityUnlockShown()
    {
        return GetFlag(InfinityUnlockShownKey);
    }

    public static void SetInfinityUnlockShown()
    {
        SetFlag(InfinityUnlockShownKey);
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
#if UNITY_EDITOR
        editorFlags.Clear();
#else
        for (int stage = 1; stage <= stageCount; stage++)
            PlayerPrefs.DeleteKey(ClearKeyPrefix + stage);
        PlayerPrefs.DeleteKey(InfinityUnlockShownKey);
        PlayerPrefs.Save();
#endif
    }

    static bool GetFlag(string key)
    {
#if UNITY_EDITOR
        return editorFlags.Contains(key);
#else
        return PlayerPrefs.GetInt(key, 0) == 1;
#endif
    }

    static void SetFlag(string key)
    {
#if UNITY_EDITOR
        editorFlags.Add(key);
#else
        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();
#endif
    }
}
