using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

public class CollisionDetection : MonoBehaviour
{
    [SerializeField]
    private GameObject targetCamera;
    [SerializeField]
    private float limitTime = 1f;
    [SerializeField]
    private float sharitaka = 0.5f; // shariの高さを調整するための変数
    float currentTime = 0;
    float missTime = 0f;
    private int idou = 0;
    private Rigidbody _rigidbody;
    private bool isLanded = false;
    

    Dictionary<string,GameObject> collisionlist = new Dictionary<string,GameObject>(); // 衝突したオブジェクトのリスト

    private Vector3 shariPos; // shariの位置を保存する変数

    // 必要な衝突数の設定
    [SerializeField]
    private int requiredShariCollisions = 3;
    [SerializeField]
    private float detectionDelay = 1f;

    bool kansei = false;

    private void Update()
    {
        if (collisionlist.Count > 0 )
        {
            foreach (var item in collisionlist)
            {

                Debug.Log($"{item.Key}:{item.Value}");
            }

            if (collisionlist.ContainsKey("shari"))
            {
                shariPos = collisionlist["shari"].transform.position; 
                Sharinori();
                Debug.Log("Shari collision detected");

            }
            else if(!isLanded)
            {
                Debug.Log("Non-shari collision detected");
                NonShari();
                collisionlist.Clear(); // 衝突リストをクリア
                AddScore(-100);
                Debug.Log("Score decreased by 100 due to non-shari collision.");
            }
            isLanded = true;
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!collisionlist.ContainsKey(other.gameObject.tag) && !kansei)
        {
            collisionlist.Add(other.gameObject.tag,other.gameObject);
            Debug.Log($"Collision detected with: {other.gameObject.tag}");
        }
        
    }
    private IEnumerator WaitDetection(float WaitTime)
    {
        yield return new WaitForSeconds(WaitTime);
    }
    private void OnTriggerExit(Collider other)
    {
        collisionlist.Remove(other.gameObject.tag);
    }

    private void Sharinori()
    {
        currentTime += Time.deltaTime;
        Debug.Log($"Collision with shari detected. Current time: {currentTime}");
        if (currentTime >= limitTime && kansei != true)
        { 
            // 一定時間以上衝突している場合、スコアを追加
            Debug.Log("Sufficient collision time with shari detected. Adding score and updating position.");
            AddScore(500);
            _rigidbody = gameObject.GetComponent<Rigidbody>();
            _rigidbody.isKinematic = true; // shariの物理挙動を停止
            Vector3 KanseiPos = new Vector3(shariPos.x, shariPos.y + sharitaka, shariPos.z);
            gameObject.transform.rotation = Quaternion.Euler(0, 0, 0); // 回転をリセット
            gameObject.transform.position = KanseiPos;
            currentTime = 0; // タイマーをリセット
            kansei = true;
            targetCamera.SetActive(false);
            Destroy(targetCamera, 5f);
            GameManager.Instance.MainCamera.SetActive(true); 
            isLanded = true;
            GameManager.Instance.SliderSpeed = GameManager.Instance.SliderMaxSpeed;
            collisionlist.Clear();
        }
    }
    private void NonShari()
    {
        Debug.Log("non-shari collision");
        GameManager.Instance.SliderSpeed = GameManager.Instance.SliderMaxSpeed; // スライダーの速度を元に戻す
        // カメラの切り替え処理
        if (targetCamera != null)
        {
            targetCamera.SetActive(false);
            Destroy(targetCamera);
        }

        if (GameManager.Instance != null && GameManager.Instance.MainCamera != null)
        {
            GameManager.Instance.MainCamera.SetActive(true);
        }
        isLanded = true; 
    }

    private void AddScore(int points)
    {
        GameManager.Instance.Score += points;

    }
    private void Start()
    {
        targetCamera.SetActive(true);
    }
}