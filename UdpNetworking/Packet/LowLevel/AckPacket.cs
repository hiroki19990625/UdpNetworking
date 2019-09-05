using System.Collections.ObjectModel;
using BinaryIO;

namespace UdpNetworking.Packet.LowLevel
{
    public class AckPacket : LowLevelPacket
    {
        public override byte PacketId { get; } = Global.AckPacket;

        public ReadOnlyCollection<uint> SequenceIds { get; private set; } = new ReadOnlyCollection<uint>(new uint[0]);

        public AckPacket()
        {
        }

        public AckPacket(uint[] seqIds)
        {
            if (seqIds.Length > 16)
                throw new InvalidPacketException("seqIds max length 16");

            SequenceIds = new ReadOnlyCollection<uint>(seqIds);
        }

        public override byte[] Encode()
        {
            base.Encode();

            _networkStream.WriteByte((byte) SequenceIds.Count);
            foreach (uint sequenceId in SequenceIds)
            {
                _networkStream.WriteUInt(sequenceId, ByteOrder.Little);
            }

            return _networkStream.GetBuffer();
        }

        public override void Decode(byte[] buf)
        {
            base.Decode(buf);

            int len = _networkStream.ReadByte();
            uint[] seqIds = new uint[len];
            for (int i = 0; i < len; i++)
            {
                seqIds[i] = _networkStream.ReadUInt(ByteOrder.Little);
            }

            SequenceIds = new ReadOnlyCollection<uint>(seqIds);
        }
    }
}