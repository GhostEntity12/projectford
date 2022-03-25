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
			_finishQuestionStreakAmount = finishAmount;
		}

		public int _difAnimals;
		public int _finishQuestionStreakAmount;
	}

	[Header("Easy")]
	[SerializeField] private int _easyDifAnimals = 0;
	[SerializeField] private int _easyFinishQuestionStreakAmount = 0;

	[Header("Medium")]
	[SerializeField] private int _mediumDifAnimals = 0;
	[SerializeField] private int _mediumFinishQuestionStreakAmount = 0;

	[Header("Hard")]
	[SerializeField] private int _hardDifAnimals = 0;
	[SerializeField] private int _hardFinishQuestionStreakAmount = 0;

	private static DifficultyManager _instance;

	private DifficultyEnum _currentDifficulty = DifficultyEnum.None;

	void Awake()
	{
		_instance = this;
	}

	public DifficultySettings GetEasyDifficultySettings()
	{
		return new DifficultySettings(_easyDifAnimals, _easyFinishQuestionStreakAmount);
	}

	public DifficultySettings GetMediumDifficultySettings()
	{
		return new DifficultySettings(_mediumDifAnimals, _mediumFinishQuestionStreakAmount);
	}

	public DifficultySettings GetHardDifficultySettings()
	{
		return new DifficultySettings(_hardDifAnimals, _hardFinishQuestionStreakAmount);
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