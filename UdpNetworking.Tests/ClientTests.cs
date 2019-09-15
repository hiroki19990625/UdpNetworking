using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using UdpNetworking.Event;
using UdpNetworking.Packet.HighLevel;

namespace UdpNetworking.Tests
{
    public class ClientTests
    {
        private ReliabilityUdpClient _client;
        private ReliabilityUdpClient _client2;

        private int cnt;

        [SetUp]
        public void Setup()
        {
            _client = new ReliabilityUdpClient(new IPEndPoint(IPAddress.Any, 0), OnConnection, OnCustomPacket, Option);
            _client.Listen();
            _client2 = new ReliabilityUdpClient(new IPEndPoint(IPAddress.Any, 10002), OnConnection, OnCustomPacket,
                Option);
            _client2.Listen();
        }

        private void OnCustomPacket(ReceiveCustomDataPacketData obj)
        {
            Console.WriteLine("Handle Custom: " + obj.CustomDataPacket.Payload.Length);
            cnt++;
        }

        [Test]
        public async Task Test1()
        {
            bool r = await _client.ConnectionAsync(new IPEndPoint(IPAddress.Loopback, 10002));

            _client.GetSession(new IPEndPoint(IPAddress.Loopback, 10002))
                .SendPacket(new CustomDataPacket(new byte[100000]));

            for (int i = 0; i < 1000; i++)
            {
                _client.GetSession(new IPEndPoint(IPAddress.Loopback, 10002))
                    .SendPacket(new CustomDataPacket(new byte[5000]));
                Thread.Sleep(1);
            }

            Thread.Sleep(100);

            Console.WriteLine(cnt);
        }

        private void OnConnection(ConnectionData obj)
        {
            Console.WriteLine($"Connect! " + obj.MtuSize);
        }

        private void Option(UdpClient obj)
        {
            obj.EnableBroadcast = true;
            obj.DontFragment = false;
        }
    }
}