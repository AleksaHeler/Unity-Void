using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manager script in charge of actually playing all the sounds.
/// This is a singleton and has a DontDestroyOnLoad property, so other scripts can call
/// functions like AudioManager.Instance.Play("Sound Name");
/// Also has functions: Pause(), Stop() and PlayPlatformSound(platform)
/// </summary>
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

            sound.source.clip = sound.Clip;
            sound.source.volume = sound.Volume;
            sound.source.pitch = sound.Pitch;
            sound.source.loop = sound.Loop;
        }
    }

    /// <summary>
    /// Plays the clip with given name and returns true if successful
    /// </summary>
    /// <param name="name">Name of the source clip, same as in audio manager</param>
    public void PlaySound(string name)
    {
        Sound sound = FindSoundInArray(name);

        if (sound == null)
        {
            Debug.LogWarning("Sound with name: '" + name + "' was not found");
            return;
        }

        sound.source.Play();
        return;
    }

    /// <summary>
    /// Stops the clip with given name and returns true if successful
    /// </summary>
    /// <param name="name">Name of the source clip, same as in audio manager</param>
    public void StopSound(string name)
    {
        Sound sound = FindSoundInArray(name);

        if (sound == null)
        {
            Debug.LogWarning("Sound with name: '" + name + "' was not found");
            return;
        }

        sound.source.Stop();
        return;
    }

    /// <summary>
    /// Pauses the clip with given name so when it is played again it is resumed, and returns true if successful
    /// </summary>
    /// <param name="name">Name of the source clip, same as in audio manager</param>
    public void PauseSound(string name)
    {
        Sound sound = FindSoundInArray(name);

        if (sound == null)
        {
            Debug.LogWarning("Sound with name: '" + name + "' was not found");
            return;
        }

        sound.source.Pause();
        return;
    }

    private Sound FindSoundInArray(string name)
    {
        return Array.Find(Sounds, sound => sound.name == name);
    }

    // Play the sound that corresponds to given platforms type
    public void PlayPlatformSound(PlatformType platformType)
    {
        string soundName = SettingsReader.Instance.GameSettings.PlatformTypeToSound[platformType];
        PlaySound(soundName);
    }
}
