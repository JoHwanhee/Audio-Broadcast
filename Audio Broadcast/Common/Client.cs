using Audio_Broadcast.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Audio_Broadcast
{
    public class Client
    {
        private readonly Socket _clientSocket;
        private readonly Thread _receiveThread;
        private readonly Thread _sendThread;

        private AudioSingleTone audiomanager = AudioSingleTone.GetAudio();

        public bool IsReceiveThreadOpened { get; set; }
        public bool IsSendThreadOpened { get; set; }
        public bool IsClientOpened { get; set; }

        public Client(Socket acceptSocket)
        {
            _clientSocket = acceptSocket;
            _receiveThread = new Thread(ReceiveThreadProc);
            _sendThread = new Thread(SendThreadProc);
        }

        public void Run()
        {
            Initialize();
            _receiveThread.Start();
            _sendThread.Start();
        }

        private void Initialize()
        {
            MessageBox.Show("Initialize");
            IsClientOpened = true;
            IsReceiveThreadOpened = false;
            IsSendThreadOpened = false;
        }

        private void ReceiveThreadProc()
        {
            while (IsClientOpened)
            {
                while (IsReceiveThreadOpened)
                {


                    Thread.Sleep(100);
                }
                Thread.Sleep(100);
            }
        }

        private void SendThreadProc()
        {
            while (IsClientOpened)
            {
                while (IsSendThreadOpened)
                {
                    byte[] senddata = audiomanager.GetAudioData();
                    _clientSocket.Send(senddata);
                }
                Thread.Sleep(100);
            }
        }
    }
}
