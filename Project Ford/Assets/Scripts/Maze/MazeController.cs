using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeController : MonoBehaviour
{
	[Header("Car Data")]
	public GameObject carObject;
	public GameObject[] arrows;
	public float moveSpeed = 1f;

	[Header("Maze Data")]
	public GameObject mazeObject;
	MazeData[] mazes;
	Material mazeMaterial;
	MazeData maze;

	[Header("Other")]
	public Camera camera;

	// Movement stuff
	// Position doesn't denote the actual current position,
	// it instead refers to the position the car will be in after moving
	// Check currentDestination for that instead
	Vector2Int position = new Vector2Int(9, 20);
	// Previous position used to iterate through corridors.
	// Similarly to position, does not store the actual cell the car was previously in
	Vector2Int previousPosition;
	Queue<Vector2Int> path = new Queue<Vector2Int>();
	Vector2 currentDestination;
	bool isMoving;

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

	readonly Vector2Int[] cardinals = new Vector2Int[4]
	{
		new Vector2Int(0, 1),
		new Vector2Int(1, 0),
		new Vector2Int(0, -1),
		new Vector2Int(-1, 0)
	};

	// Start is called before the first frame update
	void Start()
	{
		mazes = Resources.LoadAll<MazeData>("MapData");
		mazeMaterial = mazeObject.GetComponent<Renderer>().material;
		LoadMaze();
	}

	private void Update()
	{
		// Handling the start when the car is outside of the maze
		if (position.y >= maze.dimensions.y)
			return;

		// Queue is empty and car is no longer moving, show arrows for tile
		if (path.Count == 0 && !isMoving)
		{
			SetActiveArrows();
		}
		else
		{
			// Set the next new destination
			if (!isMoving)
			{
				Vector2Int cell = path.Dequeue();
				currentDestination = MazeCoordstoWorldCoords(cell);
				isMoving = true;
			}
			// Move the car
			else
			{
				// Put rotation code here?
				carObject.transform.position = Vector2.MoveTowards(carObject.transform.position, currentDestination, moveSpeed * Time.deltaTime);
				if ((Vector2)carObject.transform.position == currentDestination)
				{
					isMoving = false;
				}
			}
		}
	}

	void LoadMaze()
	{
		// Loads a random maze
		int rand = Random.Range(0, mazes.Length);
		maze = mazes[rand];
		carObject.transform.position = MazeCoordstoWorldCoords(maze.startLocation);
		position = maze.startLocation;
		mazeMaterial.mainTexture = maze.map;
		mazeObject.transform.localScale = new Vector3(maze.dimensions.x, 1, maze.dimensions.y);
		camera.transform.position = new Vector3(maze.dimensions.x / 4, maze.dimensions.y / 4, -10);
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

		// This line is temp, car needs to be animated moving through the maze
		//carObject.transform.position = new Vector2(nextTile.x * 0.5f + 0.25f, nextTile.y * 0.5f + 0.25f);

		// Query based on open passages
		if (position.y < 0)
		{
			// Map complete
			return;
		}
		// Stuff below here needs to happen after the car finishes moving
		MazeCell cell = maze.Cells2D[position.x, position.y];

		// Comment out from here to the end of the function if you want the player to click every cell
		// May need to rethink structure?
		// Maybe have a stack of the cell positions the car needs to move through stored and use Update()?
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

	public Vector2 MazeCoordstoWorldCoords(Vector2 mazeCoords) => new Vector2(mazeCoords.x * 0.5f + 0.25f, mazeCoords.y * 0.5f + 0.25f);
}
