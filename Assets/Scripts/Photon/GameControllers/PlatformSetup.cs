using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script spawns platform sprite and manages its type/sprite/movement
// This is located on "PlatformAvatar" prefab
public class PlatformSetup : MonoBehaviour
{
	private PhotonView photonView;

	[SerializeField]
	private GameObject platformPrefab;
	private PlatformType platformType;
	private GameObject myPlatform;

	public PlatformType PlatformType { get => platformType; }


	void Awake()
	{
		photonView = GetComponent<PhotonView>();
	}


	[PunRPC]
	// Removes platform sprite if it is set to glass
	void RPC_BreakGlass()
	{
		if (platformType != PlatformType.GLASS)
		{
			return;
		}
		platformType = PlatformType.NONE;
		Sprite newSprite = SettingsReader.Instance.GameSettings.PlatformTypeToSprite(platformType);
		myPlatform.GetComponent<SpriteRenderer>().sprite = newSprite;
		// TODO: add coroutine that will regenerate the glass once more
	}

	[PunRPC]
	// Spawn platform prefab and set its sprite to none
	void RPC_AddPlatform()
	{
		platformType = PlatformType.NONE;
		myPlatform = Instantiate(platformPrefab, transform.position, transform.rotation, transform);
		Sprite newSprite = SettingsReader.Instance.GameSettings.PlatformTypeToSprite(PlatformType.NONE);
		myPlatform.GetComponent<SpriteRenderer>().sprite = newSprite;
	}

	[PunRPC]
	// Change platform sprite to match its type
	void RPC_SetPlatformType(PlatformType type)
	{
		platformType = type;
		Sprite newSprite = SettingsReader.Instance.GameSettings.PlatformTypeToSprite(type);
		myPlatform.GetComponent<SpriteRenderer>().sprite = newSprite;
	}

	[PunRPC]
	// Animate platform by moving it by given amount
	void RPC_MovePlatformDown(Vector3 movement)
	{
		myPlatform.transform.position += movement;
	}
}
