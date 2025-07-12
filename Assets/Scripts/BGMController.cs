using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
public class BGMController : MonoBehaviour
{
    [SerializeField]
    AudioMixer audioMixer;
    [SerializeField]
    AudioSource bgmAudioSource;
    [SerializeField]
    Slider bgmSlider;
    private void Start()
    {
        bgmSlider.onValueChanged.AddListener((value =>
        {
            value = Mathf.Clamp01(value);

            float decibel = 20f * Mathf.Log10(value);
            decibel = Mathf.Clamp(decibel, -80f, 0f); // Clamp to avoid negative infinity
            audioMixer.SetFloat("BGM", decibel);
        }));
    }
}
