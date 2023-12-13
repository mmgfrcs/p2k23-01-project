using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TowerDetailPanel : MonoBehaviour
{
    [Header("UI"), SerializeField] private TextMeshProUGUI towerNameText;
    [SerializeField] private Image towerIcon;
    [SerializeField] private TextMeshProUGUI priceText, sellPriceText;
    [SerializeField] private TowerBuyStats[] statList;
    [SerializeField] private TowerBuyStats[] reportList;
    [SerializeField] private EfficiencyListItem[] efficiencyList;
    [SerializeField] private Button upgradeBtn, sellBtn;
    [SerializeField]  private TextMeshProUGUI levelText;
    
    public bool IsOpen { get; private set; }
    
    private Sprite _damageStatIcon;
    private Sprite _attackSpeedStatIcon;
    private Sprite _rangeStatIcon;
    private Sprite _bulletSpeedStatIcon;
    private Sprite _rotationSpeedStatIcon;
    
    private GameObject _sBox;
    private Tower _currTower;
    private bool _isOpen;
    private bool _isUpgrade;
    
    public event Action<Vector3, Tower> TowerSell;

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
        if (!_isOpen) return;
        for (int i = 0; i < reportList.Length; i++)
        {
            reportList[i].gameObject.SetActive(true);
            switch (i)
            {
                case 0:
                    reportList[i].SetStat("Kills", _currTower.Reports.Kills, "", false, _damageStatIcon);
                    break;
                // TODO: More stats
                default:
                    reportList[i].gameObject.SetActive(false);
                    break;
            }
        }
    }
    
    private void CreateSelectionBox()
    {
        if (_sBox != null) return;
        _sBox = new GameObject("SelectionBox");
        var sr = _sBox.AddComponent<SpriteRenderer>();
        sr.sprite = Resources.Load<Sprite>("UI/SelectionBox");
        sr.sortingOrder = 999;
        _sBox.transform.localScale = Vector3.one;
        _sBox.gameObject.SetActive(false);
    }
    
    public void ResetPanel()
    {
        towerIcon.gameObject.SetActive(false);
        towerNameText.text = "";
        for (int i = 0; i < statList.Length; i++)
            statList[i].gameObject.SetActive(false);
        for (int i = 0; i < reportList.Length; i++)
            reportList[i].gameObject.SetActive(false);
        
        IsOpen = gameObject.activeInHierarchy;
    }

    public void OpenPanel(Vector3 loc, Tower tower)
    {
        ResetPanel();
        CreateSelectionBox();
        _isOpen = true;
        _currTower = tower;
        _sBox.gameObject.SetActive(true);
        _sBox.transform.position = loc;
        _isUpgrade = false;
        gameObject.SetActive(true);
        
        towerIcon.sprite = tower.Icon;
        towerNameText.text = tower.Type.ToString();
        priceText.text = tower.GetUpgradePrice().ToString("N0");
        levelText.text = $"L{tower.Level}";
        sellPriceText.text = tower.SellPrice.ToString();
        
        upgradeBtn.interactable = GameManager.Instance.Money >= tower.GetUpgradePrice();

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
                    else
                    {
                        var stat = tower.OtherStatistics[i - 5];
                        switch (stat.type)
                        {
                            case Tower.StatType.Slow:
                                statList[i].SetStat(stat.name, tower.Slow, stat.unitString, stat.isDecimal, stat.icon);
                                break;
                            case Tower.StatType.SplashRange:
                                statList[i].SetStat(stat.name, tower.SplashRange, stat.unitString, stat.isDecimal, stat.icon);
                                break;
                            default:
                                statList[i].SetStat(stat.name, stat.value, stat.unitString, stat.isDecimal, stat.icon);
                                break;
                        }
                    }
                    break;
            }
        }
        
        var enemyTypes = Enum.GetValues(typeof(EnemyType)).Cast<EnemyType>().ToList();

        for (int i = 0; i < efficiencyList.Length; i++)
        {
            if (i >= enemyTypes.Count) efficiencyList[i].gameObject.SetActive(false);
            else
            {
                efficiencyList[i].gameObject.SetActive(true);
                efficiencyList[i].SetEfficiency(enemyTypes[i], tower.GetEfficiency(enemyTypes[i]));
            }
        }
        
        tower.ShowRange();
        
        
    }
    
    public void ClosePanel()
    {
        CreateSelectionBox();
        if(_isOpen) _currTower.HideRange();
        _isOpen = false;
        _sBox.gameObject.SetActive(false);
        gameObject.SetActive(false);
        
    }

    public void OnSell()
    {
        _currTower.Sell();
        TowerSell?.Invoke(_sBox.transform.position, _currTower);
        ClosePanel();
    }
    
    public void OnLevelUp()
    {
        if (_isUpgrade)
        {
            if (!GameManager.Instance.Purchase(Convert.ToUInt32(_currTower.GetUpgradePrice()))) return;
            _currTower.LevelUp();
            StopCoroutine(LevelUpConfirmDelay());
            OpenPanel(_sBox.transform.position, _currTower);
        }
        else StartCoroutine(LevelUpConfirmDelay());
    }

    private IEnumerator LevelUpConfirmDelay()
    {
        _isUpgrade = true;
        priceText.text = $"{_currTower.GetUpgradePrice()} (Confirm)";
        for (int i = 0; i < statList.Length; i++)
        {
            switch (i)
            {
                case 0:
                    statList[i].SetUpgrade(_currTower.GetDamageLevelUpEffect());
                    break;
                case 1:
                    statList[i].SetUpgrade(_currTower.GetProjectileSpeedLevelUpEffect());
                    break;
                case 2:
                    statList[i].SetUpgrade(_currTower.GetRotationSpeedLevelUpEffect());
                    break;
                case 3:
                    statList[i].SetUpgrade(_currTower.GetAttackSpeedLevelUpEffect());
                    break;
                case 4:
                    statList[i].SetUpgrade(_currTower.GetRangeLevelUpEffect());
                    break;
                default:
                    if (i-5 >= _currTower.OtherStatistics.Length) continue;
                    switch (_currTower.OtherStatistics[i-5].type)
                    {
                        case Tower.StatType.Slow:
                            statList[i].SetUpgrade(_currTower.GetSlowLevelUpEffect());
                            break;
                        case Tower.StatType.SplashRange:
                            statList[i].SetUpgrade(_currTower.GetSplashRangeLevelUpEffect());
                            break;
                    }
                    break;
            }
        }
        yield return new WaitForSeconds(2f);
        if(_isOpen) OpenPanel(_sBox.transform.position, _currTower);
        _isUpgrade = false;
    }
}
