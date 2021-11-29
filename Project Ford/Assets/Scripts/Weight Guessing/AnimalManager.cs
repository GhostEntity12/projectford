using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AnimalManager : MonoBehaviour
{
	[SerializeField] private List<Animal> _possibleAnimals = new List<Animal>();

	[SerializeField] private List<Image> _animalImages = new List<Image>();

	[SerializeField] private TMP_InputField _inputText = null;

	[SerializeField] private Image _indicator = null;

	private List<Animal> _selectedAnimals = new List<Animal>();

	private int _weightSum = 0;

	void Awake()
	{
		// Seed Unity rng with the clock time.
		UnityEngine.Random.InitState(((int)System.DateTime.Now.Ticks));

		// Select 3 random shapes.
		for(int i = 0; i < 3; ++i)
		{
			int randomSelect = UnityEngine.Random.Range(0, 3);
			Animal randomShape = _possibleAnimals[randomSelect];

			_selectedAnimals.Add(randomShape);
			_animalImages[i].sprite = _selectedAnimals[i].GetSprite();
		}

		// Get the sum of the sides of the different shapes.
		foreach(Animal animal in _selectedAnimals)
			_weightSum += animal.GetWeight();
	}

	public void Submit()
	{
		int playerGuess = 0;

		// Parse the input as an integer, if it couldn't be passed display an error.
		if (!Int32.TryParse(_inputText.text, out playerGuess))
		{
			Debug.LogError("Input '" + _inputText.text + "' could not be passed as integer!");
			_indicator.color = Color.red;
			return;
		}

		// Input is an integer, check if it was right.
		if (playerGuess == _weightSum)
			_indicator.color = Color.green;
		else
			_indicator.color = Color.red;
	}
}