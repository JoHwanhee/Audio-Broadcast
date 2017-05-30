using System.Net.Sockets;
using System.Threading;

namespace Audio_Broadcast.Common
{
    interface IServer
    {
        int Port { get; set; }
        bool IsServerOpened { get; set; }
        bool IsAcceptThreadOpened { get; set; }

        void Run();
    }
}
