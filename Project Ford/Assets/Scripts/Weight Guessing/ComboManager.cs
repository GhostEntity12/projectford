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
	[SerializeField] private List<Image> _comboCounterImages = new List<Image>();

	/// <summary>
	/// The asset of the sky that can change when the player gets 3 right answers.
	/// </summary>
	[SerializeField] private Image _comboSkyImage;

	/// <summary>
	/// List of images the sky can change to.
	/// </summary>
	[SerializeField] private List<Sprite> _skyImages = new List<Sprite>();

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

	/// <summary>
	/// On startup.
	/// </summary>
	void Awake()
	{
		_instance = this;

		foreach(Image img in _comboCounterImages)
		{
			img.color = Color.white;
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
			_comboCounterImages[i].color = Color.green;
		}

		// When the player reaches 3 consecutive right answers, do something.
		if (_answerCombo >= 3)
		{
			_comboSkyImage.sprite = _skyImages[1];

			switch(_comboStep)
			{
				case ComboStep.OneVariety:
				Debug.Log("Alright.");
				_amInstance.IncreaseAnimalVariety();
				_comboStep++;
				break;

				case ComboStep.TwoVariety:
				Debug.Log("Epic!");
				_amInstance.IncreaseAnimalVariety();
				_comboStep++;
				break;

				case ComboStep.ThreeAmount:
				Debug.Log("SUPER SEXY STYLE!");
				Debug.Log("TODO: Increase the amount of animals spawned!");
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

		foreach(Image img in _comboCounterImages)
		{
			img.color = Color.white;
		}
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