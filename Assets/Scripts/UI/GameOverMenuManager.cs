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

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Application.Quit();
		}
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
				int myID = player.GetComponent<PhotonView>().ViewID;
				int loserID = PhotonRoom.Instance.LoserID;
				if (myID != loserID)
				{
					return true;
				}
			}
		}

		return false;
	}

	public void MainMenu()
	{
		AudioManager.Instance.StopAllSounds();
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
		if (AudioManager.Instance != null)
		{
			Destroy(AudioManager.Instance.gameObject);
		}
		sceneTransitionAnimator.SetTrigger(animatorTriggerString);

		yield return new WaitForSeconds(transitionAnimationDuration);

		SceneManager.LoadScene(0);
	}
}
