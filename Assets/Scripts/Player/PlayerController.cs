using System;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Subscribes to PlayerInput scripts event system which triggers the event on detected swipe.
// So this script then adds a movement action to actions queue on swipe
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerInventory))]
partial class PlayerController : MonoBehaviour
{
	#region Variables
	private PlayerState playerState;
	private float lastFallDistance;
	private PlayerAction lastPlayerAction;
	private Platform currentPlatform;

	private float moveVectorMaxMagnitude;
	private float moveVectorMinMagnitude;
	private float playerSpeed;
	private float platformSnapRange;
	private Vector3 platformOffset;
	private Vector3 movePoint;

	private PlayerActionQueue actions;
	private GameSettings gameSettings;
	private PlayerInventory playerInventory;
	private PlatformHandler platformHandler;
	private PhotonView photonView;

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
		photonView = GetComponent<PhotonView>();

		lastFallDistance = 0;
		lastPlayerAction = PlayerAction.NONE;


		moveVectorMinMagnitude = gameSettings.MoveVectorMinMagnitude;
		moveVectorMaxMagnitude = gameSettings.MoveVectorMaxMagnitude;
		platformOffset = gameSettings.PlayerToPlatformOffset;
		platformSnapRange = gameSettings.PlayerToPlatformSnapRange;
		playerSpeed = gameSettings.PlayerSpeed;

		SnapToStartingPosition();
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

		if (IsBelowScreenBorder())
		{
			PlayerDie();
		}

		if (playerState == PlayerState.MOVING && IsCloseToMovePoint())
		{
			playerState = PlayerState.NOT_MOVING;
			if(currentPlatform != null)
			{
				AudioManager.Instance.PlayPlatformSound(currentPlatform.PlatformType);
			}
		}

		// Update players position to match target position
		SnapToClosestPlatformInRange();
		HandlePhysics();

		// Handle different platforms
		if (PlayerIsNotMoving())
		{
			Platform currentPlatform = GetPlatformWithinRange(transform.position);

			// Fall into the void
			if (currentPlatform == null)
			{
				actions.PushFront(PlayerAction.MOVE_DOWN);
				HandleMoveActions();
				return;
			}

			platformHandler.InvokeAction(currentPlatform.PlatformType, this);

			// Calls function Move() when there is input
			HandleMoveActions();
		}
	}
	#endregion // Start, LateUpdate & OnDestroy

	#region Movement
	private void HandleMoveActions()
	{
		if(actions.ActionCount == 0)
		{
			return;
		}

		PlayerAction action = actions.Pop();


		if (ActionIsMove(action) && PlayerIsNotMoving())
		{
			Move(action);
		}
	}

	private void Move(PlayerAction action)
	{
		lastPlayerAction = action;

		if (playerState == PlayerState.STUCK_IN_SLIME)
		{
			return;
		}

		Vector3 movement = gameSettings.PlayerActionToVector3(action);
		movePoint += movement;

		playerState = PlayerState.MOVING;
		Platform snappedPlatform = SnapToClosestPlatformInRange();

		if (snappedPlatform == null)
		{
			actions.PushFront(PlayerAction.MOVE_DOWN);
			currentPlatform = null;
		}

		if (action == PlayerAction.MOVE_DOWN)
		{
			lastFallDistance += Mathf.Abs(movement.y);
		}
		else
		{
			lastFallDistance = 0;
		}
	}

	// Calculates difference player has to move to get to 'movePoint'
	// and clamps the vector to some min and max values and sums that vector and position
	private void HandlePhysics()
	{
		Vector3 playerToDestinationVector = movePoint - transform.position;

		// If move is on X axis (left/right)
		Vector3 sineOffset = Vector3.zero;
		if (playerToDestinationVector.x != 0)
		{
			float leftoverMoveDistance = playerToDestinationVector.x;
			float totalMoveDistance = gameSettings.PlatformSpacingX;
			float animationPercent = Mathf.Abs(leftoverMoveDistance / totalMoveDistance);
			float sine = Mathf.Sin(animationPercent * Mathf.PI) * gameSettings.PlayerJumpAnimationHeight;
			sineOffset = Vector3.up * sine * Time.deltaTime;
		}

		transform.position = Vector3.MoveTowards(transform.position, movePoint, playerSpeed * Time.deltaTime) + sineOffset;
	}
	#endregion // Movement

	#region Helper functions
	public void GetStuckInSlime()
	{
		lastPlayerAction = PlayerAction.NONE;
		playerState = PlayerState.STUCK_IN_SLIME;
	}

	public void GetUnstuckFromSlime()
	{
		playerState = PlayerState.NOT_MOVING;
		actions.PushFront(lastPlayerAction);
	}
	private void CheckForCollectible()
	{
		Platform itemCheckPlatform = WorldManager.Instance.GetPlatformWithinRange(transform.position, platformSnapRange);
		ItemType item = ItemManager.Instance.GetItemTypeAtPlatform(itemCheckPlatform);
		if (item != ItemType.NONE)
		{
			playerInventory.CollectItem(itemCheckPlatform);
		}
	}

	private Platform GetPlatformWithinRange(Vector3 position)
	{
		return WorldManager.Instance.GetPlatformWithinRange(movePoint, platformSnapRange);
	}

	private Platform SnapToClosestPlatformInRange()
	{
		Platform platform = WorldManager.Instance.GetPlatformWithinRange(movePoint, platformSnapRange);

		if (platform == null)
		{
			return platform;
		}

		SnapToPlatform(platform);

		return platform;
	}

	private void SnapToStartingPosition()
	{
		float platformSpacingX = gameSettings.PlatformSpacingX;
		float platformSpacingY = gameSettings.PlatformSpacingY;
		float positionX = -gameSettings.PlatformsCount / 2.0f;
		Vector3 startingPosition = new Vector3(positionX * platformSpacingX, -platformSpacingY);

		Platform platform = WorldManager.Instance.GetPlatformWithinRange(startingPosition, platformSnapRange);
		WorldManager.Instance.SetPlatformToSafe(platform);

		movePoint = startingPosition;
		transform.position = startingPosition;

		SnapToPlatform(platform);
	}

	// If the given platform exists, snap to it (with some offset)
	private void SnapToPlatform(Platform platform)
	{
		Vector3 newPosition = platform.transform.position + platformOffset;
		movePoint = newPosition;
		currentPlatform = platform;

		CheckForCollectible();
	}


	private bool PlayerIsNotMoving()
	{
		return playerState == PlayerState.NOT_MOVING || playerState == PlayerState.STUCK_IN_SLIME;
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
		Instantiate(gameSettings.PlayerDeathParticles, transform.position, Quaternion.identity);
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
