using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UDPShield
{
    static class BedrockPinger
    {
        static Socket s = new Socket(SocketType.Dgram, ProtocolType.Udp);
        
        static ArrayPool<byte> pool = ArrayPool<byte>.Shared;

        static BedrockPinger()
        {
            s.Bind(new IPEndPoint(IPAddress.IPv6Any, 0));
        }

        //                                        | ID |          Time          |                               MAGIC                                 |          GUID          |
        static byte[] UnConnectedPing = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 0, 254, 254, 254, 254, 253, 253, 253, 253, 18, 52, 86, 120, 0, 0, 0, 0, 0, 0, 0, 0 };

        public static string Ping(IPEndPoint ep)
        {
            EndPoint Ep = ep;

            s.SendTo(UnConnectedPing, ep);

            byte[] buf = pool.Rent(1500);

            try
            {
                int len = s.ReceiveFrom(buf, ref Ep);

                if (len <= 35)
                    throw new Exception("Couldn't read server's Motd Response");

                return Encoding.UTF8.GetString(buf, 35, len - 35);

            }
            catch (Exception e)
            {
                throw;
            }
            finally
            {
                pool.Return(buf);
            }
        }

    }
}
