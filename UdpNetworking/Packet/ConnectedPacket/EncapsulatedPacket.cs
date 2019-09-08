using System.Collections.Generic;
using BinaryIO;

namespace UdpNetworking.Packet.ConnectedPacket
{
    public class EncapsulatedPacket
    {
        public Reliability Reliability { get; private set; }
        public uint MessageId { get; private set; }

        public bool IsSplit { get; private set; }
        public ushort SplitId { get; private set; }
        public ushort SplitIndex { get; private set; }
        public ushort LastSplitIndex { get; private set; }

        public byte[] Payload { get; private set; }

        public EncapsulatedPacket()
        {
        }

        public EncapsulatedPacket(Reliability reliability, uint messageId, byte[] payload)
        {
            Reliability = reliability;
            MessageId = messageId;

            Payload = payload;
        }

        private EncapsulatedPacket(Reliability reliability,
            uint messageId,
            ushort splitId,
            ushort splitIndex,
            ushort lastSplitIndex,
            byte[] payload)
        {
            Reliability = reliability;
            MessageId = messageId;

            IsSplit = true;
            SplitId = splitId;
            SplitIndex = splitIndex;
            LastSplitIndex = lastSplitIndex;
            Payload = payload;
        }

        public EncapsulatedPacket[] GetSplitEncapsulatedPackets(ushort splitId, ushort mtuSize)
        {
            int calcMtu = mtuSize - 80; //DataPacket
            List<EncapsulatedPacket> packets = new List<EncapsulatedPacket>();
            int splitCount = Payload.Length / calcMtu;
            if (splitCount == 0)
            {
                return new[] {this};
            }
            else
            {
                int mod = Payload.Length % calcMtu;
                int splitMod = mod != 0 ? 1 : 0;
                ushort calcSplitCount = (ushort) (splitCount + splitMod - 1);
                BinaryStream stream = new BinaryStream(Payload);
                for (ushort i = 0; i < splitCount; i++)
                {
                    packets.Add(new EncapsulatedPacket(Reliability, MessageId, splitId, i,
                        calcSplitCount, stream.ReadBytes(calcMtu)));
                }

                if (splitMod == 1)
                    packets.Add(new EncapsulatedPacket(Reliability, MessageId, splitId, calcSplitCount, calcSplitCount,
                        stream.ReadBytes(mod)));
            }


            return packets.ToArray();
        }

        public byte[] Encode()
        {
            BinaryStream stream = new BinaryStream();
            stream.WriteByte((byte) ((byte) Reliability << 4 | (IsSplit ? 1 : 0)));
            stream.WriteUInt(MessageId, ByteOrder.Little);
            stream.WriteUShort(SplitId, ByteOrder.Little);
            stream.WriteUShort(SplitIndex, ByteOrder.Little);
            stream.WriteUShort(LastSplitIndex, ByteOrder.Little);

            stream.WriteUShort((ushort) Payload.Length, ByteOrder.Little);
            stream.WriteBytes(Payload);

            return stream.GetBuffer();
        }

        public void Decode(byte[] buf)
        {
            BinaryStream stream = new BinaryStream(buf);
            byte b = stream.ReadByte();
            IsSplit = (b & 0x1) == 1;
            Reliability = (Reliability) (b >> 4);
            MessageId = stream.ReadUInt(ByteOrder.Little);
            SplitId = stream.ReadUShort(ByteOrder.Little);
            SplitIndex = stream.ReadUShort(ByteOrder.Little);
            LastSplitIndex = stream.ReadUShort(ByteOrder.Little);

            int len = stream.ReadUShort(ByteOrder.Little);
            Payload = stream.ReadBytes(len);
        }
    }
}