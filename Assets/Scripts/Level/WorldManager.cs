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

        if(Vector3.Distance(platform.transform.position, position) <= range)
		{
            return platform;
		}

        return null;
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

                if (platform.PlatformType == PlatformType.NONE)
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
