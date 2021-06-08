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
    public float moveAnimationSpeed = 2;
    [Tooltip("How much sould player be offset from platforms while animating movement, so it looks like jump")]
    public float moveAnimationOffset;

    // From how far away can player snap to a platform (used for movement)
    private float platformSnapRange;
    private bool isMoving;
    private Vector3 originalPosition, targetPosition;
    private float timeToMove;
    private PlayerActionQueue actions;

    private void Awake()
    {
        PlayerInput.OnSwipe += OnSwipe;
        WorldManager.OnPlatformDestroy += OnPlatformDestroy;
        actions = new PlayerActionQueue();
        timeToMove = 1.0f / moveAnimationSpeed; 
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
			if (PlayerInput.ActionIsMove(action) && !isMoving)
            {
                Move(action);
            }
            else if (PlayerInput.ActionIsMove(action) && isMoving)
            {
                // If player wants to move but is already moving, put it back in queue (still processing the movement)
                actions.PushFront(action);
            }

        }

		if (!isMoving)
        {
            SnapToClosestPlatformInRange();
        }
    }

    private void Move(PlayerAction action)
    {
        // Get how much to move and in what direction
        Vector3 movement = PlayerInput.MoveActionToVector3(action);
        movement.x *= WorldManager.Instance.PlatformDistanceX;
        movement.y *= WorldManager.Instance.PlatformDistanceY;

        // Before we move we have to check of there is a platform at that point
        Platform platform = WorldManager.Instance.GetPlatformWithinRange(transform.position + movement, platformSnapRange);

        if (platform != null)
		{
            //SnapToPlatform(platform);
            // TODO: here initiate player move
            StartCoroutine(MovePlayer(movement));
        }
    }

    private IEnumerator MovePlayer(Vector3 movement)
    {
        isMoving = true;

        float elapsedTime = 0;
        originalPosition = transform.position;
        targetPosition = originalPosition + movement;

        while (elapsedTime < timeToMove)
        {
            float percent = elapsedTime / timeToMove;
            
            // If movement is on X axis, add part of the sin() wave to move so it looks like parabola
            Vector3 sinOffset = Vector3.zero;
			if (Mathf.Abs(movement.x) > 0)
			{
                float f = Mathf.PI * percent;
                sinOffset = new Vector3(0, Mathf.Sin(f)) * moveAnimationOffset;
            }

            transform.position = Vector3.Lerp(originalPosition, targetPosition, percent) + sinOffset;
            elapsedTime += Time.deltaTime;
            yield return null;
		}

        transform.position = targetPosition;

        isMoving = false;
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
            PlayerDie();
        }
    }

    // If the given platform exists, snap to it (with some offset)
    private void SnapToPlatform(Platform platform)
    {
        if (platform.Type == PlatformType.SPIKES)
        {
            PlayerDie();
            return;
        }

        if (platform != null)
        {
            transform.position = platform.transform.position + platformOffset;
        }
    }

    private void PlayerDie()
	{
        Debug.Log("Player died...");
        Destroy(gameObject);
        Time.timeScale = 0f;
    }
}
