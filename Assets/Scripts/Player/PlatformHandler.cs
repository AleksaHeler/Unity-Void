using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformHandler
{
	private Dictionary<PlatformType, Action<float>> platformActions;
	PlayerController playerController;

	public void InvokeAction(PlatformType name, float parameter)
	{
		platformActions[name](parameter);
	}

	public PlatformHandler()
	{
		platformActions = new Dictionary<PlatformType, Action<float>>();
		playerController = GameObject.FindObjectOfType<PlayerController>();
		AddFunctions();
	}

	private void AddFunctions()
	{

		platformActions.Add(PlatformType.NONE, (parameter) =>
		{
			playerController.PushFrontToActionQueue(PlayerAction.MOVE_DOWN);
		});

		platformActions.Add(PlatformType.NORMAL, (parameter) =>
		{

		});

		platformActions.Add(PlatformType.SLIDE_LEFT, (parameter) =>
		{
			playerController.PushFrontToActionQueue(PlayerAction.MOVE_LEFT);
		});

		platformActions.Add(PlatformType.SLIDE_RIGHT, (parameter) =>
		{
			playerController.PushFrontToActionQueue(PlayerAction.MOVE_RIGHT);
		});

		platformActions.Add(PlatformType.SPIKES, (parameter) =>
		{
			playerController.PlayerDie();
		});

		platformActions.Add(PlatformType.SLIME, (parameter) =>
		{
			// TODO
		});

		platformActions.Add(PlatformType.GRASS, (parameter) =>
		{
			// TODO
		});

		platformActions.Add(PlatformType.GLASS, (parameter) =>
		{
			// TODO
		});
	}
}
