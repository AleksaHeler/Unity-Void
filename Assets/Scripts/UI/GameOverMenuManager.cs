using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverMenuManager : MonoBehaviour
{
	[SerializeField]
	private TMPro.TextMeshProUGUI titleText;
	[SerializeField]
	private TMPro.TextMeshProUGUI demotivationalText;
	[SerializeField]
	private Animator sceneTransitionAnimator;
	[SerializeField]
	private float transitionAnimationDuration;

	private bool win = false;

	private void Start()
	{
		List<string> demotivationalQuotes = SettingsReader.Instance.GameSettings.DemotivationalQuotes;
		int index = Random.Range(0, demotivationalQuotes.Count);
		demotivationalText.text = demotivationalQuotes[index];

		StartCoroutine(AudioManager.Instance.FadeIn("Main Menu Music", transitionAnimationDuration));

		GameObject[] playerGameObjects = GameObject.FindGameObjectsWithTag("Player");

		win = false;
		foreach (GameObject player in playerGameObjects)
		{
			if (player.GetComponent<PhotonView>().IsMine)
			{
				if (player.GetComponent<PhotonView>().Controller.ActorNumber == PhotonRoom.Instance.WinningPlayer)
				{
					win = true;
				}
			}
		}

		if (win)
		{
			titleText.text = "YOU WIN";
			demotivationalText.gameObject.SetActive(false);
		}
		else
		{
			titleText.text = "YOU LOSE";
		}
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
