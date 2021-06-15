using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using Photon.Pun;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.IO;

public class PhotonRoom : MonoBehaviourPunCallbacks, IInRoomCallbacks
{
    // Singleton
    private static PhotonRoom room;
    public static PhotonRoom Room { get => room; }

    // Room info
    private PhotonView photonView;
    public bool isGameLoaded;
    public int currentScene;

    // Player info
    private Player[] photonPlayers;
    public int playersInRoom;
    public int myNumberInRoom;
    public int playersInGame;

    // Delayed start
    private bool readyToCount;
    private bool readyToStart;
    public float startingTime;
    private float lessThanMaxPlayers;
    private float atMaxPlayers;
    private float timeToStart;

    private void Awake()
    {
        // Singleton
        if (room != null && room != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            room = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

	private void Start()
	{
        photonView = GetComponent<PhotonView>();
        readyToCount = false;
        readyToStart = false;
        lessThanMaxPlayers = startingTime;
        atMaxPlayers = 3;
        timeToStart = startingTime;
    }

	private void Update()
	{
        if(playersInRoom == 1)
		{
            RestartTimer();
		}
		if (!isGameLoaded)
		{
			if (readyToStart)
			{
                atMaxPlayers -= Time.deltaTime;
                lessThanMaxPlayers = atMaxPlayers;
                timeToStart = atMaxPlayers;
			}
            else if (readyToCount)
			{
                lessThanMaxPlayers -= Time.deltaTime;
                timeToStart = lessThanMaxPlayers;
			}

            Debug.Log("time to start: " + timeToStart);
            if(timeToStart <= 0)
			{
                StartGame();
			}
		}
	}

	public override void OnEnable()
    {
        base.OnEnable();
        // Subscribe to callbacks
        PhotonNetwork.AddCallbackTarget(this);
        SceneManager.sceneLoaded += OnSceneFinishedLoading;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        // Unsubscribe from callbacks
        PhotonNetwork.RemoveCallbackTarget(this);
        SceneManager.sceneLoaded -= OnSceneFinishedLoading;
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        Debug.Log("We are now in a room");
        photonPlayers = PhotonNetwork.PlayerList;
        playersInRoom = photonPlayers.Length;
        myNumberInRoom = playersInRoom;
        PhotonNetwork.NickName = myNumberInRoom.ToString();
        Debug.Log("Players in room out of max players possible (" + playersInRoom + ":" + MultiplayerSettings.Instance.MaxPlayers + ")");
            
        if(playersInRoom > 1)
		{
            readyToCount = true;
		}

        // Is the room full?
        if(playersInRoom == MultiplayerSettings.Instance.MaxPlayers)
		{
            readyToStart = true;

            // If we are not the master client -> return
			if (!PhotonNetwork.IsMasterClient)
			{
                return;
			}

            PhotonNetwork.CurrentRoom.IsOpen = false;
		}
    }

	public override void OnPlayerEnteredRoom(Player newPlayer)
	{
		base.OnPlayerEnteredRoom(newPlayer);
        Debug.Log("A new player has joined the room");
        photonPlayers = PhotonNetwork.PlayerList;
        playersInRoom++;

        Debug.Log("Players in room out of max players possible (" + playersInRoom + ":" + MultiplayerSettings.Instance.MaxPlayers + ")");
        if(playersInRoom > 1)
		{
            readyToCount = true;
        }
        if (playersInRoom == MultiplayerSettings.Instance.MaxPlayers)
		{
            readyToStart = true;
			if (!PhotonNetwork.IsMasterClient)
			{
                return;
			}

            PhotonNetwork.CurrentRoom.IsOpen = false;
		}
    }

    private void OnSceneFinishedLoading(Scene scene, LoadSceneMode mode)
	{
        currentScene = scene.buildIndex;

        if(currentScene == MultiplayerSettings.Instance.MultiplayerScene)
		{
            isGameLoaded = true;

            photonView.RPC("RPC_LoadedGameScene", RpcTarget.MasterClient);
		}
	}

    private void StartGame()
	{
        isGameLoaded = true;

		if (!PhotonNetwork.IsMasterClient)
		{
            return;
		}

        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.LoadLevel(MultiplayerSettings.Instance.MultiplayerScene);
	}

    private void RestartTimer()
	{
        lessThanMaxPlayers = startingTime;
        timeToStart = startingTime;
        atMaxPlayers = 3;
        readyToCount = false;
        readyToStart = false;
	}

    [PunRPC]
    private void RPC_LoadedGameScene()
	{
        playersInGame++;

        // Make sure not to create duplicate players
        if(playersInGame == PhotonNetwork.PlayerList.Length)
        {
            photonView.RPC("RPC_CreatePlayer", RpcTarget.All);
        }
	}

    [PunRPC]
    private void RPC_CreatePlayer()
    {
        PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PhotonNetworkPlayer"), transform.position, Quaternion.identity, 0);
    }
}
