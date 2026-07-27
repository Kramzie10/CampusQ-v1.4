using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using CampusQ.MVP.Models;
using CampusQ.MVP.Presenters;
using CampusQ.MVP.Views;
using CampusQ; // for CashierWindows

namespace CampusQ.MVP.Views
{
    public class CashierView : Form, ICashierView
    {
        private readonly DataGridView _dgvQueue;
        private readonly ComboBox _cmbService;
        private readonly Button _btnServeNext;
        private readonly Button _btnRefresh;
        private readonly Button _btnServiceWindow;

        private readonly CashierPresenter _presenter;
        private BindingSource? _bindingSource;

        public CashierView()
        {
            Text = "Cashier Queue";
            Size = new Size(800, 600);
            StartPosition = FormStartPosition.CenterScreen;
            BackgroundImage = Properties.Resources.Purple_and_White_Minimalist_Modern_Computer_Repair_Logo__23_386_x_16_535_in___8_5_x_22_cm___500_x_500_px___11_7_x_8_27_in_;

            // Service selector
            _cmbService = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(12, 12),
                Width = 200
            };
            // Use friendly names so parsing is reliable
            _cmbService.Items.AddRange(new object[] { "All", "Window1", "Window2", "Window3", "Window4" });
            _cmbService.SelectedIndex = 0;
            Controls.Add(_cmbService);

            // Buttons
            _btnServeNext = new Button { Text = "Serve Next", Location = new Point(220, 10), Width = 100 };
            Controls.Add(_btnServeNext);

            _btnRefresh = new Button { Text = "Refresh", Location = new Point(330, 10), Width = 80 };
            Controls.Add(_btnRefresh);

            // Service window button to open the CashierWindows form
            _btnServiceWindow = new Button { Text = "Service Window", Location = new Point(420, 10), Width = 120 };
            Controls.Add(_btnServiceWindow);

            // DataGridView
            _dgvQueue = new DataGridView
            {
                Location = new Point(12, 80),
                Size = new Size(760, 470),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            _dgvQueue.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TicketLabel", HeaderText = "Ticket", Width = 80 });
            _dgvQueue.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Service", HeaderText = "Service", Width = 120 });
            _dgvQueue.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Purpose", HeaderText = "Purpose", Width = 150 });
            var timeCol = new DataGridViewTextBoxColumn { DataPropertyName = "TimeAdded", HeaderText = "Time Added", Width = 150 };
            timeCol.DefaultCellStyle.Format = "g";
            _dgvQueue.Columns.Add(timeCol);

            Controls.Add(_dgvQueue);

            // create presenter after UI so presenter can call back into IStaffView
            _presenter = new CashierPresenter(this);

            // wire events after presenter is created
            _cmbService.SelectedIndexChanged += (s, e) => _presenter.RefreshQueueView();

            // ServeNext follows presenter pattern: presenter shows message and returns void
            _btnServeNext.Click += (s, e) =>
            {
                try
                {
                    _presenter.ServeNext();
                }
                catch (Exception ex)
                {
                    ShowMessage($"Serve failed: {ex.Message}", "Error", MessageBoxIcon.Error);
                }
            };

            _btnRefresh.Click += (s, e) => _presenter.RefreshQueueView();

            // open the cashier windows form when service button clicked
            _btnServiceWindow.Click += (s, e) =>
            {
                try
                {
                    var svc = new CashierWindows();
                    // immediately populate labels from current grid
                    svc.DisplayFromDataGrid(_dgvQueue);

                    // keep labels in sync when grid binding changes
                    DataGridViewBindingCompleteEventHandler handler = null;
                    handler = (sender, args) => svc.DisplayFromDataGrid(_dgvQueue);
                    _dgvQueue.DataBindingComplete += handler;

                    // unsubscribe when service window closes
                    svc.FormClosed += (o, args) => _dgvQueue.DataBindingComplete -= handler;

                    svc.Show();
                }
                catch (Exception ex)
                {
                    ShowMessage($"Failed to open service window: {ex.Message}", "Error", MessageBoxIcon.Error);
                }
            };
        }

        // IStaffView / ICashierView implementation
        public BindingList<QueueEntry>? CurrentQueue { get; private set; }
        public event EventHandler? QueueChanged;

        public void BindQueue(BindingList<QueueEntry> view)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => BindQueue(view)));
                return;
            }

            try
            {
                // Use a BindingSource to ensure DataGridView receives change notifications reliably
                if (_bindingSource == null)
                {
                    _bindingSource = new BindingSource();
                    _dgvQueue.DataSource = _bindingSource;
                }

                _bindingSource.DataSource = view;
                CurrentQueue = view;
                QueueChanged?.Invoke(this, EventArgs.Empty);

                // enable/disable ServeNext based on whether there are rows
                _btnServeNext.Enabled = view != null && view.Count > 0;

                Debug.WriteLine($"[CashierView] BindQueue called. ViewCount={view?.Count ?? 0}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CashierView] BindQueue failed: {ex}");
            }
        }

        public string SelectedService
        {
            get
            {
                if (InvokeRequired)
                    return (string)Invoke(new Func<string>(() => SelectedService));

                return _cmbService.SelectedItem?.ToString() ?? "All";
            }
        }

        public void DisplayServedTicket(QueueEntry entry)
        {
            // Default UI: show a message box. Implementations may choose to show in a dedicated area.
            ShowMessage($"Now serving {entry.TicketLabel}\nService: {entry.Service}\nPurpose: {entry.Purpose}", "Serving Next", MessageBoxIcon.Information);
        }

        public void ShowMessage(string text, string caption, MessageBoxIcon icon)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => ShowMessage(text, caption, icon)));
                return;
            }

            MessageBox.Show(this, text, caption, MessageBoxButtons.OK, icon);
        }

        public void SetSelectedService(string service)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => SetSelectedService(service)));
                return;
            }

            if (string.IsNullOrWhiteSpace(service))
                return;

            var index = _cmbService.FindStringExact(service);
            if (index >= 0) _cmbService.SelectedIndex = index;
            else _cmbService.Text = service;
        }

        // Keep a refresh helper so host or other code can request a refresh.
        public void RefreshQueueView()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => RefreshQueueView()));
                return;
            }

            _presenter.RefreshQueueView();
        }
    }
}