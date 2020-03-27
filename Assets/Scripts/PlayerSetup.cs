using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Player))]
[RequireComponent(typeof(Player))]
public class PlayerSetup : NetworkBehaviour
{
    #region Fields
    [Header("Player Setup Settings:")]
    [SerializeField]
    private Behaviour[] componentsToDisable;

    [Header("Layer Settings:")]
    [SerializeField]
    private string remoteLayerName = "RemotePlayer";
    [SerializeField]
    private string ignoreGraphicsLayerName = "IgnoreGraphics";
    [SerializeField]
    private GameObject playerGraphics;

    [Header("PlayerUI & Crosshair Settings:")]
    [SerializeField]
    private GameObject playerUIPrefab;
    [HideInInspector]
    public GameObject playerUIInstance;
    #endregion

    #region Mono/NetworkBehaviour
    // Use this for initialization
    void Start()
    {
        if (this.playerGraphics == null)
        {
            Debug.LogError("PlayerSetup: playerGraphics reference is Missing!");
        }

        if (this.playerUIPrefab == null)
        {
            Debug.LogError("PlayerSetup: playerUIPrefab reference is Missing!");
        }

        if (!isLocalPlayer)
        {
            this.DisableComponents();
            this.SetRemoteLayer();
        }
        else
        {
            // Disable Player Graphics for local player
            int layerIndex = LayerMask.NameToLayer(this.ignoreGraphicsLayerName);
            this.SetLayerRecursively(this.playerGraphics, layerIndex);

            // Create PlayerUI
            this.playerUIInstance = Instantiate(this.playerUIPrefab) as GameObject;
            this.playerUIInstance.name = this.playerUIPrefab.name;

            // Configure PlayerUI
            PlayerUI playerUI = this.playerUIInstance.GetComponent<PlayerUI>();
            if (playerUI == null)
            {
                Debug.LogError("Player Setup: PlayerUI component is Missing in PlayerUI Prefab!");
            }
            playerUI.SetPlayer(GetComponent<Player>());

            // Local Player Setup
            GetComponent<Player>().SetupPlayer();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        string netID = GetComponent<NetworkIdentity>().netId.ToString();
        Player player = GetComponent<Player>();
        GameManager.RegisterPlayer(netID, player);
    }

    void OnDisable()
    {
        // Destroy PlayerUIPrefab
        Destroy(playerUIInstance);

        if (isLocalPlayer)
        {
            // Re-enable scene camera
            GameManager.singletonInstance.SetSceneCameraActive(true);
        }

        string playerID = this.transform.name;
        GameManager.UnRegisterPlayer(playerID);
    }
    #endregion

    #region Player_Setup_Methods
    /// <summary>
    /// Set Remote Player layer in Runtime
    /// </summary>
    private void SetRemoteLayer()
    {
        int layerIndex = LayerMask.NameToLayer(this.remoteLayerName);
        this.gameObject.layer = layerIndex;
    }

    /// <summary>
    /// Disable a collection of components
    /// </summary>
    private void DisableComponents()
    {
        for (int i = 0; i < componentsToDisable.Length; i++)
        {
            componentsToDisable[i].enabled = false;
        }
    }

    /// <summary>
    /// Set layers Recursively to the nested GameObjects
    /// </summary>
    /// <param name="gameObject"></param>
    /// <param name="layerIndex"></param>
    private void SetLayerRecursively(GameObject _gameObject, int layerIndex)
    {
        _gameObject.layer = layerIndex;

        foreach (Transform child in _gameObject.transform)
        {
            SetLayerRecursively(child.gameObject, layerIndex);
        }
    }
    #endregion
}
