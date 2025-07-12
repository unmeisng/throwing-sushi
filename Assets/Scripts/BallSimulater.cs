using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Rigidbodyにvecを加えた時の弾道をシュミレート。
 */
public class BallSimulator : MonoBehaviour
{

    [SerializeField]
    private GameObject _ballSimPrefab; // 何でもOK。予測位置を表示するオブジェクト

    private const int SIMULATE_COUNT = 20; // いくつ先までシュミレートするか

    private Vector3 _startPosition; // 発射開始位置
    private List<GameObject> _simuratePointList; // シュミレートするゲームオブジェクトリスト

    void Start()
    {
        Init();
    }

    void Update()
    {
        // デバッグ用に線を出してみる。必要無いなら無くても問題なし。
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

    // 初期化
    private void Init()
    {
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

    /**
     * 弾道を予測計算する。オブジェクトを再生成せず、位置だけ動かす。
     * targetにはRigidbodyが必須です
     **/
    public void Simulate(GameObject target, Vector3 _vec)
    {
        if (_simuratePointList != null && _simuratePointList.Count > 0)
        {
            // 発射位置を保存する
            _startPosition = target.transform.position;
            var r = target.GetComponent<Rigidbody>();
            if (r != null)
            {
                // ベクトルはmassで割る
                Vector3 force = (_vec / r.mass);

                //弾道予測の位置に点を移動
                for (int i = 0; i < SIMULATE_COUNT; i++)
                {
                    var t = (i * 0.5f); // 0.5秒ごとの位置を予測。
                    var x = t * force.x;
                    var y = (force.y * t) - 0.5f * (-Physics.gravity.y) * Mathf.Pow(t, 2.0f);
                    var z = t * force.z;

                    _simuratePointList[i].transform.position = _startPosition + new Vector3(x, y, z);
                }
            }
        }
    }
}