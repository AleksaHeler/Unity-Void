using System.Collections;
using UnityEngine;

public enum PlatformType { NONE, GLASS, GRASS, NORMAL, SLIDE_LEFT, SLIDE_RIGHT, SLIME, SPIKES }

[RequireComponent(typeof(SpriteRenderer))]
public class Platform : MonoBehaviour
{

	protected PlatformType type = PlatformType.NONE;
	public PlatformType Type { get => type; }


	// Constructor: just passes given platform type
	public void SetType(PlatformType platformType)
	{
		type = platformType;
		SetSprite(platformType);
	}

	// Helper function sets sprite of this platform
	private void SetSprite(PlatformType type)
	{
		Sprite platformSprite = SettingsReader.Instance.GameSettings.PlatformTypeToSprite(type);
		GetComponent<SpriteRenderer>().sprite = platformSprite;
	}
}
