using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DifficultyEnum = DifficultyManager.DifficultyEnum;

public class ComboManager : MonoBehaviour
{
	public enum ComboStep
	{
		OneVariety,
		TwoVariety,
		ThreeAmount,
		Count
	}

	[Header("Skybox")]
	/// <summary>
	/// List of images the sky can change to.
	/// </summary>
	[SerializeField] private List<Material> _skyboxMaterials = new List<Material>();

	[Header("Car")]
	[SerializeField] private SpriteRenderer _carRenderer = null;

	[SerializeField] private List<Sprite> _carSprites = new List<Sprite>();

	[Header("Misc.")]
	[SerializeField] private GameObject _finishScreen = null;

	[SerializeField] private StreakCounterManager _counterManager = null;

	[SerializeField] private GameObject _difficultyFinishScreen;

	/// <summary>
	/// Instance of the combo manager.
	/// </summary>
	static private ComboManager _instance;

	/// <summary>
	/// Counter for consecutive correct answer the player gets.
	/// </summary>
	private int _answerCombo = 0;

	/// <summary>
	/// The current step through the combos.
	/// </summary>
	private ComboStep _comboStep = 0;

	/// <summary>
	/// The Animal Manager.
	/// </summary>
	private AnimalManager _amInstance = null;

	/// <summary>
	/// The current skybox material.
	/// </summary>
	private int _currentSkyboxMaterial = 0;

	/// <summary>
	/// The amount of questions streaks to 'finish' the game.
	/// </summary>
	private int _finishQuestionsAmount = 0;

	// Endless mode, don't check for end.
	private bool _endlessMode = false;

	private ProgressManager _progInstance = null;

	private int _currentQuestionCount = 0;

	public bool EndlessMode => _endlessMode;

	/// <summary>
	/// On startup.
	/// </summary>
	void Awake()
	{
		_instance = this;

		// Make sure the finish screen is off when starting the game.
		if (_finishScreen.activeSelf)
			_finishScreen.SetActive(false);
	}

	void Start()
	{
		_amInstance = AnimalManager.GetInstance();
		_progInstance = ProgressManager.GetInstance();
	}

	/// <summary>
	/// Increment combo counter by one and check if it has reached the target.
	/// If it does, it will change something.
	/// </summary>
	public void IncrementComboCounter()
	{
		_answerCombo++;

		_progInstance.UpdateProgressBar(++_currentQuestionCount);

		// When the player reaches 3 consecutive right answers.
		if (_answerCombo >= 3)
		{
			// Assign new car sprite.
			Sprite newCarSprite = _carSprites[Random.Range(0, _carSprites.Count)];
			while (newCarSprite == _carRenderer.sprite)
				newCarSprite = _carSprites[Random.Range(0, _carSprites.Count)];
			_carRenderer.sprite = newCarSprite;

			// Iterate through the skybox materials.
			if (_currentSkyboxMaterial >= _skyboxMaterials.Count - 1)
				_currentSkyboxMaterial = -1;
			RenderSettings.skybox = _skyboxMaterials[++_currentSkyboxMaterial];

			// The combos step through these increases to difficulty in this order.
			switch(_comboStep)
			{
				case ComboStep.OneVariety:
					_amInstance.IncreaseAnimalVariety();
					_comboStep++;
				break;

				case ComboStep.TwoVariety:
					_amInstance.IncreaseAnimalVariety();
					_comboStep++;
				break;

				case ComboStep.ThreeAmount:
					_amInstance.IncreaseAnimalSpawnAmount();
					_comboStep = 0;
				break;

				default:
					Debug.LogError("Error in combo steps! Combo step is " + _comboStep, this);
				break;
			}

			ResetComboCounter();
		}

		// Only do this if not in endless mode.
		if (!_endlessMode)
		{
			if (_currentQuestionCount >= _finishQuestionsAmount)
			{
				DifficultyManager dmInstance = DifficultyManager.GetInstance();
				if (dmInstance == null) return;

				int currentDifficultyInt = (int)dmInstance.GetDifficulty();

				if (currentDifficultyInt < (int)DifficultyEnum.Count - 1)
				{
					_difficultyFinishScreen.SetActive(true);
				}
				else
				{
					_finishScreen.SetActive(true);
				}
				return;
			}
		}
		// Only increment streak counter when in endless mode.
		else
			_counterManager.IncrementStreakCount();
	}

	/// <summary>
	/// Reset the combo counter to 0.
	/// </summary>
	/// <param name="correct">If the reason was for a correct answer, defaults to true.</param>
	public void ResetComboCounter(bool correct = true)
	{
		_answerCombo = 0;

		// Only reset streak counter if incorrect answer and in endless mode.
		if (!correct && _endlessMode)
			_counterManager.ResetStreakCount();
	}

	public void SetComboCount(int newCount)
	{
		_answerCombo = newCount;
	}

	public void SetFinishQuestionAmount(int finishAmount)
	{
		_finishQuestionsAmount = finishAmount;
		
		_progInstance.SetMaxQuestionCount(_finishQuestionsAmount);
	}

	public void SetEndlessMode(bool endless)
	{
		_endlessMode = endless;

		if (!_endlessMode)
		{
			_counterManager.gameObject.SetActive(false);
			_progInstance.gameObject.SetActive(true);
		}
		else
		{
			_counterManager.gameObject.SetActive(true);
			_progInstance.gameObject.SetActive(false);
		}
	}

	public void ResetCurrentQuestionCount()
	{
		_currentQuestionCount = 0;
	}

	public void IncrementDifficulty()
	{
		DifficultyManager dmInstance = DifficultyManager.GetInstance();
		if (dmInstance == null) return;
		
		dmInstance.SetDifficulty((int)dmInstance.GetDifficulty() + 1);

		AnimalManager.GetInstance()?.GetDifficultyFromManagerNoTutorial();
		ProgressManager.GetInstance()?.ResetProgressBar();
		ComboManager.GetInstance()?.ResetCurrentQuestionCount();
	}

	/// <summary>
	/// Get the combo counter.
	/// </summary>
	/// <returns>The instance of the combo counter.</returns>
	static public ComboManager GetInstance()
	{
		return _instance;
	}
}
