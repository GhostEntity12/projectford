using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class TutorialScreenEntity : MonoBehaviour
{
	[SerializeField] private Button _nextButton;
	[SerializeField] private Text _buttonText;

	public void SetNextButtonAction(UnityAction action)
	{
		_nextButton.onClick.RemoveAllListeners();

		_nextButton.onClick.AddListener(action);
	}

	public void SetNextButtonText(string text)
	{
		_buttonText.text = text;
	}
}
