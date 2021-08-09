using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UDPShield
{
    class AsyncSender
    {
        struct AsyncMessage
        {
            public byte[] data;
            public int len;
            public EndPoint ep;
        }

        Socket s;
        Queue<AsyncMessage> messages = new Queue<AsyncMessage>();

        public bool Disposed = false;

        object Locker = new object();
        bool Sending = false;

        public AsyncSender(Socket s) => this.s = s;

        public void Enqueue(byte[] data, EndPoint ep) => Enqueue(data, data.Length, ep);
        public void Enqueue(byte[] data, int len, EndPoint ep)
        {
            if (Disposed)
                return;

            AsyncMessage mes;
            mes.data = data;
            mes.len = len;
            mes.ep = ep;

            lock (Locker)
            {
                if (Sending)
                    messages.Enqueue(mes);

                StartSending(mes);
            }
        }

        void StartSending(AsyncMessage mes)
        {
            if (Disposed)
                return;

            Sending = true;

            byte test = 0;

            a:

            try
            {
                test++;
                s.BeginSendTo(mes.data, 0, mes.len, SocketFlags.None, mes.ep, OnSent, this);
            }
            catch (Exception)
            {
                if (test < 5)
                    goto a;
            }
        }


        static void OnSent(IAsyncResult res)
        {
            AsyncSender sender = (AsyncSender)res.AsyncState;

            try
            {
                sender.s.EndSendTo(res);
            }
            catch (Exception)
            {

            }

            lock (sender.Locker)
            {
                sender.Sending = false;

                if (sender.messages.Count > 0)
                    sender.StartSending(sender.messages.Dequeue());
            }
        }
    }
}
