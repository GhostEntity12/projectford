using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class TutorialScreen : MonoBehaviour
{
	[SerializeField] private Button _nextButton;
	[SerializeField] private Text _buttonText;

	public void SetNextButtonText(string text)
	{
		_buttonText.text = text;
	}

	public void SetNextButtonAction(UnityAction action)
	{
		_nextButton.onClick.RemoveAllListeners();
		_nextButton.onClick.AddListener(action);
	}
}
