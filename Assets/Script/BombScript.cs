
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BombScript : MonoBehaviour
{
    public float explosionRadius = 5f;
    public float explosionForce = 100f;
    public float slowDuration = 5f;
    public float delay = 3f;
    public AudioClip explosionSound;

    public ParticleSystem explosionVFX;

    public GameObject LineVFX;
    private ParticleSystem[] lineParticleSystems;
    float countdown;

    bool hasExploded = false;
    bool hasBeenThrown = false;
    bool canExplode = false;

    void Start()
    {
        countdown = delay;
        lineParticleSystems = LineVFX.GetComponentsInChildren<ParticleSystem>(true);
        // foreach (ParticleSystem ps in lineParticleSystems)
        // {
        //     ps.Stop();
        // }
    }

    void Update()
    {
        if (hasBeenThrown)
        {
            countdown -= Time.deltaTime;
            if (countdown <= 0f && !hasExploded)
            {
                Explode();
                hasExploded = true;
            }
        }
    }

    public void SetThrown()
    {
        foreach (ParticleSystem ps in lineParticleSystems)
        {
            ps.Play();
        }

        hasBeenThrown = true;
    }

    public void SetCanExplode()
    {
        canExplode = true;
    }

    public void Explode()
    {
        if (!canExplode)
        {
            return;
        }

        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider hit in colliders)
        {
            if (hit.CompareTag("Enemy"))
            {
                EnemyLogic enemyMovement = hit.GetComponent<EnemyLogic>();
                if (enemyMovement != null)
                {
                    enemyMovement.ApplySlow(slowDuration);
                }
            }
            else if (hit.CompareTag("Building"))
            {
                Debug.Log("Jangan Meledakan Bangunan");
            }
        }

        if (explosionSound != null && explosionVFX != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, transform.position);
            explosionVFX.Play();

            // Tambahkan handler untuk mendeteksi selesainya efek visual
            StartCoroutine(DestroyAfterVFX());
        }

    }

    IEnumerator DestroyAfterVFX()
    {
        while (explosionVFX.isPlaying)
        {
            yield return null;
        }
        Destroy(gameObject);
    }
}
