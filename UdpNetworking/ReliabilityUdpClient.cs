using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UdpNetworking.Utils;

namespace UdpNetworking
{
    public class ReliabilityUdpClient : IDisposable
    {
        private UdpClient _client;
        private Task _task;
        private CancellationTokenSource _tokenSource;

        public ClientState ClientState { get; private set; }

        public ReliabilityUdpClient(IPEndPoint endPoint)
        {
            _client = new UdpClient(endPoint);
        }

        public ReliabilityUdpClient(string hostname, ushort port)
        {
            _client = new UdpClient(hostname, port);
        }

        public bool Connection(IPEndPoint endPoint, Action<ConnectionData> callback)
        {
            if (ClientState == ClientState.Initialized)
            {
                _tokenSource = new CancellationTokenSource();
                _task = Task.Factory.StartNew(Receive, _tokenSource.Token, TaskCreationOptions.LongRunning,
                    TaskScheduler.Default);

                

                return true;
            }

            return false;
        }

        private void Receive()
        {
            ClientState = ClientState.Listening;

            while (true)
            {
                _tokenSource.Token.ThrowIfCancellationRequested();
                IPEndPoint endPoint = null;
                byte[] buffer = _client.Receive(ref endPoint);
            }
        }

        public void Dispose()
        {
            _client?.Dispose();
            _task?.Dispose();
            _tokenSource?.Dispose();
        }
    }
}