using UnityEngine;

public class MurofushiContoroller : MonoBehaviour
{
    Vector3 _touchDownPos;
    [SerializeField]
    GameObject _shooterBase;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _touchDownPos = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0))
        {
            var tempPos = Input.mousePosition;
            Vector3 value = Vector3.zero;
            value.x = (_touchDownPos.x - tempPos.x);
            value.y = (_touchDownPos.y - tempPos.y);
            value.z = 0;
            _touchDownPos = tempPos;

            var qot1 = Quaternion.AngleAxis(value.x, new Vector3(0, 1, 0));
            var qot2 = Quaternion.AngleAxis(value.y, new Vector3(1, 0, 0));
            _shooterBase.transform.rotation *= qot1; // 土台を回す
            this.transform.rotation *= qot2;

            this.transform.rotation *= qot1 * qot2;// Quartanion同士は掛け算で合体させる
        }

        // forwardチェック用のデバッグライン
        Debug.DrawLine(this.transform.position, this.transform.position + (this.transform.forward * 5));
    }
}

