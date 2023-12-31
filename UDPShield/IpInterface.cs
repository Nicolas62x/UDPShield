using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace UDPShield;
static class IpInterface
{
    static Socket s = new Socket(Program.server.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

    public static void Init(ushort port = 42024)
    {
        s.Bind(new IPEndPoint(IPAddress.Any, port));
        s.Listen(5);

        s.BeginAccept(OnCo, null);

        Task.Run(() =>
        {

            while (true)
            {
                Pipe[] pipes = Pipe.Connections.Values.ToArray();

                foreach (Pipe item in pipes)
                {
                    IPEndPoint ep = Pipe.GetIpWithPort((ushort)((IPEndPoint)item.s.LocalEndPoint).Port);
                }

                Thread.Sleep(2000);

            }

        });

    }

    static void OnCo(IAsyncResult res)
    {
        try
        {
            Socket Co = s.EndAccept(res);
        }
        catch (Exception)
        {

        }
    }

}