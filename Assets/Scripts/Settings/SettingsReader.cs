using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used by other scripts to get global game settings data from one place
/// </summary>
public class SettingsReader : MonoBehaviour
{ 
	private static SettingsReader _instance;
	public static SettingsReader Instance { get => _instance; }


	[SerializeField]
	private GameSettings gameSettings;
	public GameSettings GameSettings { get => gameSettings; }


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
            DontDestroyOnLoad(this);
        }
    }
}
