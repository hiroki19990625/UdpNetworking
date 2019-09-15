using System;
using BinaryIO;

namespace UdpNetworking.Packet.LowLevel
{
    public class DataPacket : LowLevelPacket
    {
        public override byte PacketId { get; } = Global.DataPacket;

        public uint SequenceId { get; private set; }
        public byte[] Data { get; private set; }

        public DateTime Timestamp { get; internal set; }

        public DataPacket()
        {
        }

        public DataPacket(uint sequenceId, byte[] data)
        {
            SequenceId = sequenceId;
            Data = data;
        }

        public override byte[] Encode()
        {
            base.Encode();

            _networkStream.WriteUInt(SequenceId, ByteOrder.Little);

            _networkStream.WriteUShort((ushort) Data.Length, ByteOrder.Little);
            _networkStream.WriteBytes(Data);

            return _networkStream.GetBuffer();
        }

        public override void Decode(byte[] buf)
        {
            base.Decode(buf);

            SequenceId = _networkStream.ReadUInt(ByteOrder.Little);

            int len = _networkStream.ReadUShort(ByteOrder.Little);
            Data = _networkStream.ReadBytes(len);
        }
    }
}