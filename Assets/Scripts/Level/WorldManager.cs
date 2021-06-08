using System;
using System.Collections.Generic;
using UnityEngine;

// TODO: add a float how much the player needs to move on both x and y axis
// in order to reach the next platform at that point

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
    [Range(0f, 1f)]
    [Tooltip("How many platforms sould be random vs predetermined")]
    public float percentOfRandomPlatforms = 0.1f;
    public GameObject PlatformPrefab;

    public PlatformType[][] PredefinedPlatformTypes;

    // Singleton
    private static WorldManager _instance;
    public static WorldManager Instance { get { return _instance; } }

    // After a swipe is registered fire an event, parameter is destroyed platforms Y position
    public static event Action<float> OnPlatformDestroy = delegate { };


    // World data, contains Rows which contain Platforms
    private World world;
    private float platformDistanceX;
    private float platformDistanceY;
    public float PlatformDistanceX { get => platformDistanceX; }
    public float PlatformDistanceY { get => platformDistanceY; }


    private void Awake()
	{
        // Singleton
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }

        // My try at generating 'true' random seed with system time
        UnityEngine.Random.InitState(System.DateTime.Now.Ticks.GetHashCode());

        // Calculate screen border as the position of the top and rightmpst pixel visible on screen (top right viewport -> world coordinates)
        Vector3 screenBorderPosition = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, 0));
        float screenBorderX = screenBorderPosition.x;
        float screenBorderY = screenBorderPosition.y;

        // Space evenly (from top to bottom of screen and left to right)
        float tileSpacingY = (screenBorderY * 2 + PlatformHeight) / Height;
        float tileSpacingX = ((screenBorderX - PlatformXOffset) * 2) / Width;
        screenBorderY += PlatformHeight / 2;

        // Instantiate the world/platforms
        world = new World(Width, Height, screenBorderY, PlatformPrefab, tileSpacingX, tileSpacingY, transform, percentOfRandomPlatforms);

        platformDistanceX = tileSpacingX;
        platformDistanceY = tileSpacingY;
    }

	private void Update()
	{
        // Moves platforms down
        world.AnimateWorld(PlatformSpeed);
    }

    /// <summary>
    /// Returns a platform that is within the given range of the given position
    /// </summary>
    /// <param name="posX">Position on horizontal axis, left is 0, right is Width</param>
    /// <param name="posY">Position on vertical axis, botton is 0, top is Height</param>
    /// <returns></returns>
    public Platform GetPlatformWithinRange(Vector3 position, float range)
	{
        Platform platform = GetPlatformClosestToPos(position);

        if(Vector3.Distance(platform.transform.position, position) <= range)
		{
            return platform;
		}

        return null;
    }


    /// <summary>
    /// Returns a platform closest to given position
    /// </summary>
    /// <param name="position">Global position (transform.position)</param>
    /// <returns></returns>
    public Platform GetPlatformClosestToPos(Vector3 position)
	{
        // for each platform check distance and return one with min
        // private World world;
        Platform closestPlatform = null;
        float minDistance = Mathf.Infinity;
        foreach(Row row in world.Rows)
		{
            foreach (Platform platform in row.Platforms)
            {
                if (closestPlatform == null)
                {
                    closestPlatform = platform;
                    continue;
                }

                if(platform.Type == PlatformType.NONE)
				{
                    continue;
				}

                float distance = Vector3.Distance(platform.transform.position, position);
                if (distance < minDistance)
				{
                    minDistance = distance;
                    closestPlatform = platform;
				}
            }
		}

        return closestPlatform;
	}

    public static void TriggerPlatformDestroyEvent(float platformYPosition)
	{
        OnPlatformDestroy(platformYPosition);
	}
}
