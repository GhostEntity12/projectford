using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GTWDebugger : MonoBehaviour
{
	private ComboManager _cmInstance = null;

	void Start()
	{
		_cmInstance = ComboManager.GetInstance();
	}

	// Update is called once per frame
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			_cmInstance.SetComboCount(2);
			_cmInstance.IncrementComboCounter();
		}
	}
}
