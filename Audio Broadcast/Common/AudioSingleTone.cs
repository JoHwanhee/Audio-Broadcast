using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Codecs;
using NAudio.Wave;

namespace Audio_Broadcast.Common
{
    class AudioSingleTone 
    {

        private AudioSingleTone() { }

        private static AudioSingleTone audioInstance;

        public static AudioSingleTone GetAudio()
        {
            if (audioInstance != null)
            {
                audioInstance = new AudioSingleTone();
            }

            return audioInstance;
        }

        private IWaveIn wavein;

        private Mutex Lock = new Mutex();
        private WaveFormat CommonFormat;

        private Semaphore SenderKick = new Semaphore(0, int.MaxValue);
        private LinkedList<byte[]> SenderQueue = new LinkedList<byte[]>();

        private delegate byte EncoderMethod(short _raw);
        private EncoderMethod Encoder = MuLawEncoder.LinearToMuLawSample;

        public bool IsAudioOpened { get; set; }

        public void AudioSetting()
        {
            CommonFormat = new WaveFormat(16000, 16, 1);
            wavein = new WaveInEvent();
            wavein.WaveFormat = CommonFormat;
            wavein.DataAvailable += wavein_DataAvailable;
        }

        public void AudioStart()
        {
            IsAudioOpened = true;
            wavein.StartRecording();
        }

        public void AudioStop()
        {
            IsAudioOpened = false;
            wavein.StopRecording();
        }

        public byte[] GetAudioData()
        {
            byte[] qbuffer = null;

            SenderKick.WaitOne();

            Lock.WaitOne();
            bool dataavailable = (SenderQueue.Count != 0);
            if (dataavailable)
            {
                qbuffer = SenderQueue.First.Value;
                SenderQueue.RemoveFirst();
            }
            Lock.ReleaseMutex();

            if (!dataavailable) return null;

            int numsamples = qbuffer.Length / sizeof(short);
            byte[] g711buff = new byte[numsamples];

            unsafe
            {
                fixed (byte* inbytes = &qbuffer[0])
                fixed (byte* outbytes = &g711buff[0])
                {
                    short* buff = (short*)inbytes;

                    for (int index = 0; index < numsamples; ++index)
                    {
                        outbytes[index] = Encoder(buff[index]);
                    }
                }
            }

            return g711buff;
        }

        private void wavein_DataAvailable(object sender, WaveInEventArgs e)
        {
            byte[] buffer = new byte[e.BytesRecorded];
            Buffer.BlockCopy(e.Buffer, 0, buffer, 0, e.BytesRecorded);

            Lock.WaitOne();
            SenderQueue.AddLast(buffer);
            Lock.ReleaseMutex();
            SenderKick.Release();
        }
    }
}
