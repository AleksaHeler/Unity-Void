using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState { NOT_MOVING, MOVING, FALLING, FALLING_TO_VOID, DIED }

// Subscribes to PlayerInput scripts event system which triggers the event on detected swipe.
// So this script then adds a movement action to actions queue on swipe
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerInventory))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
	#region Variables
	private PlayerState playerState;

	private float playerSpeed;
	private float platformSnapRange;
	private Vector3 platformOffset;
	private Vector3 movePoint;
	private Animator playerAnimator;

	private PlayerActionQueue actions;
	private GameSettings gameSettings;
	private PlayerInventory playerInventory;
	private PlatformHandler platformHandler;

	public static event Action<int> OnPlayerDeath = delegate { };
	#endregion // Variables

	#region Start, LateUpdate & OnDestroy
	private void Start()
	{
		// Subsctribe to events
		PlayerInput.OnSwipe += OnSwipe;
		WorldManager.OnPlatformDestroy += OnPlatformDestroy;

		actions = new PlayerActionQueue();
		platformHandler = new PlatformHandler();

		gameSettings = SettingsReader.Instance.GameSettings;
		playerInventory = GetComponent<PlayerInventory>();
		playerAnimator = GetComponent<Animator>();

		platformOffset = gameSettings.PlayerToPlatformOffset;
		platformSnapRange = gameSettings.PlayerToPlatformSnapRange;
		playerSpeed = gameSettings.PlayerSpeed;

		SnapToClosestSafePlatform();
	}

	private void OnDestroy()
	{
		// Unsubscribe from events
		PlayerInput.OnSwipe -= OnSwipe;
		WorldManager.OnPlatformDestroy -= OnPlatformDestroy;
	}

	private void LateUpdate()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Application.Quit();
		}

		if (playerState == PlayerState.DIED)
		{
			return;
		}

		if (playerState == PlayerState.MOVING && IsCloseToMovePoint())
		{
			playerState = PlayerState.NOT_MOVING;
		}

		// Update players position to match wanted position
		SnapToClosestPlatformInRange();
		HandlePhysics();

		// Handle different platforms
		if (playerState == PlayerState.NOT_MOVING)
		{
			Platform currentPlatform = GetPlatformWithinRange(transform.position);

			// Fall into the void
			if (currentPlatform == null)
			{
				actions.PushFront(PlayerAction.MOVE_DOWN);
				HandleMoveActions();
				return;
			}

			// Handle platform action by calling specific function from PlatformHandler class
			// Argument passed should be height from which the player has fallen
			platformHandler.InvokeAction(currentPlatform.PlatformType, 0);

			// Calls function Move() when there is input
			HandleMoveActions();
		}

		if (IsBelowScreenBorder())
		{
			PlayerDie();
		}
	}
	#endregion // Start, LateUpdate & OnDestroy

	Dictionary<PlatformType, Delegate> keyValuePairs;

	#region Movement

	private void HandleMoveActions()
	{
		if (actions.ActionCount > 0)
		{
			PlayerAction action = actions.Pop();

			if (ActionIsMove(action) && playerState == PlayerState.NOT_MOVING)
			{
				Move(action);
				return;
			}

			if (ActionIsMove(action) && playerState == PlayerState.NOT_MOVING)
			{
				actions.PushFront(action);
			}
		}
	}

	private void Move(PlayerAction action)
	{
		Vector3 movement = gameSettings.MovePlayerActionToVector3(action);
		movePoint += movement;
		playerState = PlayerState.MOVING;
		playerAnimator.SetTrigger("Jump");
		SnapToClosestPlatformInRange();
	}

	private void HandlePhysics() 
	{
		// TODO: add physics here
		transform.position = Vector3.MoveTowards(transform.position, movePoint, playerSpeed * Time.deltaTime); 
	}
	#endregion // Movement

	#region Helper functions
	private void CheckForCollectible()
	{
		Platform itemCheckPlatform = WorldManager.Instance.GetPlatformWithinRange(transform.position, platformSnapRange);
		ItemType item = ItemManager.Instance.ItemTypeAtPlatform(itemCheckPlatform);
		if (item != ItemType.NONE)
		{
			playerInventory.CollectItem(itemCheckPlatform);
		}
	}

	private Platform GetPlatformWithinRange(Vector3 position)
	{
		return WorldManager.Instance.GetPlatformWithinRange(movePoint, platformSnapRange);
	}

	private void SnapToClosestPlatformInRange()
	{
		Platform platform = WorldManager.Instance.GetPlatformWithinRange(movePoint, platformSnapRange);

		if (platform == null)
		{
			return;
		}

		SnapToPlatform(platform);
	}

	private void SnapToClosestSafePlatform()
	{
		Platform platform = WorldManager.Instance.GetSafePlatformClosestToPos(movePoint);
		SnapToPlatform(platform);
	}

	// If the given platform exists, snap to it (with some offset)
	private void SnapToPlatform(Platform platform)
	{
		Vector3 newPosition = platform.transform.position + platformOffset;
		movePoint = newPosition;

		CheckForCollectible();
	}

	private bool IsCloseToMovePoint()
	{
		float playerCheckTolerance = gameSettings.PlayerCheckTolerance;
		float distance = Vector3.Distance(transform.position, movePoint);
		return distance < playerCheckTolerance;
	}

	private bool IsBelowScreenBorder()
	{
		float playerCheckTolerance = gameSettings.PlayerCheckTolerance;
		float checkPositionY = gameSettings.ScreenBorderBottom + playerCheckTolerance;
		return transform.position.y < checkPositionY;
	}

	private bool ActionIsMove(PlayerAction action)
	{
		return gameSettings.MovePlayerActions.Contains(action);
	}

	public void PushFrontToActionQueue(PlayerAction action)
	{
		actions.PushFront(action);
	}

	// Destroy player, play lose sound and stop time
	public void PlayerDie()
	{
		Debug.LogWarning("Player died...");
		AudioManager.Instance.PlaySound("Lose");
		playerState = PlayerState.DIED;
		//playerAnimator.SetTrigger("Die");
		GetComponent<SpriteRenderer>().sprite = null;
		//Destroy(gameObject);
		OnPlayerDeath(0);
	}
	#endregion // Helper functions

	#region Event callbacks
	// Convert swipe direction to player action and add it to actions queue
	private void OnSwipe(SwipeDirection swipeDirection)
	{
		if (playerState == PlayerState.DIED)
		{
			return;
		}
		PlayerAction action = gameSettings.SwipeDirectionToPlayerAction[swipeDirection];
		actions.Push(action);
	}

	// Check if our platform was destroyed, and if it was its game over
	private void OnPlatformDestroy(float platformYPosition)
	{
		if (playerState == PlayerState.DIED)
		{
			return;
		}
		float distanceToDestroyedPlatform = Mathf.Abs(transform.position.y - platformYPosition);
		if (distanceToDestroyedPlatform < 1.5f * platformOffset.y)
		{
			PlayerDie();
		}
	}
	#endregion // Event callbacks

}
