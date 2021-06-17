using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using System.IO;
using UnityEngine;

// This is the script that contains all the world data and platforms
// This is located on "PhotonNetworkWorld" GameObject
public class PhotonWorld : MonoBehaviour
{
	private PhotonView photonView;

	// Singleton
	private static PhotonWorld instance;
	public static PhotonWorld Instance { get => instance; }


	// World data
	private GameObject[,] platforms;
	private int width;
	private int height;
	private float platformSpeed;
	private float bottomWorldBorder;
	private float topWorldBorder;


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

		photonView = GetComponent<PhotonView>();
	}

	private void Start()
	{
		GameSettings gameSettings = SettingsReader.Instance.GameSettings;

		// Get data from settings
		width = gameSettings.Width;
		height = gameSettings.Height;
		platformSpeed = gameSettings.PlatformSpeed;
		bottomWorldBorder = gameSettings.ScreenBorderBottom;
		topWorldBorder = gameSettings.ScreenBorderTop;

		if (!PhotonNetwork.IsMasterClient)
		{
			return;
		}

		platforms = new GameObject[width, height];

		// Instantiate all platform avatars, and have them spawn sprites and set them based on platform types
		for (int y = 0; y < height; y++)
		{
			// Get a random predefined row
			int predefinedRowsCount = SettingsReader.Instance.GameSettings.PredefinedRows.Length;
			int randomRowIndex = Random.Range(0, predefinedRowsCount);
			PlatformType[] predefinedRow = SettingsReader.Instance.GameSettings.PredefinedRows[randomRowIndex];

			for (int x = 0; x < width; x++)
			{
				float xPos = ((float)x - width / 2f + 0.5f) * gameSettings.PlatformSpacingHorizontal;
				float yPos = ((float)y - height / 2f + 0.5f) * gameSettings.PlatformSpacingVertical;
				Vector3 position = new Vector3(xPos, yPos);
				Quaternion rotation = Quaternion.identity;
				platforms[x, y] = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlatformAvatar"), position, rotation, 0);
				platforms[x, y].GetComponent<PhotonView>().RPC("RPC_AddPlatform", RpcTarget.All);
				platforms[x, y].GetComponent<PhotonView>().RPC("RPC_SetPlatformType", RpcTarget.All, predefinedRow[x]);
			}
		}

		SetStartingPlatformsAsNormal();
	}

	private void Update()
	{
		// Only the master client updates world/platforms
		if (!PhotonNetwork.IsMasterClient)
		{
			return;
		}

		if (platforms == null)
		{
			CollectExistingPlatformsInArray();
		}

		// Animate all platforms -> move them down
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				// Move down
				platforms[x, y].transform.position += Vector3.down * Time.deltaTime * platformSpeed;

				// Respawn up above screen if needed
				if (platforms[x, y].transform.position.y < bottomWorldBorder)
				{
					RespawnRow(y);
				}
			}
		}
	}

	// Moves platforms to top of the screen and sets their types to predefined row
	private void RespawnRow(int y)
	{
		int predefinedRowsCount = SettingsReader.Instance.GameSettings.PredefinedRows.Length;
		int randomRowIndex = Random.Range(0, predefinedRowsCount);
		PlatformType[] predefinedRow = SettingsReader.Instance.GameSettings.PredefinedRows[randomRowIndex];

		for (int x = 0; x < width; x++)
		{
			Vector3 newPos = platforms[x, y].transform.position;
			newPos.y = topWorldBorder;
			platforms[x, y].transform.position = newPos;
			platforms[x, y].GetComponent<PhotonView>().RPC("RPC_SetPlatformType", RpcTarget.All, predefinedRow[x]);
		}
	}

	// Returns platform that is the closest distance to given position
	private GameObject GetPlatformClosestToPosition(Vector3 position)
	{
		GameObject closestPlatform = null;
		float closestDistance = Mathf.Infinity;

		if (platforms == null || platforms.Length == 0)
		{
			CollectExistingPlatformsInArray();
		}

		if(platforms == null)
		{
			return null;
		}

		// Go trough all platforms to find closest one
		foreach (GameObject platform in platforms)
		{
			if(platform == null)
			{
				continue;
			}
			float distance = Vector3.Distance(position, platform.transform.position);
			if (distance < closestDistance)
			{
				closestDistance = distance;
				closestPlatform = platform;
			}
		}

		return closestPlatform;
	}

	// Finds closest platform, and if it is within range returns it, else returns null
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

	// Finds all platform objects and stores them in the platforms array
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

	// For all spawn points find closest platform and set it to normal type
	private void SetStartingPlatformsAsNormal()
	{
		foreach (Transform spawnPoint in GameSetup.Instance.PlayerSpawnPoints)
		{
			GameObject platform = GetPlatformClosestToPosition(spawnPoint.position);

			if (platform == null)
			{
				continue;
			}

			platform.GetComponent<PhotonView>().RPC("RPC_SetPlatformType", RpcTarget.All, PlatformType.NORMAL);
		}
	}
}
