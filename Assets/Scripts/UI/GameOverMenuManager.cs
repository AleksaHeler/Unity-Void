using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
 If the life doesn't break you today, don't worry. It will try again tomorrow.
 Everybody sucks at something.
 Those who doubt your ability probably have a valid reason.
 If you hate yourself remember you are not alone. A lot of people hate you too.
 It's never too late to fail.
 Trying is the first step toward failure.
 Not everything is a lesson. Sometimes you just fail.
 It could be that your purpose in life is to serve as a warning to others.
 The light at the end of the tunnel has been turned off due to budget cuts.
 Hope is the first step on the road to disappointment.
 

 */
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
		yield return new WaitForSeconds(transitionAnimationDuration);

		AudioManager.Instance.StopSound("Game Music");
		AudioManager.Instance.PlaySound("Main Menu Music");

		SceneManager.LoadScene(0);
	}
}
