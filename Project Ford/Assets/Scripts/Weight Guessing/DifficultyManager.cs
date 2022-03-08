using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
	public enum DifficultyEnum
	{
		Easy,
		Medium,
		Hard,
		Count,
		None
	}

	[SerializeField] private int _easyDifAnimals = 0;
	[SerializeField] private int _mediumDifAnimals = 0;
	[SerializeField] private int _hardDifAnimals = 0;

	private static DifficultyManager _instance;

	private DifficultyEnum _currentDifficulty = DifficultyEnum.None;

	void Awake()
	{
		_instance = this;
	}

	public int GetEasyDifficultyAnimals()
	{
		return _easyDifAnimals;
	}

	public int GetMediumDifficultyAnimals()
	{
		return _mediumDifAnimals;
	}

	public int GetHardDifficultyAnimals()
	{
		return _hardDifAnimals;
	}

	public void SetDifficulty(int selectedDifficulty)
	{
		_currentDifficulty = (DifficultyEnum)selectedDifficulty;
	}

	public DifficultyEnum GetDifficulty()
	{
		return _currentDifficulty;
	}

	public static DifficultyManager GetInstance()
	{
		return _instance;
	}
}
