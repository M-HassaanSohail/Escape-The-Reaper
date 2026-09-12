using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Drives the main-menu volume slider. Sets the global AudioListener volume and
/// remembers it between runs via PlayerPrefs.
/// </summary>
public class SettingsController : MonoBehaviour
{
    public const string VolumeKey = "MasterVolume";

    public Slider volumeSlider;
    public Text volumeLabel;

    void Start()
    {
        float v = PlayerPrefs.GetFloat(VolumeKey, 1f);
        AudioListener.volume = v;

        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            volumeSlider.value = v;
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }
        UpdateLabel(v);
    }

    public void SetVolume(float v)
    {
        AudioListener.volume = v;
        PlayerPrefs.SetFloat(VolumeKey, v);
        PlayerPrefs.Save();
        UpdateLabel(v);
    }

    private void UpdateLabel(float v)
    {
        if (volumeLabel != null)
            volumeLabel.text = "Volume: " + Mathf.RoundToInt(v * 100f) + "%";
    }
}
