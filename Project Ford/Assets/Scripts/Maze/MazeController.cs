using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeController : MonoBehaviour
{
	public GameObject carObject;
	public GameObject mazeObject;
	MazeData[] mazes;
	Material mazeMaterial;
	MazeData maze;
	Vector2Int position = new Vector2Int(9, 20);
	Vector2Int previousPosition;
	bool[] queryStates = new bool[16]
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

	Vector2Int[] cardinals = new Vector2Int[4]
	{
		new Vector2Int(0, 1),
		new Vector2Int(1, 0),
		new Vector2Int(0, -1),
		new Vector2Int(-1, 0)
	};

	public GameObject[] arrows;

	// Start is called before the first frame update
	void Start()
	{
		mazes = Resources.LoadAll<MazeData>("MapData");
		mazeMaterial = mazeObject.GetComponent<Renderer>().material;
		LoadMaze();
	}

	void LoadMaze()
	{
		// Loads a random maze
		maze = mazes[Random.Range(0, mazes.Length)];
		mazeMaterial.mainTexture = maze.map;
		SetActiveArrows(Direction.South);
	}

	public void OnArrowClick(int index)
	{
		Vector2Int newPosition = position + cardinals[index];

		if (newPosition.y < maze.dimensions.x)
		{   

			MoveTile(newPosition);
			SetActiveArrows();
		}
	}

	void MoveTile(Vector2Int newTile)
	{
		previousPosition = position;
		position = newTile;

		// This line is temp, car needs to be animated moving through the maze
		carObject.transform.position = new Vector2(newTile.x * 0.5f + 0.25f, newTile.y * 0.5f + 0.25f);

		// Stuff below here needs to happen after the car finishes moving
		MazeCell cell = maze.Cells2D[position.x, position.y];
		// Query based on open passages
		if (position.y < 0)
		{
			// Map complete
			return;
		}
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
					MoveTile(_position);
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

	public int CountOnBits(int x)
	{
		int count = 0;
		while (x != 0)
		{
			if ((x & 1) != 0) count++;
			x >>= 1;
		}
		return count;
	}

}
