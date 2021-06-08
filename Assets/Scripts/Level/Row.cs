using UnityEngine;

// Contains an array of Platforms
public class Row : MonoBehaviour
{
	private int width;				// Number of platforms in this row
	private float border;			// Top/bottom max world coordinates of the screen
	private float spacingX;			// Horizontal distance between two platforms 

	private Platform[] platforms;   // References to Platform scripts on platform GameObjects

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
	public Row(float yPosition, int width, GameObject platformPrefab, float spacingX, float border, int id, Transform rowParent)
	{
		this.width = width;
		this.border = border;
		this.spacingX = spacingX;

		platforms = new Platform[width];
		rowGameObject = new GameObject("Row " + id);
		rowGameObject.transform.parent = rowParent;	// Set parent of Row to WorldManager

		for (int i = 0; i < width; i++)
		{
			// Calculate position of this Platform
			Vector2 platformPosition = new Vector2((i - width / 2) * spacingX, yPosition);
			// Instantiate the object and keep track of its script component
			GameObject obj = Instantiate(platformPrefab, platformPosition, Quaternion.identity, rowGameObject.transform);
			obj.name = "Platform " + id + ":" + i;
			platforms[i] = obj.GetComponent<Platform>();
			// Generate this platform based on given parameters
			platforms[i].GeneratePlatform(platformPosition, Item.NONE);
		}
	}

	// Move row tiles down by given speed
	public void AnimateRow(float speed)
	{
		MoveTiles(speed);
		
		if (isBelowEndOfScreen())
		{
			ResetRow();
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
		for (int i = 0; i < width; i++)
		{
			// Calculate position of this Platform
			Vector2 platformPosition = new Vector2(( i - width / 2) * spacingX, border);
			// Generate this platform based on given parameters
			platforms[i].GeneratePlatform(platformPosition, Item.NONE);
		}
	}
}
