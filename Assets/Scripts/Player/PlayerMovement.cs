using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Subscribes(?) to PlayerInput scripts event system which triggers the event on detected swipe.
/// So this script then adds a movement action to actions queue on swipe
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    [Tooltip("Players offset from center of the platform, so he is actually on top of the platform")]
    public Vector3 platformOffset;

    // From how far away can player snap to a platform (used for movement)
    private float platformSnapRange;
    private PlayerActionQueue actions;

    private void Awake()
    {
        PlayerInput.OnSwipe += OnSwipe;
        WorldManager.OnPlatformDestroy += OnPlatformDestroy;
        actions = new PlayerActionQueue();
    }

    private void Start()
    {
        CalculatePlatformSnapRange();
        SnapToClosestPlatform();
    }

    private void Update()
    {
        // Check if there is an action to be done and do it
        if (actions.Count > 0)
        {
            PlayerAction action = actions.Pop();
            ExecuteAtion(action);
        }

        SnapToClosestPlatformInRange();
    }

    private void ExecuteAtion(PlayerAction action)
    {
        float deltaX = WorldManager.Instance.PlatformDistanceX;
        float deltaY = WorldManager.Instance.PlatformDistanceY;
        Vector3 movement;

        switch (action)
        {
            case PlayerAction.MOVE_UP:
                movement = new Vector3(0, deltaY, 0);
                break;
            case PlayerAction.MOVE_DOWN:
                movement = new Vector3(0, -deltaY, 0);
                break;
            case PlayerAction.MOVE_LEFT:
                movement = new Vector3(-deltaX, 0, 0);
                break;
            case PlayerAction.MOVE_RIGHT:
                movement = new Vector3(deltaX, 0, 0);
                break;
            default:
                movement = Vector3.zero;
                break;
        }

        // Before we move we have to check of there is a platform at that point
        Vector3 checkPosition = transform.position + movement;

        Platform platform = WorldManager.Instance.GetPlatformWithinRange(checkPosition, platformSnapRange);

        if (platform != null)
            SnapToPlatform(platform);
    }

    // Snap range is the minimum of platform spacings on both axis (*0.6)
    private void CalculatePlatformSnapRange()
    {
        float snapRangeX = WorldManager.Instance.PlatformDistanceX;
        float snapRangeY = WorldManager.Instance.PlatformDistanceY;
        platformSnapRange = Mathf.Min(snapRangeX, snapRangeY) * 0.8f;
	}

    private void SnapToClosestPlatformInRange()
    {
        Platform platform = WorldManager.Instance.GetPlatformWithinRange(transform.position, platformSnapRange);
        SnapToPlatform(platform);
    }

    private void SnapToClosestPlatform()
    {
        Platform platform = WorldManager.Instance.GetPlatformClosestToPos(transform.position);
        SnapToPlatform(platform);
    }

    // Convert swipe direction to player action and add it to actions queue
    private void OnSwipe(SwipeData swipeData)
    {
        PlayerAction action = PlayerInput.SwipeDirectionToPlayerAction(swipeData.Direction);

        if (action != PlayerAction.NONE)
        {
            actions.Push(action);
        }
    }

    // Check if our platform was destroyed, and if it was its game over
    private void OnPlatformDestroy(float platformYPosition)
    {
        if (Mathf.Abs(transform.position.y - platformYPosition) < 1.5f * platformOffset.y)
        {
            Debug.Log("Player died...");
            Destroy(gameObject);
        }
    }

    // If the given platform exists, snap to it (with some offset)
    private void SnapToPlatform(Platform platform)
    {
        if (platform != null)
        {
            transform.position = platform.transform.position + platformOffset;
        }
    }
}
