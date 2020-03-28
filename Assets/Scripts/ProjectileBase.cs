using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ProjectileType
{
    Standard,
    Seeker,
    Cluster
}

public enum DamageType
{
    Direct,
    Explosion
}

public class ProjectileBase: MonoBehaviour
{

    private Transform target;
    public float projectileSpeed = 50f;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 direction = target.position - transform.position;
        float distanceThisFrame = projectileSpeed * Time.deltaTime;

        if (direction.magnitude <= distanceThisFrame)
        {
            HitTarget();
            return;
        }

        transform.Translate(direction.normalized * distanceThisFrame, Space.World);

        DestroyProjectile();
    }

    public void SeekTarget(Transform shootTarget)
    {
        target = shootTarget;
    }

    private void HitTarget()
    {
        Destroy(gameObject);
    }

    private void DestroyProjectile()
    {
        Destroy(gameObject, 3f);
    }

    void OnParticleCollision(GameObject other)
    {
        Debug.Log("Hit Player");
    }
}
