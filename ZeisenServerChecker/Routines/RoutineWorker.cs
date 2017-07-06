using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using ZeisenServerChecker.Interfaces;
using ZeisenServerChecker.Models;

namespace ZeisenServerChecker.Routines
{
	class RoutineWorker
	{
		private Thread worker;
		private RoutineAbstract routine;
		private ManualResetEvent workerWaitControl = new ManualResetEvent(true);
		private System.Timers.Timer timeWaitController;

		private const int TimeWait = 35000;

		private RoutineWorker() { }

		public RoutineWorker(RoutineAbstract routine, StatusSetter setter, IPTableModel[] tables, int index)
		{
			worker = new Thread(ThreadRoutine);
			timeWaitController = new System.Timers.Timer(TimeWait);
			timeWaitController.AutoReset = true;
			timeWaitController.Elapsed += (a, b) =>
			{
				workerWaitControl.Set();
			};

			this.routine = routine;
			worker.Start(new RoutineArgs(routine, setter, tables, index));
		}

		public void ForceStartCycle()
		{
				workerWaitControl.Set();
		}

		public void Stop()
		{
			worker.Abort();
		}

		private void ThreadRoutine(object args)
		{
			RoutineArgs routineArg = (RoutineArgs)args;
			routineArg.Routine.Initialize(routineArg.Tables, routineArg.Setter, routineArg.Index);

			while (true)
			{
				try
				{
					workerWaitControl.WaitOne();
					workerWaitControl.Reset();
					routineArg.Routine.Work();
				}
				catch (ThreadAbortException)
				{
					// Do nothing !
				}
			}
		}
	}
}
