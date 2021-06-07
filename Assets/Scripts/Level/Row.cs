using UnityEngine;

// Contains an array of Platforms
public class Row : MonoBehaviour
{
	private int width;
	private float border;
	private float spacingX;

	private Platform[] platforms;	// References to Platform scripts on platform GameObjects

	// Instantiate each platform in this row and set its parameters
	public Row(Vector2 newPosition, int width, GameObject platformPrefab, float spacingX, float border)
	{
		this.width = width;
		this.border = border;
		this.spacingX = spacingX;

		platforms = new Platform[width];

		for (int i = 0; i < width; i++)
		{
			// Calculate position of this Platform
			Vector2 platformPosition = new Vector2((newPosition.x + i - width / 2) * spacingX, newPosition.y);
			// Instantiate the object and keep track of its script component
			GameObject obj = Instantiate(platformPrefab, newPosition, Quaternion.identity);
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
