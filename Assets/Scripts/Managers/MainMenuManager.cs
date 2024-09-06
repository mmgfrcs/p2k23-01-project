using AdInfinitum.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AdInfinitum.Managers
{
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
}
