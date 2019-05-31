using Core.Util;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;

namespace Chronos.Audio
{
    public class AudioPlayer : Observable
    {
        private static AudioPlayer _instance;
        public static AudioPlayer I
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AudioPlayer();
                }
                return _instance;
            }
        }

        private DirectSoundOut _audioOutput;
        private MixingSampleProvider mixer;

        public WaveFormat Format {get; private set;}

        public static WaveFormat StreamFormat { get; private set; } = new WaveFormat(48000, 16, 2);

        public static void SetStreamChannels(int channels)
        {
            if (StreamFormat.Channels != channels)
                StreamFormat = new WaveFormat(StreamFormat.SampleRate, StreamFormat.BitsPerSample, channels);
        }

        private AudioPlayer()
        {
            Format = WaveFormat.CreateIeeeFloatWaveFormat(48000, 2);
        }

        public void Init(int channels)
        {
            if (Format.Channels == channels)
                return;

            Format = WaveFormat.CreateIeeeFloatWaveFormat(48000, channels);

            mixer = new MixingSampleProvider(Format);
            mixer.ReadFully = true;

            if (_audioOutput != null) _audioOutput.Dispose();
            _audioOutput = new DirectSoundOut();
            _audioOutput.Init(mixer);
            _audioOutput.Play();
        }

        public void Resume()
        {
            _audioOutput.Play();
        }

        public void Pause()
        {
            _audioOutput.Pause();
        }

        private ISampleProvider ConvertToRightChannelCount(ISampleProvider input)
        {
            if (input.WaveFormat.Channels == mixer.WaveFormat.Channels)
            {
                return input;
            }
            if (input.WaveFormat.Channels == 1 && mixer.WaveFormat.Channels == 2)
            {
                return new MonoToStereoSampleProvider(input);
            }
            if (input.WaveFormat.Channels == 2 && mixer.WaveFormat.Channels == 1)
            {
                return new StereoToMonoSampleProvider(input);
            }
            throw new NotImplementedException("Not yet implemented this channel count conversion");
        }

        private void AddMixerInput(ISampleProvider input)
        {
            mixer.AddMixerInput(ConvertToRightChannelCount(input));
        }

        public void PlaySound(CachedSound sound)
        {
            AddMixerInput(new CachedSoundSampleProvider(sound));
        }

        public void Dispose()
        {
            _audioOutput.Dispose();
        }
    }
}
