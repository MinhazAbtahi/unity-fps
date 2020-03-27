using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(WeaponManager))]
public class Shooter : NetworkBehaviour
{

    #region Fields
    private const string PLAYER_TAG = "Player";

    [Header("Shooter Settings:")]
    [SerializeField]
    private Weapon currentWeapon;
    private WeaponManager weaponManager;
    private WeaponGraphics weaponGraphics;
    [SerializeField]
    private Camera playerCam;
    [SerializeField]
    private LayerMask layerMask;


    #endregion

    #region Mono/NetworkBehaviour
    // Use this for initialization
    void Start()
    {
        if (!playerCam)
        {
            Debug.LogError("Shooter: Player Camera reference is Missing!");
            this.enabled = false;
        }

        this.weaponManager = GetComponent<WeaponManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (PauseMenu.isOn)
        {
            return;
        }

        this.currentWeapon = this.weaponManager.GetCurrentWeapon();
        this.weaponGraphics = weaponManager.GetCurrentWeaponGraphics();

        if (this.currentWeapon.bullets < this.currentWeapon.maxBullets)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                this.weaponManager.Reload();

                return;
            }
        }

        if (this.currentWeapon.fireRate <= 0f)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                this.Shoot();
            }
        }
        else
        {
            if (Input.GetButtonDown("Fire1"))
            {
                InvokeRepeating("Shoot", 0f, 1f / currentWeapon.fireRate);
            }
            else if (Input.GetButtonUp("Fire1"))
            {
                CancelInvoke("Shoot");
                weaponGraphics.weaponAnimator.StopPlayback();
            }
        }
    }
    #endregion

    #region Server-Client Communication
    /// <summary>
    /// Weapon Shoot/Fire Method for each Local Client
    /// </summary>
    private void Shoot()
    {
        if (!isLocalPlayer || weaponManager.isReloading)
        {
            return;
        }

        // Reload
        if (this.currentWeapon.bullets <= 0)
        {
            this.weaponManager.Reload();

            return;
        }

        // Decrese bullet amount in each shoot
        this.currentWeapon.bullets--;

        // Calls on the server for each client shoot
        CmdOnShoot();

        // Recoil Animation
        weaponGraphics.PlayRecoilAnim();

        Vector3 origin = playerCam.transform.position;
        Vector3 direction = playerCam.transform.forward;
        float maxDistance = currentWeapon.range;
        RaycastHit hitInfo;

        if (Physics.Raycast(origin, direction, out hitInfo, maxDistance, this.layerMask))
        {
            //Debug.Log("Hit at " + hitInfo.collider.name);
            if (hitInfo.collider.tag == PLAYER_TAG)
            {
                string playerID = hitInfo.collider.name;
                int damageAmount = currentWeapon.damage;

                // Calls on the server for each player shot
                CmdOnPlayerShot(playerID, damageAmount);
            }

            // If player hits Enemy Turret
            if (hitInfo.collider.tag == "EnemyTurret")
            {
                Turret turretClone = hitInfo.collider.gameObject.GetComponentInChildren<Turret>();
                turretClone.turretHealth -= 4f;
                if (turretClone.turretHealth == 0f)
                {
                    turretClone.DestroyTurret();
                }
            }

            // Calls on the server for each weapon hit impact
            CmdOnCartidgeEjecetEffect();

            // Calls on the server for each weapon hit impact
            CmdOnHitImpact(hitInfo.point, hitInfo.normal);
        }

        if (this.currentWeapon.bullets <= 0)
        {
            this.weaponManager.Reload();
        }
    }

    private void CmdOnPlayerShot(string playerID, int damageAmount)
    {
        Debug.Log(playerID + " has been Shot!");

        Player player = GameManager.GetPlayer(playerID);
        player.RpcTakeDamage(damageAmount);
    }

    /// <summary>
    /// Executes on server for each local client shoot
    /// </summary>
    private void CmdOnShoot()
    {
        RpcShootEffect();
    }

    /// <summary>
    /// Executes on every client for OnShoot command to play shoot effects
    /// </summary>
    private void RpcShootEffect()
    {
        WeaponGraphics weaponGraphics = this.weaponManager.GetCurrentWeaponGraphics();
        WeaponSoundFx weaponSoundFx = this.weaponManager.GetCurrentWeaponSoundFx();

        // Enables MuzzleFlush at weapon BarrelPoint
        weaponGraphics.muzzleFlash.Play();
        weaponSoundFx.PlaySoundFx(weaponSoundFx.ShotSoundFx, 0.5f);
    }

    IEnumerator BulletShellSoundFxCoroutine()
    {
        yield return new WaitForSeconds(0.2f);
        WeaponSoundFx weaponSoundFx = this.weaponManager.GetCurrentWeaponSoundFx();
        weaponSoundFx.PlaySoundFx(weaponSoundFx.BulletShellSoundFx, 0.8f);
    }

    /// <summary>
    /// Executes on server for each local client weapon bullet eject effect
    /// </summary>
    private void CmdOnCartidgeEjecetEffect()
    {
        RpcOnCartidgrEjectEffect();
    }

    /// <summary>
    /// Executes on every client for OnCartidgeEffect command to play eject effects
    /// </summary>
    private void RpcOnCartidgrEjectEffect()
    {
        WeaponGraphics weaponGraphics = this.weaponManager.GetCurrentWeaponGraphics();
        
        // Enables CartidgeEject Effect 
        weaponGraphics.cartidgeEjectEffect.Play();
        StartCoroutine(BulletShellSoundFxCoroutine());
    }

    /// <summary>
    /// Executes on server for each local client weapon hit impact
    /// </summary>
    /// <param name="position"></param>
    /// <param name="normal"></param>
    private void CmdOnHitImpact(Vector3 position, Vector3 normal)
    {
        RpcHitImpactEffect(position, normal);
    }

    /// <summary>
    /// Executes on every client for OnShoot command to play hit impact effects
    /// </summary>
    /// <param name="position"></param>
    /// <param name="normal"></param>
    private void RpcHitImpactEffect(Vector3 position, Vector3 normal)
    {
        WeaponGraphics weaponGraphics = this.weaponManager.GetCurrentWeaponGraphics();
        GameObject hitImpactClone = Instantiate(weaponGraphics.hitImpactEffect, position, Quaternion.LookRotation(normal)) as GameObject;

        // Destroys instantiated gameObject after a certain amount of time
        Destroy(hitImpactClone, 1.0f);
    }
    #endregion

}
