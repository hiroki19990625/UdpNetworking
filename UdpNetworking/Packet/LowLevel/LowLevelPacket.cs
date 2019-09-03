using System;
using System.Collections;
using System.Linq;
using BinaryIO;

namespace UdpNetworking.Packet.LowLevel
{
    public abstract class LowLevelPacket : IPacket, IDisposable
    {
        protected NetworkStream _networkStream;

        public abstract int PacketId { get; }

        public virtual byte[] Encode()
        {
            _networkStream.Clear();

            _networkStream.WriteBytes(Global.Magic.ToArray());
            _networkStream.WriteSVarInt(PacketId);

            return _networkStream.GetBuffer();
        }

        public virtual void Decode(byte[] buf)
        {
            _networkStream.Clear();
            _networkStream.SetBuffer(buf);

            CheckMagic(_networkStream.ReadBytes(Global.Magic.Count));
            CheckId(_networkStream.ReadSVarInt());
        }

        private void CheckMagic(byte[] buf)
        {
            if (!(buf.Length == 8 &&
                  ((IStructuralEquatable) buf).Equals(Global.Magic, StructuralComparisons.StructuralEqualityComparer)))
                throw new InvalidPacketException("Magic dont match.");
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