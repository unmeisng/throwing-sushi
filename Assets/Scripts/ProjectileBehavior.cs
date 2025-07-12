using UnityEngine;

public class ProjectileBehavior : MonoBehaviour
{
    [HideInInspector]
    public ProjectileSystem projectileSystem;

    private bool hasLanded = false;
    private Rigidbody rb;
    private float landingCheckDelay = 0.5f; // ���n����̒x��
    private float timeAfterLaunch = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
    }

    void Update()
    {
        timeAfterLaunch += Time.deltaTime;
    }

    void OnCollisionEnter(Collision collision)
    {
        // ���˒���̏Փ˂͖����i�������g�┭�ˑ�Ƃ̏Փ˂�h���j
        if (timeAfterLaunch < landingCheckDelay) return;

        if (hasLanded) return;

        // ���x���\���ɒቺ���Ă��璅�n������s��
        if (rb != null && rb.linearVelocity.magnitude > 1f) return;

        hasLanded = true;

        Debug.Log("Projectile landed on: " + collision.gameObject.name);

        // ���n�����I�u�W�F�N�g��ScoreZone���擾
        ScoreZone scoreZone = collision.gameObject.GetComponent<ScoreZone>();

        int score = 0;
        if (scoreZone != null)
        {
            score = scoreZone.scoreValue;
            Debug.Log("ScoreZone found with value: " + score);
        }
        else
        {
            Debug.Log("No ScoreZone component found on: " + collision.gameObject.name);
        }

        // �X�R�A��ProjectileSystem�ɑ��M
        if (projectileSystem != null)
        {
            projectileSystem.AddScore(score);
        }

        // �������Z���~���Ĉ��肳����
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        // 10�b��ɔj��
        Destroy(gameObject, 10f);
    }

    void OnTriggerEnter(Collider other)
    {
        // ���˒���̃g���K�[����͖���
        if (timeAfterLaunch < landingCheckDelay) return;

        if (hasLanded) return;

        Debug.Log("Projectile triggered: " + other.gameObject.name);

        // �g���K�[�ł��X�R�A������s��
        ScoreZone scoreZone = other.gameObject.GetComponent<ScoreZone>();

        if (scoreZone != null && !hasLanded)
        {
            hasLanded = true;
            int score = scoreZone.scoreValue;
            Debug.Log("Trigger ScoreZone found with value: " + score);

            // �X�R�A��ProjectileSystem�ɑ��M
            if (projectileSystem != null)
            {
                projectileSystem.AddScore(score);
            }

            // �������Z���~
            if (rb != null)
            {
                rb.isKinematic = true;
            }

            // 10�b��ɔj��
            Destroy(gameObject, 10f);
        }
    }
}