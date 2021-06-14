using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerInventory : MonoBehaviour
{
    private bool hasBomb;
	private PlayerController playerController;

	private void Awake()
	{
		hasBomb = false;
		playerController = GetComponentInChildren<PlayerController>();
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
        ItemType itemType = ItemManager.Instance.GetItemTypeAtPlatform(platform);

		if (itemType == ItemType.BOMB_COLLECTIBLE && hasBomb == false)
		{
			hasBomb = true;
			ItemManager.Instance.RemoveItemAtPlatform(platform);
		}

		if (itemType == ItemType.BOMB_ACTIVE)
		{
			AudioManager.Instance.PlaySound("Bomb Explode");
			playerController.PlayerDie();
			ItemManager.Instance.RemoveItemAtPlatform(platform);
		}
	}

	public void PlaceBomb()
	{
		hasBomb = false;

		float placementRange = SettingsReader.Instance.GameSettings.PlayerToPlatformSnapRange;
		float bombPrimingTime = SettingsReader.Instance.GameSettings.BombPrimingTime;
		Platform platform = WorldManager.Instance.GetPlatformWithinRange(transform.position, placementRange);
		StartCoroutine(BombPrimeCountdown(platform, bombPrimingTime));
	}

	private IEnumerator BombPrimeCountdown(Platform platform, float duration)
	{
		AudioManager.Instance.PlaySound("Bomb Tick");
		ItemManager.Instance.PlaceItemAtPlatform(platform, ItemType.BOMB_PRIMING);

		// Wait for given amount of time
		float elapsedTime = 0;
		while (elapsedTime < duration)
		{
			elapsedTime += Time.deltaTime;
			yield return null;
		}

		AudioManager.Instance.StopSound("Bomb Tick");
		ItemManager.Instance.PlaceItemAtPlatform(platform, ItemType.BOMB_ACTIVE);
	}
}
