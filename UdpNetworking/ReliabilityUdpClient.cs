using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UdpNetworking.Event;
using UdpNetworking.Packet;
using UdpNetworking.Packet.LowLevel;

namespace UdpNetworking
{
    public class ReliabilityUdpClient : IDisposable
    {
        private UdpClient _client;
        private Action<ConnectionData> _connectionCallback;
        private IPacketFactory _packetFactory = new PacketFactory();

        private Task _task;
        private CancellationTokenSource _tokenSource;

        private Dictionary<IPEndPoint, ReliabilityUdpClientSession> _sessions =
            new Dictionary<IPEndPoint, ReliabilityUdpClientSession>();

        public ClientState ClientState { get; private set; }

        public ReliabilityUdpClient(IPEndPoint endPoint, Action<ConnectionData> callback,
            Action<UdpClient> option = null, IPacketFactory factory = null)
        {
            if (factory != null)
                _packetFactory = factory;

            _connectionCallback = callback;
            _client = new UdpClient(endPoint);
            option?.Invoke(_client);
        }

        public ReliabilityUdpClient(string hostname, ushort port, Action<ConnectionData> callback,
            Action<UdpClient> option = null, IPacketFactory factory = null)
        {
            if (factory != null)
                _packetFactory = factory;

            _connectionCallback = callback;
            _client = new UdpClient(hostname, port);
            option?.Invoke(_client);
        }

        public void Listen()
        {
            if (ClientState == ClientState.Initialized)
            {
                _tokenSource = new CancellationTokenSource();
                _task = Task.Factory.StartNew(Receive, _tokenSource.Token, TaskCreationOptions.LongRunning,
                    TaskScheduler.Default);
            }
        }

        public async Task<bool> ConnectionAsync(IPEndPoint endPoint)
        {
            if (ClientState == ClientState.Listening)
            {
                int mtu = Global.MtuLevels[0] - 77;
                if (await CheckMtuTimeouts(endPoint, mtu, 3))
                    return true;

                Console.WriteLine($"[{endPoint}] Mtu fail {mtu}");

                mtu = Global.MtuLevels[1] - 77;
                if (await CheckMtuTimeouts(endPoint, mtu, 3))
                    return true;

                Console.WriteLine($"[{endPoint}] Mtu fail {mtu}");

                mtu = Global.MtuLevels[2] - 77;
                if (await CheckMtuTimeouts(endPoint, mtu, 3))
                    return true;

                Console.WriteLine($"[{endPoint}] Mtu fail {mtu}");
            }

            return false;
        }

        public void Send(IPEndPoint endPoint, LowLevelPacket packet)
        {
            byte[] buf = packet.Encode();
            Console.WriteLine($"[{endPoint}] {buf.Length}");
            _client.Send(buf, buf.Length, endPoint);
        }

        public async Task SendAsync(IPEndPoint endPoint, LowLevelPacket packet)
        {
            byte[] buf = packet.Encode();
            Console.WriteLine($"[{endPoint}] {buf.Length}");
            await _client.SendAsync(buf, buf.Length, endPoint);
        }

        public void Dispose()
        {
            _client?.Dispose();
            _task?.Dispose();
            _tokenSource?.Dispose();
        }

        private void Receive()
        {
            ClientState = ClientState.Listening;

            while (true)
            {
                try
                {
                    _tokenSource.Token.ThrowIfCancellationRequested();
                    IPEndPoint endPoint = null;
                    byte[] buffer = _client.Receive(ref endPoint);
                    IPacket packet = _packetFactory.GetPacket(buffer[0]);
                    packet.Decode(buffer);

                    if (packet is ConnectionRequestPacket connectionRequestPacket)
                    {
                        int mtu = connectionRequestPacket.Padding.Length + 77;
                        Send(endPoint, new ConnectionEstablishmentPacket((ushort) mtu));
                    }
                    else if (packet is ConnectionEstablishmentPacket connectionEstablishmentPacket)
                    {
                        if (_sessions.ContainsKey(endPoint))
                            throw new InvalidPacketException("Now connected.");

                        _sessions[endPoint] = new ReliabilityUdpClientSession(this);
                        _connectionCallback?.Invoke(new ConnectionData(endPoint,
                            connectionEstablishmentPacket.MtuSize));
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        private async Task<bool> CheckMtu(IPEndPoint endPoint, int mtu)
        {
            await SendAsync(endPoint, new ConnectionRequestPacket(DateTime.Now, new byte[mtu]));
            await Task.Delay(1000);

            if (_sessions.ContainsKey(endPoint))
                return true;

            return false;
        }

        private async Task<bool> CheckMtuTimeouts(IPEndPoint endPoint, int mtuSize, int count)
        {
            for (int i = 0; i < count; i++)
                if (await CheckMtu(endPoint, mtuSize))
                    return true;

            return false;
        }
    }
}