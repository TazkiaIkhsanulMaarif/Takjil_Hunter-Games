using System.Net.Mime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyLogic : MonoBehaviour
{
    public float hitPoints = 100f;
    public float turnSpeed = 15f;
    public Transform target;
    public float ChaseRange;
    private UnityEngine.AI.NavMeshAgent agent;
    private float DistancetoTarget;
    private float DistancetoDefault;
    private Animator anim;

    public GameObject slowRecoveryPanel;

    public Text slowRecoveryText;
    public Image slowImage;
    public Image normalImage;

    Vector3 DefaultPosition;

    public float initialMovementSpeed = 3.0f;
    private float movementSpeed;
    private bool isSlowed = false;

    [Header("Enemy SFX")]
    public AudioClip StepAudio;
    AudioSource EnemyAudio;

    // Start is called before the first frame update
    void Start()
    {
        agent = this.GetComponent<UnityEngine.AI.NavMeshAgent>();
        anim = this.GetComponentInChildren<Animator>();
        anim.SetFloat("HitPoint", hitPoints);
        DefaultPosition = this.transform.position;
        EnemyAudio = this.GetComponent<AudioSource>();
        slowRecoveryPanel.SetActive(false);
        target = FindAnyObjectByType<PlayerLogic>().transform;
    }

    // Update is called once per frame
    void Update()
    {
        DistancetoTarget = Vector3.Distance(target.position, transform.position);
        DistancetoDefault = Vector3.Distance(DefaultPosition, transform.position);

        if (isSlowed)
        {
            movementSpeed *= 0.5f;
        }
        else
        {
            movementSpeed = initialMovementSpeed;
        }

        if (DistancetoTarget <= ChaseRange && hitPoints != 0)
        {
            FaceTarget(target.position);
            if (DistancetoTarget > agent.stoppingDistance + 2f)
            {
                ChaseTarget();
            }
            else if (DistancetoTarget <= agent.stoppingDistance)
            {
                Attack();
            }
        }
        else if (DistancetoTarget >= ChaseRange * 2)
        {
            agent.SetDestination(DefaultPosition);
            FaceTarget(DefaultPosition);
            if (DistancetoDefault <= agent.stoppingDistance)
            {
                Debug.Log("Time to stop");
                anim.SetBool("Run", false);
                anim.SetBool("Attack", false);
            }
        }

    }

    private void FaceTarget(Vector3 destination)
    {
        Vector3 direction = (destination - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * turnSpeed);
    }
    public void Attack()
    {
        Debug.Log("attack");
        anim.SetBool("Run", false);
        anim.SetBool("Attack", true);
    }

    public void ChaseTarget()
    {
        agent.SetDestination(target.position);
        anim.SetBool("Run", true);
        anim.SetBool("Attack", false);
    }


    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, ChaseRange);
    }

    public void TakeDamage(float damage)
    {
        hitPoints -= damage;
        anim.SetTrigger("GetHit");
        anim.SetFloat("Hitpoint", hitPoints);
        if (hitPoints <= 0)
        {
            Destroy(gameObject, 3f);
        }
    }
    public void HitConnect()
    {
        if (DistancetoTarget <= agent.stoppingDistance)
        {
            target.GetComponent<PlayerLogic>().PlayerGetHit(50f);
        }
    }

    public void ApplySlow(float duration)
    {
        if (!isSlowed)
        {
            StartCoroutine(SlowEffect(duration));
        }

    }

    private IEnumerator SlowEffect(float duration)
    {
        isSlowed = true;

        // Menampilkan panel canvas ketika musuh melambat
        ShowSlowRecoveryPanel(slowImage, normalImage, "Musuh Melambat");

        yield return new WaitForSeconds(duration);

        // Menampilkan panel canvas ketika musuh kembali berjalan normal
        ShowSlowRecoveryPanel(normalImage, slowImage, "Musuh kembali berjalan normal!");

        isSlowed = false;
    }

    void ShowSlowRecoveryPanel(Image targetImage, Image normalImage, string customMessage = "")
    {
        slowRecoveryPanel.SetActive(true);

        if (slowRecoveryPanel == true)
        {
            // Set pesan kustom jika ada
            slowRecoveryText.text = customMessage;
        }
        else
        {
            // Set pesan default jika tidak ada pesan kustom
            // slowRecoveryText.text = "Musuh kembali berjalan normal!";
        }

        // Aktifkan UI Image sesuai dengan parameter
        targetImage.gameObject.SetActive(true);
        normalImage.gameObject.SetActive(false);

        StartCoroutine(HidePanelAfterDelay(slowRecoveryPanel, 3f, targetImage));
    }

    private IEnumerator HidePanelAfterDelay(GameObject panel, float delay, Image targetImage)
    {
        yield return new WaitForSeconds(delay);

        // Nonaktifkan UI Image setelah delay
        targetImage.gameObject.SetActive(false);
        panel.SetActive(false);
    }

    public void step()
    {
        EnemyAudio.clip = StepAudio;
        EnemyAudio.Play();
    }

}
