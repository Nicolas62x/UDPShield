using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RakNetLib
{
    static class RakData
    {
        public static byte[] magic = new byte[] { 0x00, 0xff, 0xff, 0x00, 0xfe, 0xfe, 0xfe, 0xfe, 0xfd, 0xfd, 0xfd, 0xfd, 0x12, 0x34, 0x56, 0x78 };

        public static byte ProtVersion = 10;

        public enum RakPacketIds
        {
            Connected_Ping,
            Connected_Pong = 0x03,
            Connection_Request = 0x09,
            Connection_Request_Accepted,
            New_Incoming_Connection = 0x13,
            Disconnect = 0x15,
            Game_Packet = 0xfe,
        }

        public enum PacketID
        {
            Connected_Ping = 0x0,
            Unconnected_Ping = 0x1,
            Connected_Pong = 0x3,
            Open_Connection_Request = 0x5,
            Open_Connection_Reply = 0x6,
            Open_Connection_Request2 = 0x7,
            Open_Connection_Reply2 = 0x8,
            Unconnected_Pong = 0x1c,
            Frame_Set_Packet = 0x84,
        }
    }
}
