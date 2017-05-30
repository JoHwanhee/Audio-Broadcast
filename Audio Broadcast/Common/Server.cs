using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Audio_Broadcast.Common
{
    class Server 
    {
        private readonly Thread _accepThread;
        private readonly Socket _serverSocket;
        private Dictionary<string, Client> _clientDictionary = new Dictionary<string, Client>();
        private IEnumerable<Client> _clientEnumerable => _clientDictionary.Values;

        private AudioSingleTone audioManager = AudioSingleTone.GetAudio();

        public int Port { get; set; }
        public bool IsServerOpened { get; set; }
        public bool IsAcceptThreadOpened { get; set; }


        public Server(int port)
        {
            Port = port;

            _accepThread = new Thread(AcceptThreadProc);
            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Run()
        {
            Initialise();
            ServerSetting();
            AudioSetting();
            _accepThread.Start();
        }

        public void ClientStop(string cliname)
        {
            Clistop(_clientDictionary[cliname]);
        }

        public void AudioStrt()
        {
            audioManager.AudioStart();
        }

        public void AudioStop()
        {
            audioManager.AudioStart();
        }

        private void ServerSetting()
        {
            IPEndPoint ep = new IPEndPoint(IPAddress.Any, Port);
            _serverSocket.Bind(ep);
            _serverSocket.Listen(10);
        }

        private void Initialise()
        {
            IsServerOpened = true;
            
        }

        private void AcceptThreadProc()
        {
            while (IsServerOpened)
            {
                while (IsAcceptThreadOpened)
                {
                    Socket accept = _serverSocket.Accept();
                    Client cli = new Client(accept);
                    cli.Run();
                    // TODO : cli Name 얻어야함
                    _clientDictionary.Add(cli.GetHashCode()+"", cli);

                    Thread.Sleep(100);
                }
                Thread.Sleep(100);
            }
        }

        private void AllClientsRun()
        {
            foreach (var client in _clientEnumerable)
            {
                client.IsClientOpened = true;
                client.IsSendThreadOpened = true;
                client.IsReceiveThreadOpened = true;
            }
        }

        private void ClientsAllStop()
        {
            foreach (var client in _clientEnumerable)
            {
                Clistop(client);
            }
        }

        private void Clistop(Client client)
        {
            client.IsSendThreadOpened = false;
            client.IsReceiveThreadOpened = false;
            client.IsClientOpened = false;
        }

        private void AudioSetting()
        {
            audioManager.AudioSetting();
        }

    }
}
