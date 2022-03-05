using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelpMenu : MonoBehaviour
{
    [SerializeField] private CanvasGroup _helpBackground = null;
    
    private float _progress = 0f;
    private bool _faded = false;
    [SerializeField] private float fadeSpeedModifier = 4.0f;
    // Update is called once per frame
    void Update()
    {
        UpdateProgress();

        _helpBackground.alpha = _progress;
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

    public void ToggleHelp() => _faded = !_faded;
}
