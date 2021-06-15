using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum PlayerState { NOT_MOVING, MOVING, STUCK_IN_SLIME, DIED }

[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{

    private PlayerActionQueue actions;
    private GameSettings gameSettings;
    private PhotonView photonView;

    private PlayerState playerState;
    private Vector3 targetPoint;


    void Start()
    {
        actions = new PlayerActionQueue();
        gameSettings = SettingsReader.Instance.GameSettings;
        photonView = GetComponent<PhotonView>();

        playerState = PlayerState.NOT_MOVING;

        PlayerInput.OnSwipe += OnSwipe;
    }


    void Update()
    {
        if (photonView.IsMine)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }

            if (playerState == PlayerState.MOVING && IsCloseToMovePoint())
            {
                playerState = PlayerState.NOT_MOVING;
            }

            if (actions.ActionCount > 0)
            {
                PlayerAction action = actions.Pop();

                if (ActionIsMove(action) && PlayerIsNotMoving())
                {
                    Move(action);
                }
                else
                {
                    actions.PushFront(action);
                }
            }

            HandlePhysics();
            SnapTargetToPlatform();
        }
    }

    private void Move(PlayerAction action)
    {
        Vector3 movement = gameSettings.PlayerActionToVector3(action);
        targetPoint += movement;

        playerState = PlayerState.MOVING;
    }

    private void HandlePhysics()
    {
        Vector3 playerToDestinationVector = targetPoint - transform.position;

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

        transform.position = Vector3.MoveTowards(transform.position, targetPoint, gameSettings.PlayerSpeed * Time.deltaTime) + sineOffset;
    }

    private void SnapTargetToPlatform()
	{
        try
        {
            Platform platform = World.Instance.GetClosestPlatformInRange(targetPoint, gameSettings.PlayerToPlatformSnapRange);
            
            if (platform == null)
            {
                return;
            }

            targetPoint = platform.transform.position + gameSettings.PlayerToPlatformOffset;
        }
		catch
        {
            return;
        }
    }

    private bool PlayerIsNotMoving()
    {
        return playerState == PlayerState.NOT_MOVING || playerState == PlayerState.STUCK_IN_SLIME;
    }
    private bool IsCloseToMovePoint()
    {
        float playerCheckTolerance = gameSettings.PlayerCheckTolerance;
        float distance = Vector3.Distance(transform.position, targetPoint);
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

    private void OnSwipe(SwipeDirection swipeDirection)
    {
		if (photonView.IsMine)
        {
            PlayerAction action = gameSettings.SwipeDirectionToPlayerAction[swipeDirection];
            actions.Push(action);
        }
    }
}
