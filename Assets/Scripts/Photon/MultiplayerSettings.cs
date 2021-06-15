using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiplayerSettings : MonoBehaviour
{
    private static MultiplayerSettings instance;
    public static MultiplayerSettings Instance { get => instance; }

    [SerializeField]
    private int maxPlayers;
    public int MaxPlayers { get => maxPlayers; }

    [SerializeField]
    private int multiplayerScene;
    public int MultiplayerScene { get => multiplayerScene; }

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
            DontDestroyOnLoad(this.gameObject);
        }
    }
}
