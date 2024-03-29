using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UdpNetworking.Event;
using UdpNetworking.Packet;
using UdpNetworking.Packet.HighLevel;
using UdpNetworking.Packet.LowLevel;

namespace UdpNetworking
{
    public class ReliabilityUdpClient : IDisposable
    {
        private readonly UdpClient _client;
        private readonly Action<ConnectionData> _connectionCallback;
        private Action<DisconnectionData> _disconnectionCallback;
        private readonly Action<ReceiveCustomDataPacketData> _receiveCustomDataPacketCallback;
        private readonly IPacketFactory _packetFactory = new PacketFactory();

        private Task _task;
        private CancellationTokenSource _tokenSource;

        private Task _updateTask;
        private CancellationTokenSource _tokenSource2;

        private readonly Dictionary<IPEndPoint, ReliabilityUdpClientSession> _sessions =
            new Dictionary<IPEndPoint, ReliabilityUdpClientSession>();

        public ClientState ClientState { get; private set; }

        public ReliabilityUdpClient(IPEndPoint endPoint, Action<ConnectionData> callback,
            Action<ReceiveCustomDataPacketData> callback2,
            Action<UdpClient> option = null, IPacketFactory factory = null)
        {
            if (factory != null)
                _packetFactory = factory;

            _connectionCallback = callback;
            _receiveCustomDataPacketCallback = callback2;
            _client = new UdpClient(endPoint);
            option?.Invoke(_client);
        }

        public ReliabilityUdpClient(string hostname, ushort port, Action<ConnectionData> callback,
            Action<ReceiveCustomDataPacketData> callback2,
            Action<UdpClient> option = null, IPacketFactory factory = null)
        {
            if (factory != null)
                _packetFactory = factory;

            _connectionCallback = callback;
            _receiveCustomDataPacketCallback = callback2;
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

                _tokenSource2 = new CancellationTokenSource();
                _updateTask = Task.Factory.StartNew(UpdateClient, _tokenSource2.Token, TaskCreationOptions.LongRunning,
                    TaskScheduler.Default);
            }
        }

        public async Task<bool> ConnectionAsync(IPEndPoint endPoint)
        {
            if (ClientState != ClientState.Listening) return false;

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

            return false;
        }

        public void Disconnect(IPEndPoint endPoint)
        {
            _disconnectionCallback?.Invoke(new DisconnectionData());
        }

        public void Send(IPEndPoint endPoint, LowLevelPacket packet)
        {
            byte[] buf = packet.Encode();
            _client.Send(buf, buf.Length, endPoint);
        }

        public async Task SendAsync(IPEndPoint endPoint, LowLevelPacket packet)
        {
            byte[] buf = packet.Encode();
            await _client.SendAsync(buf, buf.Length, endPoint);
        }

        public ReliabilityUdpClientSession GetSession(IPEndPoint endPoint)
        {
            return _sessions[endPoint];
        }

        public ReliabilityUdpClientSession[] GetSessions()
        {
            return _sessions.Values.ToArray();
        }

        public IPacketFactory GetPacketFactory()
        {
            return _packetFactory;
        }

        public void Dispose()
        {
            _client?.Dispose();
            _task?.Dispose();
            _tokenSource?.Cancel();
            _tokenSource?.Dispose();
            _tokenSource2?.Cancel();
            _tokenSource2?.Dispose();
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
                        Send(endPoint, new ConnectionResponsePacket((ushort) mtu));
                        if (!_sessions.ContainsKey(endPoint))
                        {
                            _sessions[endPoint] = new ReliabilityUdpClientSession(endPoint, (ushort) mtu, this,
                                _receiveCustomDataPacketCallback);
                        }
                    }
                    else if (packet is ConnectionResponsePacket connectionEstablishmentPacket)
                    {
                        if (_sessions.ContainsKey(endPoint))
                            throw new InvalidPacketException("Now connected.");

                        _sessions[endPoint] =
                            new ReliabilityUdpClientSession(endPoint, connectionEstablishmentPacket.MtuSize, this,
                                _receiveCustomDataPacketCallback);
                        _connectionCallback?.Invoke(new ConnectionData(endPoint,
                            connectionEstablishmentPacket.MtuSize));

                        _sessions[endPoint].SendPacket(new ConnectionEstablishmentPacket());
                    }
                    else if (packet is DataPacket dataPacket)
                    {
                        if (_sessions.ContainsKey(endPoint))
                            _sessions[endPoint].OnReceive(dataPacket);
                    }
                    else if (packet is NackPacket nackPacket)
                    {
                        if (_sessions.ContainsKey(endPoint))
                            _sessions[endPoint].OnNack(nackPacket);
                    }
                    else if (packet is AckPacket ackPacket)
                    {
                        if (_sessions.ContainsKey(endPoint))
                            _sessions[endPoint].OnAck(ackPacket);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        private void UpdateClient()
        {
            while (true)
            {
                try
                {
                    _tokenSource.Token.ThrowIfCancellationRequested();

                    foreach (KeyValuePair<IPEndPoint, ReliabilityUdpClientSession> session in _sessions)
                    {
                        session.Value.Update();
                    }

                    Thread.Sleep(1);
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
            await Task.Delay(300);

            return _sessions.ContainsKey(endPoint);
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