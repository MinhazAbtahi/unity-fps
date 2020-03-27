using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(PlayerSetup))]
[RequireComponent(typeof(WeaponManager))]
public class Player : NetworkBehaviour
{
    #region Fields
    [Header("Player Setup:")]
    [SerializeField]
    private int maxHealth = 100;
    private int currentHealth;

    [SerializeField]
    public Behaviour[] componentsToDisable;
    public GameObject[] gameObjectsToDisable;
    private bool[] wasEnabled;

    [Header("Spawn/Death Effects")]
    [SerializeField]
    private GameObject deathEffect;
    [SerializeField]
    private GameObject spawnEffect;

    private bool isDead = false;

    private bool firstSetup = true;
    #endregion

    #region Properties
    public bool IsDead
    {
        get
        {
            return isDead;
        }

        protected set
        {
            isDead = value;
        }
    }
    #endregion

    #region Mono/NetworkBehaviour
    // Use this for initialization
    void Start()
    {
        if (this.deathEffect == null)
        {
            Debug.Log("Player: Death Effect reference is Missing!");
        }

        if (this.spawnEffect == null)
        {
            Debug.Log("Player: Spawn Effect reference is Missing!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        //if (Input.GetKeyDown(KeyCode.K))
        //{
        //    RpcTakeDamage(9999);
        //}
    }
    #endregion

    #region Player_Setup
    /// <summary>
    /// Player Setup 
    /// </summary>
    public void SetupPlayer()
    {
        if (isLocalPlayer)
        {
            // Switch Camera
            GameManager.singletonInstance.SetSceneCameraActive(false);
            GetComponent<PlayerSetup>().playerUIInstance.SetActive(true);
        }

        // Execute Player Setup Command on server
        CmdBrodcastNewPlayerSetup();
    }

    //[Command]
    private void CmdBrodcastNewPlayerSetup()
    {
        RpcSetupPlayerOnAllClients();
    }

    //[ClientRpc]
    private void RpcSetupPlayerOnAllClients()
    {
        if (firstSetup)
        {
            wasEnabled = new bool[componentsToDisable.Length];

            for (int i = 0; i < wasEnabled.Length; i++)
            {
                wasEnabled[i] = componentsToDisable[i].enabled;
            }

            firstSetup = false;
        }

        this.SetDefaults();
    }

    /// <summary>
    /// Returns health percentage between 0 & 1
    /// </summary>
    /// <returns></returns>
    public float GetHealthPercentage()
    {
        return (float)this.currentHealth / this.maxHealth;
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetDefaults()
    {
        this.IsDead = false;

        this.currentHealth = this.maxHealth;

        // Enable components on respawn
        for (int i = 0; i < componentsToDisable.Length; i++)
        {
            componentsToDisable[i].enabled = wasEnabled[i];
        }

        // Enable gameObjects on respawn
        for (int i = 0; i < gameObjectsToDisable.Length; i++)
        {
            gameObjectsToDisable[i].SetActive(true);
        }

        // Enable colliders on respawn
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = true;
        }

        // Creates spawn effect
        GameObject spawnEffectClone = Instantiate(this.spawnEffect, this.transform.position, Quaternion.identity) as GameObject;

        // Destroys instantiated gameObject after a certain amount of time
        Destroy(spawnEffectClone, 3.0f);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="damageAmount"></param>
    //[ClientRpc]
    public void RpcTakeDamage(int damageAmount)
    {
        if (IsDead)
        {
            return;
        }

        this.currentHealth -= damageAmount;

        Debug.Log(this.transform.name + " has now " + this.currentHealth + " Health!");

        if (this.currentHealth <= 0)
        {
            this.Die();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void Die()
    {
        this.IsDead = true;

        // Disable components on death
        for (int i = 0; i < componentsToDisable.Length; i++)
        {
            componentsToDisable[i].enabled = false;
        }

        // Disable gameObjects on death
        for (int i = 0; i < gameObjectsToDisable.Length; i++)
        {
            gameObjectsToDisable[i].SetActive(false);
        }

        // Disable collider on deatch
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        // Creates death effect
        GameObject deathEffectClone = Instantiate(deathEffect, this.transform.position, Quaternion.identity) as GameObject;

        // Destroys instantiated gameObject after a certain amount of time
        Destroy(deathEffectClone, 3.0f);

        // Switch camera
        if (isLocalPlayer)
        {
            GameManager.singletonInstance.SetSceneCameraActive(true);
            GetComponent<PlayerSetup>().playerUIInstance.SetActive(false);
        }

        Debug.Log(this.transform.name + " has Died!");

        // Respawn after a certain time
        StartCoroutine(Respawn());
    }

    /// <summary>
    /// Respawn Player after a certain amount of time
    /// </summary>
    /// <returns></returns>
    private IEnumerator Respawn()
    {
        float respawnTime = GameManager.singletonInstance.matchSettings.respawnTime;
        yield return new WaitForSeconds(respawnTime);

        // respawn Player in the registered spawn position
        Transform respawnPoint = NetworkManager.singleton.GetStartPosition();
        this.transform.position = respawnPoint.position;
        this.transform.rotation = respawnPoint.rotation;

        yield return new WaitForSeconds(0.2f);

        // Player Setup
        SetupPlayer();

        Debug.Log(this.transform.name + " Respawned at position " + this.transform.position);
    }
    #endregion
}
