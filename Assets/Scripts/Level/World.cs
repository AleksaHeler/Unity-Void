using UnityEngine;

public class World
{
	private Row[] rows;				// Contains platforms
	public Row[] Rows { get => rows; }

	/// <summary>
	/// Create all rows
	/// </summary>
	/// <param name="parent">Transform of a GameObject that will be the parent of all rows and platforms (in editor hierarchy)</param>
	public World(Transform parent)
	{
		GameSettings gameSettings = SettingsReader.Instance.GameSettings;

		rows = new Row[gameSettings.RowsCount];

		// Create all rows
		for (int i = 0; i < rows.Length; i++)
		{
			// Create rows across the screen
			float yPosition = (i - rows.Length / 2) * gameSettings.PlatformSpacingY;
			rows[i] = new Row(yPosition, i, parent);
		}
	}

	/// Animates all rows
	public void AnimateWorld()
	{
		for (int i = 0; i < rows.Length; i++)
		{
			rows[i].AnimateRow();
		}
	}
}
