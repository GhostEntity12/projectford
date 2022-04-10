using System;
using UnityEditor;
using UnityEngine;

[Flags]
public enum Direction
{
	North = 1,
	East = 2,
	South = 4,
	West = 8
}

public class MazeReadWriter
{
#if UNITY_EDITOR

	/// <summary>
	/// Generates ScriptableObject assets for map files
	/// </summary>
	[MenuItem("Mazes/Generate Maze Data")]
	static void CreateMaze()
	{
		// Maze dimension information
		// By default, cells are 14x14px internal with 2px border
		// Change here if neccessary
		int borderWidth = 2;
		int internalSize = 14;

		int largeOffset = 14;
		int smallOffset = 1;

		Texture2D[] maps = Resources.LoadAll<Texture2D>("Maze/MapSources");
		foreach (Texture2D map in maps)
		{
			Vector2Int cellCount = new Vector2Int((map.width - borderWidth) / (borderWidth + internalSize), (map.height - 2) / (borderWidth + internalSize));
			MazeCell[,] cells = new MazeCell[cellCount.x, cellCount.y];

			// Cell (0, 0) is bottom left
			// Iterate through each cell of the maze and check for walls
			// Y direction
			for (int i = 0; i < cellCount.y; i++)
			{
				// X direction
				for (int j = 0; j < cellCount.x; j++)
				{
					MazeCell cell = new MazeCell();
					int xCoord = j * (borderWidth + internalSize) + borderWidth;
					int yCoord = i * (borderWidth + internalSize) + borderWidth;

					// Maybe come back to this and fine tune?
					if (map.GetPixel(xCoord + (largeOffset / 2), yCoord + (largeOffset / 2)).r == 1)
					{
						cell._fuel = true;
						cell._fuelTaken = false;
					}
					// Iterating through the edges
					for (int edge = 0; edge < 4; edge++)
					{
						Vector2Int cellCoords = edge switch
						{
							0 => new Vector2Int(xCoord, yCoord + largeOffset),
							1 => new Vector2Int(xCoord + largeOffset, yCoord),
							2 => new Vector2Int(xCoord, yCoord - smallOffset),
							3 => new Vector2Int(xCoord - smallOffset, yCoord),
							_ => throw new IndexOutOfRangeException("Something went very wrong... (Generating map data)"),
						};
						// Set flag if wall was found
						if (map.GetPixel(cellCoords.x, cellCoords.y) == Color.black)
						{
							cell.walls |= (Direction)Mathf.Pow(2, edge);
						}
					}

					// Save to location in 2D array
					cells[j, i] = cell;

					// Set cell position.
					cell._position = (Vector2Int.right * j) + (Vector2Int.up * i);
				}
			}

			MazeData maze = ScriptableObject.CreateInstance<MazeData>();
			maze.map = map;
			maze.dimensions = cellCount;

			// Need to crush this down to a 1D array, because of Unity serialization :rolling_eyes:
			// Use MazeData.Cells2D to get back a usable 2D array
			MazeCell[] squashedArray = new MazeCell[cellCount.x * cellCount.y];
			Debug.Log(cellCount);
			Debug.Log(cells.Length);
			int index = 0;
			for (int i = 0; i < cellCount.y; i++)
			{
				for (int j = 0; j < cellCount.x; j++)
				{
					//Debug.Log(index);
					//Debug.Log(j + "/" + cellCount.y);
					squashedArray[index] = cells[j, i];
					//Debug.Log(squashedArray[index].walls);
					index++;
				}
			}
			maze.cells = squashedArray;

			// Check for start of maze by finding a gap along the west wall of the map.
			for (int i = 0; i < cellCount.y; i++)
			{
				if (!maze.cells2D[maze.dimensions.x - 1, i].walls.HasFlag(Direction.West))
				{
					maze.startLocation = new Vector2Int(i, maze.dimensions.y);
					break;
				}
			}

			// Create and save the assets
			AssetDatabase.CreateAsset(maze, $"Assets/Resources/Maze/MapData/{maze.map.name}.asset");
			AssetDatabase.SaveAssets();
		}
	}
#endif

	// Kept this in as a backup, use the Cells2D property on the MazeData instead
	//public static MazeCell[,] GetCells(MazeData mazeData)
	//{
	//	MazeCell[,] cells = new MazeCell[mazeData.dimensions.x, mazeData.dimensions.y];
	//	for (int i = 0; i < mazeData.dimensions.x; i++)
	//	{
	//		for (int j = 0; j < mazeData.dimensions.y; j++)
	//		{
	//			cells[i, j] = mazeData.cells[i * mazeData.dimensions.x + j];
	//		}
	//	}
	//	return cells;
	//}
}

[Serializable]
public class MazeCell
{
	public Direction walls;
	public bool _fuel;
	public bool _fuelTaken;
	public Vector2 _position;
	public GameObject _fuelCanObject;
}