using RakNetLib.Data;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading;

namespace UDPShield
{
    static class Program
    {
        static EndPoint Any = new IPEndPoint(IPAddress.IPv6Any, 19132);
        static ArrayPool<byte> pool = ArrayPool<byte>.Shared;
        static Socket MainSocket = new Socket(SocketType.Dgram, ProtocolType.Udp);

        static AsyncSender MainSender = new AsyncSender(MainSocket);

        static long Guid = new Random().NextInt64();

        static string Motd = $"MCPE;§f    UDPShield;0;;0;42;{Guid};;";

        static Thread t = new Thread(MotdUpdater);

        static IPEndPoint server = new IPEndPoint(IPAddress.Parse("136.243.59.145"), 19132);


        static void MotdUpdater()
        {
            while (true)
            {
                Thread.Sleep(5000);

                try
                {
                    string tmp = BedrockPinger.Ping(server);
                                        
                    Motd = tmp.Replace(tmp.Split(';')[6], Guid.ToString()).Replace(tmp.Split(';')[7],"UDPShield");
                }
                catch
                {
                }
            }
        }

        static void Main(string[] args)
        {
            MainSocket.Bind(Any);
            StartListening();
            
            t.Start();

            while (true)
            {

            }
        }

        static void StartListening()
        {
            byte[] buf = pool.Rent(1500);

            a:

            try
            {
                MainSocket.BeginReceiveFrom(buf, 0, buf.Length, SocketFlags.None, ref Any, OnMainRCV, buf);
            }
            catch
            {
                goto a;
            }
        }

        static void OnMainRCV(IAsyncResult res)
        {
            EndPoint ep = new IPEndPoint(IPAddress.IPv6Any, 0);
            byte[] buf = (byte[])res.AsyncState;
            int len = 0;

            try
            {
                len = MainSocket.EndReceiveFrom(res, ref ep);
            }
            catch 
            {
            }

            StartListening();

            if (len > 0)
            {
                OnReceived(buf, len, ep);
            }

            pool.Return(buf);
        }


        static void OnReceived(byte[] buf, int len, EndPoint ep)
        {
            switch (buf[0])
            {
                case 0x01:
                case 0x02:

                    if (Magic.IsMagic(buf,9))
                    {
                        List<byte> msg = new List<byte>();

                        msg.Add(0x1c);

                        Long.AddLong(msg, DateTimeOffset.Now.ToUnixTimeMilliseconds());
                        Long.AddLong(msg, Guid);

                        msg.AddRange(Magic.Value);

                        RakNetLib.Data.String.AddString(msg, Motd);

                        MainSender.Enqueue(msg.ToArray(), ep);
                    }

                    break;

                case 0x05:

                    bool Accepted = false;

                    if (len >= 512 && Magic.IsMagic(buf,1) && buf[17] == 10)
                    {
                        for (int i = 18; i < len; i++)
                        {
                            if (i == len - 1 && buf[i] == 0)
                                Accepted = true;
                            else if (buf[i] != 0)
                                break;
                        }
                    }

                    if (Accepted)
                        lock (Pipe.locker)
                            if (Pipe.Connections.TryGetValue(ep, out Pipe p))
                                p.SendData(buf, len);
                            else
                                CreatePipe(buf, len, ep);

                    break;

                case 0xfe:

                    if (buf[1] == 0xfd && (buf[2] == 0x09 || buf[2] == 0x0))
                        lock (Pipe.locker)
                            if (Pipe.Connections.TryGetValue(ep, out Pipe p))
                                p.SendData(buf, len);
                            else
                                CreatePipe(buf, len, ep);

                    break;

                case 0xa0:
                case 0xc0:
                case 0x8c:
                case 0x09:
                case 0x13:
                case 0x84:
                case 0x07:

                    lock (Pipe.locker)
                        if (Pipe.Connections.TryGetValue(ep, out Pipe p))
                            p.SendData(buf, len);

                    break;
            }
        }

        static void CreatePipe(byte[] data, int len, EndPoint ep)
        {
            Pipe p = new Pipe(ep, server, MainSocket);

            p.SendData(data, len);

            Pipe.Connections.Add(ep, p);
        }

    }
}
