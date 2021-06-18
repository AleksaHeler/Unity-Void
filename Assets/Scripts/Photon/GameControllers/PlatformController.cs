using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script spawns platform sprite and manages its type/sprite/movement
// This is located on "PlatformAvatar" prefab
public class PlatformController : MonoBehaviour
{
	private const float spriteDisableDuration = 0.5f;

	private PhotonView photonView;

	[SerializeField]
	private GameObject platformPrefab;
	private PlatformType platformType;
	private ItemType itemType;
	private GameObject myPlatform;

	public PlatformType PlatformType { get => platformType; }
	public ItemType ItemType { get => itemType; }


	void Awake()
	{
		photonView = GetComponent<PhotonView>();
	}

	private void SetItemType(ItemType type)
	{
		itemType = type;
		Sprite newSprite = SettingsReader.Instance.GameSettings.ItemTypeToSprite(itemType);
		myPlatform.transform.Find("Item").GetComponent<SpriteRenderer>().sprite = newSprite;
	}

	private void SetPlatformType(PlatformType type)
	{
		platformType = type;
		Sprite newSprite = SettingsReader.Instance.GameSettings.PlatformTypeToSprite(platformType);
		myPlatform.GetComponent<SpriteRenderer>().sprite = newSprite;
	}


	[PunRPC]
	// Removes platform sprite if it is set to glass
	void RPC_BreakGlass()
	{
		if (platformType != PlatformType.GLASS)
		{
			return;
		}
		SetPlatformType(PlatformType.NONE);
		StartCoroutine(RegenerateGlassCoroutine());
	}

	IEnumerator RegenerateGlassCoroutine()
	{
		float elapsedTime = 0;
		float duration = SettingsReader.Instance.GameSettings.GlassPlatformRegenerationTime;

		while (elapsedTime < duration)
		{
			elapsedTime += Time.deltaTime;
			yield return null;
		}

		SetPlatformType(PlatformType.GLASS);
	}

	[PunRPC]
	// Spawn platform prefab and set its sprite to none
	void RPC_AddPlatform()
	{
		myPlatform = Instantiate(platformPrefab, transform.position, transform.rotation, transform);
		SetPlatformType(PlatformType.NONE);
		SetItemType(ItemType.NONE);
	}

	[PunRPC]
	// Change platform sprite to match its type
	void RPC_SetPlatformType(PlatformType type)
	{
		SetPlatformType(type);
	}

	[PunRPC]
	// Change platform sprite to match its type
	void RPC_SetItemType(ItemType type)
	{
		SetItemType(type);
	}

	[PunRPC]
	// Animate platform by moving it by given amount
	void RPC_MovePlatformDown(Vector3 movement)
	{
		myPlatform.transform.position += movement;
	}

	[PunRPC]
	void RPC_DisableSpriteTemporarily()
	{
		StartCoroutine(DisableSpriteTemporarily(spriteDisableDuration));
	}

	IEnumerator DisableSpriteTemporarily(float duration)
	{
		myPlatform.SetActive(false);

		float elapsedTime = 0;
		while(elapsedTime < duration)
		{
			elapsedTime += Time.deltaTime;
			yield return null;
		}

		myPlatform.SetActive(true);
	}
}
