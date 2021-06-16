using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType { NONE, BOMB_COLLECTIBLE, BOMB_PRIMING, BOMB_ACTIVE }

[CreateAssetMenu(menuName = "ItemSettings")]
public class ItemSettings : ScriptableObject
{
	[SerializeField]
	private ItemType itemType;
	[SerializeField]
	private Sprite itemSprite;
	[SerializeField]
	private float itemChance;

	public ItemType ItemType { get => itemType; }
	public Sprite ItemSprite { get => itemSprite; }
	public float ItemChance { get => itemChance; }
}
