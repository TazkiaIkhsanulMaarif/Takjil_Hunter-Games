using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLogic : MonoBehaviour
{
    private Rigidbody rb;
    public float walkspeed, runspeed, jumppower, fallspeed, airMultiplier, HitPoints = 100f;
    public Transform PlayerOrientation;
    float horizontalInput;
    float verticalInput;
    Vector3 moveDirection;
    bool grounded = true, aerialboost = true;
    public Animator anim;
    private bool AimMode = false, TPSMode = true;
    private bool HoldingBomb, PickingUp = false, Throwing, Falling = false, GameOver = false;
    public CameraLogic camlogic;
    private GameObject bombToPickUp;
    public GameObject handPosition;
    private BombScript heldBomb;
    public float maxThrowDistance = 10f;
    public float throwForce = 10f;

    [Header("Display Controls")]
    [SerializeField]
    private LineRenderer lineRenderer;
    [SerializeField]
    private int lineSegments = 60;

    [SerializeField, Min(3)]
    private float timeOfTheFlight = 5;
    private Vector3 startpoint, startVelocity;
    private Vector3[] trajectoryPoints;

    [Header("SFX")]
    public AudioClip ThrowAudio;
    public AudioClip DeathAudio;
    public AudioClip StepAudio;
    public AudioClip RunAudio;
    AudioSource PlayerAudio;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        PlayerAudio = GetComponent<AudioSource>();
        trajectoryPoints = new Vector3[lineSegments];
    }

    void Update()
    {
        Movement();
        Jump();
        StartDrawingTrajectory();
        AimModeAdjuster();
        CheckBrokenRoadWarning();
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!HoldingBomb && !PickingUp)
            {
                // Mengambil bom
                PickUpBomb();
            }
            else if (HoldingBomb && PickingUp)
            {
                // Membatalkan pengambilan bom
                PickingUp = false;
                HoldingBomb = false;
                StopHeldAnimation();
                StopDrawingTrajectory();
            }
        }

        if (Input.GetKey(KeyCode.R) && HoldingBomb && PickingUp)
        {
            // Pemain melempar bom
            ThrowBomb();
            StartCoroutine(ResetHoldingBomb(0.3f));
        }
        if (HoldingBomb && !PickingUp)
        {
            // Posisi bom mengikuti tangan pemain
            bombToPickUp.transform.position = handPosition.transform.position;
        }
    }

    private void StopHeldAnimation()
    {
        anim.SetBool("HoldingBomb", false);
        StopDrawingTrajectory();
    }

    private IEnumerator ResetHoldingBomb(float delay)
    {
        yield return new WaitForSeconds(delay);
        HoldingBomb = false;
        anim.SetBool("HoldingBomb", false);
    }

    void PickUpBomb()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 3f);
        foreach (Collider col in hitColliders)
        {
            if (col.CompareTag("Bomb"))
            {
                bombToPickUp = col.gameObject;
                bombToPickUp.GetComponent<Rigidbody>().isKinematic = true;
                bombToPickUp.transform.SetParent(handPosition.transform);
                bombToPickUp.transform.localPosition = Vector3.zero;
                lineRenderer.SetPosition(0, handPosition.transform.position);

                // Aktifkan trigger "HoldingBomb" pada animator pemain
                anim.SetBool("HoldingBomb", true);
                HoldingBomb = true;
                PickingUp = true;
                startpoint = handPosition.transform.position;
                startVelocity = PlayerOrientation.forward * throwForce;
            }
        }
    }

    void ThrowBomb()
    {
        if (bombToPickUp != null)
        {
            anim.SetTrigger("Throwing");

            bombToPickUp.GetComponent<Rigidbody>().isKinematic = false;
            bombToPickUp.transform.SetParent(null); // Lepaskan dari tangan pemain
            bombToPickUp.GetComponent<Rigidbody>().velocity = PlayerOrientation.forward * throwForce;
            heldBomb = bombToPickUp.GetComponent<BombScript>();
            bombToPickUp = null;
            HoldingBomb = false;
            PickingUp = false;
            StopDrawingTrajectory();
            ThrowingFinished();
        }
    }

    void StartDrawingTrajectory()
    {
        if (HoldingBomb)
        {
            startpoint = handPosition.transform.position;
            startVelocity = PlayerOrientation.forward * throwForce;
            float timeStep = timeOfTheFlight / lineSegments;
            Vector3[] lineRendererPoints = CalculateTrajectoryLine(startpoint, startVelocity, timeStep);
            lineRenderer.positionCount = lineSegments;
            for (int i = 0; i < lineSegments; i++)
            {
                lineRenderer.SetPosition(i, lineRendererPoints[i]);
            }
        }
    }

    private Vector3[] CalculateTrajectoryLine(Vector3 startpoint, Vector3 startVelocity, float timeStep)
    {
        Vector3[] lineRendererPoints = new Vector3[lineSegments];
        lineRendererPoints[0] = startpoint;

        for (int i = 1; i < lineSegments; i++)
        {
            float timeOffset = timeStep * i;

            Vector3 progressBeforeGravity = startVelocity * timeOffset;
            Vector3 gravityOffset = Vector3.up * -0.5f * Physics.gravity.y * timeOffset * timeOffset;
            Vector3 newPosition = startpoint + progressBeforeGravity - gravityOffset;
            lineRendererPoints[i] = newPosition;
        }
        return lineRendererPoints;
    }

    void StopDrawingTrajectory()
    {
        lineRenderer.positionCount = 0;
    }

    public void ThrowingFinished()
    {
        Throwing = false;
        PickingUp = false;
        if (heldBomb != null)
        {
            heldBomb.Explode();
            heldBomb.SetThrown();
            heldBomb.SetCanExplode();
        }
    }

    public void AimModeAdjuster()
    {
        if (!HoldingBomb)
        {
            TPSMode = true;
            AimMode = false;
        }
        else if (HoldingBomb)
        {
            TPSMode = false;
            AimMode = true;
        }
        camlogic.CameraModeChanger(TPSMode, AimMode);
    }

    void Movement()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (!anim.GetBool("IdleShoot") && !anim.GetBool("Death") && !anim.GetBool("HoldingBomb"))
        {
            moveDirection = PlayerOrientation.forward * verticalInput + PlayerOrientation.right * horizontalInput;

            if (grounded && moveDirection != Vector3.zero)
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    anim.SetBool("Run", true);
                    anim.SetBool("Walk", false);
                    rb.AddForce(moveDirection.normalized * runspeed * 10f, ForceMode.Force);
                }
                else
                {
                    anim.SetBool("Walk", true);
                    anim.SetBool("Run", false);
                    rb.AddForce(moveDirection.normalized * walkspeed * 10f, ForceMode.Force);
                }
            }
            else
            {
                anim.SetBool("Walk", false);
                anim.SetBool("Run", false);
            }
        }
        else
        {
            anim.SetBool("Walk", false);
            anim.SetBool("Run", false);
        }
    }

    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && grounded)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(transform.up * jumppower, ForceMode.Impulse);
            grounded = false;
            anim.SetBool("Jump", true);
        }
        else if (!grounded)
        {
            rb.AddForce(Vector3.down * fallspeed * rb.mass, ForceMode.Force);
            if (aerialboost)
            {
                rb.AddForce(moveDirection.normalized * walkspeed * 10f * airMultiplier, ForceMode.Impulse);
                aerialboost = false;
            }
        }
    }
    void CheckBrokenRoadWarning()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 2f);

        foreach (Collider col in hitColliders)
        {
            if (col.CompareTag("BrokenRoad"))
            {
                Debug.Log("Peringatan: Mendekati Jalan Rusak!");
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("BrokenRoad"))
        {
            if (!Falling)
            {
                // Saat pemain bersentuhan dengan objek yang ditandai sebagai "FallTrigger"
                Falling = true;
                rb.useGravity = true;
                // gameManager.GameOver();
            }
            else
            {
                // Saat pemain bersentuhan dengan objek yang ditandai sebagai "BrokenRoad" setelah jatuh
                Debug.Log("Anda Telah melewati Jalan Rusak setelah jatuh!");
            }
        }
    }

    public void step()
    {
        PlayerAudio.clip = StepAudio;
        PlayerAudio.Play();
    }

    public void run()
    {
        PlayerAudio.clip = RunAudio;
        PlayerAudio.Play();
    }

    public void death()
    {
        PlayerAudio.clip = DeathAudio;
        PlayerAudio.Play();
    }

    public void groundedchanger()
    {
        grounded = true;
        aerialboost = true;
        anim.SetBool("Jump", false);
    }

    public void PlayerGetHit(float damage)
    {
        HitPoints -= damage;
        Debug.Log("Player Receive Damage - " + damage);

        if (HitPoints <= 0f)
        {
            HitPoints = 0f;
            anim.SetBool("Death", true);
        }
    }

    public void TriggerGameOver()
    {
        if (!GameOver)
        {
            GameOver = true;
            Debug.Log("Game Over!");
            // Tambahkan logika tambahan di sini, seperti menampilkan layar game over atau mengatur ulang level.
            // Contoh:
            // SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
