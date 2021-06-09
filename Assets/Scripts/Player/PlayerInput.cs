using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum SwipeDirection { UP, DOWN, LEFT, RIGHT }

/// <summary>
/// Detects swipes and triggers an event on each swipe
/// Event passes SwipeData as parameter to delegate functions
/// </summary>
public class PlayerInput : MonoBehaviour
{
	[Tooltip("Minimum distance (in pixels) needed to detect swipe")]
	public float minDistanceForSwipe = 20f;

	private Vector2 mouseDownPosition;
	private Vector2 mouseUpPosition;

	// After a swipe is registered fire an event
	public static event Action<SwipeDirection> OnSwipe = delegate { };

	public void Update()
	{
		// Begin/end swipes based on mouse clicks
		if (Input.GetMouseButtonDown(0))
		{
			mouseDownPosition = Input.mousePosition;
			mouseUpPosition = Input.mousePosition;
		} 

		if (Input.GetMouseButtonUp(0))
		{
			mouseDownPosition = Input.mousePosition;
			DetectSwipe();
		}
	}

	/// <summary>
	/// Check if swipe was ok
	/// </summary>
	private void DetectSwipe()
	{
		if (SwipeDistanceCheckMet())
		{
			if (IsVerticalSwipe())
			{
				var direction = mouseDownPosition.y - mouseUpPosition.y > 0 ? SwipeDirection.UP : SwipeDirection.DOWN;
				OnSwipe(direction);
			}
			else
			{
				var direction = mouseDownPosition.x - mouseUpPosition.x > 0 ? SwipeDirection.RIGHT : SwipeDirection.LEFT;
				OnSwipe(direction);
			}
			mouseUpPosition = mouseDownPosition;
		}
	}

	#region Helper functions

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

	/// <summary>
	/// Triggers swipe event with given swipe direction
	/// </summary>
	private void SendSwipe(SwipeDirection direction)
	{
	}

	#endregion

	/// <summary>
	/// Converts swipe direction to player movement action (for example SwipeDirection.UP -> PlayerAction.MOVE_UP)
	/// </summary>
	public static PlayerAction SwipeDirectionToPlayerAction(SwipeDirection direction)
	{
		// Dictionary: key-value pair za ovo umesto switch
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

	/// Returns true if passed action is a move
	public static bool ActionIsMove(PlayerAction action)
	{
		return SettingsReader.Instance.GameSettings.MovePlayerActions.Contains(action);
	}

	
}
