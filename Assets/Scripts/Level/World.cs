using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
	private static World instance;
	public static World Instance { get => instance; }

	[SerializeField]
	private GameObject platformPrefab;
	private GameSettings gameSettings;
	private PhotonView photonView;

	private Platform[,] platforms;
	//public Platform[,] Platforms { get => platforms; }

	int height;
	int width;

	private void Awake()
	{
		instance = this;

	}

	void Start()
	{
		photonView = GetComponent<PhotonView>();

		if (photonView.IsMine && PhotonNetwork.IsMasterClient)
		{
			photonView.RPC("RPC_AddWorld", RpcTarget.AllBuffered, platforms);
		}
	}

	[PunRPC]
	void RPC_AddWorld(Platform[,] _platforms)
	{
		gameSettings = SettingsReader.Instance.GameSettings;

		height = gameSettings.Height;
		width = gameSettings.Width;

		_platforms = new Platform[width, height];

		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				float positionX = (x - width / 2) * gameSettings.PlatformSpacingX;
				float positionY = (y - height / 2) * gameSettings.PlatformSpacingY;
				Vector3 position = new Vector3(positionX, positionY);

				GameObject platformGameObject = Instantiate(platformPrefab, position, Quaternion.identity, transform);
				_platforms[x, y] = platformGameObject.GetComponent<Platform>();
				// TODO: make this actually follow predefined rows
				_platforms[x, y].SetType(GetRandomPlatformType());
			}
		}
	}

	void Update()
	{
		if (photonView.IsMine)
		{
			Vector3 movement = Vector3.down * Time.deltaTime * gameSettings.PlatformSpeed;

			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					platforms[x, y].transform.position += movement;

					Vector3 platformPosition = platforms[x, y].transform.position;
					if (platformPosition.y < gameSettings.ScreenBorderBottom)
					{
						platformPosition.y = gameSettings.ScreenBorderTop;
						platforms[x, y].transform.position = platformPosition;

						// TODO: make this actually follow predefined rows
						platforms[x, y].SetType(GetRandomPlatformType());
					}
				}
			}
		}
	}

	public Platform GetClosestPlatformInRange(Vector3 position, float range)
	{
		List<Platform> platformsInRange = GetPlatformsInRange(position, range);

		float closestDistance = Mathf.Infinity;
		Platform closestPlatform = null;
		foreach (Platform platform in platformsInRange)
		{
			float distance = Vector3.Distance(platform.transform.position, position);
			if (distance < closestDistance)
			{
				closestDistance = distance;
				closestPlatform = platform;
			}
		}

		return closestPlatform;
	}

	public List<Platform> GetPlatformsInRange(Vector3 position, float range)
	{
		List<Platform> platformsInRange = new List<Platform>();

		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				Vector3 platformPosition = platforms[x, y].transform.position;
				float distance = Vector3.Distance(position, platformPosition);
				if (distance <= range)
				{
					platformsInRange.Add(platforms[x, y]);
				}
			}
		}

		return platformsInRange;
	}

	private PlatformType GetRandomPlatformType()
	{
		int numOfPlatforms = gameSettings.PlatformSettings.Length;
		return (PlatformType)Random.Range(0, numOfPlatforms);
	}
}
