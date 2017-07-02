using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using ZeisenServerChecker.Models;
using ZeisenServerChecker.Interfaces;
using System.Net;
using System.Net.Sockets;

namespace ZeisenServerChecker.Routines
{
	class SrcdsInfoRoutine : RoutineAbstract
	{
		private Socket connectChecker = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		private byte[] buffer = new byte[2048];

		private IPTableModel[] tables;
		private StatusSetter setter;
		private int index;

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
						setter.SetOnline(data, index);
					}
					catch (SocketException)
					{
						setter.SetOffline(data, index);
					}
				}
			}
		}
	}
}

