using System;
using BinaryIO;

namespace UdpNetworking.Packet.HighLevel
{
    public abstract class HighLevelPacket : IPacket, IDisposable
    {
        protected NetworkStream _networkStream = new NetworkStream();

        public abstract uint PacketId { get; }

        public virtual byte[] Encode()
        {
            _networkStream.Clear();

            _networkStream.WriteUInt(PacketId, ByteOrder.Little);

            return _networkStream.GetBuffer();
        }

        public virtual void Decode(byte[] buf)
        {
            _networkStream.Clear();
            _networkStream.SetBuffer(buf);

            CheckId(_networkStream.ReadUInt(ByteOrder.Little));
        }

        private void CheckId(uint id)
        {
            if (id != PacketId)
                throw new InvalidPacketException("Id dont match.");
        }

        public void Dispose()
        {
            _networkStream?.Dispose();
        }
    }
}