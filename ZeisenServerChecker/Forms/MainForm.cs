using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Net.NetworkInformation;

using ZeisenProjectServerChecker.Models;
using ZeisenProjectServerChecker.Routines;
using ZeisenProjectServerChecker.Interfaces;

namespace ZeisenProjectServerChecker
{
	public partial class MainForm : Form, StatusSetter
	{
		private const int NotifyTime = 3000;
		private const int MainStatusItemIndex = 2;
		private const int SubStatusItemIndex = 3;

		private readonly List<IPTableModel> IPTable = new List<IPTableModel>();
		private List<RoutineWorker> workers = new List<RoutineWorker>();
		private ManualResetEvent notifyWait = new ManualResetEvent(true);
		private System.Timers.Timer notifyTimer = new System.Timers.Timer();

		public MainForm()
		{
			InitializeComponent();

			IPTable.Add(new IPTableModel("Zeisen Project ★ 1 서버", "58.232.44.73", 27025, Enums.CheckType.SocketConnect));
			IPTable.Add(new IPTableModel("Zeisen Project ★ 2 서버", "58.232.44.73", 27026, Enums.CheckType.SocketConnect));
			IPTable.Add(new IPTableModel("Zeisen Project ★ 일본 서버", "45.32.255.140", 27015, Enums.CheckType.SocketConnect));
			IPTable.Add(new IPTableModel("황혼주점 서버", "58.232.44.73", 27020, Enums.CheckType.SocketConnect));
			IPTable.Add(new IPTableModel("서버 정보 페이지", "58.232.44.73", 12345, Enums.CheckType.Http));
			// IPTable.Add(new IPTableModel("서버 웹 쉐어", "58.232.44.73", 44444, Enums.CheckType.Http)); //

			InitListView();
			this.Visible = false;
			this.WindowState = FormWindowState.Minimized;
			this.ShowInTaskbar = false;

			notifyTimer.Interval = NotifyTime;
			notifyTimer.AutoReset = false;
			notifyTimer.Elapsed += (a, b) =>
			{
				notifyWait.Set();
			};
		}

		private void MainForm_Load(object sender, EventArgs e)
		{
			RegisterWorker(new SrcdsInfoRoutine(), MainStatusItemIndex);
			RegisterWorker(new ConnectRoutine(), MainStatusItemIndex);
			RegisterWorker(new StatusPageRotuine(), SubStatusItemIndex);
		}

		private void RegisterWorker(RoutineAbstract routine, int index)
		{
			workers.Add(new RoutineWorker(routine, this, IPTable.ToArray(), index));
		}

		private void InitListView()
		{
			for (int i = 0; i < IPTable.Count; i++)
			{
				IPTableModel value = IPTable[i];
				ListViewItem item = new ListViewItem(value.Name);
				item.SubItems.Add(value.IPAddress);
				item.SubItems.Add(StringTable.StatusChecking);
				item.SubItems.Add(StringTable.StatusChecking);

				listView1.Items.Add(item);
				value.FormsIndex = listView1.Items.Count - 1;
				value.IsOnline = null;
			}

			listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
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
				});
			}
		}

		public void SetChecking(IPTableModel table, int index)
		{
			table.IsOnline = null;
			Invoke((MethodInvoker)delegate
			{
				listView1.Items[table.FormsIndex].SubItems[index].Text = StringTable.StatusChecking;
			});
		}

		public void SetCustomValue(IPTableModel table, int index, string value)
		{
			Invoke((MethodInvoker)delegate
			{
				listView1.Items[table.FormsIndex].SubItems[index].Text = value;
			});
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

		private void 종료하기ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			foreach (var worker in workers)
			{
				worker.Stop();
			}

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
	}
}
