using UnityEngine;
using System.Collections;
using TMPro;

public class GameManager : SingletonMonoBehaviour<GameManager>
{
    [SerializeField]
    public GameObject MainCamera;

    [HideInInspector]
    public float SliderSpeed;

    [HideInInspector]
    public float Score;

    [SerializeField]
    TMP_Text ScoreText;


    public float SliderMaxSpeed = 0.57f;
    public void Test()
    {
        Debug.Log("シングルトン！");
    }
    private void Update()
    {
        ScoreText.text = "Score: " + Score.ToString("F0");
    }
}
