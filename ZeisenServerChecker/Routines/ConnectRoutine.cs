using System.Threading;
using System.Net.Sockets;
using System.Net;

using ZeisenServerChecker.Interfaces;
using ZeisenServerChecker.Models;

namespace ZeisenServerChecker.Routines
{
	class ConnectRoutine : RoutineTemplate
	{
		private Socket httpChecker = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		private ManualResetEvent connectControl = new ManualResetEvent(true);

		private IPTableModel[] tables;
		private StatusSetter setter;
		int index;

		public ConnectRoutine()
		{
			httpChecker.ReceiveTimeout = 300;
			httpChecker.SendTimeout = 300;
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

						connectControl.Set();
					};

					try
					{
						connectControl.WaitOne();
						connectControl.Reset();

						setter.SetChecking(data, index);
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
