using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverMenuManager : MonoBehaviour
{
	private const string mainMenuMusicName = "Main Menu Music";
	private const string animatorTriggerString = "FadeOut";
	private const string winText= "YOU WIN";
	private const string loseText = "YOU LOSE";

	[SerializeField]
	private TMPro.TextMeshProUGUI titleText;
	[SerializeField]
	private TMPro.TextMeshProUGUI demotivationalText;
	[SerializeField]
	private Animator sceneTransitionAnimator;
	[SerializeField]
	private float transitionAnimationDuration;

	private void Start()
	{
		DisplayDemotivationalQuote();

		StartCoroutine(AudioManager.Instance.FadeIn(mainMenuMusicName, transitionAnimationDuration));

		if (IsLocalPlayerWinner())
		{
			titleText.text = winText;
			demotivationalText.gameObject.SetActive(false);
		}
		else
		{
			titleText.text = loseText;
		}
	}

	private void DisplayDemotivationalQuote()
	{
		List<string> demotivationalQuotes = SettingsReader.Instance.GameSettings.DemotivationalQuotes;
		int index = Random.Range(0, demotivationalQuotes.Count);
		demotivationalText.text = demotivationalQuotes[index];
	}

	private bool IsLocalPlayerWinner()
	{
		GameObject[] playerGameObjects = GameObject.FindGameObjectsWithTag("Player");

		foreach (GameObject player in playerGameObjects)
		{
			if (player.GetComponent<PhotonView>().IsMine)
			{
				bool iAmHost = PhotonNetwork.IsMasterClient;
				bool isHostWinner = PhotonRoom.Instance.IsHostWinner;
				if (iAmHost == isHostWinner)
				{
					return true;
				}
			}
		}

		return false;
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
		sceneTransitionAnimator.SetTrigger(animatorTriggerString);
		StartCoroutine(AudioManager.Instance.FadeOut(mainMenuMusicName, transitionAnimationDuration));

		yield return new WaitForSeconds(transitionAnimationDuration);

		SceneManager.LoadScene(0);
	}
}
