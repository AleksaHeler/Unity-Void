using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Detects swipes and triggers an event on each swipe
// Event passes SwipeData as parameter to delegate functions
public class PlayerInput : MonoBehaviour
{
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

	// Check if swipe was ok
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
		int minDistanceForSwipe = SettingsReader.Instance.GameSettings.MinDistanceToSwipe;
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

	// Returns true if passed action is a move
	public static bool ActionIsMove(PlayerAction action)
	{
		return SettingsReader.Instance.GameSettings.MovePlayerActions.Contains(action);
	}

	#endregion

}
