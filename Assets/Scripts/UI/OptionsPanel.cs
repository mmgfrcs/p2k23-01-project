using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace AdInfinitum.UI
{
    public class OptionsPanel : MonoBehaviour
    {
        [SerializeField] private AudioMixer mixer;
        [Header("Options"), SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;

        private CanvasGroup _group;
        private void Start()
        {
            _group = GetComponent<CanvasGroup>();

            _group.alpha = 0;
            _group.blocksRaycasts = false;
        }

        public void Open()
        {
            _group.blocksRaycasts = true;
            _group.DOFade(1f, 1f).SetUpdate(true);
            float mVol, bVol, sVol;
            if (!mixer.GetFloat("MasterVol", out mVol))
                Debug.LogError("Master volume not found");
            if (!mixer.GetFloat("BGMVol", out bVol))
                Debug.LogError("BGM volume not found");
            if (!mixer.GetFloat("SFXVol", out sVol))
                Debug.LogError("SFX volume not found");

            print(mVol + "," + bVol + "," + sVol);

            masterVolumeSlider.value = DbToLinear(mVol);
            musicVolumeSlider.value = DbToLinear(bVol);
            sfxVolumeSlider.value = DbToLinear(sVol);
        }

        public void Close()
        {
            _group.DOFade(0f, 1f).OnComplete(() => {
                _group.blocksRaycasts = false;
            }).SetUpdate(true);
        }

        public void OnMasterVolChange(float vol)
        {
            mixer.SetFloat("MasterVol", LinearToDb(vol));
        }

        public void OnBGMVolChange(float vol)
        {
            mixer.SetFloat("BGMVol", LinearToDb(vol));
        }

        public void OnSFXVolChange(float vol)
        {
            mixer.SetFloat("SFXVol", LinearToDb(vol));
        }

        private float LinearToDb(float linear)
        {
            if (linear == 0) return -80f;
            return Mathf.Log10(linear) * 20;
        }

        private float DbToLinear(float db)
        {
            return Mathf.Pow(10.0f, db / 20.0f);
        }
    }
}