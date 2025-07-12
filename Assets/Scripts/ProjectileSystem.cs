using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class ProjectileSystem : MonoBehaviour
{
    [Header("���ːݒ�")]
    public GameObject cubePrefab;
    public Transform firePoint; // �J�����̈ʒu
    public float minPower = 5f;
    public float maxPower = 25f;
    public float powerChargeSpeed = 20f;

    [Header("�\���ݒ�")]
    public GameObject previewPrefab; // �\���p�v���t�@�u
    public float previewInterval = 1f; // �\�����ˊԊu

    [Header("�J�����Ǐ]�ݒ�")]
    public bool enableCameraFollow = true;
    public float cameraFollowSpeed = 5f;
    public Vector3 cameraOffset = new Vector3(0, 2, -5);
    public float autoReturnDelay = 5f; // �����ŃJ������߂��܂ł̎���

    [Header("UI�ݒ�")]
    public Slider powerSlider;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI totalScoreText;
    public TextMeshProUGUI throwCountText;

    [Header("�X�R�A�ݒ�")]
    public int maxThrows = 5;

    [Header("�V�[���J�ڐݒ�")]
    public string titleSceneName = "Title"; // Title�V�[���̖��O
    public float sceneTransitionDelay = 10f; // ���ʕ\����A�V�[���J�ڂ܂ł̎���

    // �����ϐ�
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
    private bool gameCompleted = false; // �Q�[�������t���O

    void Start()
    {
        // �J�����擾
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            playerCamera = FindObjectOfType<Camera>();
        }

        // �J������������Ȃ��ꍇ�̃G���[�n���h�����O
        if (playerCamera == null)
        {
            Debug.LogError("PlayerCamera not found! Please ensure there is a camera in the scene.");
            return;
        }

        // FirePoint���ݒ肳��Ă��Ȃ��ꍇ�A�J�����̈ʒu���g�p
        if (firePoint == null)
        {
            firePoint = playerCamera.transform;
        }

        // �J�����̏�����Ԃ�ۑ�
        originalCameraPosition = playerCamera.transform.localPosition;
        originalCameraRotation = playerCamera.transform.localRotation;
        originalCameraParent = playerCamera.transform.parent;

        // UI������
        if (powerSlider != null)
        {
            powerSlider.minValue = 0f;
            powerSlider.maxValue = 1f;
            powerSlider.value = 0f;
        }

        UpdateUI();

        // �\���V�X�e���J�n
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
        // �Q�[��������͓��͂𖳌���
        if (gameCompleted) return;

        // Space�L�[�ŃJ�������蓮�Ŗ߂�
        if (Input.GetKeyDown(KeyCode.Space) && isFollowingProjectile)
        {
            ForceReturnCamera();
        }

        // �J�����Ǐ]���͔��˖���
        if (isFollowingProjectile) return;

        // ���N���b�N�J�n
        if (Input.GetMouseButtonDown(0) && throwCount < maxThrows)
        {
            isCharging = true;
            currentPower = minPower;
        }

        // ���N���b�N��������
        if (Input.GetMouseButton(0) && isCharging)
        {
            currentPower += powerChargeSpeed * Time.deltaTime;
            currentPower = Mathf.Clamp(currentPower, minPower, maxPower);
        }

        // ���N���b�N����
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

        // �\���I�u�W�F�N�g���N���A
        ClearPreviewObjects();

        // Cube�𐶐�
        GameObject cube = Instantiate(cubePrefab, firePoint.position, firePoint.rotation);
        currentProjectile = cube;
        projectileScored = false; // �X�R�A�擾�t���O�����Z�b�g

        // Rigidbody��ǉ��i�Ȃ���΁j
        Rigidbody rb = cube.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = cube.AddComponent<Rigidbody>();
        }

        // ���˕����Ɨ͂��v�Z
        Vector3 fireDirection = playerCamera.transform.forward;
        rb.AddForce(fireDirection * currentPower, ForceMode.Impulse);

        // ProjectileBehavior�R���|�[�l���g��ǉ�
        ProjectileBehavior behavior = cube.AddComponent<ProjectileBehavior>();
        behavior.projectileSystem = this;

        // �J�����Ǐ]�J�n
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
            // �v���W�F�N�^�C�������݂��邩�`�F�b�N
            if (currentProjectile == null)
            {
                ReturnCamera();
                return;
            }

            // �v���W�F�N�^�C���̈ʒu�ɃI�t�Z�b�g���������ʒu���^�[�Q�b�g�Ƃ���
            Vector3 targetPosition = currentProjectile.transform.position + cameraOffset;

            // �X���[�Y�ɃJ�������ړ�
            playerCamera.transform.position = Vector3.Lerp(
                playerCamera.transform.position,
                targetPosition,
                cameraFollowSpeed * Time.deltaTime
            );

            // �v���W�F�N�^�C��������
            playerCamera.transform.LookAt(currentProjectile.transform.position);
        }
    }

    void StartFollowingProjectile(GameObject projectile)
    {
        if (playerCamera == null || projectile == null) return;

        isFollowingProjectile = true;
        currentProjectile = projectile;

        // �O�̃R���[�`��������Β�~
        if (cameraReturnCoroutine != null)
        {
            StopCoroutine(cameraReturnCoroutine);
        }

        // �J������e����O��
        playerCamera.transform.SetParent(null);

        // �����ŃJ������߂��R���[�`�����J�n
        cameraReturnCoroutine = StartCoroutine(AutoReturnCamera());
    }

    IEnumerator AutoReturnCamera()
    {
        yield return new WaitForSeconds(autoReturnDelay);

        // �܂��Ǐ]���ł���Ύ����Ŗ߂�
        if (isFollowingProjectile)
        {
            ReturnCamera();
        }
    }

    void ReturnCamera()
    {
        if (playerCamera == null) return;

        // �R���[�`�����~
        if (cameraReturnCoroutine != null)
        {
            StopCoroutine(cameraReturnCoroutine);
            cameraReturnCoroutine = null;
        }

        isFollowingProjectile = false;
        currentProjectile = null;

        // �J���������S�ɖ߂�
        try
        {
            // �J���������̐e�ɖ߂�
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
            // �t�H�[���o�b�N�F�J�����������ʒu�ɔz�u
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
                // �O�񂩂�1�b�o�߂�����\������
                if (Time.time - lastPreviewTime >= previewInterval)
                {
                    FirePreviewProjectile();
                    lastPreviewTime = Time.time;
                }
            }
            else if (!isCharging)
            {
                // �`���[�W�I�����Ƀv���r���[�I�u�W�F�N�g���N���A
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
            // �\���p�I�u�W�F�N�g����
            GameObject preview = Instantiate(previewPrefab, firePoint.position, firePoint.rotation);

            // �J�����R���|�[�l���g���폜�i��������΁j
            Camera[] cameras = preview.GetComponentsInChildren<Camera>();
            foreach (Camera cam in cameras)
            {
                DestroyImmediate(cam);
            }

            // �R���C�_�[�����O�i����Ȃ��j
            Collider[] colliders = preview.GetComponentsInChildren<Collider>();
            foreach (Collider col in colliders)
            {
                col.enabled = false;
            }

            // �������ɂ���
            MakeTransparent(preview);

            // Rigidbody�ݒ�
            Rigidbody rb = preview.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = preview.AddComponent<Rigidbody>();
            }

            // ����
            Vector3 fireDirection = playerCamera.transform.forward;
            rb.AddForce(fireDirection * currentPower, ForceMode.Impulse);

            // ���X�g�ɒǉ�
            previewObjects.Add(preview);

            // 5�b��ɍ폜
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
                        Material mat = new Material(materials[i]); // �V�����}�e���A���C���X�^���X���쐬

                        // Standard Shader�̏ꍇ�̓����ݒ�
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

                        // �A���t�@�l��ݒ�
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
        // ���ɃX�R�A���v�Z����Ă���ꍇ�͖���
        if (projectileScored) return;

        projectileScored = true;
        Debug.Log("Score added: " + score); // �f�o�b�O�p
        currentScore = score;
        totalScore += score;
        UpdateUI();

        // �X�R�A�擾��A�����x�����ăJ������߂�
        if (isFollowingProjectile)
        {
            StartCoroutine(ReturnCameraAfterScore());
        }

        // 5�񓊂��I������猋�ʕ\���ƃV�[���J��
        if (throwCount >= maxThrows)
        {
            gameCompleted = true; // �Q�[�������t���O��ݒ�
            StartCoroutine(ShowFinalScoreAndTransition());
        }
    }

    IEnumerator ReturnCameraAfterScore()
    {
        yield return new WaitForSeconds(3f); // �X�R�A�m�F�̂���3�b�ҋ@
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

        // �ŏI�X�R�A��\��
        if (totalScoreText != null)
        {
            totalScoreText.text = "Final Score: " + totalScore + "/" + (maxThrows * 100);
        }

        Debug.Log("Game completed! Transitioning to Title scene in " + sceneTransitionDelay + " seconds...");

        // �w�肵�����ԑҋ@���Ă���V�[���J��
        yield return new WaitForSeconds(sceneTransitionDelay);

        // Title�V�[���ɑJ��
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
            // �t�H�[���o�b�N�F�V�[���ԍ�0�������i�ʏ�̓^�C�g���V�[���j
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
        // �R���[�`�����~
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
        gameCompleted = false; // �Q�[�������t���O�����Z�b�g
        ClearPreviewObjects();
        ReturnCamera();
        UpdateUI();
    }
}