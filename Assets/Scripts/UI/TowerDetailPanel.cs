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
    [SerializeField] private TowerBuyStats[] reportList;
    [SerializeField] private Button upgradeBtn;
    
    private Sprite _damageStatIcon;
    private Sprite _attackSpeedStatIcon;
    private Sprite _rangeStatIcon;
    private Sprite _bulletSpeedStatIcon;
    private Sprite _rotationSpeedStatIcon;
    
    private GameObject _sBox;
    
    // Start is called before the first frame update
    private void Awake()
    {
        _damageStatIcon = Resources.Load<Sprite>("UI/DamageIcon");
        _attackSpeedStatIcon = Resources.Load<Sprite>("UI/AttackSpeedIcon");
        _rangeStatIcon = Resources.Load<Sprite>("UI/RangeIcon");
        _bulletSpeedStatIcon = Resources.Load<Sprite>("UI/BulletSpeedIcon");
        _rotationSpeedStatIcon = Resources.Load<Sprite>("UI/RotationSpeedIcon");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void CreateSelectionBox()
    {
        if (_sBox != null) return;
        _sBox = new GameObject("SelectionBox");
        var sr = _sBox.AddComponent<SpriteRenderer>();
        sr.sprite = Resources.Load<Sprite>("UI/SelectionBox");
        sr.sortingOrder = 999;
        _sBox.transform.localScale = Vector3.one * 1.2f;
        _sBox.gameObject.SetActive(false);
    }
    
    public void ResetPanel()
    {
        upgradeBtn.gameObject.SetActive(false);
        towerIcon.gameObject.SetActive(false);
        towerNameText.text = "";
        for (int i = 0; i < statList.Length; i++)
            statList[i].gameObject.SetActive(false);
    }

    public void OpenPanel(Vector3 loc, Tower tower)
    {
        ResetPanel();
        CreateSelectionBox();
        _sBox.gameObject.SetActive(true);
        _sBox.transform.position = loc;
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
                    statList[i].SetStat("Damage", tower.Damage, "", false, _damageStatIcon);
                    break;
                case 1:
                    statList[i].SetStat("Bullet Speed", tower.ProjectileSpeed, "m/s", false, _bulletSpeedStatIcon);
                    break;
                case 2:
                    statList[i].SetStat("Rotation Speed", tower.RotationSpeed, "deg/s", false, _rotationSpeedStatIcon);
                    break;
                case 3:
                    statList[i].SetStat("Attack Speed", tower.AttackSpeed, "p/s", true, _attackSpeedStatIcon);
                    break;
                case 4:
                    statList[i].SetStat("Range", tower.Range, "m", true, _rangeStatIcon);
                    break;
                default:
                    if (i-5 >= tower.OtherStatistics.Length) statList[i].gameObject.SetActive(false);
                    else statList[i].SetStat(tower.OtherStatistics[i-5].name, tower.OtherStatistics[i-5].value, tower.OtherStatistics[i-5].unitString, tower.OtherStatistics[i-5].isDecimal, tower.OtherStatistics[i-5].icon);
                    break;
            }
        }
        
        for (int i = 0; i < reportList.Length; i++)
        {
            reportList[i].gameObject.SetActive(true);
            switch (i)
            {
                case 0:
                    reportList[i].SetStat("Kills", tower.Damage, "", false, _damageStatIcon);
                    break;
                case 1:
                    reportList[i].SetStat("Bullet Speed", tower.ProjectileSpeed, "m/s", false, _bulletSpeedStatIcon);
                    break;
                case 2:
                    reportList[i].SetStat("Rotation Speed", tower.RotationSpeed, "deg/s", false, _rotationSpeedStatIcon);
                    break;
                case 3:
                    reportList[i].SetStat("Attack Speed", tower.AttackSpeed, "p/s", true, _attackSpeedStatIcon);
                    break;
                case 4:
                    reportList[i].SetStat("Range", tower.Range, "m", true, _rangeStatIcon);
                    break;
            }
        }
    }
    
    public void ClosePanel()
    {
        CreateSelectionBox();
        _sBox.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }
}
