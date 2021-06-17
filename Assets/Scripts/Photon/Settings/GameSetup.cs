using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script contains data about spawn points for the level
public class GameSetup : MonoBehaviour
{
	private static GameSetup instance;
	public static GameSetup Instance { get => instance; }

	[SerializeField]
	private Transform[] playerSpawnPoints;
	public Transform[] PlayerSpawnPoints { get => playerSpawnPoints; }


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
