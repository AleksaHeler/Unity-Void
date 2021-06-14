using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


#region Global defines
public enum ItemType { NONE, BOMB_COLLECTIBLE, BOMB_PRIMING, BOMB_ACTIVE }

public enum PlatformType { NONE, GLASS, GRASS, NORMAL, SLIDE_LEFT, SLIDE_RIGHT, SLIME, SPIKES }

public enum PlayerAction { NONE, MOVE_UP, MOVE_DOWN, MOVE_LEFT, MOVE_RIGHT }

public enum SwipeDirection { UP, DOWN, LEFT, RIGHT }

#endregion


[CreateAssetMenu(menuName = "GameSettings")]
public class GameSettings : ScriptableObject
{

	#region Private members for editor
	[Header("World size (# of platforms)")]
	[SerializeField]
	private int width = 5;

	[SerializeField]
	private int height = 6;

	[Header("Platform settings")]

	[SerializeField]
	private GameObject platformPrefab;

	[SerializeField]
	private PlatformSettings[] platformSettings;

	[SerializeField]
	private ItemSettings[] itemSettings;

	[Tooltip("Horizontal size of platform sprite")]
	[SerializeField]
	private float platformWidth = 1.5f;

	[Tooltip("Vertical size of platform sprite")]
	[SerializeField]
	private float platformHeight = 1f;

	[SerializeField]
	private List<PlatformType> unsafePlatforms;

	[Header("Level settings")]

	[Tooltip("How fast should platforms move down")]
	[SerializeField]
	private float platformSpeed = 0.2f;

	[Tooltip("How many platforms sould be random vs predetermined")]
	[SerializeField]
	private float percentOfRandomPlatforms = 0.1f;

	[SerializeField]
	private float chanceForBomb = 0.1f;

	[Tooltip("How much spacing should be on sides of level")]
	[SerializeField]
	private float platformSideOffset = 4f;

	[Header("Player settings")]
	[SerializeField]
	private Vector3 playerToPlatformOffset;

	[SerializeField]
	private float playerToPlatformSnapRange;

	[SerializeField]
	private float playerSpeed;

	[SerializeField]
	private float playerJumpAnimationHeight;

	[SerializeField]
	private float moveVectorMinMagnitude;

	[SerializeField]
	private float moveVectorMaxMagnitude;

	[SerializeField]
	private float playerCheckTolerance;

	[SerializeField]
	private float moveAnimationCurveOffset;

	[SerializeField]
	private int minDistanceToSwipe;

	[SerializeField]
	private float bombPrimingTime;

	#endregion


	#region Predefined rows
	private PlatformType[][] predefinedRows = new PlatformType[][] {
		new PlatformType[]{ PlatformType.NORMAL, PlatformType.SLIDE_LEFT, PlatformType.SLIME, PlatformType.SLIDE_RIGHT, PlatformType.NORMAL},
		new PlatformType[]{ PlatformType.NORMAL, PlatformType.GLASS, PlatformType.SLIME, PlatformType.GLASS, PlatformType.NORMAL},
		new PlatformType[]{ PlatformType.NORMAL, PlatformType.NONE, PlatformType.NORMAL, PlatformType.NONE, PlatformType.NORMAL},
		new PlatformType[]{ PlatformType.SPIKES, PlatformType.SLIDE_RIGHT, PlatformType.NORMAL, PlatformType.SLIDE_LEFT, PlatformType.SPIKES},
		new PlatformType[]{ PlatformType.GLASS, PlatformType.GLASS, PlatformType.GLASS, PlatformType.GLASS, PlatformType.GLASS},
		new PlatformType[]{ PlatformType.NORMAL, PlatformType.SLIME, PlatformType.GRASS, PlatformType.SLIME, PlatformType.NORMAL},
		new PlatformType[]{ PlatformType.SLIDE_RIGHT, PlatformType.GRASS, PlatformType.GLASS, PlatformType.GRASS, PlatformType.SLIDE_LEFT},
		new PlatformType[]{ PlatformType.NORMAL, PlatformType.SPIKES, PlatformType.GRASS, PlatformType.SPIKES, PlatformType.NORMAL },
		new PlatformType[]{ PlatformType.GLASS, PlatformType.SLIDE_RIGHT, PlatformType.SLIME, PlatformType.SLIDE_LEFT, PlatformType.SPIKES},
		new PlatformType[]{ PlatformType.NONE, PlatformType.NONE, PlatformType.SLIDE_RIGHT, PlatformType.NONE, PlatformType.GRASS},
		new PlatformType[]{ PlatformType.GRASS, PlatformType.NONE, PlatformType.GLASS, PlatformType.NONE, PlatformType.NONE},
		new PlatformType[]{ PlatformType.SPIKES, PlatformType.NONE, PlatformType.SLIDE_LEFT, PlatformType.NONE, PlatformType.NORMAL},
		new PlatformType[]{ PlatformType.NORMAL, PlatformType.NONE, PlatformType.NONE, PlatformType.NONE, PlatformType.GRASS},
		new PlatformType[]{ PlatformType.GRASS, PlatformType.NONE, PlatformType.NORMAL, PlatformType.NONE, PlatformType.GRASS},
		new PlatformType[]{ PlatformType.SLIDE_LEFT, PlatformType.GRASS, PlatformType.NORMAL, PlatformType.SLIME, PlatformType.NONE},
		new PlatformType[]{ PlatformType.NONE, PlatformType.GLASS, PlatformType.SLIDE_RIGHT, PlatformType.NORMAL, PlatformType.SLIDE_RIGHT}
	};
	#endregion


	#region Dictionaries & Lists for conversion
	// Used for converting PlatformType to sound name to be played
	private Dictionary<PlatformType, string> platformTypeToSound = new Dictionary<PlatformType, string>()
	{
		{ PlatformType.NONE, "" },
		{ PlatformType.NORMAL, "Step Normal" },
		{ PlatformType.SPIKES, "Step Spike" },
		{ PlatformType.SLIME, "Step Slime" },
		{ PlatformType.SLIDE_LEFT, "Step Slide" },
		{ PlatformType.SLIDE_RIGHT, "Step Slide" },
		{ PlatformType.GRASS, "Step Grass" },
		{ PlatformType.GLASS, "Step Glass" }
	};

	private Dictionary<SwipeDirection, PlayerAction> swipeDirectionToPlayerAction = new Dictionary<SwipeDirection, PlayerAction>()
	{
		{ SwipeDirection.UP, PlayerAction.MOVE_UP },
		{ SwipeDirection.DOWN, PlayerAction.MOVE_DOWN },
		{ SwipeDirection.LEFT, PlayerAction.MOVE_LEFT },
		{ SwipeDirection.RIGHT, PlayerAction.MOVE_RIGHT }
	};

	private Dictionary<PlayerAction, Vector3> movePlayerActionToVector3 = new Dictionary<PlayerAction, Vector3>()
	{
		{ PlayerAction.MOVE_UP, Vector3.up },
		{ PlayerAction.MOVE_DOWN, Vector3.down },
		{ PlayerAction.MOVE_LEFT, Vector3.left },
		{ PlayerAction.MOVE_RIGHT, Vector3.right }
	};

	private List<PlayerAction> movePlayerActions = new List<PlayerAction>() {
		PlayerAction.MOVE_UP,
		PlayerAction.MOVE_DOWN,
		PlayerAction.MOVE_LEFT,
		PlayerAction.MOVE_RIGHT
	};
	#endregion


	#region Access modifiers (getters)
	public int PlatformsCount { get => width; }
	public int RowsCount { get => height; }
	public GameObject PlatformPrefab { get => platformPrefab; }
	public float PlatformWidth { get => platformWidth; }
	public float PlatformHeight { get => platformHeight; }
	public float PlatformSpeed { get => platformSpeed; }
	public float PercentOfRandomPlatforms { get => percentOfRandomPlatforms; }
	public float ChanceForBomb { get => chanceForBomb; }
	public float ScreenBorderTop {
		get {
			Vector3 topRightViewport = new Vector3(1, 1, 0);
			Vector3 screenBorderPosition = Camera.main.ViewportToWorldPoint(topRightViewport);
			return screenBorderPosition.y + platformHeight / 2;
		}
	}
	public float ScreenBorderBottom { get => -ScreenBorderTop; }
	public float PlatformSpacingY { get { return (ScreenBorderTop * 2f) / height; } }
	public float PlatformSpacingX { get { return ((Camera.main.ViewportToWorldPoint(new Vector3(1, 1, 0)).x - platformSideOffset) * 2f) / width; } }
	public float PlayerToPlatformSnapRange { get => playerToPlatformSnapRange; }
	public Vector3 PlayerToPlatformOffset { get => playerToPlatformOffset;  }
	public float PlayerSpeed { get => playerSpeed; }
	public float PlayerJumpAnimationHeight { get => playerJumpAnimationHeight; }
	public float MoveVectorMinMagnitude { get => moveVectorMinMagnitude; }
	public float MoveVectorMaxMagnitude { get => moveVectorMaxMagnitude; }
	public float PlayerCheckTolerance { get => playerCheckTolerance; }
	public float MoveAnimationCurveOffset { get => moveAnimationCurveOffset; }
	public int MinDistanceToSwipe { get => minDistanceToSwipe; }
	public float BombPrimingTime { get => bombPrimingTime; }
	public PlatformType[][] PredefinedRows { get => predefinedRows; }
	public Dictionary<PlatformType, string> PlatformTypeToSound { get => platformTypeToSound; }
	public Dictionary<SwipeDirection, PlayerAction> SwipeDirectionToPlayerAction { get => swipeDirectionToPlayerAction; }
	public List<PlayerAction> MovePlayerActions { get => movePlayerActions; }
	public PlatformSettings[] PlatformSettings { get => platformSettings; }
	public ItemSettings[] ItemSettings { get => itemSettings; }
	public List<PlatformType> UnsafePlatforms { get => unsafePlatforms; }
	#endregion


	#region Helper functions for finding stuff
	public Sprite PlatformTypeToSprite(PlatformType type)
	{
		PlatformSettings platform = Array.Find(platformSettings, platform => platform.PlatformType == type);

		if (platform != null)
		{
			return platform.PlatformSprite;
		}

		return null;
	}
	public Sprite ItemTypeToSprite(ItemType type)
	{
		ItemSettings item = Array.Find(itemSettings, item => item.ItemType == type);

		if (item != null)
		{
			return item.ItemSprite;
		}

		return null;
	}

	public float PlatformTypeToChance(PlatformType type)
	{
		PlatformSettings platform = Array.Find(platformSettings, platform => platform.PlatformType == type);

		if (platform != null)
		{
			return platform.PlatformChance;
		}

		return 0;
	}

	public PlatformType GetRandomPlatformType()
	{
		float totalChance = 0;
		for (int i = 0; i < platformSettings.Length; i++)
		{
			totalChance += platformSettings[i].PlatformChance;
		}

		float random = UnityEngine.Random.Range(0f, totalChance);

		// Subtract possibilities one by one from the random number, and as soon as that number is less than 0 return that type
		for (int i = 0; i < platformSettings.Length; i++)
		{
			random -= platformSettings[i].PlatformChance;
			if (random <= 0)
			{
				return platformSettings[i].PlatformType;
			}
		}

		return PlatformType.NONE;
	}

	public Vector3 PlayerActionToVector3(PlayerAction action)
	{
		Vector3 movement = movePlayerActionToVector3[action];
		movement.x *= PlatformSpacingX;
		movement.y *= PlatformSpacingY;
		return movement;
	}
	#endregion

}
