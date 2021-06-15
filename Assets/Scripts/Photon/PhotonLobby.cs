using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

// Photon:
//   Documentation: https://doc.photonengine.com/en-us/pun/current/getting-started/pun-intro
//   Scripting API: https://doc-api.photonengine.com/en/pun/v2/index.html
// Maybe: https://doc.photonengine.com/zh-cn/realtime/current/connection-and-authentication/authentication/custom-authentication
public class PhotonLobby : MonoBehaviourPunCallbacks
{
    // Singleton
    private static PhotonLobby lobby;
    public static PhotonLobby Lobby { get => lobby; }

    [SerializeField]
    private GameObject playButton;
    [SerializeField]
    private GameObject cancelButton;
    [SerializeField]
    private GameObject connectingButton;


    private void Awake()
    {
        // Singleton
        if (lobby != null && lobby != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            lobby = this;
        }
    }


    private void Start()
    {
        // More info: https://doc-api.photonengine.com/en/pun/v2/class_photon_1_1_pun_1_1_photon_network.html
        PhotonNetwork.ConnectUsingSettings();
        playButton.SetActive(false);
        cancelButton.SetActive(false);
        connectingButton.SetActive(true);
    }


    public override void OnConnectedToMaster()
    {
        Debug.Log("We are now connected to " + PhotonNetwork.CloudRegion + " server!");
        PhotonNetwork.AutomaticallySyncScene = true;
        playButton.SetActive(true);
        connectingButton.SetActive(false);
    }

	public override void OnJoinRandomFailed(short returnCode, string message)
	{
        Debug.Log("Tried to join a random room but failed. There must be no open games available");
        CreateRoom();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("Tried to create a new room but failed. There must be a room with the same name");
        CreateRoom();
    }

	public void OnPlayButtonClicked()
    {
        PhotonNetwork.JoinRandomRoom();
        playButton.SetActive(false);
        cancelButton.SetActive(true);
    }

    public void OnCancelButtonClicked()
    {
        PhotonNetwork.LeaveRoom();
        cancelButton.SetActive(false);
        playButton.SetActive(true);
	}

    private void CreateRoom()
    {
        Debug.Log("Trying to create a new room");
        int randomRoomID = Random.Range(0, 10000);
        RoomOptions roomOptions = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = (byte)MultiplayerSettings.Instance.MaxPlayers };
        PhotonNetwork.CreateRoom("Room" + randomRoomID, roomOptions);
    }
}
