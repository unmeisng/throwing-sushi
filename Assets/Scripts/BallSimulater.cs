using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Rigidbody��vec�����������̒e�����V���~���[�g�B
 */
public class BallSimulator : MonoBehaviour
{

    [SerializeField]
    private GameObject _ballSimPrefab; // ���ł�OK�B�\���ʒu��\������I�u�W�F�N�g

    private const int SIMULATE_COUNT = 20; // ������܂ŃV���~���[�g���邩

    private Vector3 _startPosition; // ���ˊJ�n�ʒu
    private List<GameObject> _simuratePointList; // �V���~���[�g����Q�[���I�u�W�F�N�g���X�g

    void Start()
    {
        Init();
    }

    void Update()
    {
        // �f�o�b�O�p�ɐ����o���Ă݂�B�K�v�����Ȃ疳���Ă����Ȃ��B
        if (_simuratePointList != null && _simuratePointList.Count > 0)
        {
            for (int i = 0; i < SIMULATE_COUNT; i++)
            {
                if (i == 0)
                {
                    Debug.DrawLine(_startPosition, _simuratePointList[i].transform.position);
                }
                else
                if (i < SIMULATE_COUNT)
                {
                    Debug.DrawLine(_simuratePointList[i - 1].transform.position, _simuratePointList[i].transform.position);
                }
            }
        }

    }

    // ������
    private void Init()
    {
        if (_simuratePointList != null && _simuratePointList.Count > 0)
        {
            foreach (var go in _simuratePointList)
            {
                Destroy(go.gameObject);
            }
        }

        // �ʒu��\������I�u�W�F�N�g��\�ߍ���Ă���
        if (_ballSimPrefab != null)
        {
            _simuratePointList = new List<GameObject>();
            for (int i = 0; i < SIMULATE_COUNT; i++)
            {
                var go = Instantiate(_ballSimPrefab);
                go.transform.SetParent(this.transform);
                go.transform.position = Vector3.zero;
                _simuratePointList.Add(go);
            }
        }
    }

    /**
     * �e����\���v�Z����B�I�u�W�F�N�g���Đ��������A�ʒu�����������B
     * target�ɂ�Rigidbody���K�{�ł�
     **/
    public void Simulate(GameObject target, Vector3 _vec)
    {
        if (_simuratePointList != null && _simuratePointList.Count > 0)
        {
            // ���ˈʒu��ۑ�����
            _startPosition = target.transform.position;
            var r = target.GetComponent<Rigidbody>();
            if (r != null)
            {
                // �x�N�g����mass�Ŋ���
                Vector3 force = (_vec / r.mass);

                //�e���\���̈ʒu�ɓ_���ړ�
                for (int i = 0; i < SIMULATE_COUNT; i++)
                {
                    var t = (i * 0.5f); // 0.5�b���Ƃ̈ʒu��\���B
                    var x = t * force.x;
                    var y = (force.y * t) - 0.5f * (-Physics.gravity.y) * Mathf.Pow(t, 2.0f);
                    var z = t * force.z;

                    _simuratePointList[i].transform.position = _startPosition + new Vector3(x, y, z);
                }
            }
        }
    }
}