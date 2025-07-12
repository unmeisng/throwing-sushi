using UnityEngine;

public class kaiten : MonoBehaviour
{
    Vector3 _touchDownPos;

    [SerializeField]
    GameObject _shooterBase; // �y��̃I�u�W�F�N�g
    [SerializeField]
    GameObject _bullet;
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
            _shooterBase.transform.rotation *= qot1; // �y�����
            this.transform.rotation *= qot2; // �C������
        }

        // forward�`�F�b�N�p�̃f�o�b�O���C��
        Debug.DrawLine(this.transform.position, this.transform.position + (this.transform.forward * 5));
    }
}
