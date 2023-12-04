using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class GameOverPanel : MonoBehaviour
{
    [Header("UI"), SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI newRecordText, wavesText, dpsText, killsText;
    [SerializeField] private Button restartBtn, exitBtn;

    private CanvasGroup _group;

    private void Awake()
    {
        _group = GetComponent<CanvasGroup>();
    }

    // Start is called before the first frame update
    void Start()
    {
        _group.alpha = 0;
        _group.blocksRaycasts = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void ShowPanel(ulong score, uint waves, uint kills, float dps)
    {
        newRecordText.gameObject.SetActive(false);
        scoreText.text = "0";
        wavesText.text = "0";
        dpsText.text = "0";
        killsText.text = "0";
        restartBtn.gameObject.SetActive(false);
        exitBtn.gameObject.SetActive(false);

        _group.DOFade(1f, 1f).OnComplete(() => _group.blocksRaycasts = false);
        DOTween.To(() => ulong.Parse(scoreText.text), value => scoreText.text = value.ToString(), score, 2f).SetDelay(1f);
        DOTween.To(() => uint.Parse(wavesText.text), value => wavesText.text = value.ToString(), waves, 2f).SetDelay(1f);
        DOTween.To(() => uint.Parse(killsText.text), value => killsText.text = value.ToString(), kills, 2f).SetDelay(1f);
        DOTween.To(() => float.Parse(dpsText.text), value => dpsText.text = value.ToString("N0"), dps, 2f).SetDelay(1f);
    }
}
