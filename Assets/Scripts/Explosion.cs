using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    public float radius = 5.0F;
    public float power = 10.0F;
    public bool shakeCamera = true;             // Give camera shaking effects to nearby cameras that have the vibration component
    public float cameraShakeViolence = 0.5f;	// The violence of the camera shake effect
    public LayerMask layerMask;
    private Vibration vibration;

    void OnEnable()
    {
        vibration = FindObjectOfType<Vibration>();
        Vector3 explosionPos = transform.position;
        Collider[] colliders = Physics.OverlapSphere(explosionPos, radius, layerMask);
        if (shakeCamera && vibration)
        {
            float shakeViolence = 1 / (Vector3.Distance(transform.position, vibration.transform.position) * cameraShakeViolence);
            vibration.StartShakingRandom(-shakeViolence, shakeViolence, -shakeViolence, shakeViolence);
        }
        foreach (Collider col in colliders)
        {
            Rigidbody rb = col.gameObject.AddComponent<Rigidbody>();
            // Shake the camera if it has a vibration component
            rb.isKinematic = false;
            rb.AddExplosionForce(power, explosionPos, radius, 3.0F);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
