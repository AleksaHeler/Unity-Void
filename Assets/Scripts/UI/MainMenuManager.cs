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
		AudioManager.Instance.PlaySound("Main Menu Music");
	}

	public void Play()
	{
		StartCoroutine(LoadNextScene());
	}

	public void Quit()
	{
		Application.Quit();
	}

	IEnumerator LoadNextScene()
	{
		sceneTransitionAnimator.SetTrigger("FadeOut");
		yield return new WaitForSeconds(transitionAnimationDuration);

		AudioManager.Instance.StopSound("Main Menu Music");
		AudioManager.Instance.PlaySound("Game Music");

		int currentSceneIndex = SceneManager.GetActiveScene().buildIndex; 
		SceneManager.LoadScene(currentSceneIndex + 1);
	}
}
