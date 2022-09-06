using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
	/// <summary>
	/// Used to get the tile in a given cardinal direction
	/// </summary>
	public static readonly Vector2Int[] cardinals = new Vector2Int[4]
	{
		new Vector2Int(0, 1), // North
		new Vector2Int(1, 0), // East
		new Vector2Int(0, -1),// South
		new Vector2Int(-1, 0) // West
	};

	[SerializeField] private GameObject[] _arrows;
	[SerializeField] private CarEntity _playerCar;
	[SerializeField] private GameObject _directionButtons;

	private MazeController _mazeController;

	private static InputController _instance;
	public static InputController Instance => _instance;

	void Awake()
	{
		if (_instance == null)
			_instance = this;
		else
			Destroy(gameObject);
	}

    void Start()
    {
        _mazeController = MazeController.Instance;
    }

	public void GiveInput(int input)
	{
		Vector2Int newPosition = _playerCar.CurrentCellPosition + cardinals[input];
		_playerCar.SetPath(_mazeController.SetPath(newPosition));
		if (_playerCar.IsMoving)
		{
			SetActiveArrows(0);
		}
	}

	public void SetActiveArrows()
	{
		Direction direction;
		Vector2Int playerPosition = _playerCar.CurrentCellPosition;

		try
		{
			// Invert walls to get where you can travel, & 15 to trim to last 4 bits
			direction = (Direction)((int)~_mazeController.GetCurrentMaze().cells2D[playerPosition.x, playerPosition.y].walls & 15);
		}
		catch (System.IndexOutOfRangeException)
		{
			// No arrows if out of bounds
			direction = 0;
			Debug.LogError("Out of bounds!");
		}

		for (int i = 0; i < _arrows.Length; i++)
			_arrows[i].SetActive(direction.HasFlag((Direction)Mathf.Pow(2, i)));

		_directionButtons.transform.position = _playerCar.transform.position;
	}

	public void SetActiveArrows(Direction direction)
	{
		for (int i = 0; i < _arrows.Length; i++)
			_arrows[i].SetActive(direction.HasFlag((Direction)Mathf.Pow(2, i)));

		_directionButtons.transform.position = _playerCar.transform.position;
	}
}
