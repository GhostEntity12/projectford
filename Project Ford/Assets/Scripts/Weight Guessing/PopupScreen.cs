using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupScreen : MonoBehaviour
{
	[SerializeField] private GameObject _popupCanvas = null;

	public void TurnOffScreen()
	{
		_popupCanvas.SetActive(false);
	}
}
