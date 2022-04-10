using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Mazes/Maze Level Data")]
public class MazeLevelsData : ScriptableObject
{
	[SerializeField] private List<MazeData> _mazes = new List<MazeData>();

	[SerializeField] private List<GameObject> _mazeDecor = new List<GameObject>();

	public List<MazeData> GetMazes()
	{
		return _mazes;
	}

	public List<GameObject> GetMazeDecor()
	{
		return _mazeDecor;
	}
}
