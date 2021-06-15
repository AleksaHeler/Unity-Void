using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using System.IO;
using UnityEngine;

public class PhotonPlayer : MonoBehaviour
{
    private PhotonView photonView;

    public GameObject myAvatar;

    void Start()
    {
        photonView = GetComponent<PhotonView>();

        int spawnPointCount = GameSetup.Instance.spawnPoints.Length;
        int spawnPicker = Random.Range(0, spawnPointCount);

		if (photonView.IsMine)
        {
            Transform spawnTransform = GameSetup.Instance.spawnPoints[spawnPicker];
            Vector3 position = spawnTransform.position;
            Quaternion rotation = spawnTransform.rotation;
            myAvatar = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerAvatar"), position, rotation, 0);
		}
    }

    void Update()
    {
        
    }
}
