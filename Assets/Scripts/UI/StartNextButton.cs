using AdInfinitum.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AdInfinitum.UI
{
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

            iconImage.sprite = _originalImgs;
            if (gameManager.WaveTimer > 0)
            {
                timerText.text = gameManager.WaveTimer.ToString("N1");

                if (gameManager.EarlyWaveMoneyBonus >= 1 &&
                    !gameManager.CurrentMap.ExpandThisWave(gameManager.Wave + 1))
                {
                    earlyBonusText.text = $"+{gameManager.EarlyWaveMoneyBonus}";
                    earlyBonusText.gameObject.SetActive(true);
                }
            }
            else if (Mathf.Approximately(gameManager.WaveTimer, -3)) { // Next wave is expansion wave
                timerText.text = gameManager.EnemyAmount + "\nRemaining";
                iconImage.sprite = _enemyImgs[(int)gameManager.CurrentMap.CurrentSpawnTiming.enemyPrefab.Type];
            }
            else if (Mathf.Approximately(gameManager.WaveTimer, -1)) //At start wave
                timerText.text = "Start";
            else
            {
                timerText.text = gameManager.CurrentMap.EnemyRemaining.ToString("N0");
                iconImage.sprite = _enemyImgs[(int)gameManager.CurrentMap.CurrentSpawnTiming.enemyPrefab.Type];
            }

            //Button is interactable (clickable) if:
            // - Wave timer is greater than 0 AND The next wave is NOT an expansion wave, OR
            // - Wave timer is -1 (a Start wave), OR
            // - Wave timer is -3 (Next wave is expansion wave) AND No more enemies
            _btn.interactable =
                !gameManager.CurrentMap.ExpandThisWave(gameManager.Wave + 1) &&
                gameManager.WaveTimer > 0 ||
                Mathf.Approximately(gameManager.WaveTimer, -1) ||
                Mathf.Approximately(gameManager.WaveTimer, -3) && gameManager.EnemyAmount == 0;

        }
    }
}
