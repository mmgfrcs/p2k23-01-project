using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Pool;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : Singleton<AudioManager>
{
    [SerializeField] private AudioMixerGroup sfxGroup;
    [SerializeField] private AudioClip baseDestroyClip;
    [SerializeField] private AudioClip[] bgmList;
    [SerializeField] private AudioClip[] sfxList;
    
    private AudioSource _bgmSource;
    private ObjectPool<AudioSource> _sfxSource;
    
    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        _bgmSource = GetComponent<AudioSource>();
        _sfxSource = new ObjectPool<AudioSource>(CreateSource, OnGetSource, OnReleaseSource, OnDestroySource);
    }

    private void OnDestroySource(AudioSource obj) => Destroy(obj.gameObject);
    private void OnReleaseSource(AudioSource obj) => obj.gameObject.SetActive(false);
    private void OnGetSource(AudioSource obj) => obj.gameObject.SetActive(true);
    private AudioSource CreateSource() => new GameObject("Audio", typeof(AudioSource), typeof(AudioReturner)).GetComponent<AudioSource>();

    private void Start()
    {
        GameManager.Instance.GameOver += OnGameOver;
        _bgmSource.clip = bgmList.PickOne();
        _bgmSource.Play();
    }

    private void OnGameOver(float delay)
    {
        _bgmSource.DOPitch(0f, delay).SetUpdate(true).onComplete += () => _bgmSource.Stop();
        if (baseDestroyClip == null) return;
        var aRet = _sfxSource.Get().GetComponent<AudioReturner>();
        aRet.transform.position = transform.position;
        aRet.PlayClip(this, baseDestroyClip, sfxGroup);
    }

    public void PlayTowerSFX(Vector3 loc, TowerType tower, EntitySFXType type)
    {
        var clip = sfxList.FirstOrDefault(x => x.name == $"tower_{tower.ToString().ToLower()}_{type.ToString().ToLower()}"); 
        if(clip == null) clip = sfxList.FirstOrDefault(x => x.name == $"tower_{type.ToString().ToLower()}");
        if (clip == null && Application.isEditor) Debug.LogWarning($"Cannot play tower_{tower.ToString().ToLower()}_{type.ToString().ToLower()} SFX");
        else
        {
            var aRet = _sfxSource.Get().GetComponent<AudioReturner>();
            aRet.transform.position = loc;
            aRet.PlayClip(this, clip, sfxGroup);
        }
    }
    
    public void PlayEnemySFX(Vector3 loc, EnemyType enemy, EntitySFXType type)
    {
        var clip = sfxList.FirstOrDefault(x => x.name == $"enemy_{enemy.ToString().ToLower()}_{type.ToString().ToLower()}");
        if(clip == null) clip = sfxList.FirstOrDefault(x => x.name == $"enemy_{type.ToString().ToLower()}");
        if (clip == null && Application.isEditor) Debug.LogWarning($"Cannot play enemy_{enemy.ToString().ToLower()}_{type.ToString().ToLower()} SFX");
        else
        {
            var aRet = _sfxSource.Get().GetComponent<AudioReturner>();
            aRet.transform.position = loc;
            aRet.PlayClip(this, clip, sfxGroup);
        }
    }

    public void ReleaseAudioSource(AudioSource source)
    {
        _sfxSource.Release(source);
    }
}
