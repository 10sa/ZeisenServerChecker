using ZeisenServerChecker.Models;
using ZeisenServerChecker.Interfaces;

namespace ZeisenServerChecker.Interfaces
{
	abstract class RoutineTemplate
	{
		public abstract void Work();
		public abstract void Initialize(IPTableModel[] tables, StatusSetter setter, int index);
	}
}
