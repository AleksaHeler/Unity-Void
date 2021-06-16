using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script just contains data about multiplayers settings (player count and scene build index)
public class MultiplayerSettings : MonoBehaviour
{
    // Singleton
    private static MultiplayerSettings instance;
    public static MultiplayerSettings Instance { get => instance; }

    [SerializeField]
    private int maxPlayers;
    public int MaxPlayers { get => maxPlayers; }

    [SerializeField]
    private int multiplayerSceneBuildIndex;
    public int MultiplayerSceneBuildIndex { get => multiplayerSceneBuildIndex; }

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
