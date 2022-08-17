using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialController : MonoBehaviour
{
	[SerializeField] private TutorialScreenEntity _basicTutorial;
	[SerializeField] private TutorialScreenEntity _fuelTutorial;
	[SerializeField] private string _lastScreenText = "Begin";

	private Queue<TutorialScreenEntity> _tutorialQueue = new Queue<TutorialScreenEntity>();
	private bool _starting = true;
	private TutorialScreenEntity _currentScreen;

	private static TutorialController _instance;
	public static TutorialController Instance => _instance;

	public bool ScreenOpen => _currentScreen != null;

	void Awake()
	{
		if (TutorialController.Instance == null)
			_instance = this;
		else 
			Destroy(gameObject);
	}

	void Start()
	{
		_starting = true;
	}

	public void InitialiseTutorial()
	{
		if (_starting)
		{
			_tutorialQueue.Enqueue(_basicTutorial);
			_starting = false;
		}

		if (MazeController.Instance.GetFuelEnabled())
			_tutorialQueue.Enqueue(_fuelTutorial);

		DisplayNextScreen();
	}

	public void DisplayNextScreen()
	{
		if (_currentScreen != null)
			Destroy(_currentScreen.gameObject);

		_currentScreen = Instantiate(_tutorialQueue.Dequeue(), transform.position, transform.rotation, transform);

		if (_tutorialQueue.Count > 0)
			_currentScreen.SetNextButtonAction(() => { DisplayNextScreen(); });
		else
		{
			_currentScreen.SetNextButtonText(_lastScreenText);
			_currentScreen.SetNextButtonAction(() => { Destroy(_currentScreen.gameObject); });
		}
	}

	public void HelpButtonPressed()
	{
		_starting = true;

		InitialiseTutorial();
	}
}