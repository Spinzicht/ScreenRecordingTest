using Core;
using NAudio.Wave;
using System;

namespace Chronos.Audio
{
    public class AudioCapture : CaptureBase
    {
        protected BufferedWaveProvider _audioStream;

        WaveOut _audioOutput;
        protected SpeedControlSampleProvider _speedControl;

        protected bool IsBufferNearlyFull
        {
            get
            {
                return _audioStream != null &&
                       _audioStream.BufferLength - _audioStream.BufferedBytes
                       < _audioStream.WaveFormat.AverageBytesPerSecond / 4;
            }
        }

        public AudioCapture()
        {
            
        }

        public override void Start()
        {
            _audioStream = new BufferedWaveProvider(AudioPlayer.StreamFormat);
            _audioStream.BufferDuration = TimeSpan.FromSeconds(20); // allow us to get well ahead of ourselves

            _audioOutput = new WaveOut();
            _speedControl = new SpeedControlSampleProvider(_audioStream, 1000, new SoundTouchSharp.SoundTouchProfile(true, false));
            _audioOutput.Init(_speedControl);
            _audioOutput.Play();

            base.Start();
        }

        protected void PlayAudio()
        {
            if(_audioOutput.PlaybackState != PlaybackState.Playing)
            {
                "play".Print();
                _audioOutput.Resume();
            }
        }

        protected void PauseAudio()
        {
            if (_audioOutput.PlaybackState != PlaybackState.Paused)
            {
                "pause".Print();
                _audioOutput.Pause();
            }
        }
    }
}