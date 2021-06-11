using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    private bool hasBomb;

	private void Awake()
	{
		hasBomb = false;
	}

	// TODO: Implementirati postavljanje bombe
	private void Update()
	{
		if (Input.GetMouseButtonDown(1) && hasBomb)
		{
			PlaceBomb();
		}
	}

	// TODO: Implementirati uzimanje itema
	public void CollectItem(Platform platform)
	{
        ItemType itemType = ItemManager.Instance.ItemTypeAtPlatform(platform);

		if (itemType == ItemType.BOMB_COLLECTIBLE && hasBomb == false)
		{
			hasBomb = true;
			ItemManager.Instance.RemoveItemAtPlatform(platform);
		}

		if (itemType == ItemType.BOMB_ACTIVE)
		{
			//Debug.Log("Im standing on the bomb");
		}
	}

	private void PlaceBomb()
	{
		hasBomb = false;

		float range = SettingsReader.Instance.GameSettings.PlayerToPlatformSnapRange;
		Platform platform = WorldManager.Instance.GetPlatformWithinRange(transform.position, range);
		ItemManager.Instance.PlaceItemAtPlatform(platform, ItemType.BOMB_ACTIVE);
	}
}
