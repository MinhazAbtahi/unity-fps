using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameHost : MonoBehaviour {

    [SerializeField]
    private uint roomSize = 12;

    private string roomName;

    // Cached components
    private NetworkManager networkManager;

	// Use this for initialization
	void Start () {
        this.networkManager = NetworkManager.singleton;

        // Enables Matchmaker
        this.EnableMatchMaker();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    /// <summary>
    /// Enables Matchmaker
    /// </summary>
    private void EnableMatchMaker()
    {
        if (this.networkManager.matchMaker == null)
        {
            this.networkManager.StartMatchMaker();
        }
    }

    /// <summary>
    /// Sets Room Name
    /// </summary>
    /// <param name="roomName"></param>
    public void SetRoomName(string roomName)
    {
        this.roomName = roomName;
    }

    /// <summary>
    /// Creates Room 
    /// </summary>
    public void CreateRoom()
    {
        if (this.roomName != "" && this.roomName != null)
        {
            Debug.Log("Creating Room: " + this.roomName + " with Room Size: " + this.roomSize + " Players.");

            // Create Room
            this.networkManager.matchMaker.CreateMatch(this.roomName, this.roomSize, true, "", "", "", 0, 0, this.networkManager.OnMatchCreate);
        }
    }
}
