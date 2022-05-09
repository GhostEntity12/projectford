using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MazeController : MonoBehaviour
{
	// Constansts
	/// <summary>
	/// Whether the car should continue moving or query for a new command based on the number of walls
	/// </summary>
	readonly bool[] queryStates = new bool[16]
	{
		false,	// 00 - never happens
		false,	// 01 - dead end
		false,	// 02 - dead end
		true,	// 03 - continue 
		false,	// 04 - dead end
		true,	// 05 - continue 
		true,	// 06 - continue 
		false,	// 07 - query
		false,	// 08 - dead end
		true,	// 09 - continue 
		true,	// 10 - continue 
		false,	// 11 - query
		true,	// 12 - continue 
		false,	// 13 - query
		false,	// 14 - query
		false	// 15 - query
	};
	/// <summary>
	/// Used to get the tile in a given cardinal direction
	/// </summary>
	readonly Vector2Int[] cardinals = new Vector2Int[4]
	{
		new Vector2Int(0, 1),
		new Vector2Int(1, 0),
		new Vector2Int(0, -1),
		new Vector2Int(-1, 0)
	};

	[Header("Car Data")]
	[SerializeField] private GameObject _carObject;

	[SerializeField] private Transform _carTransform;

	[SerializeField] private GameObject[] _arrows;

	[SerializeField] private float _moveSpeed = 1f;

	[SerializeField] private float _rotSpeed = 0.2f;

	[SerializeField] private LineRenderer _line;

	[Header("Fuel Data")]
	[SerializeField] private int _startingFuel = 10;

	[SerializeField] private int _maximumFuel = 10;

	private int _currentFuel;

	[SerializeField] private GameObject _fuelGaugeObject = null;
	[SerializeField] private GameObject _fuelGaugePointer = null;
	[SerializeField] private Transform _pointerMax = null;
	[SerializeField] private Transform _pointerMin = null;

	[SerializeField] private GameObject _fuelCanPrefab;

	[Header("Maze Data")]
	[SerializeField] private GameObject _mazeObject;
	
	[SerializeField] private MazeLevelsData _easyMazeLevels = null;

	[SerializeField] private MazeLevelsData _mediumMazeLevels = null;

	[SerializeField] private MazeLevelsData _hardMazeLevels = null;

	private MazeLevelsData _currentMazeLevels = null;

	// private GameObject[] _mazeDecor;

	private Material _mazeMaterial;

	private MazeData _maze;

	private GameObject _currentMapCanvas;

	private int _currentMap = 0;

	[Header("Misc")]
	[SerializeField] private GameObject _levelCompleteScreen;

	[SerializeField] private GameObject _victoryScreen;

	[SerializeField] private GameObject _failureScreen;

	// Movement stuff

	/// <summary>
	/// The position the car will be in after moving
	/// </summary>
	private Vector2Int _targetPosition = new Vector2Int(9, 20);
	private Vector2 _currentPosition;
	private Vector2 _lastCellPos;

	/// <summary>
	/// Previous position used to iterate through corridors.
	/// Similarly to position, does not store the actual cell the car was previously in
	/// </summary>
	private Vector2Int _previousTargetPosition;

	readonly Queue<Vector2Int> _path = new Queue<Vector2Int>();

	// Flow
	private bool _isMoving;

	private bool _isRotating;

	private bool _isComplete;

	private bool _fuelActive = false;

	private List<Quaternion> _fuelGaugeRots = new List<Quaternion>();

	private List<GameObject> _fuelCans = new List<GameObject>();

	void Start()
	{
		// Caching
		// _mazes = Resources.LoadAll<MazeData>("Maze/MapData");
		// _mazeDecor = Resources.LoadAll<GameObject>("Maze/MapDecor");
		_mazeMaterial = _mazeObject.GetComponent<Renderer>().material;

		_currentFuel = _startingFuel;

		_fuelGaugeRots.Add(_pointerMin.rotation);
		for(int i = 0; i < _maximumFuel; ++i)
		{
			// Debug.Log(((float)_maximumFuel - (float)i) / (float)_maximumFuel);
			_fuelGaugeRots.Add(Quaternion.Lerp(_pointerMax.rotation, _pointerMin.rotation, ((float)_maximumFuel - (float)i) / (float)_maximumFuel));
		}
		_fuelGaugeRots.Add(_pointerMax.rotation);
	}

	private void Update()
	{
		// Make sure maze is set.
		if (_maze != null)
		{
			if (Input.GetKey(KeyCode.Alpha1))
				_levelCompleteScreen.SetActive(true);

			// Handling the start when the car is outside of the maze
			if (_targetPosition.x < 0)
				return;

			// Queue is empty and car is no longer moving, show arrows for tile
			if (_path.Count == 0 && !_isMoving && !_isRotating)
			{
				SetActiveArrows();
				_line.Simplify(0.2f);

				if (_isComplete)
				{
					// If this is the last map of the difficulty display the victory screen.
					if (_currentMap + 1 >= _currentMazeLevels.GetMazes().Count)
						_victoryScreen.SetActive(true);
					// Else display the level complete screen.
					else
						_levelCompleteScreen.SetActive(true);

					// No need to keep updating the game now.
					_maze = null;
				}
			}
			else
			{
				if (_isRotating)
				{
					Quaternion targetRotation = Quaternion.LookRotation(_carTransform.position - (Vector3)_currentPosition, Vector3.back);
					_carTransform.rotation = Quaternion.RotateTowards(_carTransform.rotation, targetRotation, _rotSpeed);
					if (_carTransform.rotation == targetRotation)
					{
						_isMoving = true;
						_isRotating = false;
					}
				}
				else
				{
					// Set the next new destination
					if (!_isMoving)
					{
						if (_fuelActive)
						{
							// Can only move if the car has fuel.
							if (_currentFuel > 0)
							{
								Vector2Int cell = _path.Dequeue();
								_lastCellPos = _currentPosition;
								_currentPosition = MazeCoordstoWorldCoords(cell);
								_isRotating = true;
								_line.SetPosition(_line.positionCount++ - 1, _carObject.transform.position);
								_line.SetPosition(_line.positionCount - 1, _carObject.transform.position);
							}
						}
						// Else fuel isn't enabled, so just move without checking for fuel.
						else
						{
							Vector2Int cell = _path.Dequeue();
							_currentPosition = MazeCoordstoWorldCoords(cell);
							_isRotating = true;
							_line.SetPosition(_line.positionCount++ - 1, _carObject.transform.position);
							_line.SetPosition(_line.positionCount - 1, _carObject.transform.position);
						}
					}
					// Move the car
					else
					{
						_line.SetPosition(_line.positionCount - 1, _carObject.transform.position);
						Vector2 newPos = Vector2.MoveTowards(_carObject.transform.position, _currentPosition, _moveSpeed * Time.deltaTime);
						_carObject.transform.position = newPos;

						if (_fuelActive)
						{
							Debug.Log((newPos - _currentPosition).magnitude / (_lastCellPos - _currentPosition).magnitude);
							_fuelGaugePointer.transform.rotation = Quaternion.Lerp(_fuelGaugeRots[_currentFuel - 1], _fuelGaugeRots[_currentFuel], (newPos - _currentPosition).magnitude / (_lastCellPos - _currentPosition).magnitude);
						}

						// Reached next cell in path.
						if ((Vector2)_carObject.transform.position == _currentPosition)
						{
							_isMoving = false;
							// Debug.Log(currentDestination);

							// Get the current cell the car is at.
							Vector2 currentCellPos = WorldCoordsToMazeCoords(_currentPosition);
							MazeCell currentCell = null;
							if (currentCellPos.x < _maze.dimensions.x && currentCellPos.y < _maze.dimensions.y)
								currentCell = _maze.cells2D[(int)currentCellPos.x, (int)currentCellPos.y];
							else
							{
								Debug.Log("Player is outside of map (maybe exiting?)", this);
								return;
							}

							// Check if the cell has a fuel canister.
							if (_fuelActive && currentCell._fuel == true && currentCell._fuelTaken == false)
							{
								// Just sets to maximum for now.
								_currentFuel = _maximumFuel;

								// Reset the fuel gauge.
								_fuelGaugePointer.transform.rotation = _pointerMax.rotation;

								// Set fuel taken to true so the player can't get the fuel more than once.
								currentCell._fuelTaken = true;

								Destroy(currentCell._fuelCanObject);
							}
							else if (_fuelActive)
							{
								_currentFuel--;
								// Ran out of fuel.
								if (_currentFuel == 0)
									_failureScreen.SetActive(true);
							}
						}
					}
				}
			}
		}
	}

	/// <summary>
	/// Loads a random maze
	/// </summary>
	public void LoadMaze(int mapIndex)
	{
		// Turn off the current decoration canvas.
		if (_currentMapCanvas != null)
		{
			GameObject.Destroy(_currentMapCanvas);
			_currentMapCanvas = null;
		}
		// Destory the fuel cans each time a new maze is loaded.
		foreach(GameObject can in _fuelCans)
		{
			GameObject.Destroy(can);
		}

		List<MazeData> mazes = _currentMazeLevels.GetMazes();
		// Get a new map.
		if (mapIndex < mazes.Count)
			_maze = mazes[mapIndex];

		// Set up the car for the start.
		_carObject.transform.position = MazeCoordstoWorldCoords(_maze.startLocation);
		_carTransform.transform.rotation = Quaternion.Euler(0, -90, 90);
		_line.SetPosition(0, _carObject.transform.position);
		_path.Clear();
		_targetPosition = _maze.startLocation;
		_lastCellPos = _maze.startLocation;

		// Load the new map.
		_mazeMaterial.mainTexture = _maze.map;
		_mazeObject.transform.localScale = new Vector3(_maze.dimensions.x, 1, _maze.dimensions.y);
		Camera.main.transform.position = new Vector3(_maze.dimensions.x / 4f, _maze.dimensions.y / 4f, -10);

		// Some misc setting up.
		_line.positionCount = 1;
		SetActiveArrows(Direction.East);
		_currentMapCanvas = GameObject.Instantiate(_currentMazeLevels.GetMazeDecor()[mapIndex]);

		if (_fuelActive)
		{
			// Reset fuel cell variables and spawn a fuel can at each cell.
			foreach(MazeCell cell in _maze.cells)
			{
				if (cell._fuel == true)
				{
					cell._fuelTaken = false;
					cell._fuelCanObject = GameObject.Instantiate(_fuelCanPrefab, MazeCoordstoWorldCoords(cell._position), Quaternion.identity, transform);
					_fuelCans.Add(cell._fuelCanObject);
				}
			}

			// Reset fuel of the car.
			_currentFuel = _startingFuel;
			_fuelGaugePointer.transform.rotation = _pointerMax.rotation;
		}
		else
		{
			_fuelGaugeObject.SetActive(false);
		}

		// Make sure these are off when starting a new map.
		_levelCompleteScreen.SetActive(false);
		_failureScreen.SetActive(false);

		_isComplete = false;
	}

	public void OnArrowClick(int index)
	{
		Vector2Int newPosition = _targetPosition + cardinals[index];

		if (newPosition.y < _maze.dimensions.x)
		{
			SetPath(newPosition);
			SetActiveArrows(0);
		}
	}

	void SetPath(Vector2Int nextTile)
	{
		_previousTargetPosition = _targetPosition;
		_targetPosition = nextTile;
		_path.Enqueue(nextTile);

		// Query based on open passages
		if (_targetPosition.x >= _currentMazeLevels.GetMazes()[_currentMap].dimensions.x)
		{
			// Map complete
			_isComplete = true;
			return;
		}
		// Stuff below here needs to happen after the car finishes moving
		MazeCell cell = _maze.cells2D[_targetPosition.x, _targetPosition.y];

		/* Comment out from here to the end of the function if you want the player to click every cell */
		// If there's two walls and two passages
		if (queryStates[(int)cell.walls])
		{
			for (int i = 0; i < 4; i++)
			{
				// If it's a wall, skip
				if (cell.walls.HasFlag((Direction)Mathf.Pow(2, i))) continue;

				// If it's not the previous tile, move there
				Vector2Int newPos = _targetPosition + cardinals[i];
				if (newPos != _previousTargetPosition)
				{
					// Recurse until a dead end or junction
					SetPath(newPos);
					break;
				}
			}
		}
	}

	public void SetActiveArrows()
	{
		Direction direction;
		try
		{
			// Invert walls to get where you can travel, & 15 to trim to last 4 bits
			direction = (Direction)((int)~_maze.cells2D[_targetPosition.x, _targetPosition.y].walls & 15);
		}
		catch (System.IndexOutOfRangeException)
		{
			// No arrows if out of bounds
			direction = 0;
		}

		for (int i = 0; i < _arrows.Length; i++)
		{
			_arrows[i].SetActive(direction.HasFlag((Direction)Mathf.Pow(2, i)));
		}
		// Handling entry arrow
		if (_targetPosition.y == 19)
		{
			_arrows[0].SetActive(false);
		}
	}

	public void SetActiveArrows(Direction direction)
	{
		for (int i = 0; i < _arrows.Length; i++)
		{
			_arrows[i].SetActive(direction.HasFlag((Direction)Mathf.Pow(2, i)));
		}
	}

	public void LoadNextMap()
	{
		LoadMaze(++_currentMap);
	}

	public void LoadCurrentMap()
	{
		LoadMaze(_currentMap);
	}

	public void SetDifficultyEasy()
	{
		_currentMazeLevels = _easyMazeLevels;
		_fuelActive = false;
	}

	public void SetDifficultyMedium()
	{
		_currentMazeLevels = _mediumMazeLevels;
		_fuelActive = true;
	}

	public void SetDifficultyHard()
	{
		_currentMazeLevels = _hardMazeLevels;
		_fuelActive = true;
	}

	public static Vector2 MazeCoordstoWorldCoords(Vector2 mazeCoords) => new Vector2(mazeCoords.x * 0.5f + 0.25f, mazeCoords.y * 0.5f + 0.25f);
	public static Vector2 WorldCoordsToMazeCoords(Vector2 worldCoords) => new Vector2(worldCoords.x / 0.5f + 0.25f, worldCoords.y / 0.5f + 0.25f);
}