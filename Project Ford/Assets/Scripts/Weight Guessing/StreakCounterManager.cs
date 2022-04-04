using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StreakCounterManager : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI _streakCountDisplay = null;

	private int _currentStreakCount = 0;

	void Awake()
	{
		UpdateStreakCounter();
	}

	public void IncrementStreakCount()
	{
		_currentStreakCount++;
		UpdateStreakCounter();
	}

	public void ResetStreakCount()
	{
		_currentStreakCount = 0;
		UpdateStreakCounter();
	}

	private void UpdateStreakCounter()
	{
		_streakCountDisplay.text = _currentStreakCount.ToString();
	}
}