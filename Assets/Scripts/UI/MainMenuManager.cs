using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
	[SerializeField]
	private Animator sceneTransitionAnimator;
	[SerializeField]
	private float transitionAnimationDuration;

	private void Start()
	{
		StartCoroutine(AudioManager.Instance.FadeIn("Main Menu Music", transitionAnimationDuration));
	}

	public void Play()
	{
		StartCoroutine(LoadNextScene());
	}

	public void Quit()
	{
		Application.Quit();
	}

	public void OnCharacterSelect(int characterType)
	{
		if(PlayerSettings.Instance != null)
		{
			PlayerSettings.Instance.MySelectedCharacter = (CharacterType)characterType;
			PlayerPrefs.SetInt("MyCharacter", characterType);
		}
	}

	IEnumerator LoadNextScene()
	{
		sceneTransitionAnimator.SetTrigger("FadeOut");
		StartCoroutine(AudioManager.Instance.FadeOut("Main Menu Music", transitionAnimationDuration));

		yield return new WaitForSeconds(transitionAnimationDuration);
		int currentSceneIndex = SceneManager.GetActiveScene().buildIndex; 
		SceneManager.LoadScene(currentSceneIndex + 1);
	}
}
