using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class describing a sound entyty that can be played in game
/// Contains fields like name (for identifying which sound to play), volume/pitch, loop
/// </summary>
[System.Serializable]
public class Sound
{
	public string name;

	public AudioClip Clip;

	[Range(0f, 1f)]
	public float Volume;
	[Range(0.1f, 3f)]
	public float Pitch;

	public bool Loop;

	[HideInInspector]
	public AudioSource source;
}
