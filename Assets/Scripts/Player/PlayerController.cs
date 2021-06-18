using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum PlayerState { NOT_MOVING, MOVING, STUCK_IN_SLIME, DIED, NOT_LOADED }

// This is the main player script
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerInventory))]
partial class PlayerController : MonoBehaviour
{

	private PhotonView photonView;
	private GameSettings gameSettings;
	private PlayerActionQueue actions;
	private PlayerInventory playerInventory;
	private PlayerState playerState;

	private Vector3 movePoint;
	private Vector3 platformOffset;
	private float snapDistance;
	private float playerSpeed;

	private float lastFallDistance;
	private PlayerAction lastPlayerAction;
	private GameObject currentPlatform;
	private PlatformHandler platformHandler;

	void Start()
	{
		DontDestroyOnLoad(this.gameObject);

		photonView = GetComponent<PhotonView>();
		playerInventory = GetComponent<PlayerInventory>();
		actions = new PlayerActionQueue();
		platformHandler = new PlatformHandler();
		gameSettings = SettingsReader.Instance.GameSettings;

		lastFallDistance = 0;
		lastPlayerAction = PlayerAction.NONE;
		playerState = PlayerState.NOT_LOADED;
		snapDistance = gameSettings.PlayerToPlatformSnapRange;
		playerSpeed = gameSettings.PlayerSpeed;
		platformOffset = gameSettings.PlayerToPlatformOffset;

		if (!photonView.IsMine)
		{
			return;
		}

		PlayerInput.OnSwipe += OnSwipe;
		movePoint = transform.position;
		SnapToClosestPlatformInRange();

		SceneManager.sceneLoaded += OnSceneFinishedLoading;
	}
	private void OnDestroy()
	{
		// Unsubscribe from events
		PlayerInput.OnSwipe -= OnSwipe;
	}

	void LateUpdate()
	{
		if (!photonView.IsMine)
		{
			return;
		}

		if(playerState == PlayerState.NOT_LOADED)
		{
			SnapToClosestPlatformInRange();
			return;
		}

		if (playerState == PlayerState.DIED)
		{
			return;
		}

		if (IsBelowScreenBorder())
		{
			PlayerDie();
		}

		// Transition from MOVING state to NOT_MOVING
		if (playerState == PlayerState.MOVING && IsCloseToMovePoint())
		{
			playerState = PlayerState.NOT_MOVING;
			if (currentPlatform != null)
			{
				AudioManager.Instance.PlayPlatformSound(currentPlatform.GetComponent<PlatformController>().PlatformType);
			}
		}

		// Handle different platforms
		if (PlayerIsNotMoving())
		{
			GameObject currentPlatform = PhotonWorld.Instance.GetPlatformWithinRange(transform.position, snapDistance);

			// Fall into the void
			if (currentPlatform == null)
			{
				actions.PushFront(PlayerAction.MOVE_DOWN);
				HandleMoveActions();
				return;
			}

			platformHandler.InvokeAction(currentPlatform.GetComponent<PlatformController>().PlatformType, this);

			// Calls function Move() when there is input
			HandleMoveActions();
		}

		HandlePhysics();
		SnapToClosestPlatformInRange();
	}

	private void HandleMoveActions()
	{
		if (actions.ActionCount == 0)
		{
			return;
		}

		PlayerAction action = actions.Pop();

		if (action != PlayerAction.NONE)
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
		GameObject snappedPlatform = SnapToClosestPlatformInRange();

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

	private void HandlePhysics()
	{
		Vector3 playerToDestinationVector = movePoint - transform.position;

		// If move is on X axis (left/right)
		Vector3 sineOffset = Vector3.zero;
		if (playerToDestinationVector.x != 0)
		{
			float leftoverMoveDistance = playerToDestinationVector.x;
			float totalMoveDistance = gameSettings.PlatformSpacingHorizontal;
			float animationPercent = Mathf.Abs(leftoverMoveDistance / totalMoveDistance);
			float sine = Mathf.Sin(animationPercent * Mathf.PI) * gameSettings.PlayerJumpAnimationHeight;
			sineOffset = Vector3.up * sine * Time.deltaTime;
		}

		transform.position = Vector3.MoveTowards(transform.position, movePoint, playerSpeed * Time.deltaTime) + sineOffset;
	}

	private GameObject SnapToClosestPlatformInRange()
	{
		GameObject platform = PhotonWorld.Instance.GetPlatformWithinRange(movePoint, snapDistance);

		if (platform == null)
		{
			return null;
		}

		if(playerState == PlayerState.NOT_LOADED)
		{
			playerState = PlayerState.NOT_MOVING;
		}

		movePoint = platform.transform.position + platformOffset;
		currentPlatform = platform;
		CheckForCollectible();
		return platform;
	}

	private void CheckForCollectible()
	{
		GameObject itemCheckPlatform = PhotonWorld.Instance.GetPlatformWithinRange(transform.position, snapDistance);
		if(itemCheckPlatform == null)
		{
			return;
		}
		ItemType item = PhotonWorld.Instance.GetItemTypeAtPlatform(itemCheckPlatform);
		if (item != ItemType.NONE)
		{
			playerInventory.CollectItem(itemCheckPlatform);
		}
	}

	private void PushToFrontOfActionQueue(PlayerAction action)
	{
		actions.PushFront(action);
	}

	private void GetStuckInSlime()
	{
		lastPlayerAction = PlayerAction.NONE;
		playerState = PlayerState.STUCK_IN_SLIME;
	}
	public void GetUnstuckFromSlime()
	{
		playerState = PlayerState.NOT_MOVING;
		actions.PushFront(lastPlayerAction);
	}
	private bool PlayerIsNotMoving()
	{
		return playerState == PlayerState.NOT_MOVING || playerState == PlayerState.STUCK_IN_SLIME;
	}
	private bool IsCloseToMovePoint()
	{
		float playerEndMovingCheckDistance = gameSettings.PlayerEndMovingCheckDistance;
		float distance = Vector3.Distance(transform.position, movePoint);
		return distance < playerEndMovingCheckDistance;
	}

	private bool IsBelowScreenBorder()
	{
		float playerCheckTolerance = gameSettings.PlayerEndMovingCheckDistance;
		float checkPositionY = gameSettings.ScreenBorderBottom + playerCheckTolerance;
		return transform.position.y < checkPositionY;
	}

	public void PlayerDie()
	{
		if (playerState == PlayerState.DIED)
		{
			return;
		}

		playerState = PlayerState.DIED;
		photonView.RPC("RPC_SpawnPlayerDeathParticles", RpcTarget.All, transform.position);
		photonView.RPC("RPC_DieAndDisableSprite", RpcTarget.All);
		PhotonRoom.Instance.photonView.RPC("RPC_GameOver", RpcTarget.All, photonView.ViewID);
	}

	private void OnSwipe(SwipeDirection swipeDirection)
	{
		PlayerAction action = gameSettings.SwipeDirectionToPlayerAction[swipeDirection];
		actions.Push(action);
	}

	private void OnSceneFinishedLoading(Scene scene, LoadSceneMode mode)
	{
		if (scene.buildIndex == 2)
		{
			photonView.RPC("RPC_DieAndDisableSprite", RpcTarget.All);
		}
	}

	[PunRPC]
	void RPC_DieAndDisableSprite()
	{
		playerState = PlayerState.DIED;
		GetComponent<AvatarSetup>().MyCharacter.GetComponent<SpriteRenderer>().sprite = null;
	}

	[PunRPC]
	void RPC_SpawnPlayerDeathParticles(Vector3 position)
	{
		Instantiate(gameSettings.PlayerDeathParticles, position, Quaternion.identity);
	}
	
}
