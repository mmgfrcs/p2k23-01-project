using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioGameOver : MonoBehaviour
{
    private AudioSource _source;
    
    
    // Start is called before the first frame update
    private void Awake()
    {
        _source = GetComponent<AudioSource>();
    }

    private void Start()
    {
        GameManager.Instance.GameOver += OnGameOver;
    }

    private void OnGameOver()
    {
        _source.DOPitch(0f, 1f).SetUpdate(true).onComplete += () => _source.Stop();
    }
}
