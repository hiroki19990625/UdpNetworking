using System;
using BinaryIO;

namespace UdpNetworking.Packet.LowLevel
{
    public class ConnectionRequestPacket : LowLevelPacket
    {
        public override int PacketId { get; } = Global.ConnectionRequestPacket;

        public DateTime Date { get; }

        public ConnectionRequestPacket(DateTime date)
        {
            Date = date;
        }

        public override byte[] Encode()
        {
            base.Encode();

            _networkStream.WriteDateTime(Date, ByteOrder.Little);

            return _networkStream.GetBuffer();
        }

        public override void Decode(byte[] buf)
        {
            base.Decode(buf);

            _networkStream.ReadDateTime(ByteOrder.Little);
        }
    }
}