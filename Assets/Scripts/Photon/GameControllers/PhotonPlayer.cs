using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using System.IO;
using UnityEngine;

// This script instantiates "PlayerAvatar" prefab at random spawn point
// This is located on "PhotonNetworkPlayer" GameObject
public class PhotonPlayer : MonoBehaviour
{
	private PhotonView photonView;

	private GameObject myAvatar;

	private void Awake()
	{
		photonView = GetComponent<PhotonView>();
	}

	private void Start()
	{
		if (!photonView.IsMine)
		{
			return;
		}

		int spawnPositionIndex = PhotonNetwork.IsMasterClient ? 0 : 1;
		Transform spawnTransform = GameSetup.Instance.PlayerSpawnPoints[spawnPositionIndex];

		Vector3 position = spawnTransform.position;
		Quaternion rotation = spawnTransform.rotation;

		myAvatar = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerAvatar"), position, rotation, 0);
	}
}
