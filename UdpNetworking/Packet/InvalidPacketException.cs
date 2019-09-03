using System;

namespace UdpNetworking.Packet
{
    public class InvalidPacketException : Exception
    {
        public InvalidPacketException(string msg) : base(msg)
        {
        }
    }
}