using System;
using UnityEngine;
using UnityEngine.UI;

// 에셋 자리 하나. 그림(Sprites)이 비어 있으면 임시 이미지(Placeholder)를 색만 바꿔 보여준다.
// Sprites는 상태 순서대로 넣는다. 메뉴 MiniGame > 스프라이트 자동 연결 로 파일 이름에 맞춰 채울 수 있다.
[Serializable]
public class SpriteSlot
{
    public string FileName;        // 예: DES_03_01
    public string Label;           // 예: 전갈 (임시 이미지 위에 표시)
    public string[] StateNames;    // 예: 움직임, 잡힘
    public Sprite[] Sprites;
    public Sprite Placeholder;
    public Color[] PlaceholderColors;

    public int StateCount
    {
        get { return Sprites == null || Sprites.Length == 0 ? 1 : Sprites.Length; }
    }

    public Sprite GetSprite(int state)
    {
        if (Sprites == null || Sprites.Length == 0)
            return null;
        return Sprites[Mathf.Clamp(state, 0, Sprites.Length - 1)];
    }

    public Color GetColor(int state)
    {
        if (PlaceholderColors == null || PlaceholderColors.Length == 0)
            return Color.gray;
        return PlaceholderColors[Mathf.Clamp(state, 0, PlaceholderColors.Length - 1)];
    }

    public string GetLabel(int state)
    {
        if (StateNames == null || StateNames.Length == 0)
            return Label;
        return Label + "\n" + StateNames[Mathf.Clamp(state, 0, StateNames.Length - 1)];
    }
}

// 시작할 때 보여줄 이미지와 에셋 자리의 짝
[Serializable]
public struct SpritePlacement
{
    public Image Image;
    public int Slot;    // 1부터 (파일 이름의 슬롯 번호)
    public int State;   // 0부터
}
