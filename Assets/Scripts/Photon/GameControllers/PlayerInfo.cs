using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CharacterType { CACTUS, TIM, ROBOTO, PILLOWY, MARSHMELLOW, HAPPY }

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
