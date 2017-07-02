using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ZeisenServerChecker.Interfaces;
using ZeisenServerChecker.Models;

namespace ZeisenServerChecker.Routines
{
	class RoutineArgs
	{
		public RoutineAbstract Routine { get; private set; }
		public StatusSetter Setter { get; private set; }
		public IPTableModel[] Tables { get; private set; }
		public int Index { get; private set;}

		private RoutineArgs() { }

		public RoutineArgs(RoutineAbstract routine, StatusSetter setter, IPTableModel[] tables, int index)
		{
			Routine = routine;
			Setter = setter;
			Tables = tables;
			Index = index;
		}
	}
}
