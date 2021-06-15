using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using System.IO;
using UnityEngine;

// This is the world class containing platforms
public class PhotonWorld : MonoBehaviour
{
    private PhotonView photonView;

    GameObject[,] platforms;

    void Start()
    {
        photonView = GetComponent<PhotonView>();

        if (photonView.IsMine && PhotonNetwork.IsMasterClient)
        {
            GameSettings gameSettings = SettingsReader.Instance.GameSettings;

            int width = gameSettings.Width;
            int height = gameSettings.Height;

            platforms = new GameObject[width, height];

            float spacingX = gameSettings.PlatformSpacingX;
            float spacingY = gameSettings.PlatformSpacingY;

            for(int x = 0; x < width; x++)
			{
                for(int y = 0; y < height; y++)
                {
                    float xPos = ((float)x - width / 2f) * spacingX;
                    float yPos = ((float)y - height / 2f) * spacingY;
                    Vector3 position = new Vector3(xPos, yPos);
                    Quaternion rotation = Quaternion.identity;
                    platforms[x, y] = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlatformAvatar"), position, rotation, 0);
                    platforms[x, y].transform.SetParent(transform);
                }
            }
        }
    }
}
