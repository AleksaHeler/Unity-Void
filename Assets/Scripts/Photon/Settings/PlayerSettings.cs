using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CharacterType { CACTUS, TIM, ROBOTO, PILLOWY, MARSHMELLOW, HAPPY }

// Keeps track of player sprite selection
public class PlayerSettings : MonoBehaviour
{
	private const string playerPrefsCharacterSelectKey = "MyCharacter";

	// Singleton
	private static PlayerSettings instance;
	public static PlayerSettings Instance { get { return instance; } }


	private CharacterType mySelectedCharacter;
	public CharacterType MySelectedCharacter { get => mySelectedCharacter; set => mySelectedCharacter = value; }

	private Sprite[] allCharacters;
	public Sprite[] AllCharacters { get => allCharacters; }

	private void Awake()
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

	private void Start()
	{
		LoadCharacterSelectionFromPlayerPrefs();
	}

	private void LoadCharacterSelectionFromPlayerPrefs()
	{
		if (PlayerPrefs.HasKey(playerPrefsCharacterSelectKey))
		{
			mySelectedCharacter = (CharacterType)PlayerPrefs.GetInt(playerPrefsCharacterSelectKey);
		}
		else
		{
			mySelectedCharacter = (CharacterType)0;
			PlayerPrefs.SetInt(playerPrefsCharacterSelectKey, (int)mySelectedCharacter);
		}
	}
}
