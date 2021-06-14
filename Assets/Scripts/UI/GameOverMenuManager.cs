using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverMenuManager : MonoBehaviour
{
	[SerializeField]
	private TMPro.TextMeshProUGUI demotivationalText;
	[SerializeField]
	private Animator sceneTransitionAnimator;
	[SerializeField]
	private float transitionAnimationDuration;

	private void Start()
	{
		List<string> demotivationalQuotes = SettingsReader.Instance.GameSettings.DemotivationalQuotes;
		int index = Random.Range(0, demotivationalQuotes.Count);
		demotivationalText.text = demotivationalQuotes[index];

		StartCoroutine(AudioManager.Instance.FadeIn("Main Menu Music", transitionAnimationDuration));
	}

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
		StartCoroutine(AudioManager.Instance.FadeOut("Main Menu Music", transitionAnimationDuration));

		yield return new WaitForSeconds(transitionAnimationDuration);
		SceneManager.LoadScene(0);
	}
}
