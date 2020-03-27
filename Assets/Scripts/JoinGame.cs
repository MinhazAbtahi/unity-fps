using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;

public class JoinGame : MonoBehaviour
{

    #region Fields
    private List<GameObject> roomList = new List<GameObject>();

    [SerializeField]
    private Text statusText;
    [SerializeField]
    private GameObject roomListItemButton;
    [SerializeField]
    private Transform roomListItemParent;

    private NetworkManager networkManager;
    #endregion

    #region Monobehaviour
    // Use this for initialization
    void Start()
    {
        if (this.statusText == null)
        {
            Debug.LogError("JoinGame: StatusText reference is Missing!");
        }

        if (this.roomListItemButton == null)
        {
            Debug.LogError("JoinGame: RoomListItemButton reference is Missing!");
        }

        if (this.roomListItemParent == null)
        {
            Debug.LogError("JoinGame: RoomListItemParent reference is Missing!");
        }

        this.networkManager = NetworkManager.singleton;

        // Enables Matchmaker
        this.EnableMatchMaker();

        // Refreshes Room List
        this.RefreshRoomList();
    }

    // Update is called once per frame
    void Update()
    {

    }
    #endregion

    #region MatchMaking/JoinGame
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
    /// Refreshe Room List
    /// </summary>
    public void RefreshRoomList()
    {
        this.ClearRoomList();

        this.EnableMatchMaker();

        this.networkManager.matchMaker.ListMatches(0, 10, "", true, 0, 0, OnMatchList);
        this.statusText.text = "Loading Room List...";
    }

    public void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matches)
    {
        this.statusText.text = "";

        if (!success && matches == null)
        {
            this.statusText.text = "Couldn't Get Matches";

            return;
        }

        // Iterate over matches/room lists 
        foreach (MatchInfoSnapshot match in matches)
        { 
            GameObject roomListItemClone = Instantiate(roomListItemButton);
            roomListItemClone.transform.SetParent(this.roomListItemParent);

            // Sets up current match's name, current size and max size
            RoomListItem roomListItem = roomListItemClone.GetComponent<RoomListItem>();
            if (roomListItem != null)
            {
                roomListItem.SetUp(match, this.JoinRoom);
            }

            this.roomList.Add(roomListItemClone);
        }

        // Currently no room
        if (this.roomList.Count == 0)
        {
            this.statusText.text = "No Active Room.";
        }
    }

    /// <summary>
    /// Clears Room list
    /// </summary>
    private void ClearRoomList()
    {
        for (int i = 0; i < this.roomList.Count; i++)
        {
            Destroy(this.roomList[i]);
        }

        // Clears references of the roomList
        this.roomList.Clear();
    }

    /// <summary>
    /// Joins a room/match
    /// </summary>
    /// <param name="match"></param>
    public void JoinRoom(MatchInfoSnapshot match)
    {
        Debug.Log("Joining Room: " + match.name);

        this.networkManager.matchMaker.JoinMatch(match.networkId, "", "", "", 0, 0, this.networkManager.OnMatchJoined);
        StartCoroutine(WaitForJoin());
    }

    IEnumerator WaitForJoin()
    {
        this.ClearRoomList();

        // Joining countdown
        int countdown = 10;
        while (countdown > 0)
        {
            this.statusText.text = "Joining Room... (" + countdown + ")";
            yield return new WaitForSeconds(1f);

            countdown--;
        }

        // Joining Error
        this.statusText.text = "Failed to Connect";
        yield return new WaitForSeconds(1f);

        // Stops Matchmaker
        MatchInfo matchInfo = this.networkManager.matchInfo;
        if (matchInfo != null)
        {
            this.networkManager.matchMaker.DropConnection(matchInfo.networkId, matchInfo.nodeId, 0, this.networkManager.OnDropConnection);
            this.networkManager.StopHost();
        }

        // Refreshes room list
        this.RefreshRoomList();
    }
    #endregion
}
