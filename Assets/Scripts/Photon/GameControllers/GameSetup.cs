using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSetup : MonoBehaviour
{
	private static GameSetup instance;
	public static GameSetup Instance { get => instance; }

	public Transform[] playerSpawnPoints;

	private void OnEnable()
	{
        // Singleton
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
    }
}
