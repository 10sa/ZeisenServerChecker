﻿using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

using ZeisenServerChecker.Models;
using ZeisenServerChecker.Interfaces;

namespace ZeisenServerChecker.Routines
{
	class SrcdsInfoRoutine : RoutineTemplate
	{
		private Socket connectChecker = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		private byte[] buffer = new byte[2048];

		private string mapCache = string.Empty;
		private Dictionary<IPTableModel, bool> lockDupCtl = new Dictionary<IPTableModel, bool>();
		private IPTableModel[] tables;
		private StatusSetter setter;
		private int index;
		
		// A2A_INFO Query.
		private readonly byte[] enginePingQuery = { 0xFF, 0xFF, 0xFF, 0xFF, 0x54, 0x53, 0x6F, 0x75, 0x72, 0x63, 0x65, 0x20, 0x45, 0x6E, 0x67, 0x69, 0x6E, 0x65, 0x20, 0x51, 0x75, 0x65, 0x72, 0x79, 0x00 };

		public SrcdsInfoRoutine()
		{
			connectChecker.ReceiveBufferSize = 2048;
			connectChecker.ReceiveTimeout = 600;
			connectChecker.SendTimeout = 600;
		}

		public override void Initialize(IPTableModel[] tables, StatusSetter setter, int index)
		{
			this.tables = tables;
			this.setter = setter;
			this.index = index;
		}

		public override void Work()
		{
			foreach (var data in tables)
			{
				if (data.Type == Enums.CheckType.SocketConnect)
				{
					setter.SetChecking(data, index);

					try
					{
						EndPoint ep = new IPEndPoint(IPAddress.Parse(data.IPAddress), data.Port);
						connectChecker.SendTo(enginePingQuery, ep);
						connectChecker.ReceiveFrom(buffer, ref ep);

						SrcdsResponsePacket response = SrcdsResponsePacket.Parse(buffer);

						if (!response.Visibility)
						{
							// Only for Zeisen Project.
							if (response.Name.Contains("[Z.P ★] Story Mode") && response.Tags.Contains("Zeisen Project ★") && !response.Map.Equals(mapCache))
							{
								mapCache = response.Map;
								setter.CustomNotify("Story Notify", string.Format("{0} 서버가 {1} 스토리 맵을 진행 중입니다.", response.Name, response.Map));
							}
							else
								mapCache = string.Empty;

							setter.SetOnline(data, index);

							lockDupCtl[data] = false;
						}
						else
						{
							bool isDupLock = false;
							lockDupCtl.TryGetValue(data, out isDupLock);
							setter.SetCustomValue(data, index, StringTable.Locked);

							if (!isDupLock)
							{
								lockDupCtl[data] = true;
								setter.CustomNotify(StringTable.Locked, data.Name + StringTable.ServerIsLocked);
							}
						}

						if (setter.IsExtend())
						{
							setter.SetCustomValue(data, 4, string.Format("{0}/{1}", response.Players, response.MaxPlayers));
							setter.SetCustomValue(data, 5, response.Map);
							setter.SetCustomValue(data, 6, response.Visibility ? StringTable.Locked : StringTable.Opened);
							setter.SetCustomValue(data, 7, response.Name);
						}
					}
					catch (SocketException)
					{
						lockDupCtl[data] = false;
						setter.SetOffline(data, index);

						if (setter.IsExtend())
						{
							for (int i = 4; i < 8; i++)
								setter.SetCustomValue(data, i, StringTable.StatusOffline);
						}
					}
				}
			}
		}
	}
}

