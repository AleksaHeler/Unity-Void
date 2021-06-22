using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script just spawns sprite image for player on all clients
// This is located on "PlayerAvatar" prefab
public class AvatarSetup : MonoBehaviour
{
	private PhotonView photonView;

	[SerializeField]
	private GameObject characterPrefab;
	private GameObject myCharacter;
	// This GameObject contains players sprite
	public GameObject MyCharacter { get => myCharacter; }

	void Awake()
	{
		photonView = GetComponent<PhotonView>();

		// Create my sprite on all clients
		if (photonView.IsMine)
		{
			photonView.RPC("RPC_AddCharacter", RpcTarget.AllBuffered, PlayerSettings.Instance.MySelectedCharacter);
		}
	}

	[PunRPC]
	void RPC_AddCharacter(CharacterType characterType)
	{
		myCharacter = Instantiate(characterPrefab, transform.position, transform.rotation, transform);
		myCharacter.GetComponent<SpriteRenderer>().sprite = PlayerSettings.Instance.AllCharacters[(int)characterType];
	}
}
