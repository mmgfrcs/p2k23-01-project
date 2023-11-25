using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuyPanel : MonoBehaviour
{
    [Header("UI"), SerializeField] private TextMeshProUGUI towerNameText;
    [SerializeField] private Image towerIcon;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TowerBuyListItem[] towerList;
    [SerializeField] private TowerBuyStats[] statList;
}
