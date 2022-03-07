using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DifficultySelectScreen : MonoBehaviour
{
	[SerializeField] private GameObject _difficultyScreenCanvas = null;

	public void TurnOffScreen()
	{
		_difficultyScreenCanvas.SetActive(false);
	}
}
