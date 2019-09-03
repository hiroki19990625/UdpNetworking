using System.Collections.ObjectModel;

namespace UdpNetworking.Packet
{
    public class Global
    {
        public static readonly ReadOnlyCollection<byte> Magic = new ReadOnlyCollection<byte>(new byte[8]
        {
            0x11,
            0x22,
            0x33,
            0x44,
            0x55,
            0xff,
            0xee,
            0xff
        });

        public static readonly ReadOnlyCollection<ushort> MtuLevels = new ReadOnlyCollection<ushort>(new ushort[3]
        {
            1500,
            1156,
            576
        });

        public const int ConnectionRequestPacket = 0x01;
        public const int ConnectionEstablishmentPacket = 0x02;

        public const int DataPacket = 0x10;
    }
}