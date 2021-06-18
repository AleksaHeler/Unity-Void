using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
	private const string mainMenuMusicName = "Main Menu Music";
	private const float transitionAnimationDuration = 0.4f;

	private void Start()
	{
		StartCoroutine(AudioManager.Instance.FadeIn(mainMenuMusicName, transitionAnimationDuration));
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			QuitButtonClick();
		}
	}

	public void QuitButtonClick()
	{
		Application.Quit();
	}

	public void OnCharacterSelectButtonClick(int characterType)
	{
		if(PlayerSettings.Instance != null)
		{
			PlayerSettings.Instance.SetSelectedCharacter((CharacterType)characterType);
		}
	}
}
