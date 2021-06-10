using System;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    // Singleton
    private static WorldManager _instance;
    public static WorldManager Instance { get { return _instance; } }

    // When a platform is destroyed when it goes off screen it triggers this event, parameter is destroyed platforms Y position
    public static event Action<float> OnPlatformDestroy = delegate { };

    // World data, contains Rows which contain Platforms
    private World world;


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

        int seed = System.DateTime.Now.Ticks.GetHashCode();
        UnityEngine.Random.InitState(seed);

        // Instantiate the world/platforms
        world = new World(transform);
    }

	private void Update()
	{
        // Moves platforms down
        world.AnimateWorld();
    }

    // Returns a platform that is within the given range of the given position
    public Platform GetPlatformWithinRange(Vector3 position, float range)
    {
        Platform platform = GetPlatformClosestToPos(position);

        if (Vector3.Distance(platform.transform.position, position) <= range)
        {
            return platform;
        }

        return null;
    }

    // Returns a platform that is within the given range of the given position
    public Platform GetPlatformBelowPosition(Vector3 position)
    {
        List<Platform> platformsBelow = new List<Platform>();
        int platformIndex = PositionToPlatformIndex(position);

        if(platformIndex < 0)
		{
            return null;
		}

        // For each row check only the one platform in column closest to given position
        foreach(Row row in world.Rows) 
        {
            Platform platform = row.Platforms[platformIndex];

            // If the platform is below given position add it to list
            float platformY = platform.transform.position.y;
            if (platformY < position.y)
            {
                platformsBelow.Add(platform);
            }
        }

        float closestDistanceBelow = Mathf.Infinity;
        Platform closestPlatformBelow = null;
        foreach(Platform platformBelow in platformsBelow)
        {
            float distance = Vector3.Distance(transform.position, platformBelow.transform.position);
            bool walkablePlatform = platformBelow.PlatformType != PlatformType.NONE;
            bool newClosestPlatform = distance < closestDistanceBelow && walkablePlatform;

            if (closestPlatformBelow == null || newClosestPlatform)
			{
                closestDistanceBelow = distance;
                closestPlatformBelow = platformBelow;
			}
		}

        return closestPlatformBelow;
    }

    // Returns a platform closest to given position that isnt spikes
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

    private int PositionToPlatformIndex(Vector3 position)
	{
        Platform closestPlatform = null;
        float closestDistanceX = Mathf.Infinity;

        Row firstRow = world.Rows[0];
        foreach (Platform platform in firstRow.Platforms)
        {
            float distanceX = Mathf.Abs(position.x - platform.transform.position.x);

            if (closestPlatform == null || distanceX < closestDistanceX)
			{
                closestDistanceX = distanceX;
                closestPlatform = platform;
			}
        }

        float snapRange = SettingsReader.Instance.GameSettings.PlayerToPlatformSnapRange;
        if (closestDistanceX > snapRange)
		{
            return -1;
		}

        return closestPlatform.PlatformID;
	}

    public static void TriggerPlatformDestroyEvent(float platformYPosition)
	{
      OnPlatformDestroy(platformYPosition);
	}
}
