using System.Collections.Generic;
using UnityEngine;


// Contains an array of Platforms
public class Row : MonoBehaviour
{
	private int numberOfPlatforms;                      // Number of platforms in this row
	private float worldBorderTop;
	private float worldBorderBottom;
	private float platformSpacing;						// Horizontal distance between two platforms 
	private float percentOfRandomRows;                  // Ratio of random and pregenerated rows
	private GameObject rowGameObject;					// Empty GameObject that will be the parent of all Platforms of this row 


	private Platform[] platforms;						// References to Platform scripts on platform GameObjects
	public Platform[] Platforms { get => platforms; }


	/// <summary>
	/// Instantiate each platform in this row and set their parameters
	/// </summary>
	/// <param name="yPosition">Position of this row on vertical axis</param>
	/// <param name="id">Unique ID for this row (usually 0-5)</param>
	/// <param name="rowParent">GameObject that will be the parent of this row</param>
	public Row(float yPosition, int id, Transform rowParent)
	{
		// Get data from game settings
		GameSettings gameSettings = SettingsReader.Instance.GameSettings;
		worldBorderTop = gameSettings.ScreenBorderTop;
		worldBorderBottom = gameSettings.ScreenBorderBottom;
		platformSpacing = gameSettings.PlatformSpacingX;
		percentOfRandomRows = gameSettings.PercentOfRandomPlatforms;
		numberOfPlatforms = gameSettings.PlatformsCount;

		platforms = new Platform[numberOfPlatforms];

		// Parent object of all platforms in this row
		string gameObjectName = "Row " + id;
		rowGameObject = new GameObject(gameObjectName);
		rowGameObject.transform.SetParent(rowParent);

		InstantiateRow(yPosition);
		GenerateRow(yPosition);
	}

	// Move platforms down by given speed
	public void AnimateRow()
	{
		MovePlatforms();
		
		if (RowIsBelowEndOfScreen())
		{
			ResetRow();
		}
	}

	// Calculate difference in position between frames and apply to all platforms
	private void MovePlatforms()
	{
		float speed = SettingsReader.Instance.GameSettings.PlatformSpeed;
		Vector3 delta = Vector3.down * speed * Time.deltaTime;

		for (int i = 0; i < numberOfPlatforms; i++)
		{
			platforms[i].transform.position += delta;
		}
	}

	private bool RowIsBelowEndOfScreen()
	{
		return platforms[0].transform.position.y <= worldBorderBottom;
	}

	// Respawns the row at the top of the screen
	private void ResetRow()
	{
		// Trigger event: platforms are destroyed when they go below '-border' coordinates on Y axis
		WorldManager.TriggerPlatformDestroyEvent(worldBorderBottom);

		GenerateRow(worldBorderTop);
	}

	// Instantate platform game objects
	private void InstantiateRow(float yPosition)
	{
		for (int i = 0; i < numberOfPlatforms; i++)
		{
			Vector2 platformPosition = new Vector2((i - numberOfPlatforms / 2) * platformSpacing, yPosition);

			GameObject platformPrefab = SettingsReader.Instance.GameSettings.PlatformPrefab;
			GameObject platformGameObject = Instantiate(platformPrefab, platformPosition, Quaternion.identity, rowGameObject.transform);
			platformGameObject.name = "Platform " + (int)yPosition + ":" + i;
			platforms[i] = platformGameObject.GetComponent<Platform>();
		}
	}

	// Moves platforms to top of the screen and generates platform types for the row
	private void GenerateRow(float yPosition)
	{
		// Check if the row should be random or predetermined
		float randomPercent = Random.Range(0f, 1f);
		bool rowIsRandom = randomPercent < percentOfRandomRows;

		// Used in case the row is predetermined
		int predefinedRowsCount = SettingsReader.Instance.GameSettings.PredefinedRows.Length;
		int randomIndex = Random.Range(0, predefinedRowsCount);

		// Go trough the row and generate each platform
		int i = 0;
		foreach(Platform platform in platforms)
		{
			float xPosition = (i - numberOfPlatforms / 2) * platformSpacing;
			Vector2 platformPosition = new Vector2(xPosition, yPosition);
			if (rowIsRandom)
			{
				platform.GeneratePlatform(platformPosition, ItemType.NONE, i);
			}
			if (!rowIsRandom)
			{
				PlatformType type = SettingsReader.Instance.GameSettings.PredefinedRows[randomIndex][i];
				platform.GeneratePlatform(platformPosition, ItemType.NONE, i, type);
			}
			i++;
		}

		ItemManager.Instance.GenerateItemArrayForRow(platforms);

		return;
	}
}
