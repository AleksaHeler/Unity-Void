using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour
{
	PhotonView photonView;

	GameSettings gameSettings;
	float platformSpeed;
	float topBorder;
	float bottomBorder;

	void Start()
	{
		photonView = GetComponent<PhotonView>();
		 
		gameSettings = SettingsReader.Instance.GameSettings;
		platformSpeed = gameSettings.PlatformSpeed;
		topBorder = gameSettings.ScreenBorderTop;
		bottomBorder = gameSettings.ScreenBorderBottom;
	}

	// Move platform down and if it is all the way down, regenerate it up above the screen
	void Update()
	{
		if (!photonView.IsMine)
		{
			return;
		}

		transform.position += Vector3.down * Time.deltaTime * platformSpeed;

		if(transform.position.y < bottomBorder)
		{
			Vector3 newPos = transform.position;
			newPos.y = topBorder;
			transform.position = newPos;
		}
	}
}
