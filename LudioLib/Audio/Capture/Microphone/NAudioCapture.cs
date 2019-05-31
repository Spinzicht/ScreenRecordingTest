using System;
using System.Linq;
using System.Collections.ObjectModel;

using NAudio.Mixer;
using NAudio.Wave;
using Codec = FragLabs.Audio.Codecs;

using Chronos.Networking;
using Core.Networking;
using Core;

namespace Chronos.Audio
{
    public class NAudioCapture : AudioCapture
    {
        int len = 0;
        private ulong _bytesSent;
        private int _segmentFrames;
        private Codec.OpusEncoder _encoder;
        private int _bytesPerSegment;
        private WaveIn _waveIn;
        private Codec.OpusDecoder _decoder;
        private WaveOut _waveOut;

        public ObservableCollection<WaveInCapabilities> Microphones { get; set; } = new ObservableCollection<WaveInCapabilities>();

        private int _index = -1;
        protected int CurrentMicrophoneIndex
        {
            get => _index;
            set => Set(ref _index, value, nameof(CurrentMicrophone));
        }

        public WaveInCapabilities CurrentMicrophone
        {
            get { return Microphones[CurrentMicrophoneIndex]; }
        }

        private WaveIn _audioInput;
        private UnsignedMixerControl volumeControl;
        private WaveFileWriter writer;

        public double? MicrophoneLevel
        {
            get => volumeControl?.Percent;
            private set
            {
                if (volumeControl != null)
                    volumeControl.Percent = value.Value;
            }
        }

        public NAudioCapture()
        {
            for (int i = 0; i < WaveIn.DeviceCount; i++)
            {
                Microphones.Add(WaveIn.GetCapabilities(i));
            }
            if (Microphones.Any())
            {
                CurrentMicrophoneIndex = 0;
            }
            foreach (var mic in Microphones)
                mic.ProductName.Print();

            PacketHandler.I.AddPacket(Packet.AudioInit, StreamInitialized);
        }

        private void StreamInitialized(IPPacket data)
        {
            _syncContext.Post(o =>
            {
                _streamInitialized = true;
            }, null);
        }

        bool _streamInitialized = false;
        public override void Start()
        {
            AudioPlayer.SetStreamChannels(CurrentMicrophone.Channels);
            var format = AudioPlayer.StreamFormat;

            _bytesSent = 0;
            _segmentFrames = 2880;
            _encoder = Codec.OpusEncoder.Create(format.SampleRate, format.Channels, Codec.Opus.Application.Voip);
            _decoder = Codec.OpusDecoder.Create(format.SampleRate, format.Channels);
            _encoder.Bitrate = 8192;
            _bytesPerSegment = _encoder.FrameByteCount(_segmentFrames);

            _audioInput = new WaveIn();
            _audioInput.DeviceNumber = CurrentMicrophoneIndex;
            _audioInput.DataAvailable += DataAvailable;
            _audioInput.RecordingStopped += RecordingStopped;
            _audioInput.WaveFormat = format;

            base.Start();

            _audioInput.StartRecording();

            playing = true;
        }

        private void RecordingStopped(object sender, StoppedEventArgs e)
        {
            "recording stopped".Print();
            writer?.Dispose();
            _audioInput?.Dispose();
        }

        byte[] _notEncodedBuffer = new byte[0];
        private void DataAvailable(object sender, WaveInEventArgs e)
        {
            //("audio avaiable: " + e.Buffer.Length).Print();
            //Data.AddRange(e.Buffer);
            if (!_streamInitialized)
            {
                //PeerHandler.I.InitAudio(AudioPlayer.StreamFormat.SampleRate, AudioPlayer.StreamFormat.Channels);
            }
            else
            {
                byte[] soundBuffer = new byte[e.BytesRecorded + _notEncodedBuffer.Length];
                for (int i = 0; i < _notEncodedBuffer.Length; i++)
                    soundBuffer[i] = _notEncodedBuffer[i];
                for (int i = 0; i < e.BytesRecorded; i++)
                    soundBuffer[i + _notEncodedBuffer.Length] = e.Buffer[i];

                int byteCap = _bytesPerSegment;
                int segmentCount = (int)Math.Floor((decimal)soundBuffer.Length / byteCap);
                int segmentsEnd = segmentCount * byteCap;
                int notEncodedCount = soundBuffer.Length - segmentsEnd;
                _notEncodedBuffer = new byte[notEncodedCount];
                for (int i = 0; i < notEncodedCount; i++)
                {
                    _notEncodedBuffer[i] = soundBuffer[segmentsEnd + i];
                }

                for (int i = 0; i < segmentCount; i++)
                {
                    byte[] segment = new byte[byteCap];
                    for (int j = 0; j < segment.Length; j++)
                        segment[j] = soundBuffer[(i * byteCap) + j];
                    int len;
                    byte[] buff = _encoder.Encode(segment, segment.Length, out len);
                    _bytesSent += (ulong)len;

                    /*PeerHandler.I.Client?.SendAudio(buff, segment.Length, len);

                    if(PeerHandler.I.Client == null)
                    {
                        buff = _decoder.Decode(buff, len, out len);
                        _audioStream.AddSamples(buff, 0, len);
                    }*/
                }

            }
            //WriteToFile(buffer, bytesRecorded);
        }

        bool playing = false;
        public override void Stop()
        {
            if (!playing) return;
            playing = false;

            writer?.Close();

            if(_audioInput != null)
                _audioInput.StopRecording();
        }

        public void InitVolume(double volume)
        {
            int waveInDeviceNumber = _audioInput.DeviceNumber;
            if (Environment.OSVersion.Version.Major >= 6) // Vista and over
            {
                var mixerLine = _audioInput.GetMixerLine();
                //new MixerLine((IntPtr)waveInDeviceNumber, 0, MixerFlags.WaveIn);
                foreach (var control in mixerLine.Controls)
                {
                    control.ControlType.Print();
                    if (control.ControlType == MixerControlType.Volume)
                    {
                        this.volumeControl = control as UnsignedMixerControl;
                        MicrophoneLevel = volume;
                        volume.Print();
                        break;
                    }
                }
            }
        }


        private void WriteToFile(byte[] buffer, int bytesRecorded)
        {
            long maxFileLength = AudioPlayer.StreamFormat.AverageBytesPerSecond * 5;

            var toWrite = (int)Math.Min(maxFileLength - writer.Length, bytesRecorded);
            len += toWrite;

            if (toWrite > 0 && len < maxFileLength) 
            {
                writer.Write(buffer, 0, toWrite);
                
            }
            else
            {
                Stop();
            }
        }

        ~NAudioCapture()
        {
            Stop();
        }
    }
}
