using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
	private const string mainMenuMusicName = "Main Menu Music";
	private const string animatorTriggerString = "FadeOut";

	[SerializeField]
	private Animator sceneTransitionAnimator;
	[SerializeField]
	private float transitionAnimationDuration;

	private void Start()
	{
		StartCoroutine(AudioManager.Instance.FadeIn(mainMenuMusicName, transitionAnimationDuration));
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

	IEnumerator LoadNextScene()
	{
		sceneTransitionAnimator.SetTrigger(animatorTriggerString);
		StartCoroutine(AudioManager.Instance.FadeOut(mainMenuMusicName, transitionAnimationDuration));

		yield return new WaitForSeconds(transitionAnimationDuration);

		int currentSceneIndex = SceneManager.GetActiveScene().buildIndex; 
		SceneManager.LoadScene(currentSceneIndex + 1);
	}
}
