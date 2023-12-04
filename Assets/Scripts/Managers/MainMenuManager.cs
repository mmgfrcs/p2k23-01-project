using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    private SceneFader _fader;
    private void Awake()
    {
        _fader = FindObjectOfType<SceneFader>();
    }

    public void OnStart()
    {
        _fader.Show(() => SceneManager.LoadScene("Game"));
    }
    
    public void OnExit()
    {
        Application.Quit();
    }
}
