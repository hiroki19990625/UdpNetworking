using System;
using System.Collections;
using System.Linq;
using BinaryIO;

namespace UdpNetworking.Packet.LowLevel
{
    public class ConnectionRequestPacket : LowLevelPacket
    {
        public override byte PacketId { get; } = Global.ConnectionRequestPacket;

        public DateTime Date { get; private set; }
        public byte[] Padding { get; private set; }

        public ConnectionRequestPacket()
        {
        }

        public ConnectionRequestPacket(DateTime date, byte[] padding)
        {
            Date = date;
            Padding = padding;
        }

        public override byte[] Encode()
        {
            base.Encode();

            _networkStream.WriteBytes(Global.Magic.ToArray());
            _networkStream.WriteDateTime(Date, ByteOrder.Little);
            _networkStream.WriteBytes(Padding);

            return _networkStream.GetBuffer();
        }

        public override void Decode(byte[] buf)
        {
            base.Decode(buf);

            CheckMagic(_networkStream.ReadBytes(Global.Magic.Count));
            Date = _networkStream.ReadDateTime(ByteOrder.Little);
            Padding = _networkStream.ReadBytes();
        }

        private void CheckMagic(byte[] buf)
        {
            if (!(buf.Length == 8 &&
                  ((IStructuralEquatable) buf).Equals(Global.Magic, StructuralComparisons.StructuralEqualityComparer)))
                throw new InvalidPacketException("Magic dont match.");
        }
    }
}