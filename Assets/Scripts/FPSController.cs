using UnityEngine;

public class FPSController : MonoBehaviour
{
    [Header("�}�E�X���x�ݒ�")]
    public float mouseSensitivity = 100f;

    [Header("��]�����ݒ�")]
    public float maxLookAngle = 90f; // �㉺�̐����p�x�i90�x = 180�x�͈́j
    public float maxTurnAngle = 90f; // ���E�̐����p�x�i90�x = 180�x�͈́j

    [Header("�J�����Q��")]
    public Camera playerCamera;

    // �����ϐ�
    private float verticalRotation = 0f;
    private float horizontalRotation = 0f;
    private Vector3 initialForward;

    void Start()
    {
        // �J�������ݒ肳��Ă��Ȃ��ꍇ�A�����Ŏ擾
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null)
            {
                playerCamera = FindObjectOfType<Camera>();
            }
        }

        // �����̑O�������L�^
        initialForward = transform.forward;

        // �J�[�\�������b�N
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
        // �}�E�X���͂��擾
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // ������]�i���E�j
        horizontalRotation += mouseX;
        horizontalRotation = Mathf.Clamp(horizontalRotation, -maxTurnAngle, maxTurnAngle);

        // ������]�i�㉺�j
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -maxLookAngle, maxLookAngle);

        // ��]��K�p
        ApplyRotation();
    }

    void ApplyRotation()
    {
        // �J�����̐�����]�i�㉺�j
        if (playerCamera != null)
        {
            playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        }

        // �v���C���[�I�u�W�F�N�g�̐�����]�i���E�j
        transform.localRotation = Quaternion.Euler(0f, horizontalRotation, 0f);
    }

    void HandleCursorToggle()
    {
        // Escape�L�[�ŃJ�[�\���̕\��/��\����؂�ւ�
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

    // ��]�����Z�b�g������J���\�b�h
    public void ResetRotation()
    {
        verticalRotation = 0f;
        horizontalRotation = 0f;
        ApplyRotation();
    }

    // ���݂̉�]�p�x���擾
    public Vector2 GetCurrentRotation()
    {
        return new Vector2(horizontalRotation, verticalRotation);
    }

    // ��]�����𓮓I�ɕύX
    public void SetRotationLimits(float maxLook, float maxTurn)
    {
        maxLookAngle = Mathf.Clamp(maxLook, 0f, 90f);
        maxTurnAngle = Mathf.Clamp(maxTurn, 0f, 90f);
    }
}