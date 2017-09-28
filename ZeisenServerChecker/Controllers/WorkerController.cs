using System;
using System.Collections.Generic;

using ZeisenServerChecker.Models;
using ZeisenServerChecker.Routines;
using ZeisenServerChecker.Interfaces;

namespace ZeisenServerChecker.Controllers
{
	class WorkerController : IDisposable
	{
		public List<IPTableModel> IPTable { get; private set; } = new List<IPTableModel>();
		private List<RoutineWorker> workers = new List<RoutineWorker>();
		private StatusSetter observer;

		public WorkerController()
		{
			IPTable.Add(new IPTableModel("Zeisen Project ★ 1 서버", "115.143.203.41", 27025, Enums.CheckType.SocketConnect));
			IPTable.Add(new IPTableModel("Zeisen Project ★ 2 서버", "115.143.203.41", 27026, Enums.CheckType.SocketConnect));
			IPTable.Add(new IPTableModel("황혼주점 서버", "115.143.203.41", 27021, Enums.CheckType.SocketConnect));
			IPTable.Add(new IPTableModel("서버 정보 페이지", "115.143.203.41", 12345, Enums.CheckType.Http));
			IPTable.Add(new IPTableModel("서버 웹 쉐어", "115.143.203.41", 44444, Enums.CheckType.Http));
		}

		public void Run(int itemIndex, int subItemIndex, StatusSetter observer)
		{
			this.observer = observer;

			RegisterWorker(new SrcdsInfoRoutine(), itemIndex);
			RegisterWorker(new ConnectRoutine(), itemIndex);
			RegisterWorker(new StatusPageRotuine(), subItemIndex);
		}

		private void RegisterWorker(AbstractRoutineTemplate routine, int index)
		{
			workers.Add(new RoutineWorker(routine, observer, IPTable.ToArray(), index));
		}

		public void Dispose()
		{
			foreach (var worker in workers)
				worker.Stop();
		}

		public void Refresh()
		{
			foreach (var worker in workers)
				worker.ForceStartCycle();
		}
	}
}
