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


    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(AudioManager.Instance.FadeIn("Game Music", transitionAnimationDuration));
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
        StartCoroutine(AudioManager.Instance.FadeOut("Game Music", transitionAnimationDuration));

        yield return new WaitForSeconds(transitionAnimationDuration);
        SceneManager.LoadScene(index);
    }

}
