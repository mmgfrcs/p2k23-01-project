using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AdInfinitum.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class PauseMenu : MonoBehaviour
    {
        private OptionsPanel _optionsPanel;
        private SceneFader _fader;
        private CanvasGroup _group;
        private void Start()
        {
            _group = GetComponent<CanvasGroup>();
            _optionsPanel = GetComponentInChildren<OptionsPanel>();
            _fader = FindObjectOfType<SceneFader>();

            _group.alpha = 0;
            _group.blocksRaycasts = false;
        }

        public void Pause()
        {
            DOTween.To(() => Time.timeScale, value => Time.timeScale = value, 0f, 1f).SetUpdate(true);
            _group.blocksRaycasts = true;
            _group.DOFade(1f, 1f).SetUpdate(true);
        }

        public void OnOptions()
        {
            _optionsPanel.Open();
        }

        public void OnResume()
        {
            _optionsPanel.Close();
            _group.DOFade(0f, 1f).OnComplete(() =>
            {
                _group.blocksRaycasts = false;
            }).SetUpdate(true);
            DOTween.To(() => Time.timeScale, value => Time.timeScale = value, 1f, 1f).SetUpdate(true);
        }

        public void OnExit()
        {
            Debug.Log("Exit");
            _fader.Show(() =>
            {
                Time.timeScale = 1;
                SceneManager.LoadScene(0);
            });
        }
    }
}
