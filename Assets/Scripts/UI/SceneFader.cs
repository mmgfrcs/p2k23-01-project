using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class SceneFader : MonoBehaviour
{
    [SerializeField] private float delay = 1;
    [SerializeField] private bool fadeOnStart = true;

    public float Delay => delay;

    private CanvasGroup _group;
    
    // Start is called before the first frame update
    private void Awake()
    {
        _group = GetComponent<CanvasGroup>();
    }

    private void Start()
    {        
        _group.blocksRaycasts = false;
        _group.alpha = 0;
        if(fadeOnStart) Hide();
    }

    public void Show(Action onShown = null)
    {
        _group.alpha = 0;
        _group.DOFade(1f, delay).OnComplete(() =>
        {
            _group.blocksRaycasts = true;
            onShown?.Invoke(); 
        });
    }

    public void Hide(Action onHidden = null)
    {
        _group.alpha = 1;
        _group.blocksRaycasts = false;
        _group.DOFade(0f, delay).OnComplete(() => onHidden?.Invoke());
    }
}
