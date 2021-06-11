using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
	// Singleton
	private static ItemManager _instance;
	public static ItemManager Instance { get { return _instance; } }

	// TODO: mozda koristiti ID umesto reference na Platform
	Dictionary<Platform, ItemType> itemsOnPlatforms;


	private void Awake()
	{
		// Singleton
		if (_instance != null && _instance != this)
		{
			Destroy(this.gameObject);
		}
		else
		{
			_instance = this;
		}

		// Set random seed
		int seed = System.DateTime.Now.Ticks.GetHashCode();
		UnityEngine.Random.InitState(seed);

		itemsOnPlatforms = new Dictionary<Platform, ItemType>();
	}


	// Generate random items to go in this row
	public void GenerateItemArrayForRow(Platform[] platforms)
	{
		// Get all possible items from settings
		ItemSettings[] itemSettingsArray = SettingsReader.Instance.GameSettings.ItemSettings;

		foreach (Platform platform in platforms)
		{
			// Reset platform item to none
			itemsOnPlatforms[platform] = ItemType.NONE;
			platform.SetItemSprite(ItemType.NONE);

			// Find some item to place
			foreach (ItemSettings itemSettings in itemSettingsArray) 
			{
				float randomNumber = Random.Range(0f, 1f);
				if (randomNumber < itemSettings.ItemChance)
				{
					PlaceItemAtPlatform(platform, itemSettings.ItemType);
					break;
				}
			}
		}
	}

	public void RemoveItemAtPlatform(Platform platform)
	{
		itemsOnPlatforms[platform] = ItemType.NONE;
		platform.SetItemSprite(null);
	}

	public void PlaceItemAtPlatform(Platform platform, ItemType item)
	{
		itemsOnPlatforms[platform] = item;
		platform.SetItemSprite(item);
	}

	public ItemType ItemTypeAtPlatform(Platform platform)
	{
		if(platform == null)
		{
			return ItemType.NONE;
		}
		ItemType item;
		itemsOnPlatforms.TryGetValue(platform, out item);
		return item;
	}
}
