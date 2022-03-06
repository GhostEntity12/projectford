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

	/// <summary>
	/// Images of the combo counter.
	/// </summary>
	[SerializeField] private List<GameObject> _comboCounterObjects = new List<GameObject>();

	[Header("Skybox")]
	/// <summary>
	/// List of images the sky can change to.
	/// </summary>
	[SerializeField] private List<Material> _skyboxMaterials = new List<Material>();

	[Header("Car")]
	[SerializeField] private SpriteRenderer _carRenderer = null;

	[SerializeField] private List<Sprite> _carSprites = new List<Sprite>();

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

	private AnimalManager _amInstance = null;

	private int _currentSkyboxMaterial = 0;

	/// <summary>
	/// On startup.
	/// </summary>
	void Awake()
	{
		_instance = this;

		foreach(GameObject counter in _comboCounterObjects)
		{
			counter.SetActive(false);
		}
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

		for(int i = 0; i < _answerCombo; ++i)
		{
			_comboCounterObjects[i].SetActive(true);
		}

		// When the player reaches 3 consecutive right answers, do something.
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
	}

	/// <summary>
	/// Reset the combo counter to 0.
	/// </summary>
	public void ResetComboCounter()
	{
		_answerCombo = 0;

		foreach(GameObject counter in _comboCounterObjects)
		{
			counter.SetActive(false);
		}
	}

	public void SetComboCount(int newCount)
	{
		_answerCombo = newCount;
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