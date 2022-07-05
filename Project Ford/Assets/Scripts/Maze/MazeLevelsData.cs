using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Mazes/Maze Level Data")]
public class MazeLevelsData : ScriptableObject
{
	[SerializeField] private List<MazeData> _mazes = new List<MazeData>();

	[SerializeField] private List<MazeDecor> _mazeDecor = new List<MazeDecor>();

	public List<MazeData> GetMazes()
	{
		return _mazes;
	}

	public List<MazeDecor> GetMazeDecor()
	{
		return _mazeDecor;
	}
}
