using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MazeUIController : MonoBehaviour
{
	[SerializeField] private float _lerpTime;

	private static MazeUIController _instance;
	public static MazeUIController Instance => _instance;

	void Awake()
	{
		if (_instance == null)
			_instance = this;
		else
			Destroy(gameObject);
	}
}