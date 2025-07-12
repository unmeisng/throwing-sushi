using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class ProjectileSystem : MonoBehaviour
{
    [Header("発射設定")]
    public GameObject cubePrefab;
    public Transform firePoint; // カメラの位置
    public float minPower = 5f;
    public float maxPower = 25f;
    public float powerChargeSpeed = 20f;

    [Header("予測設定")]
    public GameObject previewPrefab; // 予測用プレファブ
    public float previewInterval = 1f; // 予測発射間隔

    [Header("カメラ追従設定")]
    public bool enableCameraFollow = true;
    public float cameraFollowSpeed = 5f;
    public Vector3 cameraOffset = new Vector3(0, 2, -5);
    public float autoReturnDelay = 5f; // 自動でカメラを戻すまでの時間

    [Header("UI設定")]
    public Slider powerSlider;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI totalScoreText;
    public TextMeshProUGUI throwCountText;

    [Header("スコア設定")]
    public int maxThrows = 5;

    [Header("シーン遷移設定")]
    public string titleSceneName = "Title"; // Titleシーンの名前
    public float sceneTransitionDelay = 10f; // 結果表示後、シーン遷移までの時間

    // 内部変数
    private float currentPower = 0f;
    private bool isCharging = false;
    private int currentScore = 0;
    private int totalScore = 0;
    private int throwCount = 0;
    private List<GameObject> previewObjects = new List<GameObject>();
    private Camera playerCamera;
    private GameObject currentProjectile;
    private bool isFollowingProjectile = false;
    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;
    private Transform originalCameraParent;
    private float lastPreviewTime = 0f;
    private Coroutine cameraReturnCoroutine;
    private bool projectileScored = false;
    private bool gameCompleted = false; // ゲーム完了フラグ

    void Start()
    {
        // カメラ取得
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            playerCamera = FindObjectOfType<Camera>();
        }

        // カメラが見つからない場合のエラーハンドリング
        if (playerCamera == null)
        {
            Debug.LogError("PlayerCamera not found! Please ensure there is a camera in the scene.");
            return;
        }

        // FirePointが設定されていない場合、カメラの位置を使用
        if (firePoint == null)
        {
            firePoint = playerCamera.transform;
        }

        // カメラの初期状態を保存
        originalCameraPosition = playerCamera.transform.localPosition;
        originalCameraRotation = playerCamera.transform.localRotation;
        originalCameraParent = playerCamera.transform.parent;

        // UI初期化
        if (powerSlider != null)
        {
            powerSlider.minValue = 0f;
            powerSlider.maxValue = 1f;
            powerSlider.value = 0f;
        }

        UpdateUI();

        // 予測システム開始
        StartCoroutine(PreviewSystem());
    }

    void Update()
    {
        HandleInput();
        UpdatePowerSlider();
        HandleCameraFollow();
    }

    void HandleInput()
    {
        // ゲーム完了後は入力を無効化
        if (gameCompleted) return;

        // Spaceキーでカメラを手動で戻す
        if (Input.GetKeyDown(KeyCode.Space) && isFollowingProjectile)
        {
            ForceReturnCamera();
        }

        // カメラ追従中は発射無効
        if (isFollowingProjectile) return;

        // 左クリック開始
        if (Input.GetMouseButtonDown(0) && throwCount < maxThrows)
        {
            isCharging = true;
            currentPower = minPower;
        }

        // 左クリック長押し中
        if (Input.GetMouseButton(0) && isCharging)
        {
            currentPower += powerChargeSpeed * Time.deltaTime;
            currentPower = Mathf.Clamp(currentPower, minPower, maxPower);
        }

        // 左クリック離す
        if (Input.GetMouseButtonUp(0) && isCharging)
        {
            isCharging = false;
            FireProjectile();
            currentPower = 0f;
        }
    }

    void UpdatePowerSlider()
    {
        if (powerSlider != null)
        {
            float normalizedPower = (currentPower - minPower) / (maxPower - minPower);
            powerSlider.value = normalizedPower;
        }
    }

    void FireProjectile()
    {
        if (cubePrefab == null || throwCount >= maxThrows) return;

        // 予測オブジェクトをクリア
        ClearPreviewObjects();

        // Cubeを生成
        GameObject cube = Instantiate(cubePrefab, firePoint.position, firePoint.rotation);
        currentProjectile = cube;
        projectileScored = false; // スコア取得フラグをリセット

        // Rigidbodyを追加（なければ）
        Rigidbody rb = cube.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = cube.AddComponent<Rigidbody>();
        }

        // 発射方向と力を計算
        Vector3 fireDirection = playerCamera.transform.forward;
        rb.AddForce(fireDirection * currentPower, ForceMode.Impulse);

        // ProjectileBehaviorコンポーネントを追加
        ProjectileBehavior behavior = cube.AddComponent<ProjectileBehavior>();
        behavior.projectileSystem = this;

        // カメラ追従開始
        if (enableCameraFollow)
        {
            StartFollowingProjectile(cube);
        }

        throwCount++;
        UpdateUI();
    }

    void HandleCameraFollow()
    {
        if (isFollowingProjectile && currentProjectile != null)
        {
            // プロジェクタイルが存在するかチェック
            if (currentProjectile == null)
            {
                ReturnCamera();
                return;
            }

            // プロジェクタイルの位置にオフセットを加えた位置をターゲットとする
            Vector3 targetPosition = currentProjectile.transform.position + cameraOffset;

            // スムーズにカメラを移動
            playerCamera.transform.position = Vector3.Lerp(
                playerCamera.transform.position,
                targetPosition,
                cameraFollowSpeed * Time.deltaTime
            );

            // プロジェクタイルを見る
            playerCamera.transform.LookAt(currentProjectile.transform.position);
        }
    }

    void StartFollowingProjectile(GameObject projectile)
    {
        if (playerCamera == null || projectile == null) return;

        isFollowingProjectile = true;
        currentProjectile = projectile;

        // 前のコルーチンがあれば停止
        if (cameraReturnCoroutine != null)
        {
            StopCoroutine(cameraReturnCoroutine);
        }

        // カメラを親から外す
        playerCamera.transform.SetParent(null);

        // 自動でカメラを戻すコルーチンを開始
        cameraReturnCoroutine = StartCoroutine(AutoReturnCamera());
    }

    IEnumerator AutoReturnCamera()
    {
        yield return new WaitForSeconds(autoReturnDelay);

        // まだ追従中であれば自動で戻す
        if (isFollowingProjectile)
        {
            ReturnCamera();
        }
    }

    void ReturnCamera()
    {
        if (playerCamera == null) return;

        // コルーチンを停止
        if (cameraReturnCoroutine != null)
        {
            StopCoroutine(cameraReturnCoroutine);
            cameraReturnCoroutine = null;
        }

        isFollowingProjectile = false;
        currentProjectile = null;

        // カメラを安全に戻す
        try
        {
            // カメラを元の親に戻す
            if (originalCameraParent != null)
            {
                playerCamera.transform.SetParent(originalCameraParent);
            }
            else
            {
                playerCamera.transform.SetParent(null);
            }

            playerCamera.transform.localPosition = originalCameraPosition;
            playerCamera.transform.localRotation = originalCameraRotation;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error returning camera: " + e.Message);
            // フォールバック：カメラを初期位置に配置
            playerCamera.transform.position = transform.position + originalCameraPosition;
            playerCamera.transform.rotation = originalCameraRotation;
        }
    }

    void ForceReturnCamera()
    {
        ReturnCamera();
    }

    IEnumerator PreviewSystem()
    {
        while (true)
        {
            if (isCharging && !isFollowingProjectile && !gameCompleted && previewPrefab != null)
            {
                // 前回から1秒経過したら予測発射
                if (Time.time - lastPreviewTime >= previewInterval)
                {
                    FirePreviewProjectile();
                    lastPreviewTime = Time.time;
                }
            }
            else if (!isCharging)
            {
                // チャージ終了時にプレビューオブジェクトをクリア
                ClearPreviewObjects();
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    void FirePreviewProjectile()
    {
        if (previewPrefab == null || playerCamera == null) return;

        try
        {
            // 予測用オブジェクト生成
            GameObject preview = Instantiate(previewPrefab, firePoint.position, firePoint.rotation);

            // カメラコンポーネントを削除（もしあれば）
            Camera[] cameras = preview.GetComponentsInChildren<Camera>();
            foreach (Camera cam in cameras)
            {
                DestroyImmediate(cam);
            }

            // コライダーを除外（判定なし）
            Collider[] colliders = preview.GetComponentsInChildren<Collider>();
            foreach (Collider col in colliders)
            {
                col.enabled = false;
            }

            // 半透明にする
            MakeTransparent(preview);

            // Rigidbody設定
            Rigidbody rb = preview.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = preview.AddComponent<Rigidbody>();
            }

            // 発射
            Vector3 fireDirection = playerCamera.transform.forward;
            rb.AddForce(fireDirection * currentPower, ForceMode.Impulse);

            // リストに追加
            previewObjects.Add(preview);

            // 5秒後に削除
            Destroy(preview, 5f);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error creating preview projectile: " + e.Message);
        }
    }

    void MakeTransparent(GameObject obj)
    {
        try
        {
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                if (renderer.materials != null)
                {
                    Material[] materials = renderer.materials;
                    for (int i = 0; i < materials.Length; i++)
                    {
                        Material mat = new Material(materials[i]); // 新しいマテリアルインスタンスを作成

                        // Standard Shaderの場合の透明設定
                        if (mat.HasProperty("_Mode"))
                        {
                            mat.SetFloat("_Mode", 3); // Transparent mode
                            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                            mat.SetInt("_ZWrite", 0);
                            mat.DisableKeyword("_ALPHATEST_ON");
                            mat.EnableKeyword("_ALPHABLEND_ON");
                            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                            mat.renderQueue = 3000;
                        }

                        // アルファ値を設定
                        if (mat.HasProperty("_Color"))
                        {
                            Color color = mat.color;
                            color.a = 0.3f;
                            mat.color = color;
                        }

                        materials[i] = mat;
                    }
                    renderer.materials = materials;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Could not make object transparent: " + e.Message);
        }
    }

    void ClearPreviewObjects()
    {
        foreach (GameObject obj in previewObjects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        previewObjects.Clear();
    }

    public void AddScore(int score)
    {
        // 既にスコアが計算されている場合は無視
        if (projectileScored) return;

        projectileScored = true;
        Debug.Log("Score added: " + score); // デバッグ用
        currentScore = score;
        totalScore += score;
        UpdateUI();

        // スコア取得後、少し遅延してカメラを戻す
        if (isFollowingProjectile)
        {
            StartCoroutine(ReturnCameraAfterScore());
        }

        // 5回投げ終わったら結果表示とシーン遷移
        if (throwCount >= maxThrows)
        {
            gameCompleted = true; // ゲーム完了フラグを設定
            StartCoroutine(ShowFinalScoreAndTransition());
        }
    }

    IEnumerator ReturnCameraAfterScore()
    {
        yield return new WaitForSeconds(3f); // スコア確認のため3秒待機
        ReturnCamera();
    }

    void UpdateUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + currentScore;
        }

        if (totalScoreText != null)
        {
            totalScoreText.text = "Total: " + totalScore;
        }

        if (throwCountText != null)
        {
            throwCountText.text = "Throws: " + throwCount + "/" + maxThrows;
        }
    }

    IEnumerator ShowFinalScoreAndTransition()
    {
        yield return new WaitForSeconds(2f);

        // 最終スコアを表示
        if (totalScoreText != null)
        {
            totalScoreText.text = "Final Score: " + totalScore + "/" + (maxThrows * 100);
        }

        Debug.Log("Game completed! Transitioning to Title scene in " + sceneTransitionDelay + " seconds...");

        // 指定した時間待機してからシーン遷移
        yield return new WaitForSeconds(sceneTransitionDelay);

        // Titleシーンに遷移
        TransitionToTitleScene();
    }

    void TransitionToTitleScene()
    {
        try
        {
            Debug.Log("Transitioning to Title scene: " + titleSceneName);
            SceneManager.LoadScene(titleSceneName);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to load Title scene '" + titleSceneName + "': " + e.Message);
            // フォールバック：シーン番号0を試す（通常はタイトルシーン）
            try
            {
                SceneManager.LoadScene(0);
            }
            catch (System.Exception e2)
            {
                Debug.LogError("Failed to load scene by index 0: " + e2.Message);
            }
        }
    }

    public void ResetGame()
    {
        // コルーチンを停止
        if (cameraReturnCoroutine != null)
        {
            StopCoroutine(cameraReturnCoroutine);
            cameraReturnCoroutine = null;
        }

        currentScore = 0;
        totalScore = 0;
        throwCount = 0;
        currentPower = 0f;
        isCharging = false;
        projectileScored = false;
        gameCompleted = false; // ゲーム完了フラグもリセット
        ClearPreviewObjects();
        ReturnCamera();
        UpdateUI();
    }
}