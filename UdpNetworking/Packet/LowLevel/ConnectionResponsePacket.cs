using BinaryIO;

namespace UdpNetworking.Packet.LowLevel
{
    public class ConnectionResponsePacket : LowLevelPacket
    {
        public override byte PacketId { get; } = Global.ConnectionResponsePacket;

        public ushort MtuSize { get; private set; }

        public ConnectionResponsePacket()
        {
        }

        public ConnectionResponsePacket(ushort mtuSize)
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