using System;
using System.Threading;
using System.Windows.Forms;

namespace ZeisenServerChecker
{
	static class Program
	{
		/// <summary>
		/// 해당 응용 프로그램의 주 진입점입니다.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Mutex dupStartDeleter = new Mutex(true, "ZP_STATUS_CHECKER");
			if (!dupStartDeleter.WaitOne(10) && !System.Diagnostics.Debugger.IsAttached)
			{
				MessageBox.Show(StringTable.AlreadyRunning, StringTable.Warning);
				Application.Exit();
			}
			else
			{
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Application.Run(new MainForm());
			}
		}
	}
}
