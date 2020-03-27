using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Turret : MonoBehaviour
{
    private readonly int TurretFireAnimID = Animator.StringToHash("Attack");

    private Transform shootTarget;
    public float turretHealth = 100f;
    public bool isDamaged;
    public float shootRange = 15f;
    public string targetTag = "Player";
    public Transform partToRotate;
    public float rotationSpeed = 5f;
    public Transform firePoint;
    //public GameObject projectilePrefab;
    public GameObject rocketTrailFx;
    public float fireRate = 1.0f;
    public float fireCountdown = 0.0f;

    private AudioSource audioSource;
    public Animator animator;
    public GameObject ExplosionFx;
    public AudioClip missileSoundFx;
    public AudioClip explosionSoundFx;
    public AudioClip metalDebrisSoundFx;

    // Use this for initialization
    void Start()
    {
        InvokeRepeating("UpdateTarget", 0.0f, 1f);
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (shootTarget == null || isDamaged)
        {
            return;
        }

        LockTarget();

        if (fireCountdown <= 0)
        {
            Shoot();
            fireCountdown = 1.0f / fireRate;
        }

        fireCountdown -= Time.deltaTime;
    }

    /// <summary>
    /// Update nearest targets twice a second
    /// </summary>
    private void UpdateTarget()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag(targetTag);
        float shortestDistance = Mathf.Infinity;
        GameObject nearestTarget = null;

        foreach (GameObject target in targets)
        {
            float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);

            if (distanceToTarget < shortestDistance)
            {
                shortestDistance = distanceToTarget;
                nearestTarget = target;
            }
        }

        if (nearestTarget != null && shortestDistance <= shootRange)
        {
            shootTarget = nearestTarget.transform;
        }
        else
        {
            shootTarget = null;
        }
    }

    /// <summary>
    /// Lock target and Rotate accordoing to target direction
    /// </summary>
    private void LockTarget()
    {
        Vector3 direction = shootTarget.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        Quaternion lerpValue = Quaternion.Lerp(partToRotate.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        Vector3 rotation = lerpValue.eulerAngles;
        partToRotate.rotation = Quaternion.Euler(0.0f, rotation.y, 0.0f);
    }

    /// <summary>
    /// Seek and Shoot the target
    /// </summary>
    private void Shoot()
    {
        //GameObject projectileClone = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation) as GameObject;

        audioSource.PlayOneShot(missileSoundFx, 0.5f);
        animator.SetTrigger(TurretFireAnimID);
        GameObject rocketTrailFxClone = Instantiate(rocketTrailFx, firePoint.position, firePoint.rotation);
        ProjectileBase projectile = rocketTrailFxClone.GetComponent<ProjectileBase>();

        if (projectile != null)
        {
            projectile.SeekTarget(shootTarget);
        }
    }

    /// <summary>
    /// Destroys the turret upon inflicting enough damage
    /// </summary>
    public void DestroyTurret()
    {
        isDamaged = true;
        animator.enabled = false;
        audioSource.PlayOneShot(explosionSoundFx, 0.7f);
        GameObject explosionFxClone = Instantiate(ExplosionFx, transform.position, transform.rotation);
        GetComponent<Explosion>().enabled = true;
        StartCoroutine(MetalDebrisSoundFxCoroutine());
        Destroy(explosionFxClone, 10f);
        CancelInvoke();
    }

    IEnumerator MetalDebrisSoundFxCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        audioSource.PlayOneShot(metalDebrisSoundFx, 0.2f);
    }

    /// <summary>
    /// Simulates the radius/shooting range of the Turret 
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shootRange);
    }
}
