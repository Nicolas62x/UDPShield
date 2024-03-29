﻿using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace UDPShield;
class Pipe
{
    EndPoint ep;
    EndPoint Server;
    public Socket s;
    AsyncSender ServerSender;
    AsyncSender ClientSender;
    DateTimeOffset lastServerTransmition = DateTimeOffset.Now;

    static ArrayPool<byte> pool = ArrayPool<byte>.Shared;

    public static Dictionary<EndPoint, Pipe> Connections = new Dictionary<EndPoint, Pipe>();

    static Dictionary<ushort, EndPoint> PortToIp = new Dictionary<ushort, EndPoint>();

    public static object locker = new object();

    static Thread t = new Thread(PipeTimeOuter);

    bool disposed = false;

    static Pipe()
    {
        t.Start();
    }

    static void PipeTimeOuter()
    {
        while (true)
        {
            Thread.Sleep(5000);

            try
            {
                Pipe[] pipes;

                lock (locker)
                    pipes = Connections.Values.ToArray();

                for (int i = 0; i < pipes.Length; i++)
                {
                    if ((DateTimeOffset.Now - pipes[i].lastServerTransmition).TotalSeconds > 30)
                    {
                        pipes[i].Dispose();

                        ushort port = (ushort)((IPEndPoint)pipes[i].s.LocalEndPoint).Port;

                        lock (locker)
                        {
                            PortToIp.Remove(port);
                            Connections.Remove(pipes[i].ep);
                        }

                    }

                }
            }
            catch (Exception)
            {
            }
        }
    }
    //must be called in locked context (locker)
    public Pipe(EndPoint User, EndPoint server, Socket serverSocket)
    {
        this.ep = User;
        this.Server = server;

        s = new Socket(SocketType.Dgram, ProtocolType.Udp);
        s.Bind(new IPEndPoint(IPAddress.IPv6Any, 0));

        PortToIp.Add((ushort)((IPEndPoint)s.LocalEndPoint).Port, User);

        ServerSender = new AsyncSender(s);
        ClientSender = new AsyncSender(serverSocket);

        StartListening();
    }

    byte[] buf = null;

    void StartListening()
    {
        if (disposed)
            return;

        buf = pool.Rent(1500);

    a:

        try
        {
            s.BeginReceiveFrom(buf, 0, buf.Length, SocketFlags.None, ref Server, OnRcv, this);
        }
        catch
        {
            if (disposed)
            {
                pool.Return(buf);
                return;
            }

            goto a;
        }
    }

    static void OnRcv(IAsyncResult res)
    {
        Pipe p = (Pipe)res.AsyncState;

        byte[] buf = p.buf;

        int len = 0;

        try
        {
            len = p.s.EndReceiveFrom(res, ref p.Server);
        }
        catch
        {
        }

        p.StartListening();

        if (len > 0)
        {
            p.lastServerTransmition = DateTimeOffset.Now;
            p.ClientSender.Enqueue(buf, len, p.ep);

            lock (DataLogger.locker)
            {
                DataLogger.OutBound += (UInt64)len;
                DataLogger.OutPackets++;
            }
        }

        pool.Return(buf);
    }

    void Dispose()
    {
        if (disposed)
            return;

        disposed = true;

        ClientSender.Disposed = true;
        ServerSender.Disposed = true;

        try
        {
            s.Dispose();
        }
        catch
        {
        }
    }

    public void SendData(byte[] data, int len)
    {
        ServerSender.Enqueue(data, len, Server);
    }

    public static IPEndPoint GetIpWithPort(ushort port)
    {
        lock (locker)
            if (PortToIp.TryGetValue(port, out EndPoint ep))
                return (IPEndPoint)ep;
        return null;
    }

}
