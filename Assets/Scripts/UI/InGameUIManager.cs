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


    private void Start()
    {
        StartCoroutine(AudioManager.Instance.FadeIn("Game Music", transitionAnimationDuration));
    }

	private void Update()
	{
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
}
