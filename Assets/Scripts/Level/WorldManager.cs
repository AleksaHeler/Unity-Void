using UnityEngine;

public class WorldManager : MonoBehaviour
{
    [Header("World size (# of platforms)")]
    public int Width = 5;
    public int Height = 5;

    [Header("Platform size (for spacing)")]
    public float PlatformHeight = 0.4f;
    public float PlatformXOffset = 1.5f;

    [Header("Platform/game settings")]
    public float PlatformSpeed = 0.2f;
    public GameObject PlatformPrefab;
    public int RandomSeed = 1337;

    // World data, contains Rows which contain Platforms
    private World world;
    

    private void Start()
    {
        Random.InitState(RandomSeed);

        // Calculate screen border as the position of the top and rightmpst pixel visible on screen (top right viewport -> world coordinates)
        Vector3 screenBorderPosition = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, 0));
        float screenBorderX = screenBorderPosition.x;
        float screenBorderY = screenBorderPosition.y;

        // Space evenly (from top to bottom of screen and left to right)
        float tileSpacingY = (screenBorderY * 2 + PlatformHeight) / Height;
        float tileSpacingX = ((screenBorderX - PlatformXOffset) * 2) / Width;
        screenBorderY += PlatformHeight / 2;

        // Instantiate the world/platforms
        world = new World(Width, Height, screenBorderY, PlatformPrefab, tileSpacingX, tileSpacingY);
    }

	private void Update()
	{
        // Moves platforms down
        world.AnimateWorld(PlatformSpeed);
    }
}
