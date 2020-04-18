using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponGraphics : MonoBehaviour {

    public readonly int FireAnimID = Animator.StringToHash("Fire");

    public Animator weaponAnimator;
    public GameObject bulletPrefab;
    public Transform weaponBarrel;
    public ParticleSystem muzzleFlash;
    public ParticleSystem cartidgeEjectEffect;
    public GameObject hitImpactEffect;

	// Use this for initialization
	void Start ()
    {
        if (!weaponAnimator)
        {
            weaponAnimator = GetComponent<Animator>();
        }

        if (!weaponBarrel)
        {
            Debug.LogError("WeaponGraphics: WeaponBarrel reference is Missing!");
        }

        if (!bulletPrefab)
        {
            Debug.LogError("WeaponGraphics: BulletPrefab reference is Missing!");
        }

        if (!muzzleFlash)
        {
            Debug.LogError("WeaponGraphics: Muzzleflash reference is Missing!");
        }

        if (!cartidgeEjectEffect)
        {
            Debug.LogError("WeaponGraphics: CartidgeEjectEffect reference is Missing!");
        }

        if (!hitImpactEffect)
        {
            Debug.LogError("WeaponGraphics: HitImpactEffect reference is Missing!");
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    public void PlayRecoilAnim()
    {
        weaponAnimator.SetTrigger(FireAnimID);
    }
}
