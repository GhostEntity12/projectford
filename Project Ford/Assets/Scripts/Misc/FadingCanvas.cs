using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadingCanvas : MonoBehaviour
{
    [SerializeField] private CanvasGroup _elementBackground = null;
    
    private float _progress = 0f;
    private bool _faded = false;
    [SerializeField] private float fadeSpeedModifier = 4.0f;
    // Update is called once per frame
    void Update()
    {
        UpdateProgress();

        _elementBackground.alpha = _progress;
        _elementBackground.interactable = _elementBackground.alpha == 1;
        _elementBackground.blocksRaycasts = _elementBackground.alpha != 0;
    }

    void UpdateProgress()
	{
        if (_faded && _progress != 1f)
        {
            _progress = Mathf.Clamp(_progress += Time.deltaTime * fadeSpeedModifier, 0f, 1f);
            return;
        }
        if (!_faded && _progress != 0f)
        {
            _progress = Mathf.Clamp(_progress -= Time.deltaTime * fadeSpeedModifier, 0f, 1f);
            return;
        }
    }

    public void ToggleFadeStatus() => _faded = !_faded;
}
