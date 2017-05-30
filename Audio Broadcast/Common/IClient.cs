using System.Net.Sockets;
using System.Threading;

namespace Audio_Broadcast
{
    interface IClient
    {
        bool IsReceiveThreadOpened { get; set; }
        bool IsSendThreadOpened { get; set; }
        bool IsClientOpened { get; set; }

        void Run();
    }
}
