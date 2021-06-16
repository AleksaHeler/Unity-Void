using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using System.IO;
using UnityEngine;

// This is the world class containing platforms
public class PhotonWorld : MonoBehaviour
{
	private PhotonView photonView;

	// Singleton
	private static PhotonWorld instance;
	public static PhotonWorld Instance { get => instance; }

	// World data
	GameObject[,] platforms;
	int width;
	int height;
	float platformSpeed;
	float bottomWorldBorder;
	float topWorldBorder;

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

	private void Start()
	{
		photonView = GetComponent<PhotonView>();

		GameSettings gameSettings = SettingsReader.Instance.GameSettings;


		width = gameSettings.Width;
		height = gameSettings.Height;
		platformSpeed = gameSettings.PlatformSpeed;
		bottomWorldBorder = gameSettings.ScreenBorderBottom;
		topWorldBorder = gameSettings.ScreenBorderTop;
		float spacingX = gameSettings.PlatformSpacingX;
		float spacingY = gameSettings.PlatformSpacingY;

		if (!PhotonNetwork.IsMasterClient)
		{
			return;
		}

		platforms = new GameObject[width, height];

		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				float xPos = ((float)x - width / 2f + 0.5f) * spacingX;
				float yPos = ((float)y - height / 2f + 0.5f) * spacingY;
				Vector3 position = new Vector3(xPos, yPos);
				Quaternion rotation = Quaternion.identity;
				platforms[x, y] = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlatformAvatar"), position, rotation, 0);
			}
		}

		for (int y = 0; y < height; y++)
		{
			int predefinedRowsCount = SettingsReader.Instance.GameSettings.PredefinedRows.Length;
			int randomRowIndex = Random.Range(0, predefinedRowsCount);
			PlatformType[] predefinedRow = SettingsReader.Instance.GameSettings.PredefinedRows[randomRowIndex];
			for (int x = 0; x < width; x++)
			{
				platforms[x, y].GetComponent<PhotonView>().RPC("RPC_AddPlatform", RpcTarget.All);
				platforms[x, y].GetComponent<PhotonView>().RPC("RPC_SetPlatformType", RpcTarget.All, predefinedRow[x]);
			}
		}

		SetStartingPlatformsToNormal();
	}

	private void Update()
	{
		if (!PhotonNetwork.IsMasterClient)
		{
			return;
		}

		if(platforms == null)
		{
			CollectExistingPlatformsInArray();
		}

		for (int y = 0; y < height; y++)
		{
			int predefinedRowsCount = SettingsReader.Instance.GameSettings.PredefinedRows.Length;
			int randomRowIndex = Random.Range(0, predefinedRowsCount);
			PlatformType[] predefinedRow = SettingsReader.Instance.GameSettings.PredefinedRows[randomRowIndex];

			for (int x = 0; x < width; x++)
			{
				// Move down
				platforms[x, y].transform.position += Vector3.down * Time.deltaTime * platformSpeed;

				// Respawn up above screen
				if (platforms[x, y].transform.position.y < bottomWorldBorder)
				{
					Vector3 newPos = platforms[x, y].transform.position;
					newPos.y = topWorldBorder;
					platforms[x, y].transform.position = newPos;
					platforms[x, y].GetComponent<PhotonView>().RPC("RPC_SetPlatformType", RpcTarget.All, predefinedRow[x]);
				}
			}
		}
	}

	// get closest platform to position
	private GameObject GetPlatformClosestToPosition(Vector3 position)
	{
		GameObject closestPlatform = null;
		float closestDistance = Mathf.Infinity;

		if (platforms == null || platforms.Length == 0)
		{
			CollectExistingPlatformsInArray();
		}

		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				float distance = Vector3.Distance(position, platforms[x, y].transform.position);
				if (distance < closestDistance)
				{
					closestDistance = distance;
					closestPlatform = platforms[x, y];
				}
			}
		}

		return closestPlatform;
	}

	// get platform within range
	public GameObject GetPlatformPositionWithinRange(Vector3 position, float range)
	{
		GameObject platform = GetPlatformClosestToPosition(position);

		if (platform == null)
		{
			return null;
		}

		float distance = Vector3.Distance(position, platform.transform.position);

		if (distance > range)
		{
			return null;
		}

		return platform;
	}

	private void CollectExistingPlatformsInArray()
	{
		GameObject[] platformGameObjects = GameObject.FindGameObjectsWithTag("Platform");

		if (platformGameObjects.Length < width * height)
		{
			return;
		}

		platforms = new GameObject[width, height];
		int i = 0;
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				platforms[x, y] = platformGameObjects[i];
				i++;
			}
		}
	}

	private void SetStartingPlatformsToNormal()
	{
		foreach(Transform spawnPoint in GameSetup.Instance.PlayerSpawnPoints)
		{
			GameObject platform = GetPlatformClosestToPosition(spawnPoint.position);
			if(platform == null)
			{
				continue;
			}
			platform.GetComponent<PhotonView>().RPC("RPC_SetPlatformType", RpcTarget.All, PlatformType.NORMAL);
		}
	}
}
