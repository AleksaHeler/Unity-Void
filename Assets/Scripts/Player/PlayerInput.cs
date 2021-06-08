using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct SwipeData
{
	public Vector2 StartPosition;
	public Vector2 EndPosition;
	public SwipeDirection Direction;
}

public enum SwipeDirection
{
	UP,
	DOWN,
	LEFT,
	RIGHT
}

/// <summary>
/// Detects swipes and triggers an event on each swipe
/// Event passes SwipeData as parameter to delegate functions
/// SwipeData contains direction, start and end position
/// </summary>
public class PlayerInput : MonoBehaviour
{
	[Tooltip("Minimum distance in pixels needed to detect swipe")]
	public float minDistanceForSwipe = 20f;

	private Vector2 previousMousePosition;
	private Vector2 mouseDownPosition;
	private Vector2 mouseUpPosition;
	
	// After a swipe is registered fire an event
	public static event Action<SwipeData> OnSwipe = delegate { };

	public void Update()
	{
		if(Input.GetMouseButtonDown(0))
		{
			mouseUpPosition = Input.mousePosition;
			mouseDownPosition = Input.mousePosition;
		}

		if(Input.GetMouseButtonUp(0))
		{
			mouseDownPosition = Input.mousePosition;
			DetectSwipe();
		}
	}

	private void DetectSwipe()
	{
		if (SwipeDistanceCheckMet())
		{
			if (IsVerticalSwipe())
			{
				var direction = mouseDownPosition.y - mouseUpPosition.y > 0 ? SwipeDirection.UP : SwipeDirection.DOWN;
				SendSwipe(direction);
			}
			else
			{
				var direction = mouseDownPosition.x - mouseUpPosition.x > 0 ? SwipeDirection.RIGHT : SwipeDirection.LEFT;
				SendSwipe(direction);
			}
			mouseUpPosition = mouseDownPosition;
		}
	}

	#region Helper functions

	private bool MouseMoved()
	{
		Vector2 currentMousePosition = Input.mousePosition;
		if (currentMousePosition != previousMousePosition)
		{
			previousMousePosition = currentMousePosition;
			return true;
		}

		return false;
	}

	private bool IsVerticalSwipe()
	{
		return VerticalMovementDistance() > HorizontalMoveDistance();
	}

	private bool SwipeDistanceCheckMet()
	{
		return VerticalMovementDistance() > minDistanceForSwipe || HorizontalMoveDistance() > minDistanceForSwipe;
	}

	private float VerticalMovementDistance()
	{
		return Mathf.Abs(mouseDownPosition.y - mouseUpPosition.y);
	}

	private float HorizontalMoveDistance()
	{
		return Mathf.Abs(mouseDownPosition.x - mouseUpPosition.x);
	}

	private void SendSwipe(SwipeDirection direction)
	{
		SwipeData swipeData = new SwipeData()
		{
			Direction = direction,
			StartPosition = mouseDownPosition,
			EndPosition = mouseUpPosition
		};

		OnSwipe(swipeData);
	}

	#endregion

	/// <summary>
	/// Converts swipe direction to player movement action (for example SwipeDirection.UP -> PlayerAction.MOVE_UP)
	/// </summary>
	public static PlayerAction SwipeDirectionToPlayerAction(SwipeDirection direction)
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

	// Returns true if passed action is a move
	public static bool ActionIsMove(PlayerAction action)
	{
		switch (action)
		{
			case PlayerAction.MOVE_UP:
			case PlayerAction.MOVE_DOWN:
			case PlayerAction.MOVE_LEFT:
			case PlayerAction.MOVE_RIGHT:
				return true;
			default:
				return false;
		}
	}

	public static Vector3 MoveActionToVector3(PlayerAction action)
	{
		switch (action)
		{
			case PlayerAction.MOVE_UP:
				return new Vector3(0, 1, 0);
			case PlayerAction.MOVE_DOWN:
				return new Vector3(0, -1, 0);
			case PlayerAction.MOVE_LEFT:
				return new Vector3(-1, 0, 0);
			case PlayerAction.MOVE_RIGHT:
				return new Vector3(1, 0, 0);
			default:
				return Vector3.zero;
		}
	}
}
