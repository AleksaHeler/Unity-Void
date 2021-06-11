using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Subscribes to PlayerInput scripts event system which triggers the event on detected swipe.
// So this script then adds a movement action to actions queue on swipe
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerInventory))]
//[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
	#region Variables
	private bool playerDied;
	private bool isStuckInSlime;
	private Vector3 platformOffset;
	private float platformSnapRange;

	private bool isMoving;
	private Vector3 moveOriginalPosition, moveTargetPosition;
	private float moveAnimationDuration = 0.2f;
	private float moveAnimationCurveOffset;

	private PlayerActionQueue actions;
	private GameSettings gameSettings;
	private PlayerInventory playerInventory;
	private Animator playerAnimator;
	private List<PlayerAction> lastActions;


	public static event Action<int> OnPlayerDeath = delegate { };
	#endregion // Variables

	#region Start, LateUpdate & OnDestroy
	private void Start()
	{
		// Subsctribe to events
		PlayerInput.OnSwipe += OnSwipe;
		WorldManager.OnPlatformDestroy += OnPlatformDestroy;

		actions = new PlayerActionQueue();
		lastActions = new List<PlayerAction>();

		gameSettings = SettingsReader.Instance.GameSettings;
		playerInventory = GetComponent<PlayerInventory>();
		playerAnimator = GetComponent<Animator>();

		playerDied = false;
		isStuckInSlime = false;
		platformOffset = gameSettings.PlayerToPlatformOffset;
		platformSnapRange = gameSettings.PlayerToPlatformSnapRange;
		moveAnimationDuration = gameSettings.MoveAnimationDuration;
		moveAnimationCurveOffset = gameSettings.MoveAnimationCurveOffset;

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

		if (playerDied)
		{
			return;
		}

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
	#endregion // Start, LateUpdate & OnDestroy

	#region Movement
	private void Move(PlayerAction action)
	{
		// TODO: make this pretty and normal
		if (isStuckInSlime)
		{
			lastActions.Insert(0, action);

			if (lastActions.Count > 3)
			{
				lastActions.RemoveAt(3);
			}
			if (lastActions.Count == 3)
			{
				PlayerAction lastAction1 = lastActions[1];
				PlayerAction lastAction2 = lastActions[2];

				if (action == lastAction1 && action == lastAction2)
				{
					lastActions.Clear();
					isStuckInSlime = false;
				}
				else
				{
					return;
				}
			}
			else
			{
				return;
			}
		}

		// Get how much to move and in what direction
		Vector3 movement = gameSettings.MovePlayerActionToVector3(action);

		// Before we move we have to get platform at destination
		Vector3 finalPosition = transform.position + movement;
		Platform platform = WorldManager.Instance.GetPlatformWithinRange(finalPosition, platformSnapRange);

		StartCoroutine(MovePlayerCoroutine(movement, platform));
	}

	// Interpolate player jump
	private IEnumerator MovePlayerCoroutine(Vector3 movement, Platform platform)
	{
		isMoving = true;

		float elapsedTime = 0;
		moveOriginalPosition = transform.position;
		moveTargetPosition = moveOriginalPosition + movement;

		// Move player in the givent direction, with animation
		while (elapsedTime < moveAnimationDuration && movement != Vector3.zero)
		{
			MovePlayerDownOneFrame(movement, elapsedTime);
			elapsedTime += Time.deltaTime;
			yield return null;
		}

		transform.position = moveTargetPosition;


		if (platform == null)
		{
			StartCoroutine(PlayerFallToVoidCoroutine());
			yield break;
		}

		AudioManager.Instance.PlayPlatformSound(platform.PlatformType);

		// If the platform is empty, jump there and fall
		if (platform.PlatformType == PlatformType.NONE)
		{
			// Get next platform below the player, to which we should fall
			platform = WorldManager.Instance.GetPlatformBelowPosition(transform.position - platformOffset * 1.2f);
			if (platform == null)
			{
				StartCoroutine(PlayerFallToVoidCoroutine());
				yield break;
			}
			float fallDistance = Mathf.Abs(transform.position.y - platformOffset.y - platform.transform.position.y);

			// If there is no platform below player
			if (platform == null)
			{
				StartCoroutine(PlayerFallToVoidCoroutine());
				yield break;
			}

			float velocity = 0;
			float gravity = gameSettings.PlayerGravity;

			while (transform.position.y - platformOffset.y > platform.transform.position.y)
			{
				velocity += gravity;
				transform.position += Vector3.down * velocity * Time.deltaTime;
				CheckForCollectible();
				yield return null;
			}
			transform.position = platform.transform.position + platformOffset;

			// If player fell to his death
			if (platform.PlatformType != PlatformType.GRASS)
			{
				float platformSpacing = SettingsReader.Instance.GameSettings.PlatformSpacingY;
				if (fallDistance > platformSpacing * 1.5f)
				{
					PlayerDie();
					yield break;
				}
			}
			SnapToClosestPlatformInRange();
			isMoving = false;
		}

		HandleFallOnDifferentPlatformTypes(platform.PlatformType);
		
		SnapToClosestPlatformInRange();
		isMoving = false;
	}

	private void HandleFallOnDifferentPlatformTypes(PlatformType platformType)
	{
		if (platformType == PlatformType.SLIME)
		{
			isStuckInSlime = true;
		}
		if (platformType == PlatformType.SLIDE_LEFT)
		{
			Move(PlayerAction.MOVE_LEFT);
		}

		if (platformType == PlatformType.SLIDE_RIGHT)
		{
			Move(PlayerAction.MOVE_RIGHT);
		}
	}

	private IEnumerator PlayerFallToVoidCoroutine() 
	{
		float velocity = 0;
		float gravity = gameSettings.PlayerGravity;
		float worldBorderBottom = gameSettings.ScreenBorderBottom;

		// First animate player down then kill him
		while (transform.position.y > worldBorderBottom)
		{
			velocity += gravity;
			transform.position += Vector3.down * velocity * Time.deltaTime;
			yield return null;
		}

		PlayerDie();
	}

	private void MovePlayerDownOneFrame(Vector3 movement, float elapsedTime)
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
	private void SnapToClosestPlatformInRange()
	{
		Platform platform = WorldManager.Instance.GetPlatformWithinRange(transform.position, platformSnapRange);
		SnapToPlatform(platform);
	}

	private void SnapToClosestSafePlatform()
	{
		Platform platform = WorldManager.Instance.GetSafePlatformClosestToPos(transform.position);
		SnapToPlatform(platform);
	}

	// If the given platform exists, snap to it (with some offset)
	private void SnapToPlatform(Platform platform)
	{
		Vector3 newPosition = platform.transform.position + platformOffset;
		transform.position = newPosition;

		CheckForCollectible();

		if (platform.PlatformType == PlatformType.SPIKES)
		{
			PlayerDie();
		}
	}

	private bool ActionIsMove(PlayerAction action)
	{
		return gameSettings.MovePlayerActions.Contains(action);
	}

	// Destroy player, play lose sound and stop time
	private void PlayerDie()
	{
		Debug.LogWarning("Player died...");
		AudioManager.Instance.PlaySound("Lose");
		playerDied = true;
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
		if (playerDied)
		{
			return;
		}
		PlayerAction action = gameSettings.SwipeDirectionToPlayerAction[swipeDirection];
		actions.Push(action);
	}

	// Check if OUR platform was destroyed, and if it was its game over
	private void OnPlatformDestroy(float platformYPosition)
	{
		if (playerDied)
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
