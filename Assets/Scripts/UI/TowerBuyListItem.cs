using System;
using UnityEngine;
using UnityEngine.UI;

public class TowerBuyListItem : MonoBehaviour
{
    [SerializeField] private Image icon;
    private Button btn;
    
    private void Awake()
    {
        btn = GetComponent<Button>();
    }

    public void SetIndex(BuyPanel panel, int idx)
    {
        btn.onClick.AddListener(delegate { panel.Select(idx); });
    }

    public void SetIcon(Sprite sprite)
    {
        icon.sprite = sprite;
    }
}
