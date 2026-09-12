using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Lives on the Canvas. Adds a click-sound listener to every button so the UI
/// has audible feedback without wiring each button individually.
/// </summary>
public class UIManager : MonoBehaviour
{
    public Button[] allButtons;

    void Start()
    {
        if (allButtons == null || allButtons.Length == 0)
            allButtons = GetComponentsInChildren<Button>(true);

        foreach (var b in allButtons)
        {
            if (b == null) continue;
            b.onClick.AddListener(() => AudioManager.PlayButtonClickStatic());
        }
    }
}
