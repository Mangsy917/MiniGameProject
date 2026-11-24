using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

// 메뉴 MiniGame > 스프라이트 자동 연결
// Assets/_Game/Sprite/{Theme}/{ID}/ 에 넣은 그림을 파일 이름({ID}_{슬롯}.png, {ID}_{슬롯}_{상태}.png)에 맞춰
// 미니게임 프리팹의 에셋 자리(Slots)에 넣는다. 그림이 없는 자리는 건드리지 않는다.
public static class MiniGameSpriteLinker
{
    const string PrefabRoot = "Assets/_Game/Prefabs/MiniGame";
    const string SpriteRoot = "Assets/_Game/Sprite";

    [MenuItem("MiniGame/스프라이트 자동 연결")]
    static void LinkAll()
    {
        int linked = 0;
        foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { PrefabRoot }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TouchMiniGame game = AssetDatabase.LoadAssetAtPath<TouchMiniGame>(path);
            if (game == null)
                continue;

            // 예: .../Desert/MiniGame_DES_03.prefab -> Desert, DES_03
            string theme = Path.GetFileName(Path.GetDirectoryName(path));
            string id = Path.GetFileNameWithoutExtension(path).Replace("MiniGame_", "");
            linked += Link(game, SpriteRoot + "/" + theme + "/" + id + "/");
        }
        AssetDatabase.SaveAssets();
        Debug.Log("스프라이트 자동 연결: " + linked + "개 연결");
    }

    static int Link(TouchMiniGame game, string folder)
    {
        int linked = 0;
        SerializedObject so = new SerializedObject(game);
        SerializedProperty slots = so.FindProperty("Slots");
        for (int i = 0; i < slots.arraySize; i++)
        {
            SerializedProperty slot = slots.GetArrayElementAtIndex(i);
            string file = folder + slot.FindPropertyRelative("FileName").stringValue;
            SerializedProperty sprites = slot.FindPropertyRelative("Sprites");

            // 번호 없는 파일 하나뿐이면 모든 상태에 같은 그림을 쓴다
            Sprite single = Load(file);
            for (int state = 0; state < sprites.arraySize; state++)
            {
                Sprite sprite = Load(file + "_" + (state + 1));
                if (sprite == null)
                    sprite = single;
                SerializedProperty element = sprites.GetArrayElementAtIndex(state);
                if (sprite == null || element.objectReferenceValue == sprite)
                    continue;
                element.objectReferenceValue = sprite;
                linked++;
            }
            WarnExtra(file, sprites.arraySize);
        }

        if (so.ApplyModifiedPropertiesWithoutUndo())
            PrefabUtility.SavePrefabAsset(game.gameObject);
        return linked;
    }

    static void WarnExtra(string file, int count)
    {
        string extra = file + "_" + (count + 1);
        if (Load(extra) != null)
            Debug.LogWarning(extra + ".png: 프리팹의 상태 수(" + count + ")보다 그림이 많습니다. Slots의 Sprites 크기를 늘린 뒤 다시 연결하세요.");
    }

    static Sprite Load(string pathWithoutExtension)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>(pathWithoutExtension + ".png");
    }
}
