using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlatformType { NONE, GLASS, GRASS, NORMAL, SLIDE_LEFT, SLIDE_RIGHT, SLIME, SPIKES }

[CreateAssetMenu(menuName = "PlatformSettings")]
public class PlatformSettings : ScriptableObject
{
	[SerializeField]
	private PlatformType platformType;
	[SerializeField]
	private Sprite platformSprite;
	[SerializeField]
	private float platformChance;

	public PlatformType PlatformType { get => platformType; }
	public Sprite PlatformSprite { get => platformSprite; }
	public float PlatformChance { get => platformChance; }
}
