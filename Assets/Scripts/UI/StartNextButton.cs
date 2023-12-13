using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class StartNextButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI earlyBonusText;
    private Button _btn;
    
    private void Awake()
    {
        _btn = GetComponent<Button>();
        _btn.onClick.RemoveAllListeners();
        _btn.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        GameManager.Instance.NextWave();
    }

    // Update is called once per frame
    private void Update()
    {
        earlyBonusText.gameObject.SetActive(false);
        if (GameManager.Instance.WaveTimer > 0)
        {
            timerText.text = GameManager.Instance.WaveTimer.ToString("N1");
            if (GameManager.Instance.EarlyWaveMoneyBonus >= 1)
            {
                earlyBonusText.text = $"+{GameManager.Instance.EarlyWaveMoneyBonus}";
                earlyBonusText.gameObject.SetActive(true);
            }
        }
        else if (Math.Abs(GameManager.Instance.WaveTimer - (-1)) < 0.001f) timerText.text = "Start";
        else timerText.text = "";
        
        _btn.interactable = GameManager.Instance.WaveTimer > 0 || Math.Abs(GameManager.Instance.WaveTimer - (-1)) < 0.001f;

    }
}
