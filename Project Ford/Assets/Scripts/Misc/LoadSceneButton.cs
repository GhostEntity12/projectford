using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneButton : MonoBehaviour
{
	[SerializeField] private string _sceneName = "";

	public void LoadScene()
	{
		SceneManager.LoadScene(_sceneName);
	}
}
