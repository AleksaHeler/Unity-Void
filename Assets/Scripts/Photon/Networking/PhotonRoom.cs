using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using Photon.Pun;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.IO;

// This script handles players connecting to the room and starting the game on countdown timer
public class PhotonRoom : MonoBehaviourPunCallbacks, IInRoomCallbacks
{
    // Singleton
    private static PhotonRoom instance;
    public static PhotonRoom Instance { get => instance; }

    [SerializeField]
    private TMPro.TextMeshProUGUI statusText;

    // Room info
    private PhotonView photonView;
    private bool isGameLoaded;
    private int currentScene;
    private int winningPlayer;

    // Player info
    private Player[] photonPlayers;
    private int playersInRoom;
    private int myNumberInRoom;
    private int playersInGame;

    // Delayed start
    [SerializeField]
    private float startDelayWhenAllConnected;
    private bool readyToCount;
    private bool readyToStart;
    private float startingTime;
    private float lessThanMaxPlayers;
    private float atMaxPlayers;
    private float timeToStart;

    public int WinningPlayer { get => winningPlayer; }

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
            DontDestroyOnLoad(this.gameObject);
        }

        photonView = GetComponent<PhotonView>();
    }

	private void Start()
	{
        readyToCount = false;
        readyToStart = false;
        atMaxPlayers = startDelayWhenAllConnected;
        lessThanMaxPlayers = startingTime;
        timeToStart = startingTime;
    }

	private void Update()
	{
        // Don't do anything if we are the only one in the room
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
                statusText.text = "Starting in " + ((int)timeToStart + 1) + "...";
            }
            else if (readyToCount)
			{
                lessThanMaxPlayers -= Time.deltaTime;
                timeToStart = lessThanMaxPlayers;
			}

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
        photonPlayers = PhotonNetwork.PlayerList;
        playersInRoom = photonPlayers.Length;
        myNumberInRoom = playersInRoom;
        PhotonNetwork.NickName = myNumberInRoom.ToString();
        statusText.text = "Waiting for another player...";

        if (playersInRoom > 1)
		{
            readyToCount = true;
		}

        // Is the room full?
        if(playersInRoom == MultiplayerSettings.Instance.MaxPlayers)
		{
            readyToStart = true;

            // If we are not the master client -> close the room
			if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.CurrentRoom.IsOpen = false;
            }
		}
    }

	public override void OnPlayerEnteredRoom(Player newPlayer)
	{
		base.OnPlayerEnteredRoom(newPlayer);

        photonPlayers = PhotonNetwork.PlayerList;
        playersInRoom++;
        statusText.text = "Another player has joined room";

        if (playersInRoom > 1)
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

        if(currentScene == MultiplayerSettings.Instance.MultiplayerSceneBuildIndex)
		{
            isGameLoaded = true;
            photonView.RPC("RPC_LoadedGameScene", RpcTarget.MasterClient);
		}
    }

    private void RestartTimer()
    {
        lessThanMaxPlayers = startingTime;
        timeToStart = startingTime;
        atMaxPlayers = startDelayWhenAllConnected;
        readyToCount = false;
        readyToStart = false;
    }

    private void StartGame()
    {
        isGameLoaded = true;

        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.LoadLevel(MultiplayerSettings.Instance.MultiplayerSceneBuildIndex);
    }

    [PunRPC]
    // This changes scene to GameOverScene and keeps track of who won/lost
    private void RPC_GameOver(int losingPlayer)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }
        // TODO: Make game over win/lose check the nickname, not the ID or whatever
        winningPlayer = 3 - losingPlayer;
        PhotonNetwork.LoadLevel(MultiplayerSettings.Instance.MultiplayerSceneBuildIndex + 1);
    }

    [PunRPC]
    // This code only executes on host and calls function on all clients (spawn world and players)
    private void RPC_LoadedGameScene()
	{
        playersInGame++;

        if(playersInGame == PhotonNetwork.PlayerList.Length)
        {
            photonView.RPC("RPC_CreatePlayer", RpcTarget.All);
            photonView.RPC("RPC_CreateWorld", RpcTarget.MasterClient);
        }

    }

    [PunRPC]
    private void RPC_CreatePlayer()
    {
        PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PhotonNetworkPlayer"), transform.position, Quaternion.identity, 0);
    }

    [PunRPC]
    private void RPC_CreateWorld()
    {
        PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PhotonNetworkWorld"), transform.position, Quaternion.identity, 0);
    }

}
