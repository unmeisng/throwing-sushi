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
    private float sharitaka = 0.5f; // shari�̍����𒲐����邽�߂̕ϐ�
    float currentTime = 0;
    float missTime = 0f;
    private int idou = 0;
    private Rigidbody _rigidbody;
    private bool isLanded = false;
    

    Dictionary<string,GameObject> collisionlist = new Dictionary<string,GameObject>(); // �Փ˂����I�u�W�F�N�g�̃��X�g

    private Vector3 shariPos; // shari�̈ʒu��ۑ�����ϐ�

    // �K�v�ȏՓː��̐ݒ�
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
                collisionlist.Clear(); // �Փ˃��X�g���N���A
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
            // ��莞�Ԉȏ�Փ˂��Ă���ꍇ�A�X�R�A��ǉ�
            Debug.Log("Sufficient collision time with shari detected. Adding score and updating position.");
            AddScore(500);
            _rigidbody = gameObject.GetComponent<Rigidbody>();
            _rigidbody.isKinematic = true; // shari�̕����������~
            Vector3 KanseiPos = new Vector3(shariPos.x, shariPos.y + sharitaka, shariPos.z);
            gameObject.transform.rotation = Quaternion.Euler(0, 0, 0); // ��]�����Z�b�g
            gameObject.transform.position = KanseiPos;
            currentTime = 0; // �^�C�}�[�����Z�b�g
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
        GameManager.Instance.SliderSpeed = GameManager.Instance.SliderMaxSpeed; // �X���C�_�[�̑��x�����ɖ߂�
        // �J�����̐؂�ւ�����
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