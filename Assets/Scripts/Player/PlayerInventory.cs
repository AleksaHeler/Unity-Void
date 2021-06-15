using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    private bool hasBomb;
	private PlayerController playerController;
	private float platformSpacingX;
	private float platformSpacingY;
	private float platformSnapRange;
	private GameObject bombExplosionParticles;

	public bool HasBomb { get => hasBomb; }

	private void Awake()
	{
		hasBomb = false;
		playerController = GetComponentInChildren<PlayerController>();

		GameSettings gameSettings = SettingsReader.Instance.GameSettings;
		platformSpacingX = gameSettings.PlatformSpacingX;
		platformSpacingY = gameSettings.PlatformSpacingY;
		platformSnapRange = gameSettings.PlayerToPlatformSnapRange;
		bombExplosionParticles = gameSettings.BombExplosionParticles;
	}

	private void Update()
	{
		if (Input.GetMouseButtonDown(1) && hasBomb)
		{
			PlaceBomb();
		}

		CheckIfBombIsInRange();
	}

	public void CollectItem(Platform platform)
	{
        ItemType itemType = ItemManager.Instance.GetItemTypeAtPlatform(platform);

		if (itemType == ItemType.BOMB_COLLECTIBLE && hasBomb == false)
		{
			hasBomb = true;
			ItemManager.Instance.RemoveItemAtPlatform(platform);
		}
	}

	public void PlaceBomb()
	{
		if(hasBomb == false)
		{
			return;
		}

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

	private void CheckIfBombIsInRange()
	{
		// All positions to check
		Vector3[] offsets = { Vector3.up, Vector3.down, Vector3.left, Vector3.right };
		
		foreach(Vector3 offset in offsets)
		{
			bool xAxis = offset.x != 0;
			float spacing = xAxis ? platformSpacingX : platformSpacingY;

			Vector3 platformPosition = transform.position + offset * spacing;
			Platform platform = WorldManager.Instance.GetPlatformWithinRange(platformPosition, platformSnapRange);

			if(platform == null)
			{
				continue;
			}

			ItemType item = ItemManager.Instance.GetItemTypeAtPlatform(platform);
			if (item == ItemType.BOMB_ACTIVE)
			{
				AudioManager.Instance.PlaySound("Bomb Explode");
				ItemManager.Instance.RemoveItemAtPlatform(platform);
				Instantiate(bombExplosionParticles, platformPosition, Quaternion.identity);
				playerController.PlayerDie();
			}
		}
	}
}
