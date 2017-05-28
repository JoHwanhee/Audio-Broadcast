using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace Audio_Broadcast
{
    class Server
    {
        private Thread _acceptThread;
        private Socket sock;
        public int Port {get;  set; }
        public bool IsOpend { get; private set; }

        public Server(int port)
        {
            try
            {
                Port = port;
                sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IsOpend = true;
            }
            catch (Exception e)
            {
                MessageBox.Show("error : " + e);
            }
        }

        public bool OpenServer()
        {
            try
            {
                IPEndPoint ep = new IPEndPoint(IPAddress.Any, Port);
                sock.Bind(ep);
                sock.Listen(10);
            }
            catch (Exception e)
            {
                MessageBox.Show(e + "");
                throw;
            }

            return IsOpend;
        }

        public void AcceptThreadProc()
        {
            try
            {
                while (IsOpend)
                {
                    new Client(sock.Accept());
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e + "");
            }
        }

        public void Run()
        {
            try
            {
                OpenServer();
                _acceptThread = new Thread(AcceptThreadProc);
                _acceptThread.Start();
            }
            catch (Exception e)
            {
                MessageBox.Show(e + "");
            }
          
        }
    }
}
