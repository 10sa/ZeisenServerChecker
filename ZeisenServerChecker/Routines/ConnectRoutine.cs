using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.Net;

using ZeisenServerChecker.Interfaces;
using ZeisenServerChecker.Models;

namespace ZeisenServerChecker.Routines
{
	class ConnectRoutine : RoutineAbstract
	{
		private Socket httpChecker = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		private Dictionary<IPTableModel, ManualResetEvent> connectWait = new Dictionary<IPTableModel, ManualResetEvent>();

		private IPTableModel[] tables;
		private StatusSetter setter;
		int index;

		public ConnectRoutine()
		{
			httpChecker.ReceiveTimeout = 600;
			httpChecker.SendTimeout = 600;
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
				if (data.Type == Enums.CheckType.Http)
				{
					if (!connectWait.ContainsKey(data))
						connectWait.Add(data, new ManualResetEvent(true));

					setter.SetChecking(data, index);

					SocketAsyncEventArgs args = new SocketAsyncEventArgs();
					args.RemoteEndPoint = new IPEndPoint(IPAddress.Parse(data.IPAddress), data.Port);
					args.Completed += (a, b) =>
					{
						if (b.SocketError == SocketError.Success)
						{
							httpChecker.Disconnect(false);
							httpChecker = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

							setter.SetOnline(data, index);
						}
						else
							setter.SetOffline(data, index);

						connectWait[data].Set();
					};

					try
					{
						connectWait[data].WaitOne();
						connectWait[data].Reset();

						httpChecker.ConnectAsync(args);
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
