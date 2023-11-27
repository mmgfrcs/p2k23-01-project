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
        if (GameManager.Instance.WaveTimer > 0) timerText.text = GameManager.Instance.WaveTimer.ToString("N1");
        else if (Math.Abs(GameManager.Instance.WaveTimer - (-1)) < 0.001f) timerText.text = "Start";
        else timerText.text = "";

        _btn.interactable = GameManager.Instance.WaveTimer <= 0;

    }
}
