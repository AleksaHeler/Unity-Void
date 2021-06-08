using UnityEngine;

// What item is on a platform
public enum Item { NONE, BOMB_COLLECTIBLE, BOMB_ACTIVE };
[System.Serializable]
public enum PlatformType { NONE, NORMAL, SPIKES, SLIME, SLIDE_LEFT, SLIDE_RIGHT, GRASS, GLASS }

// This is located on GameObject (prefab) and when the type is set it changes the sprite
public class Platform : MonoBehaviour
{
	[Header("Platform settings")]
	[Tooltip("Sprites that will be assigned to this platform based on type (has to be the same order as enum PlatformType)")]
	public Sprite[] SpritePrefabs;
	[Tooltip("Chance for each type of platform to be generated (has to be the same order as enum PlatformType)")]
	public float[] platformChance = { 2, 1, 0.2f, 0.2f, 0.1f, 0.1f, 0.4f, 0.2f };

	private PlatformType type;
	private Item item;
	public PlatformType Type { get => type; }

	// Chance for each platform type to be selected, in order as in enum PlatformType

	public void GeneratePlatform(Vector2 position, Item item)
	{
		type = generateRandomPlatformType();
		GeneratePlatform(position, item, type);
	}
	public void GeneratePlatform(Vector2 position, Item item, PlatformType type)
	{
		this.type = type;
		this.item = item;
		transform.position = position;

		setSprite(type);
	}

	private PlatformType generateRandomPlatformType()
	{
		// We have an array of probabilities
		// Calculate the sum of it
		float totalChance = 0;
		for(int i = 0; i < 8; i++)
		{
			totalChance += platformChance[i];
		}

		// Then random number is between 0 and that sum
		float random = Random.Range(0, totalChance);

		// Subtract possibilities one by one from random number, and as soon as 
		// that number is less than 0 return it
		for (int i = 0; i < 8; i++)
		{
			random -= platformChance[i];
			if(random <= 0)
			{
				return (PlatformType)i;
			}
		}

		return PlatformType.NONE;
	}

	// Sets sprite component of this gameobject to given sprite
	private void setSprite(PlatformType type)
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
