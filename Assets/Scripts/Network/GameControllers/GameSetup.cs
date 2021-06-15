using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSetup : MonoBehaviour
{
    // Singleton
    private static GameSetup instance;
    public static GameSetup Instance { get => instance; }

    public Transform[] spawnPoints;

	private void OnEnable()
	{
		if(instance == null)
		{
            instance = this;
		}
	}
}
