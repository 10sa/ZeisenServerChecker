using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ZeisenServerChecker.Models;

namespace ZeisenServerChecker.Interfaces
{
	interface StatusSetter
	{
		void SetOnline(IPTableModel table, int index, bool isNotify = true);
		void SetOffline(IPTableModel table, int index, bool isNotify = true);
		void SetChecking(IPTableModel table, int index);
		void SetCustomValue(IPTableModel table, int index, string value);
		void CustomNotify(string title, string desc);
		bool IsExtend();
	}
}
