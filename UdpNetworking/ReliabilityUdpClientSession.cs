using System;
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
        private ReliabilityUdpClient _client;

        public IPEndPoint EndPoint { get; }
        public ushort MtuSize { get; }

        public Dictionary<uint, DataPacket> ReSendPackets = new Dictionary<uint, DataPacket>();

        public Dictionary<ushort, List<EncapsulatedPacket>> SplitsPackets =
            new Dictionary<ushort, List<EncapsulatedPacket>>();

        public uint SendSequenceId { get; private set; }
        public uint SendMessageId { get; private set; }
        public ushort SendSplitId { get; private set; }

        private Action<ReceiveCustomDataPacketData> _receiveCustomDataPacketCallback;

        public ReliabilityUdpClientSession(IPEndPoint endPoint, ushort mtuSize, ReliabilityUdpClient client)
        {
            EndPoint = endPoint;
            MtuSize = mtuSize;
            _client = client;
        }

        public void OnReceive(DataPacket dataPacket)
        {
            AckPacket packet = new AckPacket(new[] {dataPacket.SequenceId});
            _client.Send(EndPoint, packet);

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
                    BinaryStream stream = new BinaryStream();
                    foreach (var splitsPacket in SplitsPackets[encapsulatedPacket.SplitId])
                    {
                        stream.WriteBytes(splitsPacket.Payload);
                    }

                    encapsulatedPacket = new EncapsulatedPacket(encapsulatedPacket.Reliability,
                        encapsulatedPacket.MessageId, stream.GetBuffer());
                }
                else
                    return;
            }

            OnEncapsulatedPacket(encapsulatedPacket);
        }

        public void OnAck(AckPacket ackPacket)
        {
            for (int i = 0; i < ackPacket.SequenceIds.Count; i++)
            {
                if (ReSendPackets.ContainsKey(ackPacket.SequenceIds[i]))
                {
                    ReSendPackets.Remove(ackPacket.SequenceIds[i]);
                    Console.WriteLine($"[{EndPoint}] Ack: {ackPacket.SequenceIds[i]}");
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

        public void SendPacket(HighLevelPacket packet)
        {
            byte[] buf = packet.Encode();
            EncapsulatedPacket encapsulatedPacket = new EncapsulatedPacket(Reliability.Reliable, SendMessageId++, buf);
            EncapsulatedPacket[] packets = encapsulatedPacket.GetSplitEncapsulatedPackets(SendSplitId++, MtuSize);

            for (int i = 0; i < packets.Length; i++)
                SendDataPacket(packets[i].Encode());
        }

        public void SendDataPacket(byte[] buf)
        {
            DataPacket dataPacket = new DataPacket(SendSequenceId++, buf);
            ReSendPackets.Add(dataPacket.SequenceId, dataPacket);
            _client.Send(EndPoint, dataPacket);
        }

        private void OnEncapsulatedPacket(EncapsulatedPacket encapsulatedPacket)
        {
            byte[] buf = encapsulatedPacket.Payload;
            IPacket packet = _client.GetPacketFactory().GetPacket(buf[0]);
            packet.Decode(buf);
            if (packet is ConnectionEstablishmentPacket)
            {
                Console.WriteLine($"[{EndPoint}] ConnectionEstablishment!");
            }
            else if (packet is CustomDataPacket customDataPacket)
            {
                _receiveCustomDataPacketCallback?.Invoke(new ReceiveCustomDataPacketData(customDataPacket));
            }
        }
    }
}