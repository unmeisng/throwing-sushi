using System;
using UnityEngine;
using TMPro;

public class GameManager : Singleton<GameManager>
{
    public GameObject mainCamera;
    [HideInInspector]
    public float sliderSpeed;
    public float sliderDefaultSpeed;
    public float judgeTime;
    public float posCorrect;
    [HideInInspector]
    public int score;
    [SerializeField]
    TMP_Text Txt_Score;

    public EventHandler onShoot;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sliderSpeed = sliderDefaultSpeed;
        score= 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            onShoot?.Invoke(this,EventArgs.Empty);
        }   

        Txt_Score.text=$"SCORE:{score.ToString()}";
    }
}
