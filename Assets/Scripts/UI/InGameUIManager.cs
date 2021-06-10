using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameUIManager : MonoBehaviour
{
    [SerializeField]
    private Animator sceneTransitionAnimator;
    [SerializeField]
    private float transitionAnimationDuration;


    // Start is called before the first frame update
    void Start()
    {
        PlayerController.OnPlayerDeath += GameOver;
    }

    private void OnDestroy()
    {
        PlayerController.OnPlayerDeath -= GameOver;
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
