using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

using ZeisenServerChecker.Models;
using ZeisenServerChecker.Interfaces;
using ZeisenServerChecker.Controllers;

namespace ZeisenServerChecker
{
	public partial class MainForm : Form, StatusSetter
	{
		private const int NotifyTime = 3000;
		private const int MainStatusItemIndex = 2;
		private const int SubStatusItemIndex = 3;

		private WorkerController workerController;
		private ManualResetEvent notifyWait = new ManualResetEvent(true);
		private System.Timers.Timer notifyTimer = new System.Timers.Timer();
		private RegistryKey programRegistry;
		private bool isExtened = false;

		public MainForm()
		{
			InitializeComponent();

			workerController = new WorkerController();

			InitListView();
			this.Visible = false;
			this.WindowState = FormWindowState.Minimized;
			this.ShowInTaskbar = false;

			if ((programRegistry = Registry.CurrentUser.OpenSubKey(StringTable.Config_SubKeyName, true)) == null)
				programRegistry = Registry.CurrentUser.CreateSubKey(StringTable.Config_SubKeyName);

			notifyTimer.Interval = NotifyTime;
			notifyTimer.AutoReset = false;
			notifyTimer.Elapsed += (a, b) =>
			{
				notifyWait.Set();
			};
		}

		private void MainForm_Load(object sender, EventArgs e)
		{
			workerController.Run(MainStatusItemIndex, SubStatusItemIndex, this);

			bool cacheExtened = bool.Parse((string)programRegistry.GetValue(StringTable.Config_Key, bool.FalseString));
			if (Convert.ToBoolean(cacheExtened))
				SetExtendMode();
		}

		private void InitListView()
		{
			for (int i = 0; i < workerController.IPTable.Count; i++)
			{
				IPTableModel value = workerController.IPTable[i];
				ListViewItem item = new ListViewItem(value.Name);
				item.SubItems.Add(value.IPAddress);
				item.SubItems.Add(StringTable.StatusChecking);
				item.SubItems.Add(StringTable.StatusChecking);

				listView1.Items.Add(item);
				value.FormsIndex = listView1.Items.Count - 1;
				value.IsOnline = null;
			}

			AutoResizeEx();
		}

		public void SetOnline(IPTableModel table, int index, bool isNotify = true)
		{
			if (table.IsOnline == true)
				return;
			else
			{
				if (isNotify)
				{
					if (!table.LastStatus)
						Notify(table.Name, StringTable.StatusIsOnline);

					table.IsOnline = true;
					table.LastStatus = true;
				}

				Invoke((MethodInvoker)delegate
				{
					listView1.Items[table.FormsIndex].SubItems[index].Text = StringTable.StatusOnline;

					AutoResizeEx();
				});
			}
		}

		public void SetOffline(IPTableModel table, int index, bool isNotify = true)
		{
			if (table.IsOnline == false)
				return;
			else
			{
				if (isNotify)
				{
					if (table.LastStatus)
						Notify(table.Name, StringTable.StatusIsOffline);

					table.IsOnline = false;
					table.LastStatus = false;
				}
				
				Invoke((MethodInvoker)delegate
				{
					listView1.Items[table.FormsIndex].SubItems[index].Text = StringTable.StatusOffline;

					AutoResizeEx();
				});
			}
		}

		public void SetChecking(IPTableModel table, int index)
		{
			table.IsOnline = null;
			Invoke((MethodInvoker)delegate
			{
				listView1.Items[table.FormsIndex].SubItems[index].Text = StringTable.StatusChecking;

				AutoResizeEx();
			});
		}

		public void SetCustomValue(IPTableModel table, int index, string value)
		{
			Invoke((MethodInvoker)delegate
			{
				listView1.Items[table.FormsIndex].SubItems[index].Text = value;

				AutoResizeEx();
			});
		}

		private void AutoResizeEx()
		{
			listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
		}

		private void Notify(string tipTitle, string tipText)
		{
			notifyWait.WaitOne();

			notifyWait.Reset();
			notifyIcon1.ShowBalloonTip(NotifyTime, tipTitle, tipText, ToolTipIcon.Info);
			notifyTimer.Start();
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (e.CloseReason == CloseReason.ApplicationExitCall || e.CloseReason == CloseReason.WindowsShutDown || e.CloseReason == CloseReason.None)
			{
				Close();
			}
			else 
			{
				e.Cancel = true;
				Hide();
			}
		}

		private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			workerController.Dispose();

			Application.Exit();
		}

		private void notifyIcon1_DoubleClick(object sender, EventArgs e)
		{
			this.Visible = true;
			this.WindowState = FormWindowState.Normal;
			this.ShowInTaskbar = true;
		}

		private void notifyIcon1_BalloonTipClosed(object sender, EventArgs e)
		{
			if (notifyWait.WaitOne(0))
			{
				notifyWait.Set();
				if (notifyTimer.Enabled)
					notifyTimer.Stop();
			}
		}

		public void CustomNotify(string title, string desc)
		{
			this.Notify(title, desc);
		}

		private readonly string[] extendColumns = {
			"유저 수", // 4
			"맵", // 5
			"잠김 여부", // 6
			"서버 이름" // 7
		};

		private void ExtendModeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (isExtened)
				RemoveExtendMode();
			else
			{
				SetExtendMode();
				workerController.Refresh();
			}

			AutoResizeEx();
		}

		private void RemoveExtendMode()
		{
			this.Size = new Size(600, 600);
			ExtendModeToolStripMenuItem.Checked = false;

			foreach (ColumnHeader data in listView1.Columns)
			{
				foreach (var key in extendColumns)
				{
					if (data.Text == key)
						listView1.Columns.Remove(data);
				}
			}

			programRegistry.SetValue(StringTable.Config_Key, false);
			isExtened = false;
		}

		private void SetExtendMode()
		{
			this.Size = new Size(1000, 600);
			ExtendModeToolStripMenuItem.Checked = true;

			foreach (var data in extendColumns)
				listView1.Columns.Add(data);
			
			foreach (var data in workerController.IPTable)
			{
				for (int i = 0; i < 4; i++)
				{
					if (data.Type != Enums.CheckType.SocketConnect)
						listView1.Items[data.FormsIndex].SubItems.Add(StringTable.NotSupprot);
					else
						listView1.Items[data.FormsIndex].SubItems.Add(StringTable.WaitCycle);
				}
			}

			isExtened = true;
			programRegistry.SetValue(StringTable.Config_Key, true);
		}

		public bool IsExtend()
		{
			return isExtened;
		}

		private void SetStartProgramToolStripMenuItem_Click(object sender, EventArgs e)
		{
			RegistryKey startKey;

			if ((startKey = Registry.CurrentUser.OpenSubKey(StringTable.StartMenuRegPath, true)) != null)
			{
				startKey.SetValue(StringTable.ProgramKey, string.Format("\"{0}\"", Application.ExecutablePath));
				MessageBox.Show(StringTable.StartMenuCreateCompleted, StringTable.StartMenuCreateComplatedTitle, MessageBoxButtons.OK);

			}
		}
	}
}
