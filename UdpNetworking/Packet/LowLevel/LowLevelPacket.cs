using System;
using BinaryIO;

namespace UdpNetworking.Packet.LowLevel
{
    public abstract class LowLevelPacket : IPacket, IDisposable
    {
        protected NetworkStream _networkStream = new NetworkStream();

        public abstract byte PacketId { get; }

        public virtual byte[] Encode()
        {
            _networkStream.Clear();

            _networkStream.WriteByte(PacketId);

            return _networkStream.GetBuffer();
        }

        public virtual void Decode(byte[] buf)
        {
            _networkStream.Clear();
            _networkStream.SetBuffer(buf);

            CheckId(_networkStream.ReadByte());
        }

        private void CheckId(int id)
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