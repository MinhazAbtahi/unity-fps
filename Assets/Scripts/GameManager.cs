using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Singleton Instance
    public static GameManager singletonInstance; 
    #endregion

    #region Fields
    private const string PLAYER_ID_PREFIX = "Player ";

    private static Dictionary<string, Player> players = new Dictionary<string, Player>();
    public bool enableVSync;
    public MatchSettings matchSettings;

    [Header("Scene Camera:")]
    [SerializeField]
    private GameObject sceneCamera;

    [Header("Player List GUI[Legacy] Label Settings:")]
    [SerializeField]
    private float x = 10;
    [SerializeField]
    private float y = 20;
    [SerializeField]
    private float width = 200;
    [SerializeField]
    private float height = 500;
    #endregion

    #region Monobehaviour
    // Use this for initialization
    void Awake()
    {
        if (singletonInstance != null)
        {
            Debug.LogError("More than one GameManager Instance is in the Scene!");
        }
        else
        {
            singletonInstance = this;
        }

        if (this.sceneCamera == null)
        {
            Debug.LogError("GameManager: SceneCamera reference is Missing!");
        }

        if (enableVSync)
        {
            Application.targetFrameRate = 60;
        }
    }

    // Use this for initialization
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    #endregion

    /// <summary>
    /// Sets Scene Camera active or inactive
    /// </summary>
    /// <param name="isActive"></param>
    public void SetSceneCameraActive(bool isActive)
    {
        if (this.sceneCamera == null)
        {
            return;
        }

        this.sceneCamera.SetActive(isActive);
    }

    #region Player_Tracking
    /// <summary>
    /// Register a Player to the dictionary according to netID
    /// </summary>
    /// <param name="netID"></param>
    /// <param name="player"></param>
    public static void RegisterPlayer(string netID, Player player)
    {
        string playerID = PLAYER_ID_PREFIX + netID;
        players.Add(playerID, player);
        player.transform.name = playerID;
    }

    /// <summary>
    /// Unregister a Player from the dictionary according to playerID
    /// </summary>
    /// <param name="playerID"></param>
    public static void UnRegisterPlayer(string playerID)
    {
        players.Remove(playerID);
    }

    /// <summary>
    /// Gets Player according to playerID
    /// </summary>
    /// <param name="playerID"></param>
    /// <returns></returns>
    public static Player GetPlayer(string playerID)
    {
        return players[playerID];
    } 
    #endregion

    #region Legacy_GUI
    //private void OnGUI()
    //{
    //    GUILayout.BeginArea(new Rect(x, y, width, height));
    //    GUILayout.BeginVertical();

    //    GUILayout.Label("Player List: ");
    //    foreach (string playerID in players.Keys)
    //    {
    //        string playerName = players[playerID].transform.name;
    //        GUILayout.Label(playerID + " - " + playerName);
    //    }

    //    GUILayout.EndVertical();
    //    GUILayout.EndArea();
    //}
    #endregion

}
