using AdInfinitum.Managers;
using UnityEngine;
using UnityEngine.Audio;

namespace AdInfinitum.Utilities
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioReturner : MonoBehaviour
    {
        private AudioSource _source;
        private bool _isPlayed = false;

        private AudioManager _parent;

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
        }    
    
        private void Update()
        {
            if (!_isPlayed) return;

            if (!_source.isPlaying) _parent.ReleaseAudioSource(_source);
        }

        public void PlayClip(AudioManager parent, AudioClip clip, AudioMixerGroup group)
        {
            _isPlayed = true;
            _parent = parent;
            _source.volume = 1.0f;
            _source.clip = clip;
            _source.spatialBlend = 1f;
            _source.outputAudioMixerGroup = group;
            _source.loop = false;
            _source.maxDistance = 40.0f;
            _source.Play();
        }
    }
}
