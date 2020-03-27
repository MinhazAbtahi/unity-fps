using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking.Match;

public class RoomListItem : MonoBehaviour {

    public delegate void JoinRoomDelegate(MatchInfoSnapshot match);
    private JoinRoomDelegate joinRoomCallback;

    [SerializeField]
    private Text roomNameText;

    private MatchInfoSnapshot match;

	// Use this for initialization
	void Start () {
        if (this.roomNameText == null)
        {
            Debug.LogError("RoomListItem: StatusText reference is Missing!");
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SetUp(MatchInfoSnapshot match, JoinRoomDelegate joinRoomCallback)
    {
        this.match = match;
        this.joinRoomCallback = joinRoomCallback;

        this.roomNameText.text = match.name + "(" + match.currentSize + "/" + match.maxSize + ")";
    }

    public void JoinRoom()
    {
        this.joinRoomCallback.Invoke(this.match);
    }
}
