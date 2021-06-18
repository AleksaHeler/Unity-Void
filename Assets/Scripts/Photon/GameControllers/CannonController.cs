using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonController : MonoBehaviour
{
	private PhotonView photonView;
	[SerializeField]
	private GameObject cannonPrefab;
	[SerializeField]
	private GameObject bulletPrefab;
	private GameObject myCannon;
	private List<GameObject> myBullets;
	private Vector3 shootDirection;
	private bool canShoot;
	private float roundsPerMinuteMin;
	private float roundsPerMinuteMax;
	private IEnumerator shootCoroutine;

	void Awake()
	{
		photonView = GetComponent<PhotonView>();
		canShoot = false;

		myBullets = new List<GameObject>();
		int poolingCount = SettingsReader.Instance.GameSettings.CannonBulletPoolingCount;
		for(int i = 0; i < poolingCount; i++)
		{
			GameObject bullet = Instantiate(bulletPrefab);
			bullet.SetActive(false);
			myBullets.Add(bullet);
		}

		roundsPerMinuteMin = SettingsReader.Instance.GameSettings.CannonRoundsPerMinuteMin;
		roundsPerMinuteMax = SettingsReader.Instance.GameSettings.CannonRoundsPerMinuteMax;

		if (!PhotonNetwork.IsMasterClient)
		{
			return;
		}

		shootCoroutine = ShootCoroutine();
		StartCoroutine(shootCoroutine);
	}

	private void OnDestroy()
	{
		StopCoroutine(shootCoroutine);
		foreach(GameObject bullet in myBullets)
		{
			Destroy(bullet);
		}
	}

	[PunRPC]
	// Spawn platform prefab and set its sprite to none
	void RPC_Shoot()
	{
		// TODO: implement this
		foreach(GameObject bullet in myBullets)
		{
			// If already spawned dont do anything
			if (bullet.activeInHierarchy)
			{
				continue;
			}

			bullet.SetActive(true);
			bullet.transform.position = transform.position;
			float platformSpeed = SettingsReader.Instance.GameSettings.PlatformSpeed;
			bullet.GetComponent<BulletController>().SetDirectionAndPlatformSpeed(shootDirection, platformSpeed);
			break;
		}
	}

	[PunRPC]
	// Spawn platform prefab and set its sprite to none
	void RPC_AddCannon(Vector3 shootDirection)
	{
		myCannon = Instantiate(cannonPrefab, transform.position, Quaternion.Euler(0, 0, 90), transform);
		if(shootDirection.x > 0)
		{
			myCannon.GetComponent<SpriteRenderer>().flipY = true;
		}
		canShoot = true;
		this.shootDirection = shootDirection;
	}

	[PunRPC]
	// Animate cannon by moving it by given amount and destroy once off screen
	void RPC_MoveCannonDown(Vector3 movement)
	{
		myCannon.transform.position += movement;
	}

	IEnumerator ShootCoroutine()
	{
		while (!canShoot)
		{
			yield return null;
		}
		while (true)
		{
			// pick a random time to wait
			float randomRoundsPerMinute = Random.Range(roundsPerMinuteMin, roundsPerMinuteMax);
			float waitDuration = 60f / randomRoundsPerMinute;

			// Wait
			float elapsedTime = 0;
			while(elapsedTime < waitDuration)
			{
				elapsedTime += Time.deltaTime;
				yield return null;
			}

			// shoot a bullet
			photonView.RPC("RPC_Shoot", RpcTarget.All);
		}
	}

}
