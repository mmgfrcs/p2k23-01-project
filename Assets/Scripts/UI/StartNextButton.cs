using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class StartNextButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI earlyBonusText;
    [SerializeField] private Image iconImage;
    private Button _btn;
    private Sprite[] _enemyImgs;
    private Sprite _originalImgs;
    
    private void Awake()
    {
        _btn = GetComponent<Button>();
        _btn.onClick.RemoveAllListeners();
        _btn.onClick.AddListener(OnClick);
        _originalImgs = iconImage.sprite;

        _enemyImgs = new[] //Normal, Fast, Armored, Army, Jet, Boss
        {
            Resources.Load<Sprite>("UI/Enemy - Normal"),
            Resources.Load<Sprite>("UI/Enemy - Fast"),
            Resources.Load<Sprite>("UI/Enemy - Armored"),
            Resources.Load<Sprite>("UI/Enemy - Army"),
            Resources.Load<Sprite>("UI/Enemy - Jet"),
            Resources.Load<Sprite>("UI/Enemy - Boss")
        };
    }

    private void OnClick()
    {
        GameManager.Instance.NextWave();
    }

    // Update is called once per frame
    private void Update()
    {
        earlyBonusText.gameObject.SetActive(false);
        var gameManager = GameManager.Instance;
        if (gameManager.WaveTimer > 0)
        {
            timerText.text = gameManager.WaveTimer.ToString("N1");
            iconImage.sprite = _originalImgs;
            if (gameManager.EarlyWaveMoneyBonus >= 1 &&
                gameManager.Wave != gameManager.CurrentMap.GetNextExpansionWave(gameManager.Wave) - 1)
            {
                earlyBonusText.text = $"+{gameManager.EarlyWaveMoneyBonus}";
                earlyBonusText.gameObject.SetActive(true);
            }
        }
        else if (Math.Abs(gameManager.WaveTimer - (-1)) < 0.001f) timerText.text = "Start";
        else
        {
            timerText.text = gameManager.CurrentMap.EnemyRemaining.ToString("N0");
            iconImage.sprite = _enemyImgs[(int)gameManager.CurrentMap.CurrentSpawnTiming.enemyPrefab.Type];
        }
        
        _btn.interactable = gameManager.Wave != gameManager.CurrentMap.GetNextExpansionWave(gameManager.Wave) - 1
            && gameManager.WaveTimer > 0 || Math.Abs(gameManager.WaveTimer - (-1)) < 0.001f;

    }
}
