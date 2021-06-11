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

	private void Update()
	{
		if (Input.GetMouseButtonDown(1) && hasBomb)
		{
			PlaceBomb();
		}
	}

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

		float placementRange = SettingsReader.Instance.GameSettings.PlayerToPlatformSnapRange;
		float bombPrimingTime = SettingsReader.Instance.GameSettings.BombPrimingTime;
		Platform platform = WorldManager.Instance.GetPlatformWithinRange(transform.position, placementRange);
		StartCoroutine(BombPrimeCountdown(platform, bombPrimingTime));
	}

	private IEnumerator BombPrimeCountdown(Platform platform, float duration)
	{
		Debug.Log("Planted bomb with timer");
		ItemManager.Instance.PlaceItemAtPlatform(platform, ItemType.BOMB_PRIMING);

		float elapsedTime = 0;
		while (elapsedTime < duration)
		{
			elapsedTime += Time.deltaTime;
			yield return null;
		}

		Debug.Log("Bomb is active");
		ItemManager.Instance.PlaceItemAtPlatform(platform, ItemType.BOMB_ACTIVE);
	}
}
