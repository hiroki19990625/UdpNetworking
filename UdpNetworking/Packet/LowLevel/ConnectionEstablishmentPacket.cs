using BinaryIO;

namespace UdpNetworking.Packet.LowLevel
{
    public class ConnectionEstablishmentPacket : LowLevelPacket
    {
        public override byte PacketId { get; } = Global.ConnectionEstablishmentPacket;

        public ushort MtuSize { get; private set; }

        public ConnectionEstablishmentPacket()
        {
        }

        public ConnectionEstablishmentPacket(ushort mtuSize)
        {
            MtuSize = mtuSize;
        }

        public override byte[] Encode()
        {
            base.Encode();

            _networkStream.WriteUShort(MtuSize, ByteOrder.Little);

            return _networkStream.GetBuffer();
        }

        public override void Decode(byte[] buf)
        {
            base.Decode(buf);

            MtuSize = _networkStream.ReadUShort(ByteOrder.Little);
        }
    }
}