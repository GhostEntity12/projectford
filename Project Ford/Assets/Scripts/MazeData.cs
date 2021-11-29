using UnityEngine;

public class MazeData : ScriptableObject
{
#if UNITY_EDITOR
	public Texture2D map;
#endif
	public Vector2Int dimensions;
	public MazeCell[] cells;
	/// <summary>
	/// Returns the cell data in a 2D array form 
	/// </summary>
	public MazeCell[,] Cells2D
	{
		get
		{
			MazeCell[,] returnArray = new MazeCell[dimensions.x, dimensions.y];
			for (int i = 0; i < dimensions.x; i++)
			{
				for (int j = 0; j < dimensions.y; j++)
				{
					returnArray[i, j] = cells[i * dimensions.x + j];
				}
			}
			return returnArray;
		}
	}
}
