using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// 터치 입력 하나 (손가락 하나). PC에서는 마우스 좌클릭이 손가락 하나와 같다.
public struct TouchPoint
{
    public int Id;
    public Vector2 Position;   // 화면 픽셀 좌표
    public bool Began;         // 이번 프레임에 눌림
    public bool Held;          // 누르고 있는 중
    public bool Ended;         // 이번 프레임에 뗌 (빠른 탭은 Began과 Ended가 같은 프레임에 온다)
}

// 게임 전체에서 쓰는 터치 입력 창구. 마우스 우클릭/호버, 키보드는 쓰지 않는다.
public static class TouchInput
{
    const int MouseId = 0;

    static readonly List<TouchPoint> points = new List<TouchPoint>();
    static int readFrame = -1;

    // 이번 프레임의 손가락 목록. 실제 터치가 있으면 터치만, 없으면 마우스 좌클릭을 터치로 취급한다
    public static IReadOnlyList<TouchPoint> Points
    {
        get
        {
            if (readFrame != Time.frameCount)
            {
                readFrame = Time.frameCount;
                Read();
            }
            return points;
        }
    }

    static void Read()
    {
        points.Clear();

        Touchscreen screen = Touchscreen.current;
        if (screen != null)
        {
            for (int i = 0; i < screen.touches.Count; i++)
            {
                var touch = screen.touches[i];
                Add(i + 1, touch.position.ReadValue(), touch.press.wasPressedThisFrame,
                    touch.press.isPressed, touch.press.wasReleasedThisFrame);
            }
        }

        Mouse mouse = Mouse.current;
        if (points.Count == 0 && mouse != null)
        {
            Add(MouseId, mouse.position.ReadValue(), mouse.leftButton.wasPressedThisFrame,
                mouse.leftButton.isPressed, mouse.leftButton.wasReleasedThisFrame);
        }
    }

    static void Add(int id, Vector2 position, bool began, bool held, bool ended)
    {
        if (began || held || ended)
            points.Add(new TouchPoint { Id = id, Position = position, Began = began, Held = held, Ended = ended });
    }

    // 이번 프레임에 새로 누른 손가락이 있으면 그 위치
    public static bool TryGetPress(out Vector2 position)
    {
        foreach (TouchPoint point in Points)
        {
            if (point.Began)
            {
                position = point.Position;
                return true;
            }
        }
        position = Vector2.zero;
        return false;
    }
}
