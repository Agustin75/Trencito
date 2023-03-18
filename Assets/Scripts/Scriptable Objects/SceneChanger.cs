using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(menuName = "Helpers/Scene")]
public class SceneChanger : ScriptableObject
{
    public void OpenScene(string _sceneName)
    {
        SceneManager.LoadScene(_sceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
