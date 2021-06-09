using UnityEngine;

/// <summary>
/// Describes what item is located on platform
/// </summary>
//public enum Item { NONE };

//public enum PlatformType { NONE, NORMAL, SPIKES, SLIME, SLIDE_LEFT, SLIDE_RIGHT, GRASS, GLASS }

// This is located on GameObject (prefab) and when the type is set it changes the sprite
/// <summary>
/// This script is located on a prefab GameObject that represents a platform.
/// The object has a SpriteRenderer component where platform image is set.
/// </summary>
public class Platform : MonoBehaviour
{
	// Settings/parameters
	[Header("Platform settings")]
	[Tooltip("Sprites that will be assigned to this platform based on type (has to be the same order as enum PlatformType)")]
	// TODO: da ne moraju imati iste indekse: smisliti resenje
	// Dictionary:       platformType  :   struct (sprite + float)
	// 
	// TODO: Pogledati scripable object za platformu !!!
	// Na pocetku izvlaci konfiguracije 
	public Sprite[] SpritePrefabs;
	[Tooltip("Chance for each type of platform to be generated (has to be the same order as enum PlatformType)")]
	public float[] PlatformChance = { 1, 1, 0.2f, 0.2f, 0.1f, 0.1f, 0.4f, 0.2f };

	private Item item;
	private PlatformType platformType;


	public PlatformType PlatformType { get => platformType; }


	/// <summary>
	/// Generates random platform at given position with given item on it
	/// </summary>
	public void GeneratePlatform(Vector2 position, Item item)
	{
		platformType = GenerateRandomPlatformType();
		GeneratePlatform(position, item, platformType);
	}

	/// <summary>
	/// Generates a platform with given type at position with item on it
	/// </summary>
	public void GeneratePlatform(Vector2 position, Item item, PlatformType type)
	{
		this.platformType = type;
		this.item = item;
		transform.position = position;

		SetSprite(type);
	}

	/// <summary>
	/// Returns random platform type based on probabilities from 'platformChance' array
	/// </summary>
	private PlatformType GenerateRandomPlatformType()
	{
		// We have an array of probabilities
		// Calculate the sum of it and generate a random number
		float totalChance = 0;
		for(int i = 0; i < PlatformChance.Length; i++)
		{
			totalChance += PlatformChance[i];
		}

		float random = Random.Range(0, totalChance);

		// Subtract possibilities one by one from the random number, and as soon as that number is less than 0 return that type
		for (int i = 0; i < PlatformChance.Length; i++)
		{
			random -= PlatformChance[i];
			if(random <= 0)
			{
				return (PlatformType)i;
			}
		}

		return PlatformType.NONE;
	}

	/// <summary>
	/// Sets sprite component of this gameobject to given platform type
	/// </summary>
	private void SetSprite(PlatformType type)
	{
		// If this is an empty platform
		if (type == PlatformType.NONE)
		{
			GetComponent<SpriteRenderer>().sprite = null;
			return;
		}

		GetComponent<SpriteRenderer>().sprite = SpritePrefabs[(int)type - 1];
	}
}
