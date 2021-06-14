using System.Collections;
using UnityEngine;


// This script is located on a prefab GameObject that represents a platform
// The object has a SpriteRenderer component where platform image is set
public class Platform : MonoBehaviour
{
	// TODO: maybe abstract class Platform? -> for breaking glass
	[SerializeField]
	private SpriteRenderer itemSpriteRenderer;
	private PlatformType platformType;
	private int platformId;

	public PlatformType PlatformType { get => platformType; }
	public int PlatformID { get => platformId; }

	// Generates a platform with given type at position with item on it
	public void GeneratePlatform(Vector2 position, int id, PlatformType type)
	{
		this.platformType = type;
		platformId = id;
		transform.position = position;

		SetSprite(type);
	}

	public void BreakGlass()
	{
		if (platformType != PlatformType.GLASS)
		{
			return;
		}

		StartCoroutine(GlassBreakCoroutine());
	}

	// Sets sprite component of this gameobject to given platform type
	private void SetSprite(PlatformType type)
	{
		Sprite platformSprite = SettingsReader.Instance.GameSettings.PlatformTypeToSprite(type);
		GetComponent<SpriteRenderer>().sprite = platformSprite;
	}

	public void SetItemSprite(ItemType itemType)
	{
		Sprite sprite = SettingsReader.Instance.GameSettings.ItemTypeToSprite(itemType);
		SetItemSprite(sprite);
	}

	public void SetItemSprite(Sprite sprite)
	{
		itemSpriteRenderer.sprite = sprite;
	}

	IEnumerator GlassBreakCoroutine()
	{
		platformType = PlatformType.NONE;
		SetSprite(platformType);
		AudioManager.Instance.PlaySound("Glass Breaking");

		float elapsedTime = 0;
		float duration = SettingsReader.Instance.GameSettings.GlassPlatformRegenerationTime;
		while(elapsedTime < duration)
		{
			elapsedTime += Time.deltaTime;
			yield return null;
		}

		platformType = PlatformType.GLASS;
		SetSprite(platformType);
	}
}
