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

        [SetUp]
        public void Setup()
        {
            _client = new ReliabilityUdpClient(new IPEndPoint(IPAddress.Any, 0), OnConnection, Option);
            _client.Listen();
            _client2 = new ReliabilityUdpClient(new IPEndPoint(IPAddress.Any, 10002), OnConnection, Option);
            _client2.Listen();
        }

        [Test]
        public async Task Test1()
        {
            bool r = await _client.ConnectionAsync(new IPEndPoint(IPAddress.Loopback, 10002));
            Thread.Sleep(2000);
        }

        private void OnConnection(ConnectionData obj)
        {
            Console.WriteLine($"Connect! " + obj.MtuSize);

            _client.GetSession(obj.EndPoint).SendPacket(new CustomDataPacket(new byte[10000]));
        }

        private void Option(UdpClient obj)
        {
            obj.EnableBroadcast = true;
            obj.DontFragment = false;
        }
    }
}