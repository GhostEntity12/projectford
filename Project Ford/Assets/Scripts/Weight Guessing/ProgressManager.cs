using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ProgressManager : MonoBehaviour
{
	[SerializeField]
	private Transform _startPosTransform = null;

	[SerializeField]
	private Transform _endPosTransform = null;

	[SerializeField]
	private Transform _progressIndicator = null;

	[SerializeField]
	private TextMeshProUGUI _currentQuestionCounter = null;

	[SerializeField]
	private TextMeshProUGUI _maxQuestionCounter = null;

	private int _maxQuestionCount = 0;

	private Vector3 _startPosVec = Vector3.zero;

	private Vector3 _endPosVec = Vector3.zero;

	private static ProgressManager _instance = null;

	private RectTransform _rectTransform = null;

	void Awake()
	{
		_instance = this;

		_rectTransform = GetComponent<RectTransform>();

		_startPosVec = _rectTransform.InverseTransformPoint(_startPosTransform.position);
		_endPosVec = _rectTransform.InverseTransformPoint(_endPosTransform.position);

		_progressIndicator.position = _rectTransform.TransformPoint(_startPosVec);

		_currentQuestionCounter.text = "0";
	}

	public void UpdateProgressBar(int currentQuestion)
	{
		_progressIndicator.position = _rectTransform.TransformPoint(Vector3.Lerp(_startPosVec, _endPosVec, (float)currentQuestion / (float)_maxQuestionCount));
		_currentQuestionCounter.text = currentQuestion.ToString();
	}

	public void SetMaxQuestionCount(int max)
	{
		_maxQuestionCount = max;

		_maxQuestionCounter.text = _maxQuestionCount.ToString();
	}

	public void ResetProgressBar()
	{
		_progressIndicator.position = _rectTransform.TransformPoint(Vector3.Lerp(_startPosVec, _endPosVec, 0f));
		_currentQuestionCounter.text = "0";
	}

	public static ProgressManager GetInstance()
	{
		return _instance;
	}
}
