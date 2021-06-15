using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CharacterType { CACTUS, TIM, ROBOTO, PILLOWY, MARSHMELLOW, HAPPY }

// This script is located in main menu on a gameobject that persists trough scene
// It just keeps track of what CharacterType the local player has picked (it is accessed from...AvatarSetup)
public class PlayerInfo : MonoBehaviour
{  
	// Singleton
	private static PlayerInfo instance;
	public static PlayerInfo Instance { get { return instance; } }

	public CharacterType mySelectedCharacter;
	public Sprite[] allCharacters;

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

	// Start is called before the first frame update
	void Start()
	{
		if (PlayerPrefs.HasKey("MyCharacter"))
		{
			mySelectedCharacter = (CharacterType)PlayerPrefs.GetInt("MyCharacter");
		}
		else
		{
			mySelectedCharacter = 0;
			PlayerPrefs.SetInt("MyCharacter", (int)mySelectedCharacter);
		}
	}
}
