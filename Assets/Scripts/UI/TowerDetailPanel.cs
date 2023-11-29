using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TowerDetailPanel : MonoBehaviour
{
    [Header("UI"), SerializeField] private TextMeshProUGUI towerNameText;
    [SerializeField] private Image towerIcon;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TowerBuyStats[] statList;
    [SerializeField] private Button upgradeBtn;
    
    [Header("Default Icons"), SerializeField] private Sprite damageStatIcon;
    [SerializeField] private Sprite attackSpeedStatIcon;
    [SerializeField] private Sprite rangeStatIcon;
    [SerializeField] private Sprite bulletSpeedStatIcon;
    [SerializeField] private Sprite rotationSpeedStatIcon;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    
    public void ResetPanel()
    {
        upgradeBtn.gameObject.SetActive(false);
        towerIcon.gameObject.SetActive(false);
        towerNameText.text = "";
        for (int i = 0; i < statList.Length; i++)
        {
            statList[i].gameObject.SetActive(false);
        }
    }

    public void OpenPanel(Tower tower)
    {
        ResetPanel();
        gameObject.SetActive(true);
        
        towerIcon.sprite = tower.Icon;
        towerNameText.text = tower.Type.ToString();
        priceText.text = tower.Price.ToString("N0");

        //upgradeBtn.interactable = GameManager.Instance.Money >= tower.Price;

        for (int i = 0; i < statList.Length; i++)
        {
            statList[i].gameObject.SetActive(true);
            switch (i)
            {
                case 0:
                    statList[i].SetStat("Damage", tower.Damage, "", damageStatIcon);
                    break;
                case 1:
                    statList[i].SetStat("Bullet Speed", tower.ProjectileSpeed, "m/s", bulletSpeedStatIcon);
                    break;
                case 2:
                    statList[i].SetStat("Rotation Speed", tower.RotationSpeed, "deg/s", rotationSpeedStatIcon);
                    break;
                case 3:
                    statList[i].SetStat("Attack Speed", tower.AttackSpeed, "p/s", attackSpeedStatIcon);
                    break;
                case 4:
                    statList[i].SetStat("Range", tower.Range, "m", rangeStatIcon);
                    break;
                default:
                    if (i-5 >= tower.OtherStatistics.Length) statList[i].gameObject.SetActive(false);
                    else statList[i].SetStat(tower.OtherStatistics[i-5].name, tower.OtherStatistics[i-5].value, tower.OtherStatistics[i-5].unitString, tower.OtherStatistics[i-5].icon);
                    break;
            }
        }
    }
    
    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }
}
