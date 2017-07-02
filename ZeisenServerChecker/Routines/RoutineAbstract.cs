using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ZeisenServerChecker.Models;
using ZeisenServerChecker.Interfaces;

namespace ZeisenServerChecker.Routines
{
	abstract class RoutineAbstract
	{
		public abstract void Work();
		public abstract void Initialize(IPTableModel[] tables, StatusSetter setter, int index);
	}
}
