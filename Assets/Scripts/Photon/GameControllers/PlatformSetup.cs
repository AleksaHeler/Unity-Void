using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformSetup : MonoBehaviour
{
    private PhotonView photonView;

    public GameObject platformPrefab;
    public PlatformType platformType;
    [HideInInspector]
    public GameObject myPlatform;

    // Start is called before the first frame update
    void Start()
    {
        photonView = GetComponent<PhotonView>();
    }

    [PunRPC]
    void RPC_BreakGlass()
    {
        platformType = PlatformType.NONE;
        myPlatform.GetComponent<SpriteRenderer>().sprite = SettingsReader.Instance.GameSettings.PlatformTypeToSprite(platformType);
        // TODO: add coroutine that will regenerate the glass once more
    }

    [PunRPC]
    void RPC_AddPlatform()
    {
        platformType = PlatformType.NONE;
        myPlatform = Instantiate(platformPrefab, transform.position, transform.rotation, transform);
        myPlatform.GetComponent<SpriteRenderer>().sprite = SettingsReader.Instance.GameSettings.PlatformTypeToSprite(PlatformType.NONE);
    }

    [PunRPC]
    void RPC_SetPlatformType(PlatformType type)
    {
        platformType = type;
        myPlatform.GetComponent<SpriteRenderer>().sprite = SettingsReader.Instance.GameSettings.PlatformTypeToSprite(type);
    }

    [PunRPC]
    void RPC_MovePlatformDown(Vector3 movement)
    {
        myPlatform.transform.position += movement;
    }
}
