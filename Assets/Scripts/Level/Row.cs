using System.Collections.Generic;
using UnityEngine;

// Contains an array of Platforms
public class Row : MonoBehaviour
{
	private int width;				// Number of platforms in this row
	private float border;			// Top/bottom max world coordinates of the screen
	private float spacingX;         // Horizontal distance between two platforms 
	private float percentOfRandomPlatforms;
	[Range(0.001f, 0.01f)]
	private float percentOfRandomPlatformsIncrement;

	private Platform[] platforms;   // References to Platform scripts on platform GameObjects
	public Platform[] Platforms { get => platforms; }

	private GameObject rowGameObject;      // Empty GameObject that will be the parent of all Platforms of this row 

	/// <summary>
	/// Instantiate each platform in this row and set its parameters
	/// </summary>
	/// <param name="position">Vector2(0, y) where y is position on Y axis of this row</param>
	/// <param name="width">Number of platforms in this row</param>
	/// <param name="platformPrefab">Prefab object to instantiate for each platform</param>
	/// <param name="spacingX">Horizontal distance between two platforms</param>
	/// <param name="border">Top/bottom edge of world</param>
	/// <param name="id">Unique ID for this row</param>
	public Row(float yPosition, int width, GameObject platformPrefab, float spacingX, float border, int id, Transform rowParent, float percentOfRandomPlatforms)
	{
		this.width = width;
		this.border = border;
		this.spacingX = spacingX;
		this.percentOfRandomPlatforms = percentOfRandomPlatforms;

		platforms = new Platform[width];

		// Parent object of all platforms in this row
		rowGameObject = new GameObject("Row " + id);
		rowGameObject.transform.parent = rowParent; // Set parent of Row to WorldManager

		// First only instantiate all the platforms in the world
		InstantiateRow(platformPrefab, yPosition);
		// Then generate platform types
		GenerateRow(yPosition);
	}

	// Move row tiles down by given speed
	public void AnimateRow(float speed)
	{
		MoveTiles(speed);
		
		if (isBelowEndOfScreen())
		{
			ResetRow();
		}

		// By time more and more platforms should be random
		if(percentOfRandomPlatforms < 1f)
		{
			percentOfRandomPlatforms += percentOfRandomPlatformsIncrement * Time.deltaTime;
		}
	}

	// Calculate difference in position between frames and apply to all tiles
	private void MoveTiles(float speed)
	{
		for (int i = 0; i < width; i++)
		{
			Vector3 delta = Vector3.down * speed * Time.deltaTime;
			platforms[i].transform.position += delta;
		}
	}

	private bool isBelowEndOfScreen()
	{
		return platforms[0].transform.position.y <= -border;
	}

	// Respawns the row at the top of the screen
	private void ResetRow()
	{
		// Trigger event: platforms are destroyed when they go below '-border' coordinates
		WorldManager.TriggerPlatformDestroyEvent(-border);

		GenerateRow(border);
	}

	private void InstantiateRow(GameObject prefab, float yPosition)
	{
		for (int i = 0; i < width; i++)
		{
			Vector2 platformPosition = new Vector2((i - width / 2) * spacingX, yPosition);
			GameObject obj = Instantiate(prefab, platformPosition, Quaternion.identity, rowGameObject.transform);
			obj.name = "Platform " + (int)yPosition + ":" + i;
			platforms[i] = obj.GetComponent<Platform>();
		}
	}

	private void GenerateRow(float posY)
	{
		float randomPercent = Random.Range(0f, 1f);

		// Generate random row
		if(randomPercent < percentOfRandomPlatforms)
		{
			for (int i = 0; i < width; i++)
			{
				Vector2 platformPosition = new Vector2((i - width / 2) * spacingX, posY);
				platforms[i].GeneratePlatform(platformPosition, Item.NONE);
			}
			return;
		}

		// Generate predefined row
		// TODO: find a way to actually add predefined rows here
		int randomIndex = Random.Range(0, WorldManager.Instance.PredefinedPlatformTypes.Length);
		PlatformType[] platformTypes = WorldManager.Instance.PredefinedPlatformTypes[randomIndex];
		for (int i = 0; i < width; i++)
		{
			Vector2 platformPosition = new Vector2((i - width / 2) * spacingX, posY);
			PlatformType type = platformTypes[i];
			platforms[i].GeneratePlatform(platformPosition, Item.NONE, type);
		}
	}
}
