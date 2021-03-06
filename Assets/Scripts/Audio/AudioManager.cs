using Photon.Pun;
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
    private static AudioManager instance;
    public static AudioManager Instance { get { return instance; } }

    private PhotonView photonView;
    public PhotonView PhotonView { get => photonView; }


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

        photonView = GetComponent<PhotonView>();

        // Create an audio source on this game object for each sound and copy settings
        foreach (Sound sound in Sounds)
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
            return;
        }

        if(sound.source != null)
        {
            sound.source.Play();
        }
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
            return;
        }

        if (sound.source != null)
        {
            sound.source.Stop();
        }
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
            return;
        }

        if (sound.source != null)
        {
            sound.source.Pause();
        }
    }

    [PunRPC]
    void RPC_PlaySound(string name)
    {
        PlaySound(name);
    }

    [PunRPC]
    void RPC_StopSound(string name)
    {
        StopSound(name);
    }


    [PunRPC]
    void RPC_StopAllSounds()
    {
        StopAllSounds();
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

    public void StopAllSounds()
	{
        foreach(Sound sound in Sounds)
		{
            if(sound.source != null)
            {
                sound.source.Stop();
            }
		}
	}

    public IEnumerator FadeOut(string soundName, float duration)
    {
        Sound sound = FindSoundInArray(soundName);
        if (sound == null)
        {
            yield break;
        }

        float startVolume = sound.source.volume;

        float elapsedTime = 0;
        while (elapsedTime < duration)
        {
            if (sound.source == null)
            {
                yield break;
            }
            float percent = elapsedTime / duration;
            sound.source.volume = startVolume * (1f - percent);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        StopSound(soundName);
        sound.source.volume = startVolume;
    }

    public IEnumerator FadeIn(string soundName, float duration)
    {
        Sound sound = FindSoundInArray(soundName);
        if (sound == null)
        {
            yield break;
        }

        float targetVolume = sound.source.volume;
        sound.source.volume = 0f;
        PlaySound(soundName);

        float elapsedTime = 0;
        while (elapsedTime < duration)
        {
            if(sound.source == null)
			{
                yield break;
			}
            float percent = elapsedTime / duration;
            sound.source.volume = percent * targetVolume;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    public void FadeOutAllSounds(float duration)
	{
        foreach(Sound sound in Sounds)
		{
            StartCoroutine(FadeOut(sound.name, duration));
		}
    }
}
