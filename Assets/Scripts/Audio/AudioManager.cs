using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public Sound[] Sounds;

    // Singleton
    private static AudioManager _instance;
    public static AudioManager Instance { get { return _instance; } }


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

        // Create an audio source on this game object for each sound and copy settings
        foreach(Sound sound in Sounds)
		{
            sound.source = gameObject.AddComponent<AudioSource>();

            sound.source.volume = sound.Volume;
            sound.source.pitch = sound.Pitch;
            sound.source.loop = sound.Loop;
        }
    }

    /// <summary>
    /// Plays the clip with given name and returns true if successful
    /// </summary>
    /// <param name="name">Name of the source clip, same as in audio manager</param>
    /// <returns></returns>
    public bool PlaySound(string name)
    {
        Sound sound = FindSound(name);

        if (sound == null)
        {
            Debug.LogWarning("Sound with name: '" + name + "' was not found");
            return false;
        }

        sound.source.Play();
        return true;
    }

    /// <summary>
    /// Stops the clip with given name and returns true if successful
    /// </summary>
    /// <param name="name">Name of the source clip, same as in audio manager</param>
    /// <returns></returns>
    public bool StopSound(string name)
    {
        Sound sound = FindSound(name);

        if (sound == null)
        {
            Debug.LogWarning("Sound with name: '" + name + "' was not found");
            return false;
        }

        sound.source.Stop();
        return true;
    }

    /// <summary>
    /// Pauses the clip with given name so when it is played again it is resumed, and returns true if successful
    /// </summary>
    /// <param name="name">Name of the source clip, same as in audio manager</param>
    /// <returns></returns>
    public bool PauseSound(string name)
    {
        Sound sound = FindSound(name);

        if (sound == null)
        {
            Debug.LogWarning("Sound with name: '" + name + "' was not found");
            return false;
        }

        sound.source.Pause();
        return true;
    }

    private Sound FindSound(string name)
    {
        return Array.Find(Sounds, sound => sound.name == name);
    }
}
