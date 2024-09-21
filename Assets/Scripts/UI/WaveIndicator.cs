using System.Collections.Generic;
using System.Linq;
using AdInfinitum.Entities;
using AdInfinitum.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace AdInfinitum.UI
{
    public class WaveIndicator : MonoBehaviour
    {
        [SerializeField] private Image[] enemyImages;
        [SerializeField] private TextMeshProUGUI[] amountTexts;
        [SerializeField] private TextMeshProUGUI waveNumText;

        private Sprite[] enemySprites;
        private string _setHash;

        private void Start()
        {
            enemySprites = new[] //Normal, Fast, Armored, Army, Jet, Boss
            {
                Resources.Load<Sprite>("UI/Enemy - Normal"),
                Resources.Load<Sprite>("UI/Enemy - Fast"),
                Resources.Load<Sprite>("UI/Enemy - Armored"),
                Resources.Load<Sprite>("UI/Enemy - Army"),
                Resources.Load<Sprite>("UI/Enemy - Jet"),
                Resources.Load<Sprite>("UI/Enemy - Boss")
            };
        }

        public void Set(uint wave, SpawnFormation[] formations, bool expand)
        {
            Dictionary<EnemyType, uint> formationDict = new Dictionary<EnemyType, uint>();

            formationDict = formations.Aggregate(formationDict, (accumulate, formation) =>
            {
                if (!accumulate.TryAdd(formation.enemyPrefab.Type, formation.amount))
                    accumulate[formation.enemyPrefab.Type] += formation.amount;

                return accumulate;
            });

            int i = 0;
            foreach (var formation in formationDict)
            {
                enemyImages[i].gameObject.SetActive(true);
                enemyImages[i].sprite = enemySprites[(int)formation.Key];
                waveNumText.text = expand ? $"{wave}>" : wave.ToString();
                amountTexts[i].text = $"x{formation.Value}";
                i++;
            }
            for (; i < Mathf.Min(enemyImages.Length, amountTexts.Length); i++)
                enemyImages[i].gameObject.SetActive(false);

            LayoutRebuilder.MarkLayoutForRebuild(transform as RectTransform);
        }
    }
}
