using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InGameUIManager : MonoBehaviour
{
    private const string playerPrefsCoinsKey = "PlayerCoins";
    private const string gameMusicName = "Game Music";
    private const float transitionAnimationDuration = 0.4f;

    [SerializeField]
    private Animator sceneTransitionAnimator;

    [SerializeField]
    private TMPro.TextMeshProUGUI coinsText;

    [SerializeField]
    private GameObject pauseButton;
    [SerializeField]
    private GameObject playButton;
    [SerializeField]
    private GameObject bombButton;

    private int coins;

    private void Start()
    {
        StartCoroutine(AudioManager.Instance.FadeIn(gameMusicName, transitionAnimationDuration));
    }

   
	private void Update()
	{
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            DisconnectPlayer();
        }

		if (PlayerPrefs.HasKey(playerPrefsCoinsKey))
		{
            int coins = PlayerPrefs.GetInt(playerPrefsCoinsKey);
            coinsText.text = coins.ToString();
		}
		else
        {
            coinsText.text = "0";
        }
    }

    public void DisconnectPlayer()
    {
        StartCoroutine(DisconnectAndLoad());
	}

    IEnumerator DisconnectAndLoad()
	{
        PhotonNetwork.Disconnect();

        // Wait until we disconnect
		while (PhotonNetwork.IsConnected)
		{
            yield return null;
		}

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
        AudioManager.Instance.StopAllSounds();
        SceneManager.LoadScene(0);
	}
}
