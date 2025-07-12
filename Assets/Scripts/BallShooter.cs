using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/**
 * 弾を発射する
 */
public class BallShooter : MonoBehaviour
{

    [SerializeField]
    private BallSimulator _ballSimurator; // 弾道予測線

    [SerializeField]
    private Slider _powerSlider; // 力を変えるスライダー

    [SerializeField]
    private GameObject _shooterBase; // 土台

    private Vector3 _firstPosition;
    private Rigidbody _rigidbody;
    private Vector3 _touchDownPos;

    private bool _isShot;
    // Use this for initialization
    void Start()
    {
        Init();
    }


    // Update is called once per frame
    void Update()
    {

        if (!EventSystem.current.IsPointerOverGameObject())
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
                _shooterBase.transform.rotation *= qot1;
                this.transform.rotation *= qot2;
            }
        }


        Debug.DrawLine(this.transform.position, this.transform.position + this.transform.forward, Color.blue);

        // 発射中はシュミレートを止める
        if (!_isShot)
        {
            var vec = this.transform.forward * _powerSlider.value;
            _ballSimurator.Simulate(this.gameObject, vec);
        }
    }

    public void Init()
    {
        _firstPosition = this.transform.position;
        _rigidbody = this.GetComponent<Rigidbody>();
        _rigidbody.isKinematic = true;
        _powerSlider.value = 10.0f;
        _isShot = false;

    }

    //--------------------------------------------------------------------------
    // 発射(Buttonから呼ぶとか)
    public void Shoot()
    {
        _isShot = true;
        var vec = this.transform.forward * _powerSlider.value;
        _ballSimurator.Simulate(this.gameObject, vec);
        _rigidbody.isKinematic = false;
        _rigidbody.AddForce(vec, ForceMode.Impulse);
    }
}