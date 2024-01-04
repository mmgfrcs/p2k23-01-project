using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TowerBuyListItem : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI priceText;
    private Button btn;
    
    private void Awake()
    {
        btn = GetComponent<Button>();
    }

    public void SetIndex(TowerBuyPanel panel, int idx)
    {
        btn.onClick.AddListener(delegate { panel.Select(idx); });
    }

    public void SetValue(Sprite sprite, uint price)
    {
        icon.sprite = sprite;
        priceText.text = price.ToString();
    }
}
