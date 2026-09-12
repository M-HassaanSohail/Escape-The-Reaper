using UnityEngine;

/// <summary>
/// Detects swipe gestures on touch devices and exposes one-shot flags that the
/// PlayerController polls. Keyboard input is handled directly in PlayerController,
/// so this component is purely additive for mobile.
/// </summary>
public class MobileInput : MonoBehaviour
{
    [Tooltip("Minimum pixel distance for a swipe to register.")]
    public float swipeThreshold = 60f;

    public bool SwipeLeft { get; private set; }
    public bool SwipeRight { get; private set; }
    public bool SwipeUp { get; private set; }
    public bool SwipeDown { get; private set; }

    private Vector2 startPos;
    private bool tracking;

    void Update()
    {
        // reset one-shot flags every frame; they are read by PlayerController in the same frame
        SwipeLeft = SwipeRight = SwipeUp = SwipeDown = false;

#if UNITY_EDITOR || UNITY_STANDALONE
        // Mouse drag emulates a swipe in the editor / on desktop
        if (Input.GetMouseButtonDown(0)) { startPos = Input.mousePosition; tracking = true; }
        else if (Input.GetMouseButtonUp(0) && tracking)
        {
            Evaluate((Vector2)Input.mousePosition - startPos);
            tracking = false;
        }
#endif

        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began) { startPos = t.position; tracking = true; }
            else if ((t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled) && tracking)
            {
                Evaluate(t.position - startPos);
                tracking = false;
            }
        }
    }

    private void Evaluate(Vector2 delta)
    {
        if (delta.magnitude < swipeThreshold) return;

        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            if (delta.x > 0) SwipeRight = true; else SwipeLeft = true;
        }
        else
        {
            if (delta.y > 0) SwipeUp = true; else SwipeDown = true;
        }
    }
}
