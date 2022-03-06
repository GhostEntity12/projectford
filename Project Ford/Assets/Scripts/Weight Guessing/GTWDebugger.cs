using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GTWDebugger : MonoBehaviour
{
	private ComboManager _cmInstance = null;
	private AnimalManager _amInstance = null;

	void Start()
	{
		_cmInstance = ComboManager.GetInstance();
		_amInstance = AnimalManager.GetInstance();
	}

	// Update is called once per frame
	void Update()
	{
		if (Input.GetKey(KeyCode.N))
		{
			_cmInstance.SetComboCount(2);
			_cmInstance.IncrementComboCounter();

			_amInstance.ResetWeightGuessGame();
		}
	}
}
