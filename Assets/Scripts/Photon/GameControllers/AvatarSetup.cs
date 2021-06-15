using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script just creates sprite image for player
public class AvatarSetup : MonoBehaviour
{
    private PhotonView photonView;
    public GameObject characterPrefab;
    public CharacterType characterValue;
    [HideInInspector]
    public GameObject myCharacter;

    void Start()
    {
        photonView = GetComponent<PhotonView>();

		if (photonView.IsMine)
		{
            photonView.RPC("RPC_AddCharacter", RpcTarget.AllBuffered, PlayerInfo.Instance.mySelectedCharacter);
		}
    }

    
    [PunRPC]
    void RPC_AddCharacter(CharacterType characterType)
    {
        characterValue = characterType;
        myCharacter = Instantiate(characterPrefab, transform.position, transform.rotation, transform);
        myCharacter.GetComponent<SpriteRenderer>().sprite = PlayerInfo.Instance.allCharacters[(int)characterType];
    }
}
