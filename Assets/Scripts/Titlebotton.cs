using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.Audio;

public class Titlebotton : MonoBehaviour
{
    [SerializeField]
    GameObject configPanel;
    [SerializeField]
    GameObject staffPanel;
    [SerializeField]
    AudioMixer audioMixer;
    [SerializeField]
    AudioSource bgmAudioSource;
    [SerializeField]
    AudioSource bgm2AudioSource;
    public void StartBtn()
    {
        SceneManager.LoadScene("MainGame");
    }

    public void ShowConfigPanel()
    {
        configPanel.SetActive(true);
    }
    public void HideConfigPanel()
    {
        configPanel.SetActive(false);
    }
    public void ShowStaffPanel()
    {
        staffPanel.SetActive(true);
        bgmAudioSource.Stop();
        bgm2AudioSource.Play();
    }
    public void HideStaffPanel()
    {
        staffPanel.SetActive(false);
        bgmAudioSource.Play();
        bgm2AudioSource.Stop();
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
