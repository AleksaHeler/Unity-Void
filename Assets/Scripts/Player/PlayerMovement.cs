using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Subscribes to PlayerInput scripts event system which triggers the event on detected swipe.
// So this script then adds a movement action to actions queue on swipe
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerInventory))]
public class PlayerMovement : MonoBehaviour
{
	private float platformSnapRange;
	private bool isMoving;
	private Vector3 moveOriginalPosition, moveTargetPosition;
	private PlayerActionQueue actions;
	private GameSettings gameSettings;
	private PlayerInventory playerInventory;
	private Vector3 platformOffset;
	private float moveAnimationDuration = 0.2f;
	private float moveAnimationCurveOffset;

	private void Start()
	{
		// Subsctribe to events
		PlayerInput.OnSwipe += OnSwipe;
		WorldManager.OnPlatformDestroy += OnPlatformDestroy;

		actions = new PlayerActionQueue();

		gameSettings = SettingsReader.Instance.GameSettings;
		playerInventory = GetComponent<PlayerInventory>();

		platformSnapRange = gameSettings.PlayerToPlatformSnapRange;
		SnapToClosestPlatform();

		platformOffset = gameSettings.PlayerToPlatformOffset;
		moveAnimationDuration = gameSettings.MoveAnimationDuration;
		moveAnimationCurveOffset = gameSettings.MoveAnimationCurveOffset;
	}

	private void OnDestroy()
	{
		// Unsubscribe from events
		PlayerInput.OnSwipe -= OnSwipe;
		WorldManager.OnPlatformDestroy -= OnPlatformDestroy;
	}

	// If there is an available action: Get it from queue
	// If it is a move action: execute it if player is not already moving (else just add it back)
	// If it is anything else just ignore it: we dont have a way to handle them yet
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

	// Animate player jump
	private IEnumerator MovePlayerCoroutine(Vector3 movement, Platform platform)
	{
		isMoving = true;

		float elapsedTime = 0;
		moveOriginalPosition = transform.position;
		moveTargetPosition = moveOriginalPosition + movement;

		// Move player in the givent direction, with animation
		while (elapsedTime < moveAnimationDuration)
		{
			float animationPercent = elapsedTime / moveAnimationDuration;

			// If movement is on X axis, add part of the sin() wave to move so it looks like parabola
			Vector3 sineOffset = Vector3.zero;
			if (Mathf.Abs(movement.x) > 0)
			{
				float f = Mathf.PI * animationPercent;
				sineOffset = new Vector3(0, Mathf.Sin(f)) * moveAnimationCurveOffset;
			}

			transform.position = Vector3.Lerp(moveOriginalPosition, moveTargetPosition, animationPercent) + sineOffset;
			elapsedTime += Time.deltaTime;
			yield return null;
		}

		transform.position = moveTargetPosition;
		
		if (platform != null)
		{
			AudioManager.Instance.PlayPlatformSound(platform.PlatformType);
		}

		if(platform == null)
		{
			float velocity = 0;
			float gravity = gameSettings.PlayerGravity;

			// If we jumped into the void
			if (platform == null)
			{
				float worldBorderBottom = gameSettings.ScreenBorderBottom;
				// First animate player down then kill him
				while (transform.position.y > worldBorderBottom)
				{
					velocity += gravity;
					transform.position += Vector3.down * velocity * Time.deltaTime;
					yield return null;
				}
				PlayerDie();
				yield break;
			}
		}

		// If the platform is empty, jump there and fall
		if (platform.PlatformType == PlatformType.NONE)
		{
			platform = WorldManager.Instance.GetPlatformBelowPosition(transform.position);
			float velocity = 0;
			float gravity = gameSettings.PlayerGravity;

			while (transform.position.y - platformOffset.y > platform.transform.position.y)
			{
				velocity += gravity;
				transform.position += Vector3.down * velocity * Time.deltaTime;
				// Check if there is a collectible on the way down
				Platform itemCheckPlatform = WorldManager.Instance.GetPlatformWithinRange(transform.position, platformSnapRange);
				ItemType item = ItemManager.Instance.ItemTypeAtPlatform(itemCheckPlatform);
				if (item != ItemType.NONE)
				{
					playerInventory.CollectItem(itemCheckPlatform);
				}
				yield return null;
			}
			transform.position = platform.transform.position + platformOffset;
		}

		SnapToClosestPlatformInRange();

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
		ItemType item = ItemManager.Instance.ItemTypeAtPlatform(platform);
		if (item != ItemType.NONE)
		{
			playerInventory.CollectItem(platform);
		}

		transform.position = platform.transform.position + platformOffset;

		if (platform.PlatformType == PlatformType.SPIKES)
		{
			PlayerDie();
		}
	}

	// Convert swipe direction to player action and add it to actions queue
	private void OnSwipe(SwipeDirection swipeDirection)
	{
		PlayerAction action = gameSettings.SwipeDirectionToPlayerAction[swipeDirection];
		actions.Push(action);
	}

	// Check if OUR platform was destroyed, and if it was its game over
	private void OnPlatformDestroy(float platformYPosition)
	{
		float distanceToDestroyedPlatform = Mathf.Abs(transform.position.y - platformYPosition);
		if (distanceToDestroyedPlatform < 1.5f * platformOffset.y)
		{
			PlayerDie();
		}
	}

	// Destroy player, play lose sound and stop time
	private void PlayerDie()
	{
		Debug.LogWarning("Player died...");
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
