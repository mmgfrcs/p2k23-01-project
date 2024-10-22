using AdInfinitum.Managers;
using AdInfinitum.Utilities;
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
        [SerializeField] private Image iconImage, iconNextImage;
        [SerializeField] private GameObject iconSeparator;
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

            SetIcon(_originalImgs);
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
                timerText.text = gameManager.EnemyAmount == 0 ? 
                    "Expand" : gameManager.EnemyAmount.ToString();
                if(gameManager.EnemyAmount == 0)
                    SetIcon(_originalImgs);
                else SetIcon(gameManager.CurrentMap.CurrentSpawnFormation, gameManager.CurrentMap.NextSpawnFormation);
            }
            else if (Mathf.Approximately(gameManager.WaveTimer, -1)) //At start wave
                timerText.text = "Start";
            else
            {
                timerText.text = gameManager.CurrentMap.EnemyRemaining.ToString("N0");
                SetIcon(gameManager.CurrentMap.CurrentSpawnFormation, gameManager.CurrentMap.NextSpawnFormation);
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

        private void SetIcon(SpawnFormation currFormation, SpawnFormation? nextFormation = null)
        {
            Sprite sprite = nextFormation != null ? _enemyImgs[(int)nextFormation.Value.enemyPrefab.Type] : null;
            SetIcon(_enemyImgs[(int)currFormation.enemyPrefab.Type], sprite);
        }

        private void SetIcon(Sprite currSprite, Sprite nextSprite = null)
        {
            iconImage.sprite = currSprite;
            if (nextSprite == null)
            {
                iconNextImage.gameObject.SetActive(false);
                iconSeparator.SetActive(false);
                return;
            }
            iconSeparator.SetActive(true);
            iconNextImage.gameObject.SetActive(true);
            iconNextImage.sprite = nextSprite;
        }
    }
}
