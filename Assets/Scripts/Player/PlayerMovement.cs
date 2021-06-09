using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Subscribes to PlayerInput scripts event system which triggers the event on detected swipe.
// So this script then adds a movement action to actions queue on swipe
public class PlayerMovement : MonoBehaviour
{
    [Tooltip("Players offset from center of the platform, so he is actually on top of the platform")]
    public Vector3 platformOffset;
    public float moveAnimationDuration = 0.2f;
    [Tooltip("How much sould player be offset from platforms while animating movement, so it looks like jump")]
    public float moveAnimationOffset;

    // From how far away can player snap to a platform (used for movement)
    private float platformSnapRange; 
    private bool isMoving;                                  
    private Vector3 moveOriginalPosition, moveTargetPosition;
    private PlayerActionQueue actions;
    private GameSettings gameSettings;

    private void Start()
    {
        // Subsctribe to events
        PlayerInput.OnSwipe += OnSwipe;
        WorldManager.OnPlatformDestroy += OnPlatformDestroy;

        actions = new PlayerActionQueue();

        gameSettings = SettingsReader.Instance.GameSettings;

        platformSnapRange = SettingsReader.Instance.GameSettings.PlayerToPlatformSnapRange;
        SnapToClosestPlatform();
    }

	private void OnDestroy()
	{
        // Unsubscribe from events
        PlayerInput.OnSwipe -= OnSwipe;
        WorldManager.OnPlatformDestroy -= OnPlatformDestroy;
    }

	/// <summary>
	/// If there is an available action: Get it from queue
	/// If it is a move action: execute it if player is not already moving (else just add it back)
	/// If it is anything else just ignore it: we dont have a way to handle them yet
	/// </summary>
	private void Update()
    {
        if (actions.ActionCount > 0)
        {
            PlayerAction action = actions.Pop();


            if (ActionIsMove(action) && !isMoving)
            {
                Move(action);
                return;
            }

            if (ActionIsMove(action) && isMoving)
            {
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
        Vector3 movement = MovePlayerActionToVector3(action);

        // Before we move we have to check of there is a platform at that point
        Platform platform = WorldManager.Instance.GetPlatformWithinRange(transform.position + movement, platformSnapRange);

        StartCoroutine(MovePlayerCoroutine(movement, platform));
    }


    /// <summary>
    /// Animate player jump
    /// </summary>
    /// <param name="movement">How much to move</param>
    /// <param name="platform">What platform is at destination</param>
    /// <returns></returns>
    private IEnumerator MovePlayerCoroutine(Vector3 movement, Platform platform)
    {
        // If platform at destination doesnt exist, just dont move for now
        if (platform == null)
        {
            yield break;
        }

        isMoving = true;

        float elapsedTime = 0;
        moveOriginalPosition = transform.position;
        moveTargetPosition = moveOriginalPosition + movement;

        while (elapsedTime < moveAnimationDuration)
        {
            float animationPercent = elapsedTime / moveAnimationDuration;
            
            // If movement is on X axis, add part of the sin() wave to move so it looks like parabola
            Vector3 sineOffset = Vector3.zero;
			if (Mathf.Abs(movement.x) > 0)
			{
                float f = Mathf.PI * animationPercent;
                sineOffset = new Vector3(0, Mathf.Sin(f)) * moveAnimationOffset;
            }

            transform.position = Vector3.Lerp(moveOriginalPosition, moveTargetPosition, animationPercent) + sineOffset;
            elapsedTime += Time.deltaTime;
            yield return null;
		}

        if (platform != null)
        {
            AudioManager.Instance.PlayPlatformSound(platform.PlatformType);
        }

        transform.position = moveTargetPosition;

        isMoving = false;
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

    // If the given platform exists, snap to it (with some offset)
    private void SnapToPlatform(Platform platform)
    {
        if (platform.PlatformType == PlatformType.SPIKES)
        {
            PlayerDie();
            return;
        }

        if (platform != null)
        {
            transform.position = platform.transform.position + platformOffset;
        }
    }

    // Convert swipe direction to player action and add it to actions queue
    private void OnSwipe(SwipeDirection swipeDirection)
    {
        PlayerAction action = PlayerInput.SwipeDirectionToPlayerAction(swipeDirection);

        if (action != PlayerAction.NONE)
        {
            actions.Push(action);
        }
    }

    // Check if OUR platform was destroyed, and if it was its game over
    private void OnPlatformDestroy(float platformYPosition)
    {
        if (Mathf.Abs(transform.position.y - platformYPosition) < 1.5f * platformOffset.y)
        {
            PlayerDie();
        }
    }

    // Destroy player, play lose sound and stop time
	private void PlayerDie()
	{
        Debug.Log("Player died...");
        AudioManager.Instance.PlaySound("Lose");
        Destroy(gameObject);
        Time.timeScale = 0f;
    }
    
    private bool ActionIsMove(PlayerAction action)
	{
        return gameSettings.MovePlayerActions.Contains(action);
	}

    private Vector3 MovePlayerActionToVector3(PlayerAction action)
    {
        Vector3 moveAmount = gameSettings.MovePlayerActionToVector3[action];
        moveAmount.x *= gameSettings.PlatformSpacingX;
        moveAmount.y *= gameSettings.PlatformSpacingY;
        return moveAmount;
    }
}
