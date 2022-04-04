using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ComboManager : MonoBehaviour
{
	public enum ComboStep
	{
		OneVariety,
		TwoVariety,
		ThreeAmount,
		Count
	}

	// /// <summary>
	// /// Images of the combo counter.
	// /// </summary>
	// [SerializeField] private List<GameObject> _comboCounterObjects = new List<GameObject>();

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
	/// The current question streak amount. For progression.
	/// </summary>
	private int _currentQuestionStreakAmount = 0;

	/// <summary>
	/// The amount of questions streaks to 'finish' the game.
	/// </summary>
	private int _finishQuestionStreakAmount = 0;

	// Endless mode, don't check for end.
	private bool _endlessMode = false;

	/// <summary>
	/// On startup.
	/// </summary>
	void Awake()
	{
		_instance = this;

		// foreach(GameObject counter in _comboCounterObjects)
		// {
		// 	counter.SetActive(false);
		// }

		// Make sure the finish screen is off when starting the game.
		if (_finishScreen.activeSelf)
			_finishScreen.SetActive(false);
	}

	void Start()
	{
		_amInstance = AnimalManager.GetInstance();
	}

	/// <summary>
	/// Increment combo counter by one and check if it has reached the target.
	/// If it does, it will change something.
	/// </summary>
	public void IncrementComboCounter()
	{
		_answerCombo++;

		// for(int i = 0; i < _answerCombo; ++i)
		// {
		// 	_comboCounterObjects[i].SetActive(true);
		// }

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

					// Only do this if not in endless mode.
					if (!_endlessMode)
					{
						if (++_currentQuestionStreakAmount >= _finishQuestionStreakAmount)
						{
							Debug.Log("Questions finished!");
							_finishScreen.SetActive(true);
							return;
						}
					}

					_comboStep = 0;
				break;

				default:
					Debug.LogError("Error in combo steps! Combo step is " + _comboStep, this);
				break;
			}

			ResetComboCounter();
		}

		// Only increment streak counter when in endless mode.
		if (_endlessMode)
			_counterManager.IncrementStreakCount();
	}

	/// <summary>
	/// Reset the combo counter to 0.
	/// </summary>
	/// <param name="correct">If the reason was for a correct answer, defaults to true.</param>
	public void ResetComboCounter(bool correct = true)
	{
		_answerCombo = 0;

		// foreach(GameObject counter in _comboCounterObjects)
		// {
		// 	counter.SetActive(false);
		// }

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
		_finishQuestionStreakAmount = finishAmount;
	}

	public void SetEndlessMode(bool endless)
	{
		_endlessMode = endless;

		if (!_endlessMode)
			_counterManager.gameObject.SetActive(false);
		else
			_counterManager.gameObject.SetActive(true);
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