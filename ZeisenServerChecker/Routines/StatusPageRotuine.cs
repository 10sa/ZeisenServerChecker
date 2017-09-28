using System;
using System.Collections.Generic;
using System.Net;

using ZeisenServerChecker.Interfaces;
using ZeisenServerChecker.Models;

namespace ZeisenServerChecker.Routines
{
	class StatusPageRotuine : AbstractRoutineTemplate
	{
		private WebClient webStringDownloader = new WebClient();

		private IPTableModel[] tables;
		private StatusSetter setter;
		private int index;

		private readonly Dictionary<string, string> serverNameTable = new Dictionary<string, string>()
		{
			{ "ZP_BS_1", "Zeisen Project ★ 1 서버" },
			{ "ZP_BS_2", "Zeisen Project ★ 2 서버" },
			{ "ZP_NF_1", "황혼주점 서버" },
			{ "ZP_BS_JP_1", "Zeisen Project ★ 일본 서버" }
		};

		public override void Initialize(IPTableModel[] tables, StatusSetter setter, int index)
		{
			this.tables = tables;
			this.setter = setter;
			this.index = index;

			foreach(var table in tables)
			{
				if (!serverNameTable.ContainsValue(table.Name))
					setter.SetCustomValue(table, index, StringTable.NotSupprot);
			}
		}

		public override void Work()
		{
			try
			{
				string jsonString = webStringDownloader.DownloadString("http://58.232.44.73:12345/server_status_dev.php");
				string p = new string(jsonString.ToCharArray(), 3, jsonString.Length - 4);

				foreach (var data in p.Split(new string[] { "," }, StringSplitOptions.None))
				{
					string[] pair = data.Split(':');

					string key = new string(pair[0].ToCharArray(), 1, pair[0].Length - 2);
					string value = pair[1];

					foreach (var table in tables)
					{
						string name;
						serverNameTable.TryGetValue(key, out name);
						if (!string.IsNullOrEmpty(name) && table.Name == name)
						{
							int statusResult;
							int.TryParse(value, out statusResult);


							if (statusResult == 1)
								setter.SetCustomValue(table, index, StringTable.StatusOnline);
							else
								setter.SetCustomValue(table, index, StringTable.StatusOffline);

							break;
						}
					}
				}
			}
			catch (WebException)
			{
				foreach (var table in tables)
				{
					if (serverNameTable.ContainsValue(table.Name))
						setter.SetCustomValue(table, index, StringTable.LoadingFailure);
				}
			}
		}
	}
}
