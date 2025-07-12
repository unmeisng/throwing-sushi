using UnityEngine;

public class FPSController : MonoBehaviour
{
    [Header("マウス感度設定")]
    public float mouseSensitivity = 100f;

    [Header("回転制限設定")]
    public float maxLookAngle = 90f; // 上下の制限角度（90度 = 180度範囲）
    public float maxTurnAngle = 90f; // 左右の制限角度（90度 = 180度範囲）

    [Header("カメラ参照")]
    public Camera playerCamera;

    // 内部変数
    private float verticalRotation = 0f;
    private float horizontalRotation = 0f;
    private Vector3 initialForward;

    void Start()
    {
        // カメラが設定されていない場合、自動で取得
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null)
            {
                playerCamera = FindObjectOfType<Camera>();
            }
        }

        // 初期の前方向を記録
        initialForward = transform.forward;

        // カーソルをロック
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMouseInput();
        HandleCursorToggle();
    }

    void HandleMouseInput()
    {
        // マウス入力を取得
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // 水平回転（左右）
        horizontalRotation += mouseX;
        horizontalRotation = Mathf.Clamp(horizontalRotation, -maxTurnAngle, maxTurnAngle);

        // 垂直回転（上下）
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -maxLookAngle, maxLookAngle);

        // 回転を適用
        ApplyRotation();
    }

    void ApplyRotation()
    {
        // カメラの垂直回転（上下）
        if (playerCamera != null)
        {
            playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        }

        // プレイヤーオブジェクトの水平回転（左右）
        transform.localRotation = Quaternion.Euler(0f, horizontalRotation, 0f);
    }

    void HandleCursorToggle()
    {
        // Escapeキーでカーソルの表示/非表示を切り替え
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    // 回転をリセットする公開メソッド
    public void ResetRotation()
    {
        verticalRotation = 0f;
        horizontalRotation = 0f;
        ApplyRotation();
    }

    // 現在の回転角度を取得
    public Vector2 GetCurrentRotation()
    {
        return new Vector2(horizontalRotation, verticalRotation);
    }

    // 回転制限を動的に変更
    public void SetRotationLimits(float maxLook, float maxTurn)
    {
        maxLookAngle = Mathf.Clamp(maxLook, 0f, 90f);
        maxTurnAngle = Mathf.Clamp(maxTurn, 0f, 90f);
    }
}