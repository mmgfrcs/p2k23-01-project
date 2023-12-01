using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EfficiencyListItem : MonoBehaviour
{
    [SerializeField] private Image enemyImage;
    [SerializeField] private TextMeshProUGUI efficiencyText;

    private Dictionary<EnemyType, Sprite> _enemySprites = new Dictionary<EnemyType, Sprite>();

    public void SetEfficiency(EnemyType enemy, float efficiency)
    {
        if (_enemySprites.TryGetValue(enemy, out var sprite)) enemyImage.sprite = sprite;
        else
        {
            sprite = Resources.Load<Sprite>($"UI/Enemy - {enemy.ToString()}");
            _enemySprites.Add(enemy, sprite);
            enemyImage.sprite = sprite;
        }
        if(efficiency > 1) efficiencyText.text = $"<color=green>{efficiency * 100:N0}%</color>";
        else if (efficiency < 1) efficiencyText.text = $"<color=red>{efficiency * 100:N0}%</color>";
        else efficiencyText.text = $"{efficiency * 100:N0}%";
    }
}
