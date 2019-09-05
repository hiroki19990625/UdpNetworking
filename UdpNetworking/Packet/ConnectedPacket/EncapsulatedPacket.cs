namespace UdpNetworking.Packet.ConnectedPacket
{
    public class EncapsulatedPacket
    {
        public Reliability Reliability { get; } = Reliability.Reliable;
        public uint MessageId { get; }

        public bool IsSplit { get; private set; }
        public ushort SplitId { get; private set; }
        public ushort SplitIndex { get; private set; }
        public ushort LastSplitIndex { get; private set; }

        public byte[] Payload { get; }

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
            ushort lastSplitIndex)
        {
            Reliability = reliability;
            MessageId = messageId;

            IsSplit = true;
            SplitId = splitId;
            SplitIndex = splitIndex;
            LastSplitIndex = lastSplitIndex;
        }

        public EncapsulatedPacket[] GetSplitEncapsulatedPackets(int mtuSize)
        {
            int calcMtu = mtuSize - 67; //DataPacket
            return new EncapsulatedPacket[0];
        }
    }
}