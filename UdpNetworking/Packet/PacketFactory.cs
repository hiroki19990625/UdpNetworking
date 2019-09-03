using System;
using System.Collections.Generic;
using UdpNetworking.Packet.LowLevel;

namespace UdpNetworking.Packet
{
    public class PacketFactory : IPacketFactory
    {
        private readonly Dictionary<byte, Func<IPacket>> _packets = new Dictionary<byte, Func<IPacket>>();

        public PacketFactory()
        {
            Register(Global.ConnectionRequestPacket, () => new ConnectionRequestPacket());
        }

        public IPacket GetPacket(byte id)
        {
            return _packets[id]();
        }

        private void Register(byte id, Func<IPacket> packet)
        {
            _packets[id] = packet;
        }
    }
}