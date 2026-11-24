using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 공통 템플릿: 하나 고르기 (JUN_06, JUN_09, LAB_12)
// 보기 중 정답을 탭하면 성공, 오답을 탭하면 실패. 보기가 아닌 곳은 무시.
public abstract class PickOneMiniGame : TouchMiniGame
{
    readonly List<Image> choices = new List<Image>();
    Image answer;

    protected void AddChoice(Image image, bool isAnswer)
    {
        choices.Add(image);
        if (isAnswer)
            answer = image;
    }

    protected virtual void OnPicked(Image choice, bool correct) { }

    protected override void OnTap(Vector2 pos)
    {
        // 판정 여백이 겹칠 수 있으므로 가장 가까운 보기 하나만 고른다
        Image picked = null;
        float best = float.MaxValue;
        foreach (Image choice in choices)
        {
            float distance = Vector2.Distance(choice.rectTransform.anchoredPosition, pos);
            if (distance < best && Hit(choice, pos))
            {
                best = distance;
                picked = choice;
            }
        }
        if (picked == null)
            return;

        bool correct = picked == answer;
        OnPicked(picked, correct);
        Finish(correct);
    }
}
