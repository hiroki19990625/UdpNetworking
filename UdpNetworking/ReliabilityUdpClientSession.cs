using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using BinaryIO;
using UdpNetworking.Event;
using UdpNetworking.Packet;
using UdpNetworking.Packet.ConnectedPacket;
using UdpNetworking.Packet.HighLevel;
using UdpNetworking.Packet.LowLevel;

namespace UdpNetworking
{
    public class ReliabilityUdpClientSession
    {
        private readonly ReliabilityUdpClient _client;

        public IPEndPoint EndPoint { get; }
        public ushort MtuSize { get; }

        public ConcurrentDictionary<uint, DataPacket> ReSendPackets = new ConcurrentDictionary<uint, DataPacket>();

        public Dictionary<ushort, List<EncapsulatedPacket>> SplitsPackets =
            new Dictionary<ushort, List<EncapsulatedPacket>>();

        public List<uint> MessageWindow = new List<uint>();

        public Dictionary<uint, EncapsulatedPacket> OrderMessages = new Dictionary<uint, EncapsulatedPacket>();

        public uint ReceiveSequenceId { get; private set; }
        public uint SendSequenceId { get; private set; }
        public uint SendMessageId { get; private set; }
        public ushort SendSplitId { get; private set; }

        private readonly Action<ReceiveCustomDataPacketData> _receiveCustomDataPacketCallback;

        public ReliabilityUdpClientSession(IPEndPoint endPoint, ushort mtuSize, ReliabilityUdpClient client,
            Action<ReceiveCustomDataPacketData> callback)
        {
            EndPoint = endPoint;
            MtuSize = mtuSize;
            _client = client;

            _receiveCustomDataPacketCallback = callback;
        }

        public void OnReceive(DataPacket dataPacket)
        {
            AckPacket packet = new AckPacket(new[] {dataPacket.SequenceId});
            _client.Send(EndPoint, packet);

            if (dataPacket.SequenceId > ReceiveSequenceId)
            {
                uint seq = dataPacket.SequenceId - ReceiveSequenceId;
                for (uint i = 0; i < seq; i++)
                {
                    NackPacket nackPacket = new NackPacket(new[] {dataPacket.SequenceId - i - 1});
                    _client.Send(EndPoint, nackPacket);
                }
            }

            if (dataPacket.SequenceId >= ReceiveSequenceId)
                ReceiveSequenceId = dataPacket.SequenceId + 1;

            EncapsulatedPacket encapsulatedPacket = new EncapsulatedPacket();
            encapsulatedPacket.Decode(dataPacket.Data);

            if (encapsulatedPacket.IsSplit)
            {
                if (!SplitsPackets.ContainsKey(encapsulatedPacket.SplitId))
                    SplitsPackets[encapsulatedPacket.SplitId] =
                        new List<EncapsulatedPacket>();

                SplitsPackets[encapsulatedPacket.SplitId].Add(encapsulatedPacket);

                if (SplitsPackets[encapsulatedPacket.SplitId].Count == encapsulatedPacket.LastSplitIndex + 1)
                {
                    SplitsPackets[encapsulatedPacket.SplitId].Sort(Comparison);
                    BinaryStream stream = new BinaryStream();
                    foreach (var splitsPacket in SplitsPackets[encapsulatedPacket.SplitId])
                    {
                        stream.WriteBytes(splitsPacket.Payload);
                    }

                    SplitsPackets.Remove(encapsulatedPacket.SplitId);

                    encapsulatedPacket = new EncapsulatedPacket(encapsulatedPacket.Reliability,
                        encapsulatedPacket.MessageId, stream.GetBuffer());
                }
                else
                    return;
            }

            if (!MessageWindow.Contains(encapsulatedPacket.MessageId))
            {
                if (MessageWindow.Count >= 50)
                {
                    MessageWindow.RemoveAt(0);
                }

                MessageWindow.Add(encapsulatedPacket.MessageId);
                OnEncapsulatedPacket(encapsulatedPacket);
            }
        }

        public void OnAck(AckPacket ackPacket)
        {
            for (int i = 0; i < ackPacket.SequenceIds.Count; i++)
            {
                if (ReSendPackets.ContainsKey(ackPacket.SequenceIds[i]))
                {
                    ReSendPackets.TryRemove(ackPacket.SequenceIds[i], out _);
                }
            }
        }

        public void OnNack(NackPacket nackPacket)
        {
            for (int i = 0; i < nackPacket.SequenceIds.Count; i++)
            {
                if (ReSendPackets.ContainsKey(nackPacket.SequenceIds[i]))
                {
                    SendDataPacket(ReSendPackets[nackPacket.SequenceIds[i]].Data);
                }
            }
        }

        public void Update()
        {
            foreach (KeyValuePair<uint, DataPacket> dataPacket in ReSendPackets)
            {
                TimeSpan date = DateTime.Now - dataPacket.Value.Timestamp;
                if (date.TotalMilliseconds >= 1000)
                {
                    Console.WriteLine("Resend");
                    SendDataPacket(dataPacket.Value.Data);
                    ReSendPackets.TryRemove(dataPacket.Key, out _);
                }
            }
        }

        public void SendPacket(HighLevelPacket packet, Reliability reliability = Reliability.Reliable)
        {
            byte[] buf = packet.Encode();
            EncapsulatedPacket encapsulatedPacket = new EncapsulatedPacket(reliability, SendMessageId++, buf);
            EncapsulatedPacket[] packets = encapsulatedPacket.GetSplitEncapsulatedPackets(SendSplitId++, MtuSize);

            for (int i = 0; i < packets.Length; i++)
                SendDataPacket(packets[i].Encode());
        }

        public void SendDataPacket(byte[] buf)
        {
            DataPacket dataPacket = new DataPacket(SendSequenceId++, buf) {Timestamp = DateTime.Now};
            ReSendPackets.TryAdd(dataPacket.SequenceId, dataPacket);

            _client.Send(EndPoint, dataPacket);
        }

        private void OnEncapsulatedPacket(EncapsulatedPacket encapsulatedPacket)
        {
            byte[] buf = encapsulatedPacket.Payload;
            IPacket packet = _client.GetPacketFactory().GetPacket(buf[0]);
            packet.Decode(buf);
            switch (packet)
            {
                case ConnectionEstablishmentPacket _:
                    Console.WriteLine($"[{EndPoint}] ConnectionEstablishment!");
                    break;
                case CustomDataPacket customDataPacket:
                    _receiveCustomDataPacketCallback?.Invoke(new ReceiveCustomDataPacketData(this, customDataPacket));
                    break;
            }
        }

        private int Comparison(EncapsulatedPacket x, EncapsulatedPacket y)
        {
            return x.SplitIndex.CompareTo(y.SplitIndex);
        }
    }
}