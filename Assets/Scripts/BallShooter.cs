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
    private Slider _powerSlider; // 力を変えるスライダー

    [SerializeField]
    bool isDebug = false;
    [SerializeField]
    float shootStrength,sensibility,absX,minY;

    private Rigidbody _rigidbody;
    private Vector3 _touchDownPos, simPos, inputVec, muzzleRot;

    private bool _isShot;

    float screenAspect,diff;

    [SerializeField]
    private GameObject _ballSimPrefab; // 何でもOK。予測位置を表示するオブジェクト

    [SerializeField]
    int SIMULATE_COUNT;

    [SerializeField]
    float SIMLATE_LENGTH,targetHeight,sliderSpeed; // いくつ先までシュミレートするか

    private Vector3 _startPosition; // 発射開始位置
    private List<GameObject> _simuratePointList; // シュミレートするゲームオブジェクトリスト

    bool isSliderNegative;

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
                var rotX = tempPos.x - _touchDownPos.x;
                var rotY = tempPos.y - _touchDownPos.y;

                var tempRot = muzzleRot + new Vector3(rotX, rotY * screenAspect, 0).normalized * sensibility *Time.deltaTime;

                if(Mathf.Abs(tempRot.x)>= absX)
                {
                    rotX = 0;
                }

                if(tempRot.y <= minY)
                {
                    rotY = 0;
                }

                muzzleRot += new Vector3(rotX, rotY * screenAspect, 0).normalized * sensibility * Time.deltaTime;

                Debug.Log(muzzleRot);
                _touchDownPos = tempPos;
            }
        }


        // 発射中はシュミレートを止める
        if (!_isShot)
        {
            Simulate(muzzleRot);
        }

        if (_simuratePointList != null && _simuratePointList.Count > 0 && isDebug)
        {
            Debug.DrawLine(this.transform.position, this.transform.position + this.transform.forward, Color.blue);

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

        if(_powerSlider.value <= _powerSlider.minValue)
        {
            isSliderNegative = false;
        }
        
        if(_powerSlider.value >= _powerSlider.maxValue)
        {
            isSliderNegative = true;
        }

        if (isSliderNegative)
        {
            _powerSlider.value -= sliderSpeed * Time.deltaTime;
        }
        else
        {
            _powerSlider.value += sliderSpeed * Time.deltaTime;
        }
    }

    public void Init()
    {
        _rigidbody = this.GetComponent<Rigidbody>();
        _rigidbody.isKinematic = true;
        _isShot = false;
        muzzleRot = gameObject.transform.forward;
        diff = targetHeight - gameObject.transform.position.y;

        screenAspect = Screen.width / Screen.height;

        if (_simuratePointList != null && _simuratePointList.Count > 0)
        {
            foreach (var go in _simuratePointList)
            {
                Destroy(go.gameObject);
            }
        }

        // 位置を表示するオブジェクトを予め作っておく
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

    //--------------------------------------------------------------------------
    // 発射(Buttonから呼ぶとか)
    public void Shoot()
    {
        _isShot = true;
        var vec = muzzleRot;
        Simulate(vec);
        _rigidbody.isKinematic = false;
        _rigidbody.AddForce(vec, ForceMode.Impulse);
    }

    void Simulate(Vector3 _vec)
    {
        if (_simuratePointList != null && _simuratePointList.Count > 0)
        {
            // 発射位置を保存する
            _startPosition = gameObject.transform.position;
            if (_rigidbody != null)
            {
                float limit = (-_vec.y - Mathf.Sqrt(Mathf.Pow(_vec.y,2) + 2 * Physics.gravity.y * diff)) / Physics.gravity.y;
                Vector3 forward = _vec;
                forward.y = 0;

                //弾道予測の位置に点を移動
                for (int i = 0; i < SIMULATE_COUNT; i++)
                {
                    var t = (i * limit / (float)SIMULATE_COUNT); // 0.5秒ごとの位置を予測。
                    simPos = _vec * t + transform.up *(0.5f * Physics.gravity.y * Mathf.Pow(t, 2.0f)) + forward.normalized * _powerSlider.value * shootStrength * t;
                    _simuratePointList[i].transform.position = _startPosition + simPos;
                }
            }
        }
    }
}