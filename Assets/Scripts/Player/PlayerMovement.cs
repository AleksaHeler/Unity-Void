using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Subscribes(?) to PlayerInput scripts event system which triggers the event on detected swipe.
/// So this script then adds a movement action to actions queue on swipe
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    PlayerActionQueue actions;

	private void Awake()
	{
		PlayerInput.OnSwipe += OnSwipe;
        actions = new PlayerActionQueue();
    }

	private void Update()
	{
        // Check if there is an action to be done
        if(actions.Count > 0)
		{
            PlayerAction action = actions.Pop();

			switch (action)
            {
                case PlayerAction.MOVE_UP:
                    transform.position += Vector3.up;
                    break;
                case PlayerAction.MOVE_DOWN:
                    transform.position += Vector3.down;
                    break;
                case PlayerAction.MOVE_LEFT:
                    transform.position += Vector3.left;
                    break;
                case PlayerAction.MOVE_RIGHT:
                    transform.position += Vector3.right;
                    break;
            }
		}
    }

	// Convert swipe direction to player action and add it to actions queue
	private void OnSwipe(SwipeData swipeData)
    {
        //Debug.Log("Swipe detected in direction: " + swipeData.Direction);
        PlayerAction action = SwipeDirectionToPlayerAction(swipeData.Direction);

        if(action != PlayerAction.NONE)
        {
            actions.Push(action);
        }
    }

    private PlayerAction SwipeDirectionToPlayerAction(SwipeDirection direction)
	{
        switch (direction)
        {
            case SwipeDirection.UP:
                return PlayerAction.MOVE_UP;
            case SwipeDirection.DOWN:
                return PlayerAction.MOVE_DOWN;
            case SwipeDirection.LEFT:
                return PlayerAction.MOVE_LEFT;
            case SwipeDirection.RIGHT:
                return PlayerAction.MOVE_RIGHT;
            default:
                return PlayerAction.NONE;
        }
    }
}
