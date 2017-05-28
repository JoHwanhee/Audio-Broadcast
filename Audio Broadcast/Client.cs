using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using NAudio.Codecs;
using NAudio.Wave;

namespace Audio_Broadcast
{
    class Client
    {
        private Socket client;
        private Thread receiveThread;
        private Thread sendThread;
        private bool isOpend;
        private IWaveIn wavein;

        private Mutex Lock = new Mutex();
        private WaveFormat CommonFormat;

        private Semaphore SenderKick = new Semaphore(0, int.MaxValue);
        private LinkedList<byte[]> SenderQueue = new LinkedList<byte[]>();

        private delegate byte EncoderMethod(short _raw);

        private EncoderMethod Encoder = MuLawEncoder.LinearToMuLawSample;

        public void StopVoice()
        {
            wavein.StopRecording();
            try { wavein.Dispose(); } catch (Exception) { }
            SenderKick.Release();
        }

        public Client(Socket socket)
        {
            client = socket;
            receiveThread = new Thread(receiveProc);
            sendThread = new Thread(sendProc);

            isOpend = true;

            AudioRun();
            receiveThread.Start();
            sendThread.Start();
        }

        private void receiveProc()
        {
            while (isOpend)
            {
                byte[] buff = new byte[1024];
                int n = 0;
                n = client.Receive(buff);

                if (n > 0)
                {
                    string receive = buff + "";
                    if (receive.Equals(@"/quit"))
                    {
                        isOpend = false;
                        StopVoice();
                        client?.Dispose();
                    }
                }

            }
        }

        private void sendProc()
        {
            byte[] qbuffer = null;
            while (isOpend)
            {
                SenderKick.WaitOne();

                Lock.WaitOne();
                bool dataavailable = (SenderQueue.Count != 0);
                if (dataavailable)
                {
                    qbuffer = SenderQueue.First.Value;
                    SenderQueue.RemoveFirst();
                }
                Lock.ReleaseMutex();

                if (!dataavailable) break;

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
                client.Send(g711buff);
            }
        }


        public void AudioRun()
        {
            CommonFormat = new WaveFormat(16000, 16, 1);
            wavein = new WaveInEvent();
            wavein.WaveFormat = CommonFormat;
            wavein.DataAvailable += wavein_DataAvailable;
            wavein.StartRecording();
            MessageBox.Show("audio run ");
        }

        void wavein_DataAvailable(object sender, WaveInEventArgs e)
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
