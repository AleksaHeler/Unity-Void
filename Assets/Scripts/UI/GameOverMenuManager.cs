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

	[SerializeField]
	private GameObject mainMenuButton;
	[SerializeField]
	private GameObject quitButton;

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

		StartCoroutine(Disconnect());
	}

	IEnumerator Disconnect()
	{
		mainMenuButton.SetActive(false);
		quitButton.SetActive(false);
		PhotonNetwork.Disconnect();

		// Wait until we disconnect
		while (PhotonNetwork.IsConnected)
		{
			yield return null;
		}

		mainMenuButton.SetActive(true);
		quitButton.SetActive(true);
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
		if (PhotonLobby.Instance != null)
		{
			Destroy(PhotonLobby.Instance.gameObject);
		}
		if (PhotonRoom.Instance != null)
		{
			Destroy(PhotonRoom.Instance.gameObject);
		}
		sceneTransitionAnimator.SetTrigger(animatorTriggerString);
		StartCoroutine(AudioManager.Instance.FadeOut(mainMenuMusicName, transitionAnimationDuration));

		yield return new WaitForSeconds(transitionAnimationDuration);

		AudioManager.Instance.StopAllSounds();
		SceneManager.LoadScene(0);
	}
}
