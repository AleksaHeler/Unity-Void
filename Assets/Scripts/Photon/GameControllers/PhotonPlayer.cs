using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using System.IO;
using UnityEngine;

// This creates player at random spawn point
public class PhotonPlayer : MonoBehaviour
{
    private PhotonView photonView;

    public GameObject myAvatar;

    void Start()
    {
        photonView = GetComponent<PhotonView>();

        int spawnPositionIndex = photonView.Controller.ActorNumber - 1;

		if (photonView.IsMine)
        {
            Transform spawnTransform = GameSetup.Instance.playerSpawnPoints[spawnPositionIndex];
            Vector3 position = spawnTransform.position;
            Quaternion rotation = spawnTransform.rotation;
            myAvatar = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerAvatar"), position, rotation, 0);
		}
    }
}
