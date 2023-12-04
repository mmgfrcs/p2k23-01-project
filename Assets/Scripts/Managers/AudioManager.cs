using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Pool;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : Singleton<AudioManager>
{
    [SerializeField] private AudioClip[] bgmList;
    
    private AudioSource _bgmSource;
    private ObjectPool<AudioSource> _sfxSource;
    
    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        _bgmSource = GetComponent<AudioSource>();
        _sfxSource = new ObjectPool<AudioSource>(CreateSource, OnGetSource, OnReleaseSource, OnDestroySource);
    }

    private void OnDestroySource(AudioSource obj) => Destroy(obj);
    private void OnReleaseSource(AudioSource obj)
    {
        obj.Stop();
        obj.enabled = false;
    }
    private void OnGetSource(AudioSource obj) => obj.enabled = true;
    private AudioSource CreateSource() => gameObject.AddComponent<AudioSource>();
    

    private void Start()
    {
        GameManager.Instance.GameOver += OnGameOver;
        _bgmSource.clip = bgmList.PickOne();
        _bgmSource.Play();
    }

    private void OnGameOver(float delay)
    {
        _bgmSource.DOPitch(0f, delay).SetUpdate(true).onComplete += () => _bgmSource.Stop();
    }
}
