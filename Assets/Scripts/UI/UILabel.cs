using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(TextMeshProUGUI))]
public class UILabel : MonoBehaviour
{
    [SerializeField] private LabelType type;
    private TextMeshProUGUI _text;

    public enum LabelType
    {
        None,
        Life,
        Score,
        Wave,
        Money,
        EnemyAmount,
        Highscore,
        DPS,
        Kills
    }
    
    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        if (type == LabelType.None || !GameManager.Instance) return;

        switch (type)
        {
            case LabelType.Life:
                _text.text = GameManager.Instance.Life.ToString("N0");
                break;
            case LabelType.Score:
                _text.text = GameManager.Instance.Score.ToString("N0");
                break;
            case LabelType.Wave:
                _text.text = GameManager.Instance.Wave.ToString("N0");
                break;
            case LabelType.Money:
                _text.text = GameManager.Instance.Money.ToString("N0");
                break;
            case LabelType.EnemyAmount:
                _text.text = GameManager.Instance.EnemyAmount.ToString("N0");
                break;
            case LabelType.Highscore:
                if (GameManager.Instance.Score > GameManager.Instance.Highscore)
                {
                    _text.color = Color.green;
                    _text.text = GameManager.Instance.Score.ToString("N0");
                }
                else _text.text = GameManager.Instance.Highscore.ToString("N0");
                break;
            case LabelType.DPS:
                _text.text = GameManager.Instance.CurrentMap.SpawnedTowerList.Count != 0 ? 
                    GameManager.Instance.CurrentMap.SpawnedTowerList
                        .Sum(x=>x.Value.Reports.DPS).ToString("N1") : 0.ToString("N1");
                break;
            case LabelType.Kills:
                _text.text = GameManager.Instance.CurrentMap.SpawnedTowerList.Count != 0 ? 
                    GameManager.Instance.CurrentMap.SpawnedTowerList
                        .Select(x => x.Value.Reports.Kills)
                        .Aggregate((x, y) => x + y).ToString() : "0";
                break;
        }
    }
}
