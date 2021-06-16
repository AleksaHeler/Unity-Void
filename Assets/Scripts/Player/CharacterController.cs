using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This is the main player script
[RequireComponent(typeof(PlayerInput))]
public class CharacterController : MonoBehaviour
{

	private PhotonView photonView;
	private GameSettings gameSettings;
	private PlayerActionQueue actions;

	private Vector3 movePoint;
	private Vector3 platformOffset;
	private float snapDistance;
	private float playerSpeed;

	void Start()
	{
		photonView = GetComponent<PhotonView>();
		actions = new PlayerActionQueue();
		gameSettings = SettingsReader.Instance.GameSettings;

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

		HandleMoveActions();
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
		Vector3 movement = gameSettings.PlayerActionToVector3(action);
		movePoint += movement;

		GameObject snappedPlatform = SnapToClosestPlatformInRange();

		if (snappedPlatform == null)
		{
			//actions.PushFront(PlayerAction.MOVE_DOWN);
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
		Debug.Log("New position: " + movePoint);
		return platform;
	}	

	private void OnSwipe(SwipeDirection swipeDirection)
	{
		PlayerAction action = gameSettings.SwipeDirectionToPlayerAction[swipeDirection];
		actions.Push(action);
	}
}
