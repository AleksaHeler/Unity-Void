using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// This is the main player script
[RequireComponent(typeof(PlayerInput))]
partial class CharacterController : MonoBehaviour
{

	private PhotonView photonView;
	private GameSettings gameSettings;
	private PlayerActionQueue actions;
	private PlayerState playerState;

	private Vector3 movePoint;
	private Vector3 platformOffset;
	private float snapDistance;
	private float playerSpeed;

	private float lastFallDistance;
	private PlayerAction lastPlayerAction;
	private GameObject currentPlatform;
	private PlatformHandler platformHandler;

	// TODO: ADd PLAYER INVENTORY

	void Start()
	{
		DontDestroyOnLoad(this.gameObject);

		photonView = GetComponent<PhotonView>();
		actions = new PlayerActionQueue();
		platformHandler = new PlatformHandler();
		gameSettings = SettingsReader.Instance.GameSettings;

		lastFallDistance = 0;
		lastPlayerAction = PlayerAction.NONE;
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
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Application.Quit();
		}

		if (!photonView.IsMine)
		{
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
				AudioManager.Instance.PlayPlatformSound(currentPlatform.GetComponent<PlatformSetup>().platformType);
			}
		}

		// Handle different platforms
		if (PlayerIsNotMoving())
		{
			GameObject currentPlatform = PhotonWorld.Instance.GetPlatformPositionWithinRange(transform.position, snapDistance);

			// Fall into the void
			if (currentPlatform == null)
			{
				actions.PushFront(PlayerAction.MOVE_DOWN);
				HandleMoveActions();
				return;
			}

			platformHandler.InvokeAction(currentPlatform.GetComponent<PlatformSetup>().platformType, this);

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
			float totalMoveDistance = gameSettings.PlatformSpacingX;
			float animationPercent = Mathf.Abs(leftoverMoveDistance / totalMoveDistance);
			float sine = Mathf.Sin(animationPercent * Mathf.PI) * gameSettings.PlayerJumpAnimationHeight;
			sineOffset = Vector3.up * sine * Time.deltaTime;
		}

		transform.position = Vector3.MoveTowards(transform.position, movePoint, playerSpeed * Time.deltaTime) + sineOffset;
	}

	private GameObject SnapToClosestPlatformInRange()
	{
		GameObject platform = PhotonWorld.Instance.GetPlatformPositionWithinRange(movePoint, snapDistance);

		if (platform == null)
		{
			return null;
		}

		movePoint = platform.transform.position + platformOffset;
		currentPlatform = platform;
		//CheckForCollectible();
		return platform;
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

	private void PlayerDie()
	{
		if (playerState == PlayerState.DIED)
		{
			return;
		}
		playerState = PlayerState.DIED;
		Debug.Log("Player is dead now!");
		GetComponent<AvatarSetup>().myCharacter.GetComponent<SpriteRenderer>().sprite = null;
		PhotonRoom.Room.photonView.RPC("RPC_GameOver", RpcTarget.All, photonView.Controller.ActorNumber);
		//PhotonRoom.Room.GameOver(photonView.Controller.ActorNumber);
	}

	private void OnSwipe(SwipeDirection swipeDirection)
	{
		PlayerAction action = gameSettings.SwipeDirectionToPlayerAction[swipeDirection];
		actions.Push(action);
	}

	private void OnSceneFinishedLoading(Scene scene, LoadSceneMode mode)
	{
		if(scene.buildIndex == 2)
		{
			playerState = PlayerState.DIED;
			GetComponent<AvatarSetup>().myCharacter.GetComponent<SpriteRenderer>().sprite = null;
		}
	}
}
