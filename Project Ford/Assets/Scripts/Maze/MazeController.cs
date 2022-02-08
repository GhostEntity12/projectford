using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
	public GameObject carObject;
	public Transform carMesh;
	public GameObject[] arrows;
	public float moveSpeed = 1f;
	public float rotSpeed = 0.2f;
	public LineRenderer line;

	[Header("Maze Data")]
	public GameObject mazeObject;
	MazeData[] mazes;
	Material mazeMaterial;
	MazeData maze;

	[Header("Debug")]
	public Color[] colors;
	public Dictionary<string, Color> colorsDict;

	// Movement stuff

	/// <summary>
	/// Position doesn't denote the actual current position,
	/// it instead refers to the position the car will be in after moving
	/// Check currentDestination for that instead
	/// </summary>
	Vector2Int position = new Vector2Int(9, 20);
	Vector2 currentDestination;

	/// <summary>
	/// Previous position used to iterate through corridors.
	/// Similarly to position, does not store the actual cell the car was previously in
	/// </summary>
	Vector2Int previousPosition;
	readonly Queue<Vector2Int> path = new Queue<Vector2Int>();

	// Flow
	bool isMoving;
	bool isRotating;
	bool isComplete;

	void Start()
	{
		// Caching
		mazes = Resources.LoadAll<MazeData>("MapData");
		mazeMaterial = mazeObject.GetComponent<Renderer>().material;

		LoadMaze();

		colorsDict = new Dictionary<string, Color>
		{
			{ "waiting", colors[0] },
			{ "turning", colors[1] },
			{ "moving", colors[2] }
		};
	}

	private void Update()
	{
		// Handling the start when the car is outside of the maze
		if (position.y >= maze.dimensions.y)
			return;

		// Queue is empty and car is no longer moving, show arrows for tile
		if (path.Count == 0 && !isMoving && !isRotating)
		{
			//carMesh.GetComponent<Renderer>().material.color = Color.red;
			SetActiveArrows();
			line.Simplify(0.2f);

			if (isComplete)
			{
				LoadMaze();
			}
		}
		else
		{
			if (isRotating)
			{
				//carMesh.GetComponent<Renderer>().material.color = Color.green;
				Quaternion targetRotation = Quaternion.LookRotation(carMesh.position - (Vector3)currentDestination, Vector3.back);
				carMesh.rotation = Quaternion.RotateTowards(carMesh.rotation, targetRotation, rotSpeed);
				if (carMesh.rotation == targetRotation)
				{
					isMoving = true;
					isRotating = false;
				}
			}
			else
			{
				//carMesh.GetComponent<Renderer>().material.color = Color.blue;
				// Set the next new destination
				if (!isMoving)
				{
					Vector2Int cell = path.Dequeue();
					currentDestination = MazeCoordstoWorldCoords(cell);
					isRotating = true;
					line.SetPosition(line.positionCount++ - 1, carObject.transform.position);
					line.SetPosition(line.positionCount - 1, carObject.transform.position);
				}
				// Move the car
				else
				{
					line.SetPosition(line.positionCount - 1, carObject.transform.position);
					carObject.transform.position = Vector2.MoveTowards(carObject.transform.position, currentDestination, moveSpeed * Time.deltaTime);
					if ((Vector2)carObject.transform.position == currentDestination)
					{
						isMoving = false;
					}
				}
			}
		}
	}

	/// <summary>
	/// Loads a random maze
	/// </summary>
	void LoadMaze()
	{
		int rand = Random.Range(0, mazes.Length);
		maze = mazes[rand];
		carObject.transform.position = MazeCoordstoWorldCoords(maze.startLocation);
		line.SetPosition(0, carObject.transform.position);
		position = maze.startLocation;
		mazeMaterial.mainTexture = maze.map;
		mazeObject.transform.localScale = new Vector3(maze.dimensions.x, 1, maze.dimensions.y);
		Camera.main.transform.position = new Vector3(maze.dimensions.x / 4f, maze.dimensions.y / 4f, -10);
		line.positionCount = 1;
		SetActiveArrows(Direction.South);
	}

	public void OnArrowClick(int index)
	{
		Vector2Int newPosition = position + cardinals[index];

		if (newPosition.y < maze.dimensions.x)
		{
			SetPath(newPosition);
			SetActiveArrows(0);
		}
	}

	void SetPath(Vector2Int nextTile)
	{
		previousPosition = position;
		position = nextTile;
		path.Enqueue(nextTile);


		// Query based on open passages
		if (position.y < 0)
		{
			// Map complete
			isComplete = true;
			return;
		}
		// Stuff below here needs to happen after the car finishes moving
		MazeCell cell = maze.Cells2D[position.x, position.y];

		/* Comment out from here to the end of the function if you want the player to click every cell */
		// If there's two walls and two passages
		if (queryStates[(int)cell.walls])
		{
			for (int i = 0; i < 4; i++)
			{
				// If it's a wall, skip
				if (cell.walls.HasFlag((Direction)Mathf.Pow(2, i))) continue;

				// If it's not the previous tile, move there
				Vector2Int _position = position + cardinals[i];
				if (_position != previousPosition)
				{
					// Recurse until a dead end or junction
					SetPath(_position);
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
			direction = (Direction)((int)~maze.Cells2D[position.x, position.y].walls & 15);
		}
		catch (System.IndexOutOfRangeException)
		{
			// No arrows if out of bounds
			direction = 0;
		}

		for (int i = 0; i < arrows.Length; i++)
		{
			arrows[i].SetActive(direction.HasFlag((Direction)Mathf.Pow(2, i)));
		}
		// Handling entry arrow
		if (position.y == 19)
		{
			arrows[0].SetActive(false);
		}
	}

	public void SetActiveArrows(Direction direction)
	{
		for (int i = 0; i < arrows.Length; i++)
		{
			arrows[i].SetActive(direction.HasFlag((Direction)Mathf.Pow(2, i)));
		}
	}

	public static Vector2 MazeCoordstoWorldCoords(Vector2 mazeCoords) => new Vector2(mazeCoords.x * 0.5f + 0.25f, mazeCoords.y * 0.5f + 0.25f);
}
