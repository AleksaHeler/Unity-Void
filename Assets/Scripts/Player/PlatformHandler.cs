using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Here we define functions that will be called when player steps on certain platforms
// Parameter for all functions is PlayerController script
partial class CharacterController
{
	public class PlatformHandler
	{
		private Dictionary<PlatformType, Action<CharacterController>> platformActions;

		private PlayerAction getOutOfSlimeMove = PlayerAction.NONE;
		private int getOutOfSlimeMoveCount = 0;

		public void InvokeAction(PlatformType platformType, CharacterController playerController)
		{
			platformActions[platformType](playerController);
		}

		public PlatformHandler()
		{
			platformActions = new Dictionary<PlatformType, Action<CharacterController>>();

			platformActions.Add(PlatformType.NONE, PlatformCallbackNone);
			platformActions.Add(PlatformType.NORMAL, PlatformCallbackNormal);
			platformActions.Add(PlatformType.SLIDE_LEFT, PlatformCallbackSlideLeft);
			platformActions.Add(PlatformType.SLIDE_RIGHT, PlatformCallbackSlideRight);
			platformActions.Add(PlatformType.SPIKES, PlatformCallbackSpikes);
			platformActions.Add(PlatformType.SLIME, PlatformCallbackSlime);
			platformActions.Add(PlatformType.GRASS, PlatformCallbackGrass);
			platformActions.Add(PlatformType.GLASS, PlatformCallbackGlass);
		}

		private void PlatformCallbackNone(CharacterController characterController)
		{
			characterController.PushToFrontOfActionQueue(PlayerAction.MOVE_DOWN);
		}

		private void PlatformCallbackNormal(CharacterController characterController)
		{
			HandleIfPlayerFellToDeath(characterController);
		}

		private void PlatformCallbackSpikes(CharacterController characterController)
		{
			characterController.PlayerDie();
		}

		private void PlatformCallbackSlideLeft(CharacterController characterController)
		{
			HandleIfPlayerFellToDeath(characterController);
			characterController.PushToFrontOfActionQueue(PlayerAction.MOVE_LEFT);
		}

		private void PlatformCallbackSlideRight(CharacterController characterController)
		{
			HandleIfPlayerFellToDeath(characterController);
			characterController.PushToFrontOfActionQueue(PlayerAction.MOVE_RIGHT);
		}

		private void PlatformCallbackSlime(CharacterController characterController)
		{
			HandleIfPlayerFellToDeath(characterController);

			// First time getting stuck in slime
			if (characterController.playerState != PlayerState.STUCK_IN_SLIME)
			{
				characterController.GetStuckInSlime();
				return;
			}

			// Trying to get out of slime
			if (characterController.lastPlayerAction != PlayerAction.NONE)
			{
				AudioManager.Instance.PlayPlatformSound(characterController.currentPlatform.GetComponent<PlatformSetup>().platformType);

				// Setting initial get out of slime move
				if (getOutOfSlimeMove == PlayerAction.NONE)
				{
					getOutOfSlimeMove = characterController.lastPlayerAction;
					getOutOfSlimeMoveCount = 1;
				}
				else
				{
					HandleGettingOutOfSlime(characterController);
				}
			}

			characterController.lastPlayerAction = PlayerAction.NONE;
		}

		private void HandleGettingOutOfSlime(CharacterController playerController)
		{
			// Player has to make same move 3 times to escape
			if (getOutOfSlimeMove == playerController.lastPlayerAction)
			{
				getOutOfSlimeMoveCount++;
			}
			else
			{
				getOutOfSlimeMove = playerController.lastPlayerAction;
				getOutOfSlimeMoveCount = 1;
			}
			// If player made the right moves
			if (getOutOfSlimeMoveCount >= 3)
			{
				getOutOfSlimeMoveCount = 0;
				getOutOfSlimeMove = PlayerAction.NONE;
				playerController.GetUnstuckFromSlime();
			}
		}

		private void PlatformCallbackGlass(CharacterController playerController)
		{
			// If players last move is down -> break glass
			if (playerController.lastPlayerAction == PlayerAction.MOVE_DOWN)
			{
				playerController.currentPlatform.GetComponent<PhotonView>().RPC("RPC_BreakGlass", RpcTarget.All);
			}
		}

		private void PlatformCallbackGrass(CharacterController playerController)
		{
			// Don't do nothing (not even fall damage)
		}

		// Check if player has fell more than he can survive
		private void HandleIfPlayerFellToDeath(CharacterController playerController)
		{
			float platformSpacingY = SettingsReader.Instance.GameSettings.PlatformSpacingY;
			float playerFallDistance = playerController.lastFallDistance;

			// Has player fallen more than one platform spacing
			if (playerFallDistance > platformSpacingY * 1.5f)
			{
				playerController.PlayerDie();
			}

			playerController.lastFallDistance = 0;
		}
	}
}
