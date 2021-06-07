using UnityEngine;

// Contains world data: width/height
// And also an array of rows and animates them
public class World
{
	private int width;		// Length of Platforms array inside Row
	private int height;		// Length of Rows array inside this class

	private float border;	// Top-most/bottom-most coordinate of rows/platforms, used for respawning at the top of the screen
	private Row[] rows;     // Contains platforms


	// Creates all rows
	public World(int width, int height, float border, GameObject platformPrefab, float spacingX, float spacingY)
	{
		this.width = width;
		this.height = height;
		this.border = border;

		rows = new Row[height];

		for(int i = 0; i < height; i++)
		{
			// Create rows across the screen
			float yPosition = (i - height / 2) * spacingY;
			rows[i] = new Row(new Vector2(0, yPosition), width, platformPrefab, spacingX, border);
		}
	}

	// Animates all rows
	public void AnimateWorld(float speed)
	{
		for (int i = 0; i < height; i++)
		{
			rows[i].AnimateRow(speed);
		}
	}
}
