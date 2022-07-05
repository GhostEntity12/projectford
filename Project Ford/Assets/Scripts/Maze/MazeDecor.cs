using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeDecor : MonoBehaviour
{
	[SerializeField] private List<GameObject> _decorText = new List<GameObject>();
	[SerializeField] private float _lerpTime = 0.2f;
	public List<GameObject> DecorText => _decorText;
	private Dictionary<GameObject, Vector3> _textEndPositions = new Dictionary<GameObject, Vector3>();
	private bool _textShowing = false;

	void Start()
	{
		foreach(GameObject textObject in _decorText)
		{
			Vector3 textLocalPosition = textObject.transform.parent.InverseTransformPoint(textObject.transform.position);
			Vector3 textWorldPosition = textObject.transform.position;
			Vector3 textEndPositionWorld = textWorldPosition;

			_textEndPositions.Add(textObject, textEndPositionWorld);

			// Hi, this is probably incomprehensible to whoever is looking at this (myself included)
			// Basically, this figures out which side of the screen the text is closest to and moves it off screen on that side.
			// It does this by working out which side of the screen the text is on (assuming 0,0 is the centre of the screen)
			textObject.transform.position = textWorldPosition + (Vector3.right * ((textLocalPosition.x / Mathf.Abs(textLocalPosition.x)))) * Mathf.Abs(Mathf.Abs(textLocalPosition.x) - Screen.width / 10f);
		}
	}

	public void ShowMapText()
	{
		// Don't do anything if text is already showing.
		if (_textShowing) return;

		foreach(GameObject textObject in _decorText)
		{
			StartCoroutine(LerpToScreen(textObject, _lerpTime));
		}
		_textShowing = true;
	}

	private IEnumerator LerpToScreen(GameObject text, float lerpTime)
	{
		float currentTime = 0f;
		Vector3 textStartPosition = text.transform.position;

		while(currentTime < lerpTime)
		{
			text.transform.position = Vector3.Lerp(textStartPosition, _textEndPositions[text], currentTime / lerpTime);
			currentTime += Time.deltaTime;
			yield return null;
		}
		text.transform.position = _textEndPositions[text];

		yield return null;
	}
}
