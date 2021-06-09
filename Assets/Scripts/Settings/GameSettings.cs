using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region Global defines
public enum Item { NONE }

public enum PlatformType { NONE, NORMAL, SPIKES, SLIME, SLIDE_LEFT, SLIDE_RIGHT, GRASS, GLASS }

public enum PlayerAction { NONE, MOVE_LEFT, MOVE_RIGHT, MOVE_UP, MOVE_DOWN, PLACE_BOMB }
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

	[Tooltip("Horizontal size of platform sprite")]
	[SerializeField]
	private float platformWidth = 1.5f;

	[Tooltip("Vertical size of platform sprite")]
	[SerializeField]
	private float platformHeight = 1f;

	[Header("Level settings")]

	[Tooltip("How fast should platforms move down")]
	[SerializeField]
	private float platformSpeed = 0.2f;

	[Tooltip("How many platforms sould be random vs predetermined")]
	[SerializeField]
	private float percentOfRandomPlatforms = 0.1f;

	[Tooltip("How much spacing should be on sides of level")]
	[SerializeField]
	private float platformSideOffset = 4f;

	[Header("Platform settings")]
	#endregion


	#region Predefined rows
	private PlatformType[][] predefinedRows = new PlatformType[][] {
		new PlatformType[]{ PlatformType.NONE, PlatformType.SLIDE_LEFT, PlatformType.SPIKES, PlatformType.SLIDE_RIGHT, PlatformType.NONE},
		new PlatformType[]{ PlatformType.NONE, PlatformType.GLASS, PlatformType.NORMAL, PlatformType.SLIME, PlatformType.NONE},
		new PlatformType[]{ PlatformType.NORMAL, PlatformType.NONE, PlatformType.NORMAL, PlatformType.NONE, PlatformType.NORMAL},
		new PlatformType[]{ PlatformType.SPIKES, PlatformType.SLIDE_LEFT, PlatformType.NORMAL, PlatformType.SPIKES, PlatformType.SLIDE_LEFT},
		new PlatformType[]{ PlatformType.GLASS, PlatformType.NONE, PlatformType.GLASS, PlatformType.NONE, PlatformType.GLASS},
		new PlatformType[]{ PlatformType.NORMAL, PlatformType.SLIME, PlatformType.SLIME, PlatformType.GLASS, PlatformType.NORMAL},
		new PlatformType[]{ PlatformType.SLIDE_RIGHT, PlatformType.GLASS, PlatformType.GLASS, PlatformType.GLASS, PlatformType.SLIDE_LEFT},
		new PlatformType[]{ PlatformType.NORMAL, PlatformType.SPIKES, PlatformType.NORMAL, PlatformType.SPIKES, PlatformType.NORMAL }
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
	public float PlayerToPlatformSnapRange { get => Mathf.Min(PlatformSpacingX, PlatformSpacingY) * 0.8f; }
	public PlatformType[][] PredefinedRows { get => predefinedRows; }
	public Dictionary<PlatformType, string> PlatformTypeToSound { get => platformTypeToSound; }
	public Dictionary<PlayerAction, Vector3> MovePlayerActionToVector3 { get => movePlayerActionToVector3; }
	public Dictionary<SwipeDirection, PlayerAction> SwipeDirectionToPlayerAction { get => swipeDirectionToPlayerAction; }
	public List<PlayerAction> MovePlayerActions { get => movePlayerActions; }
	#endregion

}
