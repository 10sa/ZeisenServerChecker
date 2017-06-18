using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ZeisenProjectServerChecker.Enums;

namespace ZeisenProjectServerChecker.Models
{
	public class IPTableModel
	{
		public string IPAddress { get; private set; }
		public string Name { get; private set; }
		public int Port { get; private set; }
		public CheckType Type { get; private set; }
		public int FormsIndex { get; set; }
		public string SecondIsOnline { get; set; }
		public bool? IsOnline { get; set; } = null;
		public bool LastStatus  { get; set; }

		private IPTableModel() { }

		public IPTableModel(string name, string address, int port, CheckType type, int index = 0)
		{
			this.Name = name;
			this.IPAddress = address;
			this.Port = port;
			this.Type = type;
			this.FormsIndex = index;
			this.SecondIsOnline = string.Empty;
			this.LastStatus = false;
		}
	}
}
