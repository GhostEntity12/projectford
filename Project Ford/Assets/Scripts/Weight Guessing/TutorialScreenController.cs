using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialScreenController : MonoBehaviour
{
	[Header("Tutorial Screens")]
	[SerializeField] private TutorialScreen _animalScreen;
	[SerializeField] private TutorialScreen _inputScreen;
	[SerializeField] private TutorialScreen _endlessScreen;

	[Header("Misc")]
	[SerializeField] private string _lastScreenText = "Begin";
	[SerializeField] private Image _backgroundImage;

	private Queue<TutorialScreen> _tutorialQueue = new Queue<TutorialScreen>();
	private bool _starting = true;
	private TutorialScreen _currentScreen;

	private static TutorialScreenController _instance;
	public static TutorialScreenController Instance => _instance;

	public bool ScreenOpen => _currentScreen != null;

	void Awake()
	{
		if (TutorialScreenController.Instance == null)
		{
			_instance = this;
		}
		else 
		{
			Destroy(gameObject);
		}
	}

	void Start()
	{
		_starting = true;
		_backgroundImage.enabled = false;
	}

	public void InitialiseTutorial()
	{
		if (_starting)
		{
			_tutorialQueue.Enqueue(_animalScreen);
			_tutorialQueue.Enqueue(_inputScreen);
			_starting = false;
		}

		if (ComboManager.GetInstance().EndlessMode)
		{
			_tutorialQueue.Enqueue(_endlessScreen);
		}

		_backgroundImage.enabled = _tutorialQueue.Count > 0;

		DisplayNextScreen();
	}

	public void DisplayNextScreen()
	{
		if (_currentScreen != null)
		{
			Destroy(_currentScreen.gameObject);
		}

		_currentScreen = Instantiate(_tutorialQueue.Dequeue(), transform);

		if (_tutorialQueue.Count > 0)
		{
			_currentScreen.SetNextButtonAction(() => { DisplayNextScreen(); });
		}
		else
		{
			_currentScreen.SetNextButtonText(_lastScreenText);
			_currentScreen.SetNextButtonAction(() => ExitTutorial());
		}
	}

	public void HelpButtonPressed()
	{
		_starting = true;

		InitialiseTutorial();
	}

	public void ExitTutorial()
	{
		Destroy(_currentScreen.gameObject);
		_backgroundImage.enabled = false;
	}
}
