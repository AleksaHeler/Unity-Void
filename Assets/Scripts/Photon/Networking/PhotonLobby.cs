using System.Collections;
using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

// This script handles cnnecting to master server and creating/joining rooms
public class PhotonLobby : MonoBehaviourPunCallbacks
{
	// Singleton
	private static PhotonLobby instance;
	public static PhotonLobby Instance { get => instance; }

	[SerializeField]
	private GameObject playButton;
	[SerializeField]
	private GameObject cancelButton;
	[SerializeField]
	private GameObject connectingButton;
	[SerializeField]
	private TMPro.TextMeshProUGUI statusText;


	private void Awake()
	{
		// Singleton
		if (instance != null && instance != this)
		{
			Destroy(this.gameObject);
		}
		else
		{
			instance = this;
		}
	}
	public void Start()
	{
		PhotonNetwork.ConnectUsingSettings();
		playButton.SetActive(false);
		cancelButton.SetActive(false);
		connectingButton.SetActive(true);
	}

	public override void OnConnectedToMaster()
	{
		statusText.text = "We are now connected to " + PhotonNetwork.CloudRegion.ToUpper() + " server!";
		PhotonNetwork.AutomaticallySyncScene = true;
		playButton.SetActive(true);
		cancelButton.SetActive(false);
		connectingButton.SetActive(false);
	}

	// This usualy means there must be no open games available, so we should create a new one
	public override void OnJoinRandomFailed(short returnCode, string message)
	{
		CreateRoom();
	}

	// This usualy means that there is a room with the same name, so try to create one again
	public override void OnCreateRoomFailed(short returnCode, string message)
	{
		CreateRoom();
	}

	// Click on UI button to start searching for a game
	public void OnPlayButtonClicked()
	{
		PhotonNetwork.JoinRandomRoom();
		playButton.SetActive(false);
		cancelButton.SetActive(true);
	}

	// Click on UI button to stop searching for a game
	public void OnCancelButtonClicked()
	{
		statusText.text = "We are now connected to " + PhotonNetwork.CloudRegion.ToUpper() + " server!";
		PhotonNetwork.LeaveRoom();
		playButton.SetActive(true);
		cancelButton.SetActive(false);
	}

	// Try to create a new room
	private void CreateRoom()
	{
		int randomRoomID = Random.Range(0, 10000);
		int maxPlayers = MultiplayerSettings.Instance.MaxPlayers;

		RoomOptions roomOptions = new RoomOptions()
		{
			IsVisible = true,
			IsOpen = true,
			MaxPlayers = (byte)maxPlayers
		};

		PhotonNetwork.CreateRoom("Room" + randomRoomID, roomOptions);
	}
}
