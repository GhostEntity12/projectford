using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InputManager : MonoBehaviour
{
	/// <summary>
	/// TMPro input field.
	/// </summary>
	[SerializeField] private TMP_InputField _inputText = null;

	private KeyCode[] _numKeyCodes = new KeyCode[] {KeyCode.Alpha0, KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9};

	void Update()
	{
		for (int i = 0; i < _numKeyCodes.Length; ++i)
		{
			if (Input.GetKey(_numKeyCodes[i]))
			{
				AddNumber(i);
			}
		}
	}

	public void Submit()
	{
		int playerGuess = 0;
		// Parse the input as an integer, if it couldn't be parsed: display an error.
		if (!Int32.TryParse(_inputText.text, out playerGuess))
		{
			Debug.LogError($"Input '{_inputText.text}' could not be passed as integer!");
			return;
		}

		// Submit the player's input, if it was correct reset the text input.
		if (AnimalManager.GetInstance().Submit(playerGuess))
			_inputText.text = "";
	}

	public void AddNumber(int number)
	{
		_inputText.text += number.ToString();
	}

	public void Delete()
	{
		string keep = "";

		for(int i = 0; i < _inputText.text.Length - 1; ++i)
		{
			keep += _inputText.text[i];
		}

		_inputText.text = keep;
	}
}
