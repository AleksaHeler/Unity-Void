using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
	private float speed = 0;
	private Vector3 direction = Vector3.zero;
	private float screenBorder;
	private const float screenBorderOffset = 4;
	private const float hitCircleRadius = 1.2f;
	private float platformSpeed = 0f;

	private void Start()
	{
		speed = SettingsReader.Instance.GameSettings.CannonBulletSpeed;

		screenBorder = Camera.main.ViewportToWorldPoint(new Vector2(1, 0)).x;
	}

	private void Update()
    {
		transform.position += direction * speed * Time.deltaTime + Vector3.down * platformSpeed * Time.deltaTime;

		if(Mathf.Abs(transform.position.x) > screenBorder + screenBorderOffset)
		{
			gameObject.SetActive(false);
		}

		// Check if we hit the player
		GameObject[] playerGameObjects = GameObject.FindGameObjectsWithTag("Player");
		foreach(GameObject player in playerGameObjects)
		{
			float distance = Vector3.Distance(transform.position, player.transform.position);
			if(distance < hitCircleRadius)
			{
				player.GetComponent<PlayerController>().PlayerDie();
			}
		}
    }

	public void SetDirectionAndPlatformSpeed(Vector3 direction, float platformSpeed)
	{
		this.direction = direction;
		if(direction.x > 0)
		{
			GetComponent<SpriteRenderer>().flipX = true;
		}
		this.platformSpeed = platformSpeed;
	}
}
