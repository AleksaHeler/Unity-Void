using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
	// Singleton
	private static ItemManager _instance;
	public static ItemManager Instance { get { return _instance; } }

	private void Awake()
	{
		// Singleton
		if (_instance != null && _instance != this)
		{
			Destroy(this.gameObject);
		}
		else
		{
			_instance = this;
		}

		// Set random seed
		int seed = System.DateTime.Now.Ticks.GetHashCode();
		UnityEngine.Random.InitState(seed);
	}
}
