using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public enum BulletHoleSystem
{
    Tag,
    Material,
    Physic_Material
}


[System.Serializable]
public class SmartBulletHoleGroup
{
    public string tag;
    public Material material;
    public PhysicMaterial physicMaterial;
    public BulletHolePool bulletHole;

    public SmartBulletHoleGroup()
    {
        tag = "Everything";
        material = null;
        physicMaterial = null;
        bulletHole = null;
    }
    public SmartBulletHoleGroup(string t, Material m, PhysicMaterial pm, BulletHolePool bh)
    {
        tag = t;
        material = m;
        physicMaterial = pm;
        bulletHole = bh;
    }
}


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

    [Space(5)]
    [Header("BulletHole Settings:")]
    public bool makeBulletHoles = true;                 // Whether or not bullet holes should be made
    public BulletHoleSystem bhSystem = BulletHoleSystem.Tag;    // What condition the dynamic bullet holes should be based off
    public List<string> bulletHolePoolNames = new
        List<string>();                                 // A list of strings holding the names of bullet hole pools in the scene
    public List<string> defaultBulletHolePoolNames =
        new List<string>();                             // A list of strings holding the names of default bullet hole pools in the scene
    public List<SmartBulletHoleGroup> bulletHoleGroups =
        new List<SmartBulletHoleGroup>();				// A list of bullet hole groups.  Each one holds a tag for GameObjects that might be hit, as well as a corresponding bullet hole
    public List<BulletHolePool> defaultBulletHoles =
        new List<BulletHolePool>();                     // A list of default bullet holes to be instantiated when none of the custom parameters are met
    public List<SmartBulletHoleGroup> bulletHoleExceptions =
        new List<SmartBulletHoleGroup>();               // A list of SmartBulletHoleGroup objects that defines conditions for when no bullet hole will be instantiated.
                                                        // In other words, the bullet holes in the defaultBulletHoles list will be instantiated on any surface except for
                                                        // the ones specified in this list.
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
        if (PauseMenu.isOn || !weaponManager.isEquiped)
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
            switch (this.currentWeapon.weaponType)
            {
                case WeaponType.Automatic:
                    if (Input.GetButtonDown("Fire1"))
                    {
                        InvokeRepeating("Shoot", 0f, 1f / currentWeapon.fireRate);
                    }
                    else if (Input.GetButtonUp("Fire1"))
                    {
                        CancelInvoke("Shoot");
                        weaponGraphics.weaponAnimator.StopPlayback();
                    }
                    break;
                case WeaponType.Burst:
                    if (Input.GetMouseButtonDown(0))
                    {
                        this.Shoot();
                    }
                    break;
                default:
                    break;
            }
        }
    }
    #endregion

    #region Base Functionalities
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
            CmdOnHitImpact(hitInfo.point, hitInfo.normal);
        }

        // Calls on the server for each CartidgeEjection
        CmdOnCartidgeEjecetEffect();

        if (this.currentWeapon.bullets <= 0)
        {
            this.weaponManager.Reload();
        }
    }

    /// <summary>
    /// Create Bullet holes on the surface of a valid Hit
    /// </summary>
    /// <param name="hit"></param>
    private void MakeBulletHoles(RaycastHit hit)
    {
        // Bullet Holes

        // Make sure the hit GameObject is not defined as an exception for bullet holes
        bool exception = false;
        if (bhSystem == BulletHoleSystem.Tag)
        {
            foreach (SmartBulletHoleGroup bhg in bulletHoleExceptions)
            {
                if (hit.collider.gameObject.tag == bhg.tag)
                {
                    exception = true;
                    break;
                }
            }
        }
        else if (bhSystem == BulletHoleSystem.Material)
        {
            foreach (SmartBulletHoleGroup bhg in bulletHoleExceptions)
            {
                MeshRenderer mesh = FindMeshRenderer(hit.collider.gameObject);
                if (mesh != null)
                {
                    if (mesh.sharedMaterial == bhg.material)
                    {
                        exception = true;
                        break;
                    }
                }
            }
        }
        else if (bhSystem == BulletHoleSystem.Physic_Material)
        {
            foreach (SmartBulletHoleGroup bhg in bulletHoleExceptions)
            {
                if (hit.collider.sharedMaterial == bhg.physicMaterial)
                {
                    exception = true;
                    break;
                }
            }
        }

        // Select the bullet hole pools if there is no exception
        if (makeBulletHoles && !exception)
        {
            // A list of the bullet hole prefabs to choose from
            List<SmartBulletHoleGroup> holes = new List<SmartBulletHoleGroup>();

            // Display the bullet hole groups based on tags
            if (bhSystem == BulletHoleSystem.Tag)
            {
                foreach (SmartBulletHoleGroup bhg in bulletHoleGroups)
                {
                    if (hit.collider.gameObject.tag == bhg.tag)
                    {
                        holes.Add(bhg);
                    }
                }
            }

            // Display the bullet hole groups based on materials
            else if (bhSystem == BulletHoleSystem.Material)
            {
                // Get the mesh that was hit, if any
                MeshRenderer mesh = FindMeshRenderer(hit.collider.gameObject);

                foreach (SmartBulletHoleGroup bhg in bulletHoleGroups)
                {
                    if (mesh != null)
                    {
                        if (mesh.sharedMaterial == bhg.material)
                        {
                            holes.Add(bhg);
                        }
                    }
                }
            }

            // Display the bullet hole groups based on physic materials
            else if (bhSystem == BulletHoleSystem.Physic_Material)
            {
                foreach (SmartBulletHoleGroup bhg in bulletHoleGroups)
                {
                    if (hit.collider.sharedMaterial == bhg.physicMaterial)
                    {
                        holes.Add(bhg);
                    }
                }
            }


            SmartBulletHoleGroup sbhg = null;

            // If no bullet holes were specified for this parameter, use the default bullet holes
            if (holes.Count == 0)   // If no usable (for this hit GameObject) bullet holes were found...
            {
                List<SmartBulletHoleGroup> defaultsToUse = new List<SmartBulletHoleGroup>();
                foreach (BulletHolePool h in defaultBulletHoles)
                {
                    defaultsToUse.Add(new SmartBulletHoleGroup("Default", null, null, h));
                }

                // Choose a bullet hole at random from the list
                sbhg = defaultsToUse[Random.Range(0, defaultsToUse.Count)];
            }

            // Make the actual bullet hole GameObject
            else
            {
                // Choose a bullet hole at random from the list
                sbhg = holes[Random.Range(0, holes.Count)];
            }

            // Place the bullet hole in the scene
            if (sbhg.bulletHole != null)
                sbhg.bulletHole.PlaceBulletHole(hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));
        }

    }

    /// <summary>
    /// Find a mesh renderer in a specified gameobject, it's children, or its parents
    /// </summary>
    /// <param name="gameObject"></param>
    /// <returns></returns>
    MeshRenderer FindMeshRenderer(GameObject gameObject)
    {
        MeshRenderer hitMesh;

        // Use the MeshRenderer directly from this GameObject if it has one
        if (gameObject.GetComponent<Renderer>() != null)
        {
            hitMesh = gameObject.GetComponent<MeshRenderer>();
        }

        // Try to find a child or parent GameObject that has a MeshRenderer
        else
        {
            // Look for a renderer in the child GameObjects
            hitMesh = gameObject.GetComponentInChildren<MeshRenderer>();

            // If a renderer is still not found, try the parent GameObjects
            if (hitMesh == null)
            {
                GameObject curGO = gameObject;
                while (hitMesh == null && curGO.transform != curGO.transform.root)
                {
                    curGO = curGO.transform.parent.gameObject;
                    hitMesh = curGO.GetComponent<MeshRenderer>();
                }
            }
        }

        return hitMesh;
    } 
    #endregion

    #region Server-Client Communication
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
