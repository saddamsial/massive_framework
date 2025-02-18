﻿using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace MassiveCore.Framework.Runtime
{
    public class Sound : PoolObject, ISound
    {
        [SerializeField]
        private AudioSource _audioSource;

        private float _initialVolume;
        private float _initialPitch;

        public bool Playing => _audioSource.isPlaying;

        private void Awake()
        {
            InitializeInitialAudioSourceValues();
        }

        private void OnDisable()
        {
            Stop();
        }

        public UniTask Play(float volumeScale = 1f, float pitchScale = 1f)
        {
            if (_audioSource.isPlaying)
            {
                return UniTask.CompletedTask;
            }

            _audioSource.volume = _initialVolume * volumeScale;
            _audioSource.pitch = _initialPitch * pitchScale;
            _audioSource.Play();

            _logger.Print($"Sound \"{Id}\" play!");

            var task = Observable.EveryUpdate().TakeWhile(_ => _audioSource.isPlaying).ToUniTask();
            return task;
        }

        public void Stop()
        {
            Reset();
        }

        public override void Return()
        {
            Reset();
            base.Return();
        }

        private void InitializeInitialAudioSourceValues()
        {
            _initialVolume = _audioSource.volume;
            _initialPitch = _audioSource.pitch;
        }

        private void Reset()
        {
            if (!Playing)
            {
                return;
            }
            _audioSource.Stop();
            _logger.Print($"Sound \"{Id}\" stop!");
        }
    }
}
