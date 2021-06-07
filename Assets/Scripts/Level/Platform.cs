using UnityEngine;

// What item is on a platform
public enum Item { NONE, BOMB_COLLECTIBLE, BOMB_ACTIVE };
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

	// Chance for each platform type to be selected, in order as in enum PlatformType

	public void GeneratePlatform(Vector2 position, Item item)
	{
		type = generateRandomPlatformType();
		this.item = item;
		transform.position = position;

		// If this is an empty platform
		if (type == PlatformType.NONE)
		{
			setSprite(null);
			return;
		}

		setSprite(SpritePrefabs[(int)type - 1]);
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
	private void setSprite(Sprite sprite)
	{
		GetComponent<SpriteRenderer>().sprite = sprite;
	}
}
