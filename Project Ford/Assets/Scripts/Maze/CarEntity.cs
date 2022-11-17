using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path
{
	public Path(Queue<Vector2Int> path, Action endAction)
	{
		_pathCells = path;
		_endAction = endAction;
	}
	public Queue<Vector2Int> _pathCells = new Queue<Vector2Int>();
	public Action _endAction = null;
}

public class CarEntity : MonoBehaviour
{	
	[Header("Fuel Data")]
	[SerializeField] private int _startingFuel = 10;
	[SerializeField] private int _maximumFuel = 10;

	[Header("Fuel UI")]
	[SerializeField] private GameObject _fuelGaugeObject = null;
	[SerializeField] private GameObject _fuelGaugePointer = null;
	[SerializeField] private Transform _pointerMax = null;
	[SerializeField] private Transform _pointerMin = null;

	[Header("Sprites")]
	[SerializeField] private Sprite _carRightSprite;
	[SerializeField] private Sprite _carLeftSprite;

	[Header("Movement Times")]
	[SerializeField] private float _moveTime = 1f;
	[SerializeField] private float _rotationTime = 0.2f;

	[Header("Misc.")]
	[SerializeField] private LineRenderer _lineRenderer;
	[SerializeField] private LineRenderer _movingLineRenderer;
	[SerializeField] private SpriteRenderer _carSpriteRenderer;
	[SerializeField] private Transform _carTransform;

	private int _currentFuel;
	private Path _currentPath;

	private Vector2Int _currentMazeCell;
	private Vector2Int _currentCellTarget;
	private Vector3 _currentTargetPosition;
	private Vector3 _currentMazeCellPosition;
	
	private List<Quaternion> _fuelGaugeRots = new List<Quaternion>();

	private bool _fuelEnabled;
	private float _mazeScaler;

	public Vector2Int CurrentCellPosition => _currentMazeCell;
	public bool IsMoving => _currentPath._pathCells.Count > 0;
	public new Transform transform => _carTransform;
	// If you want specifically this gameObject's transform (e.g. you want it without rotation) use base.transform

	private InputController _inputController;
	private MazeController _mazeController;
	private TutorialController _tutorialController;

	private bool _movementComplete = false;

    // Start is called before the first frame update
    void Start()
    {
        _inputController = InputController.Instance;
		_mazeController = MazeController.Instance;
		_tutorialController = TutorialController.Instance;

		_fuelGaugeRots.Add(_pointerMin.rotation);
		for(int i = 0; i < _maximumFuel; ++i)
		{
			_fuelGaugeRots.Add(Quaternion.Lerp(_pointerMax.rotation, _pointerMin.rotation, ((float)_maximumFuel - (float)i) / (float)_maximumFuel));
		}
		_fuelGaugeRots.Add(_pointerMax.rotation);

		_mazeScaler = _mazeController.Scaler;
		_movingLineRenderer.positionCount = 2;
    }

	public void Initialise()
	{
		Reset();

		_inputController.SetActiveArrows(Direction.East);

		_fuelEnabled = _mazeController.GetFuelEnabled();
		_fuelGaugeObject.SetActive(_fuelEnabled);
	}

	private IEnumerator ProcessMovement()
	{
		if (_currentPath == null) yield break;

		// Get first cell.
		_currentCellTarget = _currentPath._pathCells.Dequeue();
		_currentTargetPosition = MazeController.MazeToWorldCoords(_currentCellTarget);
		
		_lineRenderer.SetPosition(_lineRenderer.positionCount - 1, _currentMazeCellPosition);
		_movingLineRenderer.SetPosition(0, _currentMazeCellPosition);
		_movingLineRenderer.SetPosition(1, _currentMazeCellPosition);

		float previousTime = Time.time;

		while (!_movementComplete)
		{
			if (_currentCellTarget.x < _currentMazeCell.x)
			{
				_carSpriteRenderer.sprite = _carLeftSprite;
			}
			else // Want right facing sprite for moving up and down.
			{
				_carSpriteRenderer.sprite = _carRightSprite;
			}

			float targetDot = Vector3.Dot(transform.forward, (_currentTargetPosition - transform.position).normalized);

			// Rotate towards target cell.
			if (targetDot < 0.5f)
			{
				_lineRenderer.SetPosition(_lineRenderer.positionCount - 1, _currentMazeCellPosition);
				_lineRenderer.positionCount++;
				_lineRenderer.SetPosition(_lineRenderer.positionCount - 1, _currentMazeCellPosition);
				_movingLineRenderer.SetPosition(0, _currentMazeCellPosition);

				Quaternion targetRotation = Quaternion.LookRotation(_currentTargetPosition - transform.position, Vector3.forward);
				Quaternion startRotation = transform.rotation;

				Vector3 spriteRotEuler = _carSpriteRenderer.transform.rotation.eulerAngles;

				float currentRotationTime = 0f;
				while (currentRotationTime <= _rotationTime)
				{
					if (!_tutorialController.ScreenOpen)
					{
						transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, currentRotationTime / _rotationTime);
						// Match the sprite's z rotation with the transform's x rotation (just rotation things)
						_carSpriteRenderer.transform.rotation = Quaternion.Euler(spriteRotEuler.x, spriteRotEuler.y, -transform.rotation.eulerAngles.x);
						
						currentRotationTime += Time.time - previousTime;
						previousTime = Time.time;
					}

					yield return null;
				}
				transform.rotation = targetRotation;
				_carSpriteRenderer.transform.rotation = Quaternion.Euler(spriteRotEuler.x, spriteRotEuler.y, -transform.rotation.eulerAngles.x);
			}

			// Move towards target cell.
			Vector3 startPosition = base.transform.position;
			float currentMoveTime = 0f;
			while (currentMoveTime <= _moveTime)
			{
				if (!_tutorialController.ScreenOpen)
				{
					base.transform.position = Vector3.Lerp(startPosition, _currentTargetPosition, currentMoveTime / _moveTime);

					if (_fuelEnabled)
					{
						_fuelGaugePointer.transform.rotation = Quaternion.Lerp
						(
							_fuelGaugeRots[_currentFuel],
							_fuelGaugeRots[_currentFuel - 1],
							currentMoveTime / _moveTime
						);
					}
					currentMoveTime += Time.time - previousTime;
					previousTime = Time.time;
					_movingLineRenderer.SetPosition(1, base.transform.position);
				}

				yield return null;
			}
			base.transform.position = _currentTargetPosition;

			if (_fuelEnabled)
			{
			}

			// Reached target cell.
			_currentMazeCell = _currentCellTarget;
			_currentMazeCellPosition = _currentTargetPosition;

			if (_fuelEnabled && _currentMazeCell.x < _mazeController.CurrentMazeDimensions.x && _currentMazeCell.y < _mazeController.CurrentMazeDimensions.y)
			{
				DecrementFuel();

				MazeCell currentMazeCell = _mazeController.GetCurrentMaze().cells2D[_currentMazeCell.x, _currentMazeCell.y];
				if (currentMazeCell._fuel && !currentMazeCell._fuelTaken)
				{
					ResetFuel();
					currentMazeCell._fuelCanObject.SetActive(false);
					currentMazeCell._fuelTaken = true;
				}
			}

			if (_fuelEnabled && _currentFuel < 1)
			{
				_mazeController.EndLevel(false);
				yield break;
			}

			if (_currentPath._pathCells.Count > 0)
			{
				_currentCellTarget = _currentPath._pathCells.Dequeue();
				_currentTargetPosition = MazeController.MazeToWorldCoords(_currentCellTarget);
			}
			else
			{
				_movementComplete = true;
				break;
			}
		}

		if (_currentPath._endAction != null)
		{
			_currentPath._endAction.Invoke();
		}
		else
		{
			_inputController.SetActiveArrows();
		}

		yield return null;
	}

	public void SetPath(Path path)
	{
		_currentPath = path;
		_movementComplete = false;

		if (_currentPath._pathCells.Count > 0)
		{
			StartCoroutine(ProcessMovement());
		}
	}

	public void Reset()
	{
		SetFuel(_startingFuel);

		// Set up the car for the start.
		_currentMazeCell = _mazeController.GetCurrentMazeStartLocation();
		_currentCellTarget = _currentMazeCell;

		_currentMazeCellPosition = MazeController.MazeToWorldCoords(_currentMazeCell);
		_currentTargetPosition = _currentMazeCellPosition;

		base.transform.position = _currentMazeCellPosition;
		base.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
		base.transform.localScale = new Vector3(1f, _mazeScaler, _mazeScaler);
		transform.rotation = Quaternion.identity;
		_carSpriteRenderer.transform.rotation = Quaternion.identity;

		_lineRenderer.SetPositions(new Vector3[]{_currentMazeCellPosition});
		_currentPath = null;
		_carSpriteRenderer.sprite = _carRightSprite;

		// Some misc setting up.
		_lineRenderer.positionCount = 1;
	}

	public void DecrementFuel()
	{
		SetFuel(_currentFuel - 1);
	}

	public void ResetFuel()
	{
		SetFuel(_startingFuel);
	}

	public void SetFuel(int fuel)
	{
		_currentFuel = fuel;
		_fuelGaugePointer.transform.rotation = _fuelGaugeRots[_currentFuel];
	}
}
