using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;

public class PauseMenu : MonoBehaviour {

    public static bool isOn = false;

    private NetworkManager networkManager;

	// Use this for initialization
	void Start () {
        if (this.networkManager == null)
        {
            this.networkManager = NetworkManager.singleton;
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    /// <summary>
    /// Disconnects the player 
    /// </summary>
    public void LeaveRoom()
    {
        MatchInfo matchInfo = this.networkManager.matchInfo;
        this.networkManager.matchMaker.DropConnection(matchInfo.networkId, matchInfo.nodeId, 0, this.networkManager.OnDropConnection);
        this.networkManager.StopHost();
    }
}
