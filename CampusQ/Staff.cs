using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using CampusQ.MVP.Presenters;
using CampusQ.MVP.Views;
using CampusQ.MVP.Models;

namespace CampusQ
{
    public partial class Staff : Form, IStaffView
    {
        private readonly StaffPresenter _presenter;
        private BindingList<QueueEntry>? currentView;


        public Staff()
        {
            InitializeComponent();
            InitializeStaffControls();
            _presenter = new StaffPresenter(this);
        }

        public void AddToQueue(string purpose, string service)
        {
            _presenter.AddToQueue(purpose, service);
        }

        private void InitializeStaffControls()
        {
            // Limit the combo to registrar windows only (no "All" or "Other")
            if (comboBoxService.Items.Count == 0)
            {
                comboBoxService.Items.AddRange(new object[]
                {
                    "Registrar - W1",
                    "Registrar - W2"
                });
            }

            if (comboBoxService.Items.Count > 0 && comboBoxService.SelectedIndex < 0)
                comboBoxService.SelectedIndex = 0;

            comboBoxService.SelectedIndexChanged += ComboBoxService_SelectedIndexChanged;
            buttonRefresh.Click += ButtonRefresh_Click;
            buttonServeNext.Click += ButtonServeNext_Click;

            dataGridViewQueue.AutoGenerateColumns = false;
            dataGridViewQueue.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewQueue.MultiSelect = false;
            dataGridViewQueue.AllowUserToAddRows = false;
            dataGridViewQueue.ReadOnly = true;

            // Only show ticket column: staff only needs to see ticket numbers assigned to the selected registrar window.
            dataGridViewQueue.Columns.Clear();
            dataGridViewQueue.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(QueueEntry.TicketLabel),
                HeaderText = "Ticket",
                Width = 80
            });
            dataGridViewQueue.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(QueueEntry.Purpose),
                HeaderText = "Purpose",
                Width = 200
            });
            dataGridViewQueue.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(QueueEntry.Service),
                HeaderText = "Service",
                Width = 120
            });
            dataGridViewQueue.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(QueueEntry.TimeAdded),
                HeaderText = "Added",
                Width = 150,
                DefaultCellStyle = { Format = "g" }
            });
        }

        private void ApplyFilter()
        {
            // presenter will call BindQueue
        }

        private void ComboBoxService_SelectedIndexChanged(object? sender, EventArgs e)
        {
            _presenter?.RefreshQueueView();
        }

        private void ButtonRefresh_Click(object? sender, EventArgs e)
        {
            _presenter?.RefreshQueueView();
        }

        private void ButtonServeNext_Click(object? sender, EventArgs e)
        {
            _presenter?.ServeNext();
        }


        public void BindQueue(BindingList<QueueEntry> view)
        {
            currentView = view;
            dataGridViewQueue.DataSource = currentView;
            labelTotal.Text = $"Total: {currentView.Count}";
        }

        public string SelectedService => (comboBoxService.SelectedItem as string) ?? "All";

        public void ShowMessage(string text, string caption, MessageBoxIcon icon)
        {
            MessageBox.Show(text, caption, MessageBoxButtons.OK, icon);
        }

        public void SetSelectedService(string service)
        {
            if (string.IsNullOrWhiteSpace(service)) return;

            // Accept both "Registrar" and the detailed "Registrar - W1"/"Registrar - W2".
            if (string.Equals(service, "Registrar", StringComparison.OrdinalIgnoreCase))
            {
                // default to Window 1 when caller asks for Registrar generically
                SelectComboBoxItem("Registrar - W1");
                return;
            }

            // If exact item exists, select it
            SelectComboBoxItem(service);
        }

        private void SelectComboBoxItem(string item)
        {
            for (int i = 0; i < comboBoxService.Items.Count; i++)
            {
                if (string.Equals(comboBoxService.Items[i]?.ToString(), item, StringComparison.OrdinalIgnoreCase))
                {
                    comboBoxService.SelectedIndex = i;
                    return;
                }
            }

            // not found: add and select
            comboBoxService.Items.Add(item);
            comboBoxService.SelectedItem = item;
        }


        private void Staff_Load(object sender, EventArgs e)
        {

        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // hide tray icon when closing
            trayIcon.Visible = false;
            base.OnFormClosing(e);

        }

        protected override void OnResize(EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
                trayIcon.Visible = true;
                trayIcon.BalloonTipTitle = "CampusQ - Staff";
                trayIcon.BalloonTipText = "The application is still running in the system tray.";
                trayIcon.ShowBalloonTip(1000);
            }
            base.OnResize(e);
        }

        // restore the form when the tray icon is double-clicked
        private void trayIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
            trayIcon.Visible = false;
            Activate();
        }

        private void btn_service_Click(object sender, EventArgs e)
        {
            _presenter.ServiceWindow();
        }
    }
}
