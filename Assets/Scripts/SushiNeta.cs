using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SushiNeta : MonoBehaviour
{
   
    Dictionary<string,GameObject> objectList = new Dictionary<string,GameObject>();
    bool endJudge;
    float timeCount;
    [SerializeField]
    GameObject localCamera;
    Rigidbody rigidbody;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        endJudge = false;
        rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if(objectList.Count > 0)
        {
            if (objectList.ContainsKey("shari"))
            {
                if(timeCount < GameManager.instance.judgeTime)
                {
                    timeCount += Time.deltaTime;
                }
                else
                {
                    endJudge = true;
                    rigidbody.isKinematic = true;
                    gameObject.transform.position = objectList["shari"].transform.position + new Vector3(0, GameManager.instance.posCorrect, 0);
                    objectList.Clear();
                    StartCoroutine(Wait(1f));
                    GameManager.instance.score += 100;
                    GameManager.instance.sliderSpeed = GameManager.instance.sliderDefaultSpeed;
                    GameManager.instance.mainCamera.SetActive(true);
                    Destroy(localCamera);
                }
            }
            else
            {
                endJudge = true;
                objectList.Clear();
                GameManager.instance.score -= 10;
                GameManager.instance.sliderSpeed = GameManager.instance.sliderDefaultSpeed;
                GameManager.instance.mainCamera.SetActive(true);
                Destroy(localCamera);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!objectList.ContainsKey(other.gameObject.tag) && !endJudge)
        {
            timeCount = 0;
            objectList.Add(other.gameObject.tag, other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (objectList.ContainsKey(other.gameObject.tag))
        {
            objectList.Remove(other.gameObject.tag);
        }
    }

    IEnumerator Wait(float sec)
    {
        yield return new WaitForSeconds(sec);
    }
}
