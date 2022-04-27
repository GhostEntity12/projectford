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

	public struct DifficultySettings
	{
		public DifficultySettings(int difAnimals, int finishAmount)
		{
			_difAnimals = difAnimals;
			_finishQuestionsAmount = finishAmount;
		}

		public int _difAnimals;
		public int _finishQuestionsAmount;
	}

	[Header("Easy")]
	[SerializeField] private int _easyDifAnimals = 0;
	[SerializeField] private int _easyFinishQuestionsAmount = 0;

	[Header("Medium")]
	[SerializeField] private int _mediumDifAnimals = 0;
	[SerializeField] private int _mediumFinishQuestionsAmount = 0;

	[Header("Hard")]
	[SerializeField] private int _hardDifAnimals = 0;
	[SerializeField] private int _hardFinishQuestionsAmount = 0;

	private static DifficultyManager _instance;

	private DifficultyEnum _currentDifficulty = DifficultyEnum.None;

	void Awake()
	{
		_instance = this;
	}

	public DifficultySettings GetEasyDifficultySettings()
	{
		return new DifficultySettings(_easyDifAnimals, _easyFinishQuestionsAmount);
	}

	public DifficultySettings GetMediumDifficultySettings()
	{
		return new DifficultySettings(_mediumDifAnimals, _mediumFinishQuestionsAmount);
	}

	public DifficultySettings GetHardDifficultySettings()
	{
		return new DifficultySettings(_hardDifAnimals, _hardFinishQuestionsAmount);
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