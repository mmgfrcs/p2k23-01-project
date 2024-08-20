using AdInfinitum.Entities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AdInfinitum.UI
{
    public class WaveIndicator : MonoBehaviour
    {
        [SerializeField] private Image enemyImage;
        [SerializeField] private TextMeshProUGUI waveNumText, amountText;

        private Sprite[] enemyImages;

        private void Start()
        {
            enemyImages = new[] //Normal, Fast, Armored, Army, Jet, Boss
            {
                Resources.Load<Sprite>("UI/Enemy - Normal"),
                Resources.Load<Sprite>("UI/Enemy - Fast"),
                Resources.Load<Sprite>("UI/Enemy - Armored"),
                Resources.Load<Sprite>("UI/Enemy - Army"),
                Resources.Load<Sprite>("UI/Enemy - Jet"),
                Resources.Load<Sprite>("UI/Enemy - Boss")
            };
        }

        public void Set(EnemyType enemy, uint wave, uint amount, bool expand)
        {
            enemyImage.sprite = enemyImages[(int)enemy];
            waveNumText.text = expand ? $"{wave}>" : wave.ToString();
            amountText.text = $"x{amount}";
        }
    }
}
