using ZeisenServerChecker.Models;
using ZeisenServerChecker.Interfaces;

namespace ZeisenServerChecker.Routines
{
	abstract class AbstractRoutineTemplate
	{
		public abstract void Work();
		public abstract void Initialize(IPTableModel[] tables, StatusSetter setter, int index);
	}
}
