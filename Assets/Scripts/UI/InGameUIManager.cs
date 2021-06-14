using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InGameUIManager : MonoBehaviour
{
    [SerializeField]
    private Animator sceneTransitionAnimator;
    [SerializeField]
    private float transitionAnimationDuration;

    [SerializeField]
    private GameObject pauseButton;
    [SerializeField]
    private GameObject playButton;
    [SerializeField]
    private GameObject bombButton;
    [SerializeField]
    private PlayerInventory playerInventory;


    // Start is called before the first frame update
    void Start()
    {
        PlayerController.OnPlayerDeath += GameOver;
    }

    private void OnDestroy()
    {
        PlayerController.OnPlayerDeath -= GameOver;
    }

	private void Update()
	{
        Color color = bombButton.GetComponent<Image>().color;
        if (playerInventory.HasBomb)
        {
            color.a = 1;
        }
		else
        {
            color.a = 0.4f;
        }
        bombButton.GetComponent<Image>().color = color;
    }

	public void PauseGame()
    {
        Time.timeScale = 0;
        pauseButton.SetActive(false);
        playButton.SetActive(true);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
        pauseButton.SetActive(true);
        playButton.SetActive(false);
    }

    private void GameOver(int param)
	{
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        StartCoroutine(LoadScene(currentSceneIndex + 1));
    }

    IEnumerator LoadScene(int index)
    {
        sceneTransitionAnimator.SetTrigger("End");
        yield return new WaitForSeconds(transitionAnimationDuration);

        AudioManager.Instance.StopSound("Main Menu Music");
        AudioManager.Instance.PlaySound("Game Music");

        SceneManager.LoadScene(index);
    }

}
