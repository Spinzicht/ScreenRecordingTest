using Core.Networking;
using System;
using Codec = FragLabs.Audio.Codecs;

namespace Chronos.Audio
{
    public class NetworkAudioCapture : AudioCapture
    {
        private int packetsSize = 0;
        private int packetsReceived = 0;

        public NetworkAudioCapture()
        {
            PacketHandler.I.AddPacket(Packet.AudioInit, AudioInitReceived);
            PacketHandler.I.AddPacket(Packet.Audio, AudioReceived);
        }

        private void AudioInitReceived(IPPacket data)
        {
            if (_started)
                return;

            data.Read(out int samples, out bool s);
            data.Read(out int channels, out s);

            if (channels != 1 && channels != 2) return;

            _syncContext.Post(o =>
            {
                AudioPlayer.SetStreamChannels(channels);
                base.Start();
                
                _decoder = Codec.OpusDecoder.Create(AudioPlayer.StreamFormat.SampleRate, AudioPlayer.StreamFormat.Channels);
                //PeerHost.I.Client.SendAudioResponse();

            }, null);
        }

        float fullSpeed = 0.025f;
        private float _playbackRate;
        private int reads = 0;
        private Codec.OpusDecoder _decoder;

        public void AudioReceived(IPPacket p)
        {
            if(_audioStream == null || IsBufferNearlyFull)
                return;

            p.Read(out int frames, out bool s);
            p.Read(out int l, out s);
            byte[] arr = new byte[l];
            p.Read(ref arr, out s);

            _syncContext.Post(o =>
            {
                arr = _decoder.Decode(arr, frames, out l);
                _audioStream.AddSamples(arr, 0, l);
                //_audioStream.BufferedDuration.Print();

                if (_audioStream.BufferedDuration.TotalSeconds < 0.01f)
                {
                    PauseAudio();
                }
                else if (_audioStream.BufferedDuration.TotalSeconds > fullSpeed)
                {
                    PlayAudio();
                }

                _playbackRate += Math.Min(1, Math.Max(0.75f, (float)_audioStream.BufferedDuration.TotalSeconds / fullSpeed));
                reads++;

                _speedControl.PlaybackRate = _playbackRate / reads;
                //_speedControl.PlaybackRate.Print();

                if (reads > 20)
                {
                    reads /= 2;
                    _playbackRate /= 2;
                }
            }, null);
        }
    }
}