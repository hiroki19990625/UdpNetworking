namespace UdpNetworking.Packet.HighLevel
{
    public class CustomDataPacket : HighLevelPacket
    {
        public override uint PacketId { get; } = Global.CustomDataPacket;

        public byte[] Payload { get; private set; }

        public CustomDataPacket()
        {
        }

        public CustomDataPacket(byte[] payload)
        {
            Payload = payload;
        }

        public override byte[] Encode()
        {
            base.Encode();

            _networkStream.WriteUVarInt((uint) Payload.Length);
            _networkStream.WriteBytes(Payload);

            return _networkStream.GetBuffer();
        }

        public override void Decode(byte[] buf)
        {
            base.Decode(buf);

            int len = (int) _networkStream.ReadUVarInt();
            Payload = _networkStream.ReadBytes(len);
        }
    }
}