using UnityEngine;

public class MazeData : ScriptableObject
{
	public Texture2D map;
	public Vector2Int dimensions;
	public MazeCell[] cells;
	public Vector2Int startLocation;
	/// <summary>
	/// Returns the cell data in a 2D array form 
	/// </summary>
	public MazeCell[,] Cells2D
	{
		get
		{
			MazeCell[,] returnArray = new MazeCell[dimensions.x, dimensions.y];
			for (int i = 0; i < cells.Length; i++)
			{
				returnArray[i % dimensions.x, Mathf.FloorToInt(i / dimensions.x)] = cells[i];
				//for (int j = 0; j < dimensions.y; j++)
				//{
				//	try
				//	{
				//		returnArray[j, i] = cells[i * dimensions.x + j];
				//	}
				//	catch (System.Exception)
				//	{
				//		Debug.Log(dimensions);
				//		Debug.Log(new Vector2Int(j, i));
				//		Debug.Log(cells.Length);
				//		Debug.Log(i * dimensions.x + j);

				//		throw;
				//	}
				//}
			}
			return returnArray;
		}
	}
}
