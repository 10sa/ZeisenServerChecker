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
			IPTable.Add(new IPTableModel("Zeisen Project -1", "115.143.203.41", 12345, Enums.CheckType.SocketConnect));
			IPTable.Add(new IPTableModel("Zeisen Project -1", "115.143.203.41", 11111, Enums.CheckType.SocketConnect));
		}

		public void Run(int itemIndex, int subItemIndex, StatusSetter observer)
		{
			this.observer = observer;

			RegisterWorker(new SrcdsInfoRoutine(), itemIndex);
		}

		private void RegisterWorker(RoutineTemplate routine, int index)
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
