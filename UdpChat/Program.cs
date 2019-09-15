using System;
using System.Net;
using BinaryIO;
using UdpNetworking;
using UdpNetworking.Event;
using UdpNetworking.Packet.HighLevel;

namespace UdpChat
{
    internal class Program
    {
        public static ReliabilityUdpClient _client;

        public static void Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "server")
            {
                _client = new ReliabilityUdpClient(new IPEndPoint(IPAddress.Any, 6000), data => { }, ReceiveServer);
                _client.Listen();
            }
            else
            {
                _client = new ReliabilityUdpClient(new IPEndPoint(IPAddress.Any, 0), Connection, ReceiveClient);
                _client.Listen();

                bool b = _client.ConnectionAsync(new IPEndPoint(IPAddress.Parse("133.167.115.186"), 6000)).Result;
                if (!b)
                {
                    Console.WriteLine("Connection Error");
                    return;
                }
            }

            while (true)
            {
                string chat = Console.ReadLine();

                if (chat == "%exit")
                    break;

                using (BinaryStream stream = new BinaryStream())
                {
                    stream.WriteByte(1);
                    stream.WriteStringUtf8(chat);

                    foreach (var session in _client.GetSessions())
                    {
                        session.SendPacket(new CustomDataPacket(stream.GetBuffer()));
                    }
                }
            }
        }

        private static void Connection(ConnectionData obj)
        {
        }

        private static void ReceiveServer(ReceiveCustomDataPacketData data)
        {
            byte[] buf = data.CustomDataPacket.Payload;
            byte b = buf[0];
            if (b == 1)
            {
                BinaryStream stream = new BinaryStream(buf);
                Console.WriteLine(stream.ReadStringUtf8());
                stream.Close();
            }
        }

        private static void ReceiveClient(ReceiveCustomDataPacketData data)
        {
            byte[] buf = data.CustomDataPacket.Payload;
            byte b = buf[0];
            if (b == 1)
            {
                BinaryStream stream = new BinaryStream(buf);
                Console.WriteLine(stream.ReadStringUtf8());
                stream.Close();
            }
        }
    }
}