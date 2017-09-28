using System;
using System.Collections.Generic;
using System.Text;

namespace ZeisenServerChecker.Models
{
	class SrcdsResponsePacket
	{
		public byte Header { get; private set; }
		public byte Protocol { get; private set; }
		public string Name { get; private set; }
		public string Map { get; private set; }
		public string Folder { get; private set; }
		public string Game { get; private set; }
		public short ID { get; private set; }
		public byte Players { get; private set; }
		public byte MaxPlayers { get; private set; }
		public byte Bots { get; private set; }
		public byte ServerType	{ get; private set; }
		public byte Environment { get; private set; }
		public bool Visibility { get; private set; } // Orignal is byte.
		public bool VAC { get; private set; } // Orignal is byte.
		public string Version { get; private set; }
		public byte ExtraDataByte	 { get; private set; }
		public short ServerPort { get; private set; }
		public UInt64 SteamID { get; private set; }
		public short SourceTVPort { get; private set; }
		public string SourceTVName { get; private set; }
		public string Tags { get; private set; }
		public UInt64 GameID { get; private set; }
		public int PacketLength { get; private set; }

		// Public construct is not useable.
		private SrcdsResponsePacket() { }

		public static SrcdsResponsePacket Parse(byte[] data)
		{
			int counter = 4;
			SrcdsResponsePacket packet = new SrcdsResponsePacket();

			packet.Header = GetByte(data, ref counter);
			packet.Protocol = GetByte(data, ref counter);
			packet.Name = GetString(data, ref counter);
			packet.Map = GetString(data, ref counter);
			packet.Folder = GetString(data, ref counter);
			packet.Game = GetString(data, ref counter);
			packet.ID = GetShort(data, ref counter);
			packet.Players = GetByte(data, ref counter);
			packet.MaxPlayers = GetByte(data, ref counter);
			packet.Bots = GetByte(data, ref counter);
			packet.ServerType = GetByte(data, ref counter);
			packet.Environment = GetByte(data, ref counter);
			packet.Visibility = GetBool(data, ref counter);
			packet.VAC = GetBool(data, ref counter);
			packet.Version = GetString(data, ref counter);
			packet.ExtraDataByte = GetByte(data, ref counter);

			if (Convert.ToBoolean(packet.ExtraDataByte & 0x80))
				packet.ServerPort = GetShort(data, ref counter);

			if (Convert.ToBoolean(packet.ExtraDataByte & 0x10))
				packet.SteamID = GetUInt64(data, ref counter);

			if (Convert.ToBoolean(packet.ExtraDataByte & 0x40))
			{
				packet.SourceTVPort = GetShort(data, ref counter);
				packet.SourceTVName = GetString(data, ref counter);
			}

			if (Convert.ToBoolean(packet.ExtraDataByte & 0x20))
				packet.Tags = GetString(data, ref counter);

			if (Convert.ToBoolean(packet.ExtraDataByte & 0x01))
				packet.GameID = GetUInt64(data, ref counter);

			packet.PacketLength = counter + 1;
			return packet;
		}

		private static UInt64 GetUInt64(byte[] data, ref int length)
		{
			byte[] value = new byte[8];
			for(int i = 0; i < 8; i++)
			{
				value[i] = data[length];
				length++;
			}

			return BitConverter.ToUInt64(value, 0);
		}
		
		private static byte GetByte(byte[] data, ref int length)
		{
			length++;
			return data[length - 1];
		}

		private static bool GetBool(byte[] data, ref int position)
		{
			byte[] value = new byte[1];
			value[0] = data[position];
			position++;

			return BitConverter.ToBoolean(value, 0);
		}

		private static short GetShort(byte[] data, ref int position)
		{
			byte[] byteData = new byte[2];
			for (int i = 0; i < 2; i++)
			{
				byteData[i] = data[position];
				position++;
			}

			return BitConverter.ToInt16(byteData, 0);
		}

		private static string GetString(byte[] data, ref int position)
		{
			List<byte> stringList = new List<byte>();

			for( ; position < data.Length; position++)
			{
				if (data[position] == 0x00)
					break;

				stringList.Add(data[position]);
			}

			position++;
			return Encoding.UTF8.GetString(stringList.ToArray());
		}
	}
}
