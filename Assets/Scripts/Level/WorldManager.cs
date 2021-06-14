using System;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{

    #region Variables
    // Singleton
    private static WorldManager _instance;
    public static WorldManager Instance { get { return _instance; } }

    // When a platform is destroyed when it goes off screen it triggers this event, parameter is destroyed platforms Y position
    public static event Action<float> OnPlatformDestroy = delegate { };

    // World data, contains Rows which contain Platforms
    private World world;
    #endregion // Variables

    #region Awake & Update functions
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

        // Instantiate the world/platforms
        world = new World(gameObject.transform);
    }

    private void Update()
    {
        // Moves platforms down
        world.AnimateWorld();
    }
    #endregion // Awake & Update functions

    #region Get platform functions
    // Convert given platform to NORMAL type
    public void SetPlatformToSafe(Platform platform)
	{
        platform.GeneratePlatform(platform.transform.position, ItemType.NONE, platform.PlatformID, PlatformType.NORMAL);
	}

    // Returns closest platform that is within the range of the given position
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

        // For each row check only the one platform in column closest to given position
        foreach (Row row in world.Rows)
        {
            Platform platform = row.Platforms[platformIndex];
            float platformY = platform.transform.position.y;

            // If the platform is below given position add it to list
            if (platformY < position.y)
            {
                platformsBelow.Add(platform);
            }
        }

        float closestDistanceBelow = Mathf.Infinity;
        Platform closestPlatformBelow = null;

        if(platformsBelow.Count == 0)
		{
            return null;
		}

        //Debug.Log("Given pos: " + position.y);
        foreach (Platform platformBelow in platformsBelow)
        {
            // position.y   vs   platformBelow.transform.position.y
            float distanceY = Mathf.Abs(position.y - platformBelow.transform.position.y);
            bool isWalkablePlatform = platformBelow.PlatformType != PlatformType.NONE;
            bool isNewClosestPlatform = distanceY < closestDistanceBelow && isWalkablePlatform;
            //Debug.Log("Below pos: " + platformBelow.transform.position.y);
            if (closestPlatformBelow == null || isNewClosestPlatform)
            {
                closestDistanceBelow = distanceY;
                closestPlatformBelow = platformBelow;
            }
        }
        return closestPlatformBelow;
    }

    // Returns a platform closest to given position that is safe to walk on
    public Platform GetSafePlatformClosestToPos(Vector3 position)
    {
        List<PlatformType> unsafePlatforms = SettingsReader.Instance.GameSettings.UnsafePlatforms;
        return GetPlatformClosestToPos(position, unsafePlatforms);
    }

    // Returns a platform closest to given position
    public Platform GetPlatformClosestToPos(Vector3 position)
    {
        return GetPlatformClosestToPos(position, new List<PlatformType>());
    }

    // Returns a platform closest to given position that isn't the given mask type
    public Platform GetPlatformClosestToPos(Vector3 position, List<PlatformType> excludedPlatformTypes)
    {
        Platform closestPlatform = null;
        float closestDistance = Mathf.Infinity;

        foreach (Row row in world.Rows)
        {
            foreach (Platform platform in row.Platforms)
            {
                if (excludedPlatformTypes.Contains(platform.PlatformType))
                {
                    continue;
                }

                float distance = Vector2.Distance(platform.transform.position, position);

                if (closestPlatform == null || distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPlatform = platform;
                }
            }
        }

        return closestPlatform;
    }

    // Returns index of the platform in a row, closest to the given positions X coordinate (0, 1, 2, 3...)
    private int PositionToPlatformIndex(Vector3 position)
    {

        int numberOfPlatforms = SettingsReader.Instance.GameSettings.PlatformsCount;
        float platformSpacing = SettingsReader.Instance.GameSettings.PlatformSpacingX;
        float closestDistance = Mathf.Infinity;
        int closestIndex = 0;

        for (int index = 0; index < numberOfPlatforms; index++)
        {
            float platformPositionX = (index - numberOfPlatforms / 2) * platformSpacing;
            float distance = Mathf.Abs(position.x - platformPositionX);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = index;
            }
        }

        return closestIndex;
    }
    #endregion // Get platform functions

    #region Helper functions
    public static void TriggerPlatformDestroyEvent(float platformYPosition)
    {
        OnPlatformDestroy(platformYPosition);
    }
    #endregion //  Helper functions

}
