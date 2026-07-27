using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using CampusQ.MVP.Presenters;
using CampusQ.MVP.Views;
using CampusQ.MVP.Models;

namespace CampusQ
{
    public partial class AdminDashboard : Form, IAdminView
    {
        private readonly Button btnOpenMain;
        private readonly Button btnOpenStaff;
        private readonly Button btnOpenLogin;
        private readonly Button btnRefresh;
        private readonly Button btnClearPersist;
        private readonly DataGridView dgvQueue;
        private readonly Label lblTotals;


        private readonly Label lblAccounts;
        private readonly DataGridView dgvAccounts;
        private readonly TextBox txtUsername;
        private readonly TextBox txtPassword;
        private readonly TextBox txtConfirm;
        private readonly ComboBox cmbRole;
        private readonly Button btnCreateAccount;
        private readonly Button btnDeleteAccount;

        // Panel-based chart for daily queue statistics (avoids external chart package)
        private readonly Panel chartDaily;

        // chart data: for each date, map service -> (purpose -> count)
        private List<DailyData> chartData = new List<DailyData>();
        private List<string> chartServices = new List<string>();
        private List<string> chartPurposes = new List<string>();
        private Dictionary<string, Color> purposeColors = new Dictionary<string, Color>(StringComparer.OrdinalIgnoreCase);

        private readonly AdminPresenter _presenter;

        public AdminDashboard()
        {
            Text = "Admin Dashboard";
            Size = new Size(1200, 920);
            StartPosition = FormStartPosition.CenterScreen;
            BackgroundImage = Properties.Resources.Purple_and_White_Minimalist_Modern_Computer_Repair_Logo__23_386_x_16_535_in___8_5_x_22_cm___500_x_500_px___11_7_x_8_27_in_;

            btnOpenMain = new Button { Text = "Open Main Form", Location = new Point(20, 20), Size = new Size(160, 36) };
            btnOpenStaff = new Button { Text = "Open Staff", Location = new Point(200, 20), Size = new Size(160, 36) };
            btnOpenLogin = new Button { Text = "Open Login", Location = new Point(380, 20), Size = new Size(160, 36) };
            btnRefresh = new Button { Text = "Refresh Queue", Location = new Point(560, 20), Size = new Size(120, 36) };
            btnClearPersist = new Button { Text = "Clear Persisted Queue", Location = new Point(700, 20), Size = new Size(180, 36) };

            lblTotals = new Label { Text = "Totals: (loading...)", Location = new Point(20, 70), AutoSize = true };


            dgvQueue = new DataGridView
            {
                Location = new Point(20, 100),
                Size = new Size(630, 480),
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoGenerateColumns = false
            };

            dgvQueue.Columns.Add(new DataGridViewTextBoxColumn { Name = "TicketLabel", DataPropertyName = "TicketLabel", HeaderText = "Ticket", Width = 70 });
            dgvQueue.Columns.Add(new DataGridViewTextBoxColumn { Name = "Purpose", DataPropertyName = "Purpose", HeaderText = "Purpose", Width = 140 });
            dgvQueue.Columns.Add(new DataGridViewTextBoxColumn { Name = "Service", DataPropertyName = "Service", HeaderText = "Service", Width = 90 });
            dgvQueue.Columns.Add(new DataGridViewTextBoxColumn { Name = "Added", DataPropertyName = "TimeAdded", HeaderText = "Added", Width = 120, DefaultCellStyle = { Format = "g" } });


            lblAccounts = new Label { Text = "Staff Accounts", Location = new Point(720, 70), AutoSize = true, Font = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Bold) };

            dgvAccounts = new DataGridView
            {
                Location = new Point(720, 100),
                Size = new Size(400, 200),
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoGenerateColumns = false
            };


            dgvAccounts.Columns.Add(new DataGridViewTextBoxColumn { Name = "Username", DataPropertyName = "Username", HeaderText = "Username", Width = 140 });
            dgvAccounts.Columns.Add(new DataGridViewTextBoxColumn { Name = "Role", DataPropertyName = "Role", HeaderText = "Role", Width = 80 });
            dgvAccounts.Columns.Add(new DataGridViewTextBoxColumn { Name = "CreatedAt", DataPropertyName = "CreatedAt", HeaderText = "Created", Width = 110, DefaultCellStyle = { Format = "g" } });


            txtUsername = new TextBox { Location = new Point(720, 320), Size = new Size(220, 26), PlaceholderText = "username" };
            txtPassword = new TextBox { Location = new Point(720, 360), Size = new Size(220, 26), UseSystemPasswordChar = true, PlaceholderText = "password" };
            txtConfirm = new TextBox { Location = new Point(720, 400), Size = new Size(220, 26), UseSystemPasswordChar = true, PlaceholderText = "confirm password" };
            cmbRole = new ComboBox { Location = new Point(720, 440), Size = new Size(140, 26), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbRole.Items.AddRange(new[] { "Staff", "Admin" });
            cmbRole.SelectedIndex = 0;

            btnCreateAccount = new Button { Text = "Create Account", Location = new Point(720, 480), Size = new Size(140, 36) };
            btnDeleteAccount = new Button { Text = "Delete Selected", Location = new Point(870, 480), Size = new Size(140, 36) };


            // create panel for daily stats
            chartDaily = new Panel
            {
                Location = new Point(20, 600),
                Size = new Size(1120, 240),
                BorderStyle = BorderStyle.FixedSingle
            };
            chartDaily.Paint += ChartDaily_Paint;
            chartDaily.Resize += (_, __) => chartDaily.Invalidate();

            btnOpenMain.Click += BtnOpenMain_Click;
            btnOpenStaff.Click += BtnOpenStaff_Click;
            btnOpenLogin.Click += BtnOpenLogin_Click;
            btnRefresh.Click += BtnRefresh_Click;
            btnClearPersist.Click += BtnClearPersist_Click;
            Load += AdminDashboard_Load;
            btnCreateAccount.Click += BtnCreateAccount_Click;
            btnDeleteAccount.Click += BtnDeleteAccount_Click;

            Controls.AddRange(new Control[] {
                    btnOpenMain, btnOpenStaff, btnOpenLogin, btnRefresh, btnClearPersist,
                    lblTotals, dgvQueue,
                    lblAccounts, dgvAccounts, txtUsername, txtPassword, txtConfirm, cmbRole, btnCreateAccount, btnDeleteAccount,
                    chartDaily
                });


            _presenter = new AdminPresenter(this);
        }

        private void AdminDashboard_Load(object? sender, EventArgs e)
        {
            // presenter already loaded data in ctor
        }

        private void BtnOpenMain_Click(object? sender, EventArgs e)
        {
            OpenOrBringToFront<Form1>(() => new Form1());
        }

        private void BtnOpenStaff_Click(object? sender, EventArgs e)
        {
            OpenOrBringToFront<Staff>(() => new Staff());
        }

        private void BtnOpenLogin_Click(object? sender, EventArgs e)
        {
            OpenOrBringToFront<Login>(() => new Login());
        }

        private void BtnRefresh_Click(object? sender, EventArgs e)
        {
            _presenter.RefreshQueueView();
             _presenter.RefreshActiveQueueView();
            _presenter.LoadAccounts();
            _presenter.RefreshAccountsView();
        }

        private void BtnClearPersist_Click(object? sender, EventArgs e)
        {
            var result = MessageBox.Show("This will delete the persisted queue file and cannot be undone. Continue?", "Confirm Clear", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result != DialogResult.Yes) return;
            _presenter.ClearPersistedQueue();
        }

        private void OpenOrBringToFront<T>(Func<T> factory) where T : Form
        {
            var open = Application.OpenForms.OfType<T>().FirstOrDefault();
            if (open != null && !open.IsDisposed)
            {
                if (!open.Visible) open.Show();
                open.BringToFront();
                open.Focus();
                return;
            }

            var form = factory();
            form.Show();
        }

        private void RefreshQueueView()
        {
            // presenter will update via IAdminView.ShowQueue
            _presenter.RefreshQueueView();
        }

        private void LoadAccounts()
        {
            // presenter handles
        }

        private void SaveAccounts()
        {
            // presenter handles
        }

        private void RefreshAccountsView()
        {
            // presenter handles
        }

        private void BtnCreateAccount_Click(object? sender, EventArgs e)
        {
            var username = txtUsername.Text.Trim();
            var password = txtPassword.Text;
            var confirm = txtConfirm.Text;
            var role = cmbRole.SelectedItem?.ToString() ?? "Staff";

            _presenter.CreateAccount(username, password, confirm, role);
        }

        private void BtnDeleteAccount_Click(object? sender, EventArgs e)
        {
            if (dgvAccounts.SelectedRows.Count == 0)
            {
                MessageBox.Show("Select an account to delete.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }


            var cell = dgvAccounts.SelectedRows[0].Cells["Username"];
            var username = cell?.Value?.ToString();
            if (string.IsNullOrEmpty(username))
            {
                MessageBox.Show("Invalid selection.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var result = MessageBox.Show($"Delete account '{username}'? This cannot be undone.", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result != DialogResult.Yes) return;

            _presenter.DeleteAccount(username);
        }

        public void ShowQueue(List<QueuePersistDto> items)
        {
            // Ensure we are on the UI thread
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => ShowQueue(items)));
                return;
            }

            dgvQueue.DataSource = null;
            dgvQueue.DataSource = items?.ToList() ?? new List<QueuePersistDto>();

            var total = items?.Count ?? 0;
            var byRegistrar = items?.Count(i => string.Equals(i.Service, "Registrar", StringComparison.OrdinalIgnoreCase)) ?? 0;
            var byCashier = items?.Count(i => string.Equals(i.Service, "Cashier", StringComparison.OrdinalIgnoreCase)) ?? 0;
            var byOther = total - byRegistrar - byCashier;

            lblTotals.Text = $"Totals: {total} (Registrar: {byRegistrar}, Cashier: {byCashier}, Other: {byOther})";

            // update daily chart data: group by date (TimeAdded.Date) -> service -> purpose
            try
            {
                // collect global services and purposes
                chartServices = (items ?? Enumerable.Empty<QueuePersistDto>())
                    .Select(i => i.Service ?? "Other")
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(s => s)
                    .ToList();
                if (!chartServices.Any()) chartServices = new List<string> { "Other" };

                chartPurposes = (items ?? Enumerable.Empty<QueuePersistDto>())
                    .Select(i => i.Purpose ?? "")
                    .Where(p => !string.IsNullOrWhiteSpace(p))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(p => p)
                    .ToList();

                // prepare color palette for purposes
                var palette = new[] { Color.SteelBlue, Color.Orange, Color.MediumSeaGreen, Color.MediumPurple, Color.Gold, Color.CadetBlue, Color.Coral, Color.SlateGray };
                purposeColors.Clear();
                for (int i = 0; i < chartPurposes.Count; i++)
                {
                    purposeColors[chartPurposes[i]] = palette[i % palette.Length];
                }

                var grouped = (items ?? Enumerable.Empty<QueuePersistDto>())
                    .GroupBy(i => i.TimeAdded.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => new DailyData
                    {
                        Date = g.Key,
                        Services = chartServices.ToDictionary(s => s, s => new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase))
                    })
                    .ToList();

                // initialize counts to zero
                foreach (var d in grouped)
                {
                    foreach (var s in chartServices)
                    {
                        d.Services[s] = chartPurposes.ToDictionary(p => p, p => 0);
                    }
                }

                // fill counts
                foreach (var item in items ?? Enumerable.Empty<QueuePersistDto>())
                {
                    var date = item.TimeAdded.Date;
                    var svc = string.IsNullOrWhiteSpace(item.Service) ? "Other" : item.Service;
                    var pur = string.IsNullOrWhiteSpace(item.Purpose) ? "Other" : item.Purpose;

                    var dd = grouped.FirstOrDefault(x => x.Date == date);
                    if (dd == null)
                    {
                        dd = new DailyData { Date = date, Services = chartServices.ToDictionary(s => s, s => chartPurposes.ToDictionary(p => p, p => 0)) };
                        grouped.Add(dd);
                    }

                    if (!dd.Services.ContainsKey(svc))
                    {
                        dd.Services[svc] = chartPurposes.ToDictionary(p => p, p => 0);
                    }

                    if (!dd.Services[svc].ContainsKey(pur))
                    {
                        // if purpose was not in global list, add and assign a color
                        chartPurposes.Add(pur);
                        purposeColors[pur] = palette[chartPurposes.Count % palette.Length];
                        // ensure all dates have this purpose key
                        foreach (var ddd in grouped)
                        {
                            foreach (var s in chartServices)
                            {
                                if (!ddd.Services[s].ContainsKey(pur)) ddd.Services[s][pur] = 0;
                            }
                        }
                    }

                    dd.Services[svc][pur] = dd.Services[svc].GetValueOrDefault(pur) + 1;
                }

                chartData = grouped.OrderBy(d => d.Date).ToList();
                chartDaily.Invalidate();
            }
            catch
            {
            }
        }

        private void ChartDaily_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Color.White);
            var rect = chartDaily.ClientRectangle;
            var paddingLeft = 60;
            var paddingBottom = 60;
            var paddingTop = 20;
            var paddingRight = 160; // leave room for legend on the right

            var plotRect = new Rectangle(rect.Left + paddingLeft, rect.Top + paddingTop, rect.Width - paddingLeft - paddingRight, rect.Height - paddingTop - paddingBottom);
            // draw axes
            using (var axisPen = new Pen(Color.Black, 1))
            {
                // Y axis
                g.DrawLine(axisPen, plotRect.Left, plotRect.Top, plotRect.Left, plotRect.Bottom);
                // X axis
                g.DrawLine(axisPen, plotRect.Left, plotRect.Bottom, plotRect.Right, plotRect.Bottom);
            }

            if (chartData == null || chartData.Count == 0)
            {
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                using (var f = new Font(FontFamily.GenericSansSerif, 10))
                {
                    g.DrawString("No queue data", f, Brushes.Gray, plotRect, sf);
                }
                return;
            }

            // compute maximum single value (used to scale individual purpose bars)
            int maxSingle = 1;
            var allValues = chartData.SelectMany(d => d.Services.Values.SelectMany(dict => dict.Values));
            if (allValues.Any()) maxSingle = Math.Max(1, allValues.Max());

            int nDates = chartData.Count;
            int nServices = Math.Max(1, chartServices.Count);

            // layout parameters
            int groupGap = 24; // gap between date groups
            int serviceGap = 8; // gap between services inside a group
            int purposeGap = 4; // gap between purpose bars inside a service

            // compute available width per date group
            var totalGroupGaps = (nDates + 1) * groupGap;
            var availableWidth = Math.Max(0, plotRect.Width - totalGroupGaps);
            var perGroupWidth = availableWidth / nDates;
            var totalServiceGaps = (nServices + 1) * serviceGap;
            var perServiceWidth = Math.Max(12, (perGroupWidth - totalServiceGaps) / nServices);

            var font = new Font(FontFamily.GenericSansSerif, 8);
            var smallFont = new Font(FontFamily.GenericSansSerif, 7);

            for (int di = 0; di < nDates; di++)
            {
                var d = chartData[di];
                int groupX = plotRect.Left + groupGap + di * (perGroupWidth + groupGap);

                // iterate services horizontally inside group
                for (int si = 0; si < nServices; si++)
                {
                    var svc = chartServices[si];
                    int serviceX = groupX + serviceGap + si * (perServiceWidth + serviceGap);

                    // determine purposes present for this service/date (avoid reserving space for global purposes)
                    var svcDict = d.Services.ContainsKey(svc) ? d.Services[svc] : new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                    // include purposes that have a count >0 to avoid drawing empty slots
                    var purposesForService = svcDict.Where(kv => kv.Value > 0).Select(kv => kv.Key).ToList();

                    int nPurposesForService = Math.Max(1, purposesForService.Count);

                    // compute bar width for purposes inside this service area
                    int availableForPurposes = perServiceWidth - 2 * purposeGap;
                    int barWidth = Math.Max(6, (availableForPurposes - (nPurposesForService - 1) * purposeGap) / nPurposesForService);

                    if (purposesForService.Count == 0)
                    {
                        // no data for this service/date — nothing to draw, keep area empty but still draw service label
                    }
                    else
                    {
                        for (int pi = 0; pi < nPurposesForService; pi++)
                        {
                            var pur = purposesForService[pi];
                            var cnt = svcDict.ContainsKey(pur) ? svcDict[pur] : 0;

                            int barX = serviceX + purposeGap + pi * (barWidth + purposeGap);
                            int barHeight = (int)Math.Round(cnt / (double)maxSingle * plotRect.Height);
                            var barRect = new Rectangle(barX, plotRect.Bottom - barHeight, barWidth, barHeight);
                            var color = purposeColors.ContainsKey(pur) ? purposeColors[pur] : Color.Gray;
                            using (var b = new SolidBrush(color))
                            {
                                g.FillRectangle(b, barRect);
                            }
                            g.DrawRectangle(Pens.Black, barRect);

                            // draw small count above bar if space
                            if (barHeight > 12)
                            {
                                var countStr = cnt.ToString();
                                var sz = g.MeasureString(countStr, smallFont);
                                g.DrawString(countStr, smallFont, Brushes.Black, barRect.Left + (barRect.Width - sz.Width) / 2, barRect.Top - sz.Height - 2);
                            }
                        }
                    }

                    // draw service label below service block
                    var svcLabelRect = new Rectangle(serviceX - 2, plotRect.Bottom + 4, perServiceWidth, 16);
                    var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Near };
                    g.DrawString(svc, smallFont, Brushes.Black, svcLabelRect, sf);
                }

                // draw date label centered under the group
                int groupCenterX = groupX + perGroupWidth / 2;
                var dateLabel = d.Date.ToString("MM-dd");
                var dateRect = new Rectangle(groupCenterX - perGroupWidth / 6, plotRect.Bottom + 22, perGroupWidth / 3, 16);
                var dateSf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Near };
                g.DrawString(dateLabel, font, Brushes.Black, dateRect, dateSf);
            }

            // draw Y axis labels
            int yLabels = Math.Min(5, maxSingle);
            for (int i = 0; i <= yLabels; i++)
            {
                var val = (int)Math.Round(i * (double)maxSingle / yLabels);
                var y = plotRect.Bottom - (int)Math.Round(val / (double)maxSingle * plotRect.Height);
                g.DrawLine(Pens.LightGray, plotRect.Left, y, plotRect.Right, y);
                g.DrawString(val.ToString(), font, Brushes.Black, new PointF(plotRect.Left - 48, y - 8));
            }

            // draw legend for purposes on the right
            var legendX = plotRect.Right + 12;
            var legendY = plotRect.Top + 4;
            int legendItemHeight = 18;
            g.DrawString("Purposes", font, Brushes.Black, legendX, legendY);
            legendY += 18;
            int li = 0;
            foreach (var pur in chartPurposes)
            {
                var color = purposeColors.ContainsKey(pur) ? purposeColors[pur] : Color.Gray;
                var box = new Rectangle(legendX, legendY + li * legendItemHeight, 14, 14);
                using (var b = new SolidBrush(color)) g.FillRectangle(b, box);
                g.DrawRectangle(Pens.Black, box);
                g.DrawString(pur, smallFont, Brushes.Black, legendX + 20, legendY + li * legendItemHeight);
                li++;
            }

            font.Dispose();
            smallFont.Dispose();
        }

        public void ShowAccountsView(List<UserAccount> accounts)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => ShowAccountsView(accounts)));
                return;
            }

            dgvAccounts.DataSource = null;
            dgvAccounts.DataSource = (accounts ?? new List<UserAccount>())
                .Select(a => new { a.Username, a.Role, a.CreatedAt })
                .OrderBy(a => a.Username)
                .ToList();
        }

        public void ShowMessage(string text, string caption, MessageBoxIcon icon)
        {
            MessageBox.Show(text, caption, MessageBoxButtons.OK, icon);
        }

        private void InitializeComponent()
        {

        }

        private class DailyData
        {
            public DateTime Date { get; set; }
            public Dictionary<string, Dictionary<string, int>> Services { get; set; } = new Dictionary<string, Dictionary<string, int>>(StringComparer.OrdinalIgnoreCase);
        }
    }
}