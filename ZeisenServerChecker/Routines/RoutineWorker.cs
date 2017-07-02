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

		private const int TimeWait = 30000;

		private RoutineWorker() { }

		public RoutineWorker(RoutineAbstract routine, StatusSetter setter, IPTableModel[] tables, int index)
		{
			worker = new Thread(ThreadRoutine);
			this.routine = routine;
			worker.Start(new RoutineArgs(routine, setter, tables, index));
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
					routineArg.Routine.Work();
					Thread.Sleep(TimeWait);
				}
				catch (ThreadAbortException)
				{
					// Do nothing !
				}
			}
		}
	}
}
