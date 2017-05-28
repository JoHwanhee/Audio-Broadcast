using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using NAudio.Codecs;
using NAudio.Wave;

namespace Audio_Clinet
{
    class AudioClient
    {
        private Socket sock;
        private IPEndPoint ep;
        private bool isOpend;
         
        private delegate short DecoderMethod(byte _encoded);
        private DecoderMethod Decoder = MuLawDecoder.MuLawToLinearSample;

        private WaveOut waveout = new WaveOut();
        private WaveFormat CommonFormat;
        private BufferedWaveProvider OutProvider;

        private Semaphore ReceiverKick = new Semaphore(0, int.MaxValue);
        private LinkedList<byte[]> ReceiverQueue = new LinkedList<byte[]>();

        public AudioClient(string Ip, int port)
        {
            sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ep = new IPEndPoint(IPAddress.Parse(Ip), port);
            isOpend = true;
        }
        
        public void Stop()
        {
            sock.Send(Encoding.UTF8.GetBytes(@"/quit"));
            isOpend = false;
            sock?.Close();
            waveout?.Dispose();
        }

        private void AudioInitialize()
        {
            CommonFormat = new WaveFormat(16000, 16, 1);
            OutProvider = new BufferedWaveProvider(CommonFormat);
            waveout.Init(OutProvider);
            waveout.Play();
        }

        public void Run()
        {
            try
            {
                sock.Connect(ep);
                AudioInitialize();

                Thread AudioReceiver = new Thread(AudioReceiverProc);
                Thread SockReceiver = new Thread(SocketReceiverProc);
                SockReceiver.Start();
                AudioReceiver.Start();
            }
            catch (Exception e)
            {
                MessageBox.Show(e + "");
                throw;
            }

        }

        void SocketReceiverProc()
        {
            while (isOpend)
            {
                byte[] qbuffer = new byte[1024 * 8];

                sock.Receive(qbuffer);
                ReceiverQueue.AddLast(qbuffer);

                ReceiverKick.Release();
                Thread.Sleep(500);
            }
        }

        void AudioReceiverProc()
        {
            byte[] qbuffer = null;

            while (isOpend)
            {
                ReceiverKick.WaitOne();

                bool dataavailable = (ReceiverQueue.Count != 0);
                if (dataavailable)
                {
                    qbuffer = ReceiverQueue.First.Value;
                    ReceiverQueue.RemoveFirst();
                }

                int numsamples = qbuffer.Length;
                byte[] outbuff = new byte[qbuffer.Length * 2];
                unsafe
                {
                    fixed (byte* inbytes = &qbuffer[0])
                    fixed (byte* outbytes = &outbuff[0])
                    {
                        short* outpcm = (short*)outbytes;

                        for (int index = 0; index < numsamples; ++index)
                        {
                            outpcm[index] = Decoder(inbytes[index]);
                        }
                    }
                }
                OutProvider.AddSamples(outbuff, 0, outbuff.Length);

            }
        }
    }
}
