using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Here we define functions that will be called when player steps on certain platforms
// Parameter for all functions is PlayerController script
public class PlatformHandler
{
	private Dictionary<PlatformType, Action<PlayerController>> platformActions;

	public void InvokeAction(PlatformType name, PlayerController parameter)
	{
		platformActions[name](parameter);
	}

	public PlatformHandler()
	{
		platformActions = new Dictionary<PlatformType, Action<PlayerController>>();

		platformActions.Add(PlatformType.NONE, PlatformCallbackNone);
		platformActions.Add(PlatformType.NORMAL, PlatformCallbackNormal);
		platformActions.Add(PlatformType.SLIDE_LEFT, PlatformCallbackSlideLeft);
		platformActions.Add(PlatformType.SLIDE_RIGHT, PlatformCallbackSlideRight);
		platformActions.Add(PlatformType.SPIKES, PlatformCallbackSpikes);
		platformActions.Add(PlatformType.SLIME, PlatformCallbackSlime);
		platformActions.Add(PlatformType.GRASS, PlatformCallbackGrass);
		platformActions.Add(PlatformType.GLASS, PlatformCallbackGlass);
	}

	private void PlatformCallbackNone(PlayerController playerController)
	{
		playerController.PushFrontToActionQueue(PlayerAction.MOVE_DOWN);
	}

	private void PlatformCallbackNormal(PlayerController playerController)
	{
		if (PlayerFellToDeath(playerController))
		{
			playerController.PlayerDie();
		}
	}

	private void PlatformCallbackSpikes(PlayerController playerController)
	{
		playerController.PlayerDie();
	}

	private void PlatformCallbackSlideLeft(PlayerController playerController)
	{
		if (PlayerFellToDeath(playerController))
		{
			playerController.PlayerDie();
		}
		playerController.PushFrontToActionQueue(PlayerAction.MOVE_LEFT);

	}

	private void PlatformCallbackSlideRight(PlayerController playerController)
	{
		if (PlayerFellToDeath(playerController))
		{
			playerController.PlayerDie();
		}
		playerController.PushFrontToActionQueue(PlayerAction.MOVE_RIGHT);
	}

	private static PlayerAction getOutOfSlimeMove = PlayerAction.NONE;
	private static int getOutOfSlimeMoveCount = 0;
	private void PlatformCallbackSlime(PlayerController playerController)
	{
		if (PlayerFellToDeath(playerController))
		{
			playerController.PlayerDie();
		}

		if(playerController.PlayerState != PlayerState.STUCK_IN_SLIME)
		{
			playerController.GetStuckInSlime();
			return;
		}

		if(playerController.LastPlayerAction != PlayerAction.NONE)
		{ 
			if (getOutOfSlimeMove == PlayerAction.NONE)
			{
				getOutOfSlimeMove = playerController.LastPlayerAction;
				getOutOfSlimeMoveCount = 1;
			}
			else
			{
				if(getOutOfSlimeMove == playerController.LastPlayerAction)
				{
					getOutOfSlimeMoveCount++;
				}
				else
				{
					getOutOfSlimeMove = playerController.LastPlayerAction;
					getOutOfSlimeMoveCount = 1;
				}

				if(getOutOfSlimeMoveCount >= 3)
				{
					getOutOfSlimeMoveCount = 0;
					getOutOfSlimeMove = PlayerAction.NONE;
					playerController.GetUnstuckFromSlime();
					return;
				}
			}
		}

		playerController.LastPlayerAction = PlayerAction.NONE;
	}

	private void PlatformCallbackGlass(PlayerController playerController)
	{
		// If players last move is down -> break glass
		if (playerController.LastPlayerAction == PlayerAction.MOVE_DOWN)
		{
			playerController.CurrentPlatform.BreakGlass();
		}
	}

	private void PlatformCallbackGrass(PlayerController playerController)
	{
		// Don't do nothing (not even fall damage)
	}

	// Check if player has fell more than he can survive
	private bool PlayerFellToDeath(PlayerController playerController)
	{
		float platformSpacingY = SettingsReader.Instance.GameSettings.PlatformSpacingY;
		float playerFallDistance = playerController.LastFallDistance;
		// Has player fallen more than one platform spacing
		return playerFallDistance > platformSpacingY * 1.5f;
	}
}
