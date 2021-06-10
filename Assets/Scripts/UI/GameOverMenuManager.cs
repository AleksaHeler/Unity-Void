using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverMenuManager : MonoBehaviour
{
	[SerializeField]
	private Animator sceneTransitionAnimator;
	[SerializeField]
	private float transitionAnimationDuration;

	public void MainMenu()
	{
		StartCoroutine(LoadMainMenuScene());
	}

	public void Quit()
	{
		Application.Quit();
	}

	IEnumerator LoadMainMenuScene()
	{
		sceneTransitionAnimator.SetTrigger("FadeOut");
		yield return new WaitForSeconds(transitionAnimationDuration);

		AudioManager.Instance.StopSound("Game Music");
		AudioManager.Instance.PlaySound("Main Menu Music");

		SceneManager.LoadScene(0);
	}
}
