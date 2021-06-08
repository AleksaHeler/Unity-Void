using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Custom data structure containing PlayerActions
/// Available operations: Push, PushFront and Pop
/// </summary>
public class PlayerActionQueue
{
	private List<PlayerAction> actions;

	public int Count { get => actions.Count; }

	// Constructor
	public PlayerActionQueue()
	{
		actions = new List<PlayerAction>();
	}

	/// <summary>
	/// Adds given PlayerAction to back of the queue
	/// </summary>
	public void Push(PlayerAction action)
	{
		actions.Add(action);
	}

	/// <summary>
	/// Adds given PlayerAction to front of the queue, so it will be executed as soon as possible
	/// </summary>
	public void PushFront(PlayerAction action)
	{
		actions.Insert(0, action);
	}

	/// <summary>
	/// Get next action in line to be executed. Warning: removes the returned action from the queue
	/// </summary>
	public PlayerAction Pop()
	{
		PlayerAction action = actions[0];
		actions.RemoveAt(0);
		return action;
	}
}
