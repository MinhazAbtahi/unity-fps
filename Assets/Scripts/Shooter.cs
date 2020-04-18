using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;

[RequireComponent(typeof(WeaponManager))]
public class Shooter : MonoBehaviour
{

    #region Fields
    private const string PLAYER_TAG = "Player";

    [Header("ECS Bullets: ")]
    EntityManager manager;
    GameObjectConversionSettings settings;
    Entity bulletEntityPrefab;

    [Space(5)]
    [Header("Shooter Settings:")]
    public bool useECS;
    [SerializeField]
    private Weapon currentWeapon;
    [SerializeField]
    private Transform weaponBarrel;
    private WeaponManager weaponManager;
    private WeaponGraphics weaponGraphics;
    [SerializeField]
    private bool spreadShot;
    [SerializeField]
    private int spreadAmount;
    [SerializeField]
    private GameObject bulletPrefab;
    [SerializeField]
    private Camera playerCamera;
    [SerializeField]
    private LayerMask layerMask;

    [Space(5)]
    [Header("BulletHole Settings:")]
    public bool makeBulletHoles = true;               
    public BulletHoleFilter bulletHoleFilter = BulletHoleFilter.Tag; 
    private List<BulletHoleGroup> bulletHoleGroups = new List<BulletHoleGroup>();
    public List<BulletHolePool> defaultBulletHoles;
    private List<BulletHoleGroup> bulletHoleExceptions = new List<BulletHoleGroup>();

    #endregion

    #region Mono/NetworkBehaviour
    // Use this for initialization
    void Start()
    {
        if (useECS)
        {
            manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);
            bulletEntityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(bulletPrefab, settings);
        }

        if (!playerCamera)
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
        this.weaponBarrel = this.weaponGraphics.weaponBarrel.transform;
        //this.bulletPrefab = this.weaponGraphics.bulletPrefab;

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
        if (/*!isLocalPlayer || */weaponManager.isReloading)
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

        Vector3 origin = playerCamera.transform.position;
        Vector3 direction = playerCamera.transform.forward;
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
            // Creates bullet holes
            //MakeBulletHoles(hitInfo);
        }

        // Spawns Bullets
        CmdOnSpawnBullets();

        // Calls on the server for each CartidgeEjection
        CmdOnCartidgeEjecetEffect();

        if (this.currentWeapon.bullets <= 0)
        {
            this.weaponManager.Reload();
        }
    }

    private void CmdOnSpawnBullets()
    {
        Vector3 rotation = playerCamera.transform.rotation.eulerAngles;
        rotation.x = 0f;

        if (useECS)
        {
            if (spreadShot)
                SpawnBulletSpreadECS(rotation);
            else
                SpawnBulletECS(rotation);
        }
        else
        {
            if (spreadShot)
                SpawnBulletSpread(rotation);
            else
                SpawnBullet(rotation);
        }
    }

    void SpawnBullet(Vector3 rotation)
    {
        GameObject bullet = Instantiate(bulletPrefab) as GameObject;

        bullet.transform.position = weaponBarrel.position;
        bullet.transform.rotation = Quaternion.Euler(rotation);
        bullet.SetActive(true);
    }

    void SpawnBulletSpread(Vector3 rotation)
    {
        int max = spreadAmount / 2;
        int min = -max;

        Vector3 tempRot = rotation;
        for (int x = min; x < max; x++)
        {
            tempRot.x = (rotation.x + 3 * x) % 360;

            for (int y = min; y < max; y++)
            {
                tempRot.y = (rotation.y + 3 * y) % 360;

                GameObject bullet = Instantiate(bulletPrefab) as GameObject;

                bullet.transform.position = weaponBarrel.position;
                bullet.transform.rotation = Quaternion.Euler(tempRot);
                bullet.SetActive(true);
            }
        }
    }

    void SpawnBulletECS(Vector3 rotation)
    {
        Entity bullet = manager.Instantiate(bulletEntityPrefab);

        manager.SetComponentData(bullet, new Translation { Value = weaponBarrel.position });
        manager.SetComponentData(bullet, new Rotation { Value = Quaternion.Euler(rotation) });
    }

    void SpawnBulletSpreadECS(Vector3 rotation)
    {
        int max = spreadAmount / 2;
        int min = -max;
        int totalAmount = spreadAmount * spreadAmount;

        Vector3 tempRot = rotation;
        int index = 0;

        NativeArray<Entity> bullets = new NativeArray<Entity>(totalAmount, Allocator.TempJob);
        manager.Instantiate(bulletEntityPrefab, bullets);

        for (int x = min; x < max; x++)
        {
            tempRot.x = (rotation.x + 3 * x) % 360;

            for (int y = min; y < max; y++)
            {
                tempRot.y = (rotation.y + 3 * y) % 360;

                manager.SetComponentData(bullets[index], new Translation { Value = weaponBarrel.position });
                manager.SetComponentData(bullets[index], new Rotation { Value = Quaternion.Euler(tempRot) });

                index++;
            }
        }
        bullets.Dispose();
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
        foreach (BulletHoleGroup bhg in bulletHoleExceptions)
        {
            switch (bulletHoleFilter)
            {
                case BulletHoleFilter.Tag:
                    if (hit.collider.gameObject.tag == bhg.tag)
                    {
                        exception = true;
                    }
                    break;
                case BulletHoleFilter.Material:
                    MeshRenderer mesh = FindMeshRenderer(hit.collider.gameObject);
                    if (mesh != null)
                    {
                        if (mesh.sharedMaterial == bhg.material)
                        {
                            exception = true;
                        }
                    }
                    break;
                case BulletHoleFilter.Physic_Material:
                    if (hit.collider.sharedMaterial == bhg.physicMaterial)
                    {
                        exception = true;
                    }
                    break;
                default:
                    break;
            }            
        }
     
        // Select the bullet hole pools if there is no exception
        if (makeBulletHoles && !exception)
        {
            // A list of the bullet hole prefabs to choose from
            List<BulletHoleGroup> holes = new List<BulletHoleGroup>();

            // Display the bullet hole groups based on tags
            if (bulletHoleFilter == BulletHoleFilter.Tag)
            {
                foreach (BulletHoleGroup bhg in bulletHoleGroups)
                {
                    if (hit.collider.gameObject.tag == bhg.tag)
                    {
                        holes.Add(bhg);
                    }
                }
            }

            // Display the bullet hole groups based on materials
            else if (bulletHoleFilter == BulletHoleFilter.Material)
            {
                // Get the mesh that was hit, if any
                MeshRenderer mesh = FindMeshRenderer(hit.collider.gameObject);

                foreach (BulletHoleGroup bhg in bulletHoleGroups)
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
            else if (bulletHoleFilter == BulletHoleFilter.Physic_Material)
            {
                foreach (BulletHoleGroup bhg in bulletHoleGroups)
                {
                    if (hit.collider.sharedMaterial == bhg.physicMaterial)
                    {
                        holes.Add(bhg);
                    }
                }
            }


            BulletHoleGroup bulletHoleGroup = null;

            // If no bullet holes were specified for this parameter, use the default bullet holes
            if (holes.Count == 0)   // If no usable (for this hit GameObject) bullet holes were found...
            {
                List<BulletHoleGroup> defaultsToUse = new List<BulletHoleGroup>();
                foreach (BulletHolePool h in defaultBulletHoles)
                {
                    defaultsToUse.Add(new BulletHoleGroup("Default", null, null, h));
                }

                // Choose a bullet hole at random from the list
                bulletHoleGroup = defaultsToUse[Random.Range(0, defaultsToUse.Count)];
            }

            // Make the actual bullet hole GameObject
            else
            {
                // Choose a bullet hole at random from the list
                bulletHoleGroup = holes[Random.Range(0, holes.Count)];
            }

            // Place the bullet hole in the scene
            if (bulletHoleGroup.bulletHole != null)
                bulletHoleGroup.bulletHole.PlaceBulletHole(hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));
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
