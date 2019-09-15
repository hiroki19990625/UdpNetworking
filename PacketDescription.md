# UdpNetworking

パケット仕様

## LowLevelPacket(抽象)
|Type|Name|説明|
|:--:|:--:|:--:|
|Byte|PacketId|パケットの識別子|

## ConnectionRequestPacket
|Type|Name|説明|
|:--:|:--:|:--:|
|Byte|PacketId|パケットの識別子|
|DateTime(Long)|Date|接続時のタイムスタンプ|
|Byte[]|Padding|Mtu計測用のパディング

## ConnectionResponsePacket
|Type|Name|説明|
|:--:|:--:|:--:|
|Byte|PacketId|パケットの識別子|
|UShort|Mtu|MaxTransferUnitの設定|

## DataPacket
|Type|Name|説明|
|:--:|:--:|:--:|
|Byte|PacketId|パケットの識別子|
|UInt|SequenceId|信頼性確保のId|
|Byte[]|Data|ペイロード|

## DisconnectPacket
|Type|Name|説明|
|:--:|:--:|:--:|
|Byte|PacketId|パケットの識別子|

## AckPacket
|Type|Name|説明|
|:--:|:--:|:--:|
|Byte|PacketId|パケットの識別子|
|Byte|Length|長さ|
|Uint[]|SequenceIds|受信完了した信頼性確保のId|

## NackPacket
|Type|Name|説明|
|:--:|:--:|:--:|
|Byte|PacketId|パケットの識別子|
|Byte|Length|長さ|
|Uint[]|SequenceIds|受信失敗した信頼性確保のId|

## EncapsulatedPacket
|Type|Name|説明|
|:--:|:--:|:--:|
|Byte|Flags|Reliability << 4と分割フラグ|
|UInt|MessageId|パケットのメッセージId|
|これより下は分割時のみ|||
|UShort|SplitId|パケットの分割Id|
|UShort|SplitIndex|パケットの分割インデックス|
|UShort|SplitLastIndex|パケットの分割最終インデックス|
|UShort|Length|ペイロードの長さ|
|Byte[]|Payload|ペイロード|

## HighLevelPacket(抽象)
|Type|Name|説明|
|:--:|:--:|:--:|
|UInt|PacketId|パケットの識別子|

## ConnectedPingPacket
|Type|Name|説明|
|:--:|:--:|:--:|
|UInt|PacketId|パケットの識別子|
|Long|Date|タイムスタンプ|

## ConnectedPongPacket
|Type|Name|説明|
|:--:|:--:|:--:|
|UInt|PacketId|パケットの識別子|
|Long|Date|タイムスタンプ|

## ConnectionEstablishmentPacket
|Type|Name|説明|
|:--:|:--:|:--:|
|UInt|PacketId|パケットの識別子|

## CustomDataPacket
|Type|Name|説明|
|:--:|:--:|:--:|
|UInt|PacketId|パケットの識別子|
|UVarInt|Length|ペイロードの長さ|
|Byte[]|Payload|ペイロード|
