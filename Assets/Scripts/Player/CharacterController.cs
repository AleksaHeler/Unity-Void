using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This is the main player script
[RequireComponent(typeof(PlayerInput))]
public class CharacterController : MonoBehaviour
{

    private PhotonView photonView;

    void Start()
    {
        photonView = GetComponent<PhotonView>();
    }

    void Update()
    {
		if (Input.GetKeyDown(KeyCode.Escape))
		{
            Application.Quit();
		}

		if (photonView.IsMine)
        {
            float moveX = Input.GetAxisRaw("Horizontal");
            float moveY = Input.GetAxisRaw("Vertical");
            Vector3 movement = new Vector3(moveX, moveY) * Time.deltaTime * 4f;
            transform.position += movement;
        }
    }
}
