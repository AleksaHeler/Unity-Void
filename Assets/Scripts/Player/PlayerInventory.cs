using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    private const string playerPrefsCoinsKey = "PlayerCoins";

    private PhotonView photonView;

    private bool hasBomb;
    private PlayerController playerController;
    private float platformSpacingX;
    private float platformSpacingY;
    private float platformSnapRange;
    private GameObject bombExplosionParticles;

    public bool HasBomb { get => hasBomb; }

    // Start is called before the first frame update
    void Awake()
    {
        photonView = GetComponent<PhotonView>();
        playerController = GetComponentInChildren<PlayerController>();

        GameSettings gameSettings = SettingsReader.Instance.GameSettings;
        platformSpacingX = gameSettings.PlatformSpacingHorizontal;
        platformSpacingY = gameSettings.PlatformSpacingVertical;
        platformSnapRange = gameSettings.PlayerToPlatformSnapRange;
        bombExplosionParticles = gameSettings.BombExplosionParticles;

        hasBomb = false;
    }

    // Update is called once per frame
    void Update()
    {
		if (!photonView.IsMine)
		{
            return;
		}

        if (Input.GetMouseButtonDown(1) && hasBomb)
        {
            PlaceBomb();
        }

        CheckIfBombIsInRange();
    }

    private void CheckIfBombIsInRange()
    {
        // All positions to check
        Vector3[] offsets = { Vector3.up, Vector3.down, Vector3.left, Vector3.right };

        foreach (Vector3 offset in offsets)
        {
            // Determine amount of spacing in current search direction
            bool xAxis = offset.x != 0;
            float spacing = xAxis ? platformSpacingX : platformSpacingY;

            Vector3 checkPosition = transform.position + offset * spacing;
            GameObject platform = PhotonWorld.Instance.GetPlatformWithinRange(checkPosition, platformSnapRange);

            if (platform == null)
            {
                continue;
            }

            ItemType item = PhotonWorld.Instance.GetItemTypeAtPlatform(platform);
            if (item == ItemType.BOMB_ACTIVE)
            {
                AudioManager.Instance.PhotonView.RPC("RPC_PlaySound", RpcTarget.All, "Bomb Explode");
                PhotonWorld.Instance.RemoveItemAtPlatform(platform);
                photonView.RPC("RPC_SpawnPlayerDeathParticles", RpcTarget.All, transform.position);
                playerController.PlayerDie();
            }
        }
    }

    public void PlaceBomb()
    {
        if (hasBomb == false)
        {
            return;
        }

        hasBomb = false;

        float placementRange = SettingsReader.Instance.GameSettings.PlayerToPlatformSnapRange;
        float bombPrimingTime = SettingsReader.Instance.GameSettings.BombPrimingTime;
        GameObject platform = PhotonWorld.Instance.GetPlatformWithinRange(transform.position, placementRange);
        StartCoroutine(BombPrimeCountdown(platform, bombPrimingTime));
    }

    private IEnumerator BombPrimeCountdown(GameObject platform, float duration)
    {
        AudioManager.Instance.PhotonView.RPC("RPC_PlaySound", RpcTarget.All, "Bomb Tick");
        PhotonWorld.Instance.PlaceItemAtPlatform(platform, ItemType.BOMB_PRIMING);

        // Wait for given amount of time
        float elapsedTime = 0;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        AudioManager.Instance.PhotonView.RPC("RPC_StopSound", RpcTarget.All, "Bomb Tick");
        PhotonWorld.Instance.PlaceItemAtPlatform(platform, ItemType.BOMB_ACTIVE);
    }

    public void CollectItem(GameObject platform)
    {
        ItemType itemType = PhotonWorld.Instance.GetItemTypeAtPlatform(platform);

        if (itemType == ItemType.BOMB_COLLECTIBLE && hasBomb == false)
        {
            hasBomb = true;
        }

        if (itemType == ItemType.COIN)
        {
            CollectCoin(platform);
        }

        PhotonWorld.Instance.RemoveItemAtPlatform(platform);
    }

    private void CollectCoin(GameObject platform)
	{
        int coins = 0;
        if (PlayerPrefs.HasKey(playerPrefsCoinsKey))
        {
            coins = PlayerPrefs.GetInt(playerPrefsCoinsKey);
        }
        coins++;

        PlayerPrefs.SetInt(playerPrefsCoinsKey, coins);
    }

    [PunRPC]
    void RPC_SpawnBombExplosionParticles(Vector3 position)
    {
        Instantiate(bombExplosionParticles, position, Quaternion.identity);
    }

}
