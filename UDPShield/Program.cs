using RakNetLib.Data;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace UDPShield;

static class Program
{
    static EndPoint Any;
    static ArrayPool<byte> pool = ArrayPool<byte>.Shared;
    static Socket MainSocket = new Socket(SocketType.Dgram, ProtocolType.Udp);

    static AsyncSender MainSender = new AsyncSender(MainSocket);

    static long Guid = new Random().NextInt64();

    static string Motd = $"MCPE;§f    UDPShield;0;;0;42;{Guid};;";

    static Thread t = new Thread(MotdUpdater);

    public static IPEndPoint server;


    static void MotdUpdater()
    {
        while (true)
        {
            Thread.Sleep(5000);

            try
            {
                string tmp = BedrockPinger.Ping(server);

                Motd = tmp.Replace(tmp.Split(';')[6], Guid.ToString()).Replace(tmp.Split(';')[7], "UDPShield");
            }
            catch
            {
            }
        }
    }

    static void Main(string[] args)
    {
        //args = ["19132", "XXX.XXX.XXX.XXX:19132"];

        try
        {
            ushort In = ushort.Parse(args[0]);

            IPAddress Out = IPAddress.Parse(args[1].Split(':')[0]);
            ushort OutPort = ushort.Parse(args[1].Split(':')[1]);

            Any = new IPEndPoint(IPAddress.IPv6Any, In);

            server = new IPEndPoint(Out, OutPort);
        }
        catch (Exception)
        {
            Console.WriteLine("Invalid Parameters: ");
            Console.WriteLine($"Usage: {System.Reflection.Assembly.GetExecutingAssembly().Location.Replace(".dll", ".exe").Split('/', '\\').Last()} [PortToListen] [ServerAddress:port]\nExemple: {System.Reflection.Assembly.GetExecutingAssembly().Location.Replace(".dll", ".exe").Split('/', '\\').Last()} 19132 127.0.0.1:19133");

            for (int i = 0; i < args.Length; i++)
            {
                Console.WriteLine(args[i]);
            }

            Console.ReadLine();
            return;
        }

        Console.WriteLine($"Running on port: {((IPEndPoint)Any).Port}\nRedirecting to: {server}");

        MainSocket.Bind(Any);
        StartListening();

        IpInterface.Init();

        t.Start();

        TimeSpan dt = TimeSpan.FromMilliseconds(250);
        DateTimeOffset t0 = DateTimeOffset.Now;

        while (true)
        {
            t0 += dt;

            while (DateTimeOffset.Now < t0)
                Thread.Sleep(5);

            DataLogger.Tick();
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

            lock (DataLogger.locker)
            {
                DataLogger.InBound += (UInt64)len;
                DataLogger.InPackets++;
            }
        }

        pool.Return(buf);
    }


    static void OnReceived(byte[] buf, int len, EndPoint ep)
    {
        switch (buf[0])
        {
            case 0x01:
            case 0x02:

                if (Magic.IsMagic(buf, 9))
                {
                    List<byte> msg = [0x1c];

                    Long.AddLong(msg, DateTimeOffset.Now.ToUnixTimeMilliseconds());
                    Long.AddLong(msg, Guid);

                    msg.AddRange(Magic.Value);

                    RakNetLib.Data.String.AddString(msg, Motd);

                    MainSender.Enqueue(msg.ToArray(), ep);
                }
                else
                {
                    lock (DataLogger.locker)
                    {
                        DataLogger.Blocked += (UInt64)len;
                        DataLogger.BlockedPackets++;
                    }
                }

                break;

            case 0x05:

                bool Accepted = false;

                if (len >= 256 && Magic.IsMagic(buf, 1) && buf[17] == 10)
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
                else
                {
                    lock (DataLogger.locker)
                    {
                        DataLogger.Blocked += (UInt64)len;
                        DataLogger.BlockedPackets++;
                    }
                }

                break;

            case 0xfe:

                if (buf[1] == 0xfd && (buf[2] == 0x09 || buf[2] == 0x0))
                    lock (Pipe.locker)
                        if (Pipe.Connections.TryGetValue(ep, out Pipe p))
                            p.SendData(buf, len);
                        else
                            CreatePipe(buf, len, ep);
                else
                {
                    lock (DataLogger.locker)
                    {
                        DataLogger.Blocked += (UInt64)len;
                        DataLogger.BlockedPackets++;
                    }
                }

                break;

            case 0xa0:
            case 0xc0:
            case 0x8c:
            case 0x09:
            case 0x13:
            case 0x84:
            case 0x88:
            case 0x80:
            case 0x07:

                lock (Pipe.locker)
                    if (Pipe.Connections.TryGetValue(ep, out Pipe p))
                        p.SendData(buf, len);
                    else
                    {
                        lock (DataLogger.locker)
                        {
                            DataLogger.Blocked += (UInt64)len;
                            DataLogger.BlockedPackets++;
                        }
                    }

                break;

            default:

                //Console.WriteLine($"Blocked: {buf[0].ToString("X")}");

                lock (DataLogger.locker)
                {
                    DataLogger.Blocked += (UInt64)len;
                    DataLogger.BlockedPackets++;
                }

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