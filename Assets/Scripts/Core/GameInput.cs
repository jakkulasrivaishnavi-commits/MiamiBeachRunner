using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

/// <summary>
/// One input layer for keyboard, mouse and touch swipes.
/// Works with the old Input Manager, the new Input System, or both enabled.
/// </summary>
public static class GameInput
{
    static int lastFrame = -1;
    static bool left, right, up, down, confirm, pause, anyPress;
    static bool swiping;
    static Vector2 startPos;

    public static bool Left { get { Poll(); return left; } }
    public static bool Right { get { Poll(); return right; } }
    public static bool Jump { get { Poll(); return up; } }
    public static bool Slide { get { Poll(); return down; } }
    public static bool Confirm { get { Poll(); return confirm; } }
    public static bool Pause { get { Poll(); return pause; } }
    public static bool AnyPress { get { Poll(); return anyPress; } }

    static void Poll()
    {
        if (lastFrame == Time.frameCount) return;
        lastFrame = Time.frameCount;

        left = right = up = down = confirm = pause = anyPress = false;
        bool pDown = false, pHeld = false;
        Vector2 pPos = Vector2.zero;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        Keyboard kb = Keyboard.current;
        if (kb != null)
        {
            left = kb.leftArrowKey.wasPressedThisFrame || kb.aKey.wasPressedThisFrame;
            right = kb.rightArrowKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame;
            up = kb.upArrowKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame;
            down = kb.downArrowKey.wasPressedThisFrame || kb.sKey.wasPressedThisFrame;
            confirm = kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame;
            pause = kb.escapeKey.wasPressedThisFrame || kb.pKey.wasPressedThisFrame;
            anyPress = kb.anyKey.wasPressedThisFrame;
        }
        Touchscreen ts = Touchscreen.current;
        Mouse ms = Mouse.current;
        if (ts != null && ts.primaryTouch.press.isPressed)
        {
            pHeld = true;
            pDown = ts.primaryTouch.press.wasPressedThisFrame;
            pPos = ts.primaryTouch.position.ReadValue();
        }
        else if (ms != null)
        {
            pHeld = ms.leftButton.isPressed;
            pDown = ms.leftButton.wasPressedThisFrame;
            pPos = ms.position.ReadValue();
        }
#else
        left = Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A);
        right = Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D);
        up = Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space);
        down = Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S);
        confirm = Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space);
        pause = Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P);
        anyPress = Input.anyKeyDown;
        pDown = Input.GetMouseButtonDown(0);
        pHeld = Input.GetMouseButton(0);
        pPos = Input.mousePosition;
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            pPos = t.position;
            pDown = t.phase == TouchPhase.Began;
            pHeld = t.phase != TouchPhase.Ended && t.phase != TouchPhase.Canceled;
        }
#endif

        if (pDown) { anyPress = true; swiping = true; startPos = pPos; }

        // Swipe detection (touch / mouse drag)
        if (pHeld && swiping)
        {
            Vector2 d = pPos - startPos;
            float threshold = Mathf.Max(50f, Screen.height * 0.06f);
            if (d.magnitude > threshold)
            {
                if (Mathf.Abs(d.x) > Mathf.Abs(d.y))
                {
                    if (d.x > 0f) right = true; else left = true;
                }
                else
                {
                    if (d.y > 0f) up = true; else down = true;
                }
                startPos = pPos;
            }
        }
        if (!pHeld) swiping = false;
    }
}
