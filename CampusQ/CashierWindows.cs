using CampusQ.MVP.Data;
using CampusQ.MVP.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace CampusQ
{
    public partial class CashierWindows : Form
    {

        private readonly QueueRepository _queueRepo;
        private System.Threading.Timer? _refreshTimer;

        // Four cashier windows: each has Now + Next labels
        private const int WindowCount =4;
        private readonly Label[] NowLabels = new Label[WindowCount];
        private readonly Label[] NextLabels = new Label[WindowCount];


        public CashierWindows()
        {
            InitializeComponent();

            // Ensure DB/tables exist and prepare repository
            DbConfig.EnsureDatabaseAndTables();
            _queueRepo = new QueueRepository(DbConfig.ConnectionString);
            NowLabels[0] = lblWindow1Current;
            NextLabels[0] = lblWindow1Next;
            NowLabels[1] = lblWindow2Current;
            NextLabels[1] = lblWindow2Next;
            NowLabels[2] = lblWindow3Current;
            NextLabels[2] = lblWindow3Next;
            NowLabels[3] = lblWindow4Current;
            NextLabels[3] = lblWindow4Next;

            // Make labels readable / set consistent font styling
            for (int i =0; i < WindowCount; i++)
            {
                ConfigureLabelForDisplay(NowLabels[i],48F, FontStyle.Bold);
                ConfigureLabelForDisplay(NextLabels[i],21F, FontStyle.Bold);

                // Ensure there's a sensible default text so UI doesn't show null
                if (string.IsNullOrWhiteSpace(NowLabels[i].Text))
                    NowLabels[i].Text = "Now: -";
                if (string.IsNullOrWhiteSpace(NextLabels[i].Text))
                    NextLabels[i].Text = "Next: -";
            }

            _refreshTimer = new System.Threading.Timer(_ => RefreshInBackground(), null, TimeSpan.Zero, TimeSpan.FromSeconds(2));

            FormClosed += (s, e) =>
            {
                try
                {
                    _refreshTimer?.Dispose();
                    _refreshTimer = null;
                }
                catch { /* swallow */ }
            };
        }



        private static void ConfigureLabelForDisplay(Label lbl, float size, FontStyle style)
        {
            if (lbl == null) return;
            lbl.Font = new Font("Copperplate Gothic Bold", size, style, GraphicsUnit.Point);
            lbl.BackColor = Color.Transparent;
            lbl.TextAlign = ContentAlignment.MiddleCenter;
            
        }

        private void RefreshInBackground()
        {
            try
            {
                var all = _queue_repo_getall_placeholder();

                var cash = all
                    .Where(q => IsCashierService(q.Service))
                    .OrderBy(q => q.ServiceTicketNumber)
                    .ToArray();

                QueueEntry[][] windows = new QueueEntry[WindowCount][];
                if (cash.Length ==0)
                {
                    for (int i =0; i < WindowCount; i++) windows[i] = Array.Empty<QueueEntry>();
                }
                else
                {
                    var temp = new List<QueueEntry>[WindowCount];
                    for (int i =0; i < WindowCount; i++) temp[i] = new List<QueueEntry>();

                    foreach (var q in cash)
                    {
                        var seq = Math.Max(1, q.ServiceTicketNumber);
                        var idx = ((seq -1) % WindowCount + WindowCount) % WindowCount;
                        temp[idx].Add(q);
                    }

                    for (int i =0; i < WindowCount; i++)
                    {
                        windows[i] = temp[i].OrderBy(q => q.ServiceTicketNumber).ToArray();
                    }
                }

                var nowArr = new QueueEntry?[WindowCount];
                var nextArr = new QueueEntry?[WindowCount];
                for (int i =0; i < WindowCount; i++)
                {
                    nowArr[i] = windows[i].ElementAtOrDefault(0);
                    nextArr[i] = windows[i].ElementAtOrDefault(1);
                }

                if (IsHandleCreated && !IsDisposed)
                {
                    try
                    {
                        BeginInvoke(new Action(() =>
                        {
                            for (int i =0; i < WindowCount; i++)
                            {
                                if (NowLabels[i] != null) NowLabels[i].Text = FormatNowLabel(nowArr[i]);
                                if (NextLabels[i] != null) NextLabels[i].Text = FormatNextLabel(nextArr[i]);
                            }
                        }));
                    }
                    catch (ObjectDisposedException) {}
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CashierServiceWindow.RefreshInBackground failed: {ex}");

                if (IsHandleCreated && !IsDisposed)
                {
                    try
                    {
                        BeginInvoke(new Action(() =>
                        {
                            for (int i =0; i < WindowCount; i++)
                            {
                                if (NowLabels[i] != null) NowLabels[i].Text = "Error";
                                if (NextLabels[i] != null) NextLabels[i].Text = "Error";
                            }
                        }));
                    }
                    catch (ObjectDisposedException) { /* ignore */ }
                }
            }
        }

        public void UpdateDisplay()
        {
            _refreshTimer?.Change(TimeSpan.Zero, TimeSpan.FromSeconds(2));
        }
        public void DisplayFromDataGrid(DataGridView dgv)
        {
            if (dgv == null) return;
            var items = new List<QueueEntry>();
            foreach (DataGridViewRow row in dgv.Rows)
            {
                if (row.IsNewRow) continue;

                if (row.DataBoundItem is QueueEntry qe)
                {
                    items.Add(qe);
                }
                else if (row.DataBoundItem is QueuePersistDto dto)
                {
                    items.Add(new QueueEntry
                    {
                        TicketNumber = dto.TicketNumber,
                        ServiceTicketNumber = dto.ServiceTicketNumber,
                        Purpose = dto.Purpose,
                        Service = dto.Service,
                        TimeAdded = dto.TimeAdded
                    });
                }
                else
                {
                    var service = TryGetCellString(row, "Service") ?? TryGetCellString(row, "Svc") ?? string.Empty;
                    var ticketLabel = TryGetCellString(row, "TicketLabel") ?? TryGetCellString(row, "Ticket") ?? string.Empty;
                    var stN = ParseNumberFromLabel(ticketLabel);

                    items.Add(new QueueEntry
                    {
                        Service = service,
                        ServiceTicketNumber = stN,
                        TicketNumber = stN
                    });
                }
            }

            var hasServiceInfo = items.Any(it => !string.IsNullOrWhiteSpace(it.Service));
            var cashList = hasServiceInfo
                ? items.Where(q => IsCashierService(q.Service)).ToList()
                : items;

            if (!cashList.Any())
            {
                if (IsHandleCreated && !IsDisposed)
                {
                    try
                    {
                        BeginInvoke(new Action(() =>
                        {
                            for (int i =0; i < WindowCount; i++)
                            {
                                if (NowLabels[i] != null) NowLabels[i].Text = "Now: -";
                                if (NextLabels[i] != null) NextLabels[i].Text = "Next: -";
                            }
                        }));
                    }
                    catch (ObjectDisposedException) { }
                }
                return;
            }

            var temp = new List<QueueEntry>[WindowCount];
            for (int i =0; i < WindowCount; i++) temp[i] = new List<QueueEntry>();

            int fallbackSeq =1;
            foreach (var q in cashList)
            {
                var seq = q.ServiceTicketNumber >0 ? q.ServiceTicketNumber : (q.TicketNumber >0 ? q.TicketNumber : fallbackSeq++);
                var idx = ((seq -1) % WindowCount + WindowCount) % WindowCount;
                temp[idx].Add(q);
            }

            var windows = new QueueEntry[WindowCount][];
            for (int i =0; i < WindowCount; i++)
                windows[i] = temp[i].OrderBy(x => (x.ServiceTicketNumber >0 ? x.ServiceTicketNumber : x.TicketNumber)).ToArray();

            var nowArr = new QueueEntry?[WindowCount];
            var nextArr = new QueueEntry?[WindowCount];
            for (int i =0; i < WindowCount; i++)
            {
                nowArr[i] = windows[i].ElementAtOrDefault(0);
                nextArr[i] = windows[i].ElementAtOrDefault(1);
            }

            if (IsHandleCreated && !IsDisposed)
            {
                try
                {
                    BeginInvoke(new Action(() =>
                    {
                        for (int i =0; i < WindowCount; i++)
                        {
                            if (NowLabels[i] != null) NowLabels[i].Text = FormatNowLabel(nowArr[i]);
                            if (NextLabels[i] != null) NextLabels[i].Text = FormatNextLabel(nextArr[i]);
                        }
                    }));
                }
                catch (ObjectDisposedException) { /* ignore */ }
            }
        }

        private static string? TryGetCellString(DataGridViewRow row, string colName)
        {
            try
            {
                var cell = row.Cells.Cast<DataGridViewCell>().FirstOrDefault(c => string.Equals(c.OwningColumn?.Name, colName, StringComparison.OrdinalIgnoreCase) ||
                                                                                 string.Equals(c.OwningColumn?.HeaderText, colName, StringComparison.OrdinalIgnoreCase));
                return cell?.Value?.ToString();
            }
            catch
            {
                return null;
            }
        }

        private static int ParseNumberFromLabel(string label)
        {
            // return0 for empty input
            if (string.IsNullOrWhiteSpace(label)) return 0;
            var digits = new string(label.Where(char.IsDigit).ToArray());
            if (int.TryParse(digits, out var n)) return n;
            return 0;
        }

        private static string FormatNowLabel(QueueEntry? now)
        {
            if (now == null) return "Now: -";
            var nowLabel = now.TicketLabel ?? "-";
            var tn = now.TicketNumber >0 ? $"#{now.TicketNumber}" : "";
            return string.IsNullOrEmpty(tn) ? $"Now: {nowLabel}" : $"Now: {nowLabel} ({tn})";
        }



        private static string FormatNextLabel(QueueEntry? next)
        {
            if (next == null) return "Next: -";
            var nextLabel = next.TicketLabel ?? "-";
            var tn = next.TicketNumber >0 ? $"#{next.TicketNumber}" : "";
            return string.IsNullOrEmpty(tn) ? $"Next: {nextLabel}" : $"Next: {nextLabel} ({tn})";
        }

        private static bool IsCashierService(string? service)
        {
            if (string.IsNullOrWhiteSpace(service)) return true;
            var s = service.Trim().ToLowerInvariant();
            if (string.Equals(s, "cashier", StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        private List<QueueEntry> _queue_repo_getall_placeholder()
        {
            try
            {
                return _queueRepo.GetAll() ?? new List<QueueEntry>();
            }
            catch
            {
                return new List<QueueEntry>();
            }
        }
    }
}