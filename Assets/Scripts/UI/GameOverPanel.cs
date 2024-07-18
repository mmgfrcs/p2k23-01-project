using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class GameOverPanel : MonoBehaviour
{
    [Header("UI"), SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI newRecordText, wavesText, dpsText, killsText;
    [SerializeField] private Button restartBtn, exitBtn;

    private CanvasGroup _group;
    private SceneFader _fader;

    private void Awake()
    {
        _group = GetComponent<CanvasGroup>();
        _fader = FindObjectOfType<SceneFader>();
    }

    // Start is called before the first frame update
    private void Start()
    {
        _group.alpha = 0;
        _group.blocksRaycasts = false;
    }

    public void OnRetry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnExit()
    {
        _fader.Show(() => SceneManager.LoadScene(0));
    }
    
    public void ShowPanel(ulong score, uint waves, uint kills, float dps)
    {
        newRecordText.gameObject.SetActive(false);
        scoreText.text = "0";
        wavesText.text = "0";
        dpsText.text = "0";
        killsText.text = "0";

        _group.DOFade(1f, 1f).OnComplete(() => _group.blocksRaycasts = true);
        DOTween.To(() => ulong.Parse(scoreText.text), value => scoreText.text = value.ToString(), score, 2f).SetDelay(1f);
        DOTween.To(() => uint.Parse(wavesText.text), value => wavesText.text = value.ToString(), waves, 2f).SetDelay(1f);
        DOTween.To(() => uint.Parse(killsText.text), value => killsText.text = value.ToString(), kills, 2f).SetDelay(1f);
        DOTween.To(() => float.Parse(dpsText.text), value => dpsText.text = value.ToString("N0"), dps, 2f).SetDelay(1f);
    }
}
