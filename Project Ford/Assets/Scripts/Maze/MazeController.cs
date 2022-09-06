using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MazeController : MonoBehaviour
{
	public enum MazeDifficulty
	{
		Easy,
		Medium,
		Hard,
		Count
	}
	// Constansts
	/// <summary>
	/// Whether the car should continue moving or query for a new command based on the number of walls
	/// </summary>
	readonly bool[] QueryWalls = new bool[16]
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
	[SerializeField] private GameObject _fuelCanPrefab;

	[SerializeField] private CarEntity _playerCar;

	[Header("Maze Data")]
	[SerializeField] private GameObject _mazeObject;
	[SerializeField] private Image _backgroundImage;
	
	[SerializeField] private MazeLevelsData _easyMazeLevels = null;

	[SerializeField] private MazeLevelsData _mediumMazeLevels = null;

	[SerializeField] private MazeLevelsData _hardMazeLevels = null;

	private MazeLevelsData _currentMazeLevels = null;

	private Material _mazeMaterial;

	private MazeData _currentMaze;

	private GameObject _currentMapCanvas;

	private int _currentMap = 0;

	[Header("Misc")]
	[SerializeField] private GameObject _levelCompleteScreen;

	[SerializeField] private GameObject _victoryScreen;

	[SerializeField] private GameObject _failureScreen;

	[SerializeField] private float _scaler = 2.0f;
	[SerializeField] private int _maxPathLoopCount = 20;

	private bool _fuelActive = false;

	private List<GameObject> _fuelCans = new List<GameObject>();

	private MazeDifficulty _currentDifficulty;

	private bool _decorSpawned = false;

	private InputController _inputController;

	public float Scaler => _scaler;
	public Transform MazeObjectTransform => _mazeObject.transform;

	private static MazeController _instance;

	public static MazeController Instance => _instance;

	public float ScaledX => _scaler / _mazeObject.transform.localScale.x;
	public float ScaledY => _scaler / _mazeObject.transform.localScale.y;
	public float ScaledZ => _scaler / _mazeObject.transform.localScale.z;

	public Vector2Int CurrentMazeDimensions => _currentMaze.dimensions;

	void Awake()
	{
		if (MazeController.Instance == null)
			_instance = this;
		else
			Destroy(gameObject);
	}

	void Start()
	{
		// Caching
		_mazeMaterial = _mazeObject.GetComponent<Renderer>().material;

		_inputController = InputController.Instance;
	}

#if UNITY_EDITOR
	private void Update()
	{
		if (Input.GetKey(KeyCode.Alpha1))
			EndLevel(true);
	}
#endif

	/// <summary>
	/// Loads a random maze
	/// </summary>
	public void LoadMaze(int mapIndex)
	{
		// Spawn decor if it hasn't spawned yet.
		if (!_decorSpawned)
		{
			// Destroy old decor if it exists.
			if (_currentMapCanvas != null)
			{
				GameObject.Destroy(_currentMapCanvas);
				_currentMapCanvas = null;
			}

			MazeDecor decor = _currentMazeLevels.GetMazeDecor()[mapIndex];
			if (decor != null)
			{
				_currentMapCanvas = GameObject.Instantiate(decor.gameObject);
				Sprite background = decor.BackgroundSprite;
				if (background != null)
				{
					if (!_backgroundImage.gameObject.activeSelf)
						_backgroundImage.gameObject.SetActive(true);
					
					_backgroundImage.sprite = background;
				}
				else
					_backgroundImage.gameObject.SetActive(false);
			}

			_decorSpawned = true;
		}
		
		// Destroy the fuel cans each time a new maze is loaded.
		foreach(GameObject can in _fuelCans)
		{
			GameObject.Destroy(can);
		}

		List<MazeData> mazes = _currentMazeLevels.GetMazes();
		// Get a new map.
		if (mapIndex < mazes.Count)
			_currentMaze = mazes[mapIndex];

		// Load the new map.
		_mazeMaterial.mainTexture = _currentMaze.map;
		Vector3 newMapScale = new Vector3(_currentMaze.dimensions.x * _scaler, 1, _currentMaze.dimensions.y * _scaler);
		_mazeObject.transform.localScale = newMapScale;
		// Move the camera to the centre of the map.
		Camera.main.transform.position = new Vector3(newMapScale.x / 4f, newMapScale.z / 4f, -10f);

		_inputController.SetActiveArrows(Direction.East);

		if (_fuelActive)
		{
			// Reset fuel cell variables and spawn a fuel can at each cell.
			foreach(MazeCell cell in _currentMaze.cells)
			{
				if (cell._fuel == true)
				{
					cell._fuelTaken = false;
					cell._fuelCanObject = GameObject.Instantiate(_fuelCanPrefab, MazeToWorldCoords(cell._position), Quaternion.identity, transform);
					cell._fuelCanObject.GetComponentInChildren<Canvas>().worldCamera = Camera.main;
					cell._fuelCanObject.transform.localScale = Vector3.one * _scaler;
					_fuelCans.Add(cell._fuelCanObject);
				}
			}
		}

		// Make sure these are off when starting a new map.
		_levelCompleteScreen.SetActive(false);
		_failureScreen.SetActive(false);

		_playerCar.Initialise();
	}

	public Path SetPath(Vector2Int startingCellPosition)
	{
		Queue<Vector2Int> path = new Queue<Vector2Int>();
		// In the event the first cell is outside the maze.
		if (startingCellPosition.x >= _currentMaze.dimensions.x)
		{
			path.Enqueue(startingCellPosition);
			return new Path(path, () => { EndLevel(true); });
		}
		else if (startingCellPosition.x < 0) // Tring to leave through entrance.
		{
			return new Path(path, null);
		}

		Vector2Int currentCellPosition = startingCellPosition;
		Vector2Int prevCellPosition = _playerCar.CurrentCellPosition;

		MazeCell currentCell = _currentMaze.cells2D[currentCellPosition.x, currentCellPosition.y];

		// Make sure can't get stuck in loop.
		int loopCount = 0;
		while (loopCount <= _maxPathLoopCount)
		{
			int index = 0;
			// Go through the 4 walls.
			for (; index < 4; ++index)
			{
				// If direction is the previous cell check next or has a wall.
				if (currentCellPosition + InputController.cardinals[index] == prevCellPosition ||
				currentCell.walls.HasFlag((Direction)Mathf.Pow(2, index)))
				{
					// If searched all 4 directions then at dead end, enqueue this last cell.
					if (index == 3)
					{
						path.Enqueue(currentCellPosition);
						break;
					}
					else
					{
						continue;
					}
				}

				// This direction is valid, add to path.
				path.Enqueue(currentCellPosition);
				break;
			}

			// If at dead end or has multiple possible directions then end path here.
			if (!QueryWalls[(int)currentCell.walls])
			{
				return new Path(path, null);
			}
			else
			{
				prevCellPosition = currentCellPosition;
				currentCellPosition = (currentCellPosition + InputController.cardinals[index]);

				// If this is greater than the maze's dimensions it is the last cell.
				if (currentCellPosition.x >= _currentMaze.dimensions.x)
				{
					path.Enqueue(currentCellPosition);
					return new Path(path, () => { EndLevel(true); });
				}
				else if (currentCellPosition.x > -1) // Else move on to checking next cell (so long as it's still in map). 
				{
					currentCell = _currentMaze.cells2D[currentCellPosition.x, currentCellPosition.y];
				}
				else // Trying to go back to start, just return path as is now.
				{
					return new Path(path, null);
				}
			}

			loopCount++;
		}
		Debug.LogError("Max loop count for finding path was reached.");

		return new Path(path, null);
	}

	public void LoadNextMap()
	{
		_decorSpawned = false;
		if (_currentMap < _currentMazeLevels.GetMazes().Count - 1)
			LoadMaze(++_currentMap);
		else
		{
			if ((int)_currentDifficulty < (int)MazeDifficulty.Count - 1)
			{
				switch(++_currentDifficulty)
				{
					// Can't go up to easy difficulty.

					case MazeDifficulty.Medium:
					SetDifficultyMedium();
					break;

					case MazeDifficulty.Hard:
					SetDifficultyHard();
					break;
				}
			}
		}
	}

	public void LoadCurrentMap()
	{
		LoadMaze(_currentMap);
	}

	public void SetDifficultyEasy()
	{
		_currentMazeLevels = _easyMazeLevels;
		_fuelActive = false;
		_currentDifficulty = MazeDifficulty.Easy;
		_currentMap = 0;

		TutorialController.Instance.InitialiseTutorial();

		LoadMaze(_currentMap);
	}

	public void SetDifficultyMedium()
	{
		_currentMazeLevels = _mediumMazeLevels;
		_fuelActive = true;
		_currentDifficulty = MazeDifficulty.Medium;
		_currentMap = 0;

		TutorialController.Instance.InitialiseTutorial();

		LoadMaze(_currentMap);
	}

	public void SetDifficultyHard()
	{
		_currentMazeLevels = _hardMazeLevels;
		_fuelActive = true;
		_currentDifficulty = MazeDifficulty.Hard;
		_currentMap = 0;

		TutorialController.Instance.InitialiseTutorial();

		LoadMaze(_currentMap);
	}

	/// <summary>
	/// End the current level.
	/// </summary>
	/// <param name="completed">If the level was completed successfully.</param>
	public void EndLevel(bool completed)
	{
		if (completed)
		{
			// If this is the last map display the victory screen.
			if (_currentMap >= _currentMazeLevels.GetMazes().Count - 1 && (int)_currentDifficulty >= (int)MazeDifficulty.Count - 1)
				_victoryScreen.SetActive(true);
			// Else display the level complete screen.
			else
				_levelCompleteScreen.SetActive(true);
		}
		else
		{
			_failureScreen.SetActive(true);
		}
	}

	public bool GetFuelEnabled()
	{
		return _fuelActive;
	}

	public MazeData GetCurrentMaze()
	{
		return _currentMaze;
	}

	public Vector2Int GetCurrentMazeStartLocation()
	{
		return _currentMaze.startLocation;
	}

	public static Vector2 MazeToWorldCoords(Vector2 mazeCoords) => new Vector2(((mazeCoords.x + 1) - MazeController.Instance.ScaledX) + (mazeCoords.x * MazeController.Instance.ScaledX), ((mazeCoords.y + 1) - MazeController.Instance.ScaledZ) + (mazeCoords.y * MazeController.Instance.ScaledZ));
	public static Vector2 WorldToMazeCoords(Vector2 worldCoords) => new Vector2((worldCoords.x - 1) + MazeController.Instance.ScaledX, (worldCoords.y - 1) + MazeController.Instance.ScaledZ);
}
