using System.Collections;
using UnityEngine;


// This script is located on a prefab GameObject that represents a platform
// The object has a SpriteRenderer component where platform image is set
public class Platform : MonoBehaviour
{
	// TODO: maybe abstract class Platform? -> for breaking glass
	[SerializeField]
	private SpriteRenderer itemSpriteRenderer;
	private ItemType platformItem;
	private PlatformType platformType;
	private int platformId;

	public PlatformType PlatformType { get => platformType; }
	public ItemType PlatformItem { get => platformItem; }
	public int PlatformID { get => platformId; }

	// Generates a platform with given type at position with item on it
	public void GeneratePlatform(Vector2 position, ItemType item, int id, PlatformType type)
	{
		this.platformType = type;
		this.platformItem = item;
		platformId = id;
		transform.position = position;

		SetSprite(type);
		SetItemSprite(item);
	}

	public void BreakGlass()
	{
		if (platformType != PlatformType.GLASS)
		{
			return;
		}

		platformType = PlatformType.NONE;
		SetSprite(PlatformType.NONE);
		AudioManager.Instance.PlaySound("Glass Breaking");
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
}
