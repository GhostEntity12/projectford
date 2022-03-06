using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public enum Scenes { MainMenu = 0, GuessTheWeight = 1, MazeGame = 2 }

public class SceneManagement : MonoBehaviour
{
	public void LoadScene(Scenes scene) => SceneManager.LoadScene((int)scene);
	public void LoadScene(int sceneIndex) => SceneManager.LoadScene(sceneIndex);
	public void LoadScene(string sceneName) => SceneManager.LoadScene(sceneName);

    public void Quit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
