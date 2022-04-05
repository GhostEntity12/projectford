using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Indicator : MonoBehaviour
{
	[SerializeField]
	private Sprite _successIndicator = null;

	[SerializeField]
	private Sprite _failIndicator = null;

	[SerializeField]
	private float _displayTime = 3.0f;

	private float _currentDisplayTime = 0.0f;

	private bool _displayed = false;

	private Image _image = null;

	private Color _whiteClear = new Vector4(1,1,1,0);

	void Awake()
	{
		_image = GetComponent<Image>();
		_image.color = _whiteClear;
	}

	void Update()
	{
		if (_displayed)
		{
			_currentDisplayTime += Time.deltaTime;
			_image.color = Vector4.Lerp(Color.white, _whiteClear, _currentDisplayTime / _displayTime);

			if (_currentDisplayTime >= _displayTime)
				_displayed = false;
		}
	}

	/// <summary>
	/// Set the indicator to display either success or failure.
	/// </summary>
	/// <param name="correct">If the answer was correct.</param>
	public void SetIndicator(bool correct)
	{
		if (correct)
			_image.sprite = _successIndicator;
		else
			_image.sprite = _failIndicator;

		_displayed = true;
		_currentDisplayTime = 0.0f;
	}
}