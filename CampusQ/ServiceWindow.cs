using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CampusQ.MVP.Data;
using CampusQ.MVP.Models;

namespace CampusQ
{
    public partial class ServiceWindow : Form
    {
        private readonly QueueRepository _queueRepo;
        private System.Threading.Timer? _refreshTimer;

        public ServiceWindow()
        {
            InitializeComponent();

            // Ensure DB/tables exist and prepare repository
            DbConfig.EnsureDatabaseAndTables();
            _queueRepo = new QueueRepository(DbConfig.ConnectionString);

            // Make labels multiline / larger for readability at runtime
            ConfigureLabelForDisplay(RegNowLabel, 65.25F, FontStyle.Bold);
            ConfigureLabelForDisplay(RegNextLabel, 30F, FontStyle.Bold);
            ConfigureLabelForDisplay(RegNowLabel2, 65.25F, FontStyle.Bold);
            ConfigureLabelForDisplay(RegNextLabel2, 30F, FontStyle.Bold);


            _refreshTimer = new System.Threading.Timer(_ => RefreshInBackground(), null, TimeSpan.Zero, TimeSpan.FromSeconds(2));

            // Ensure timers are disposed when window closes
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
            lbl.AutoSize = false;
            lbl.Font = new Font("Copperplate Gothic Bold", size, style, GraphicsUnit.Point);
            lbl.BackColor = Color.Transparent;
        }
        private void RefreshInBackground()
        {
            try
            {
                // Load data on background thread
                var all = _queueRepo.GetAll() ?? new List<QueueEntry>();

                // Build ordered registrar queue (FCFS)
                var reg = all
                    .Where(q => string.Equals(q.Service, "Registrar", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(q => q.TicketNumber)
                    .ToList();

                // Split RC (credential) requests from the rest. RC requests must be routed to Window 2.
                List<QueueEntry> rcRequests = reg.Where(IsRCRequest).OrderBy(q => q.TicketNumber).ToList();
                List<QueueEntry> nonRc = reg.Where(q => !IsRCRequest(q)).OrderBy(q => q.TicketNumber).ToList();

                QueueEntry[] regWindow1;
                QueueEntry[] regWindow2;

                // Assign all non-credential (nonRc) entries to Window 1 and only credential requests to Window 2.
                // This ensures Window 2 will never receive non-credential entries.
                regWindow1 = nonRc.ToArray();
                regWindow2 = rcRequests.ToArray();

                // For each window compute Now and Next from its assigned tickets.
                var regNow1 = regWindow1.ElementAtOrDefault(0);
                var regNext1 = regWindow1.ElementAtOrDefault(1);

                var regNow2 = regWindow2.ElementAtOrDefault(0);
                var regNext2 = regWindow2.ElementAtOrDefault(1);

                var cash = all
                    .Where(q => string.Equals(q.Service, "Cashier", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(q => q.TicketNumber)
                    .ToArray();

                var cashNow = cash.ElementAtOrDefault(0);
                var cashNext = cash.ElementAtOrDefault(1);

                // Marshal UI update to UI thread safely
                if (IsHandleCreated && !IsDisposed)
                {
                    try
                    {
                        BeginInvoke(new Action(() =>
                        {
                            // Window 1 (Registrar)
                            RegNowLabel.Text = FormatNowLabel(regNow1);
                            RegNextLabel.Text = FormatNextLabel(regNext1);

                            // Window 2 (Registrar)
                            RegNowLabel2.Text = FormatNowLabel(regNow2);
                            RegNextLabel2.Text = FormatNextLabel(regNext2);
                        }));
                    }
                    catch (ObjectDisposedException) { /* form already disposed */ }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ServiceWindow.RefreshInBackground failed: {ex}");

                // attempt to reflect error on UI if still available
                if (IsHandleCreated && !IsDisposed)
                {
                    try
                    {
                        BeginInvoke(new Action(() =>
                        {
                            RegNowLabel.Text = "Error";
                            RegNextLabel.Text = "Error";
                            RegNowLabel2.Text = "Error";
                            RegNextLabel2.Text = "Error";

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

        private static string FormatNowLabel(QueueEntry? now)
        {
            var nowLabel = now?.TicketLabel ?? "-";
            return $"Now: {nowLabel}";
        }

        private static string FormatNextLabel(QueueEntry? next)
        {
            var nextLabel = next?.TicketLabel ?? "-";
            return $"Next: {nextLabel}";
        }

        private static bool IsRCRequest(QueueEntry entry)
        {
            if (entry == null) return false;

            var label = entry.TicketLabel ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(label) && label.StartsWith("RC", StringComparison.OrdinalIgnoreCase))
                return true;

            var purpose = entry.Purpose ?? string.Empty;
            if (purpose.IndexOf("credential", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            var p = purpose.ToLowerInvariant();
            var separators = new[] { ' ', '\t', '/', '\\', ',', ';', '-', '_', '.', '(', ')', '[', ']' };
            var tokens = p.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Any(t => t == "rc"))
                return true;

            return false;
        }
    }
}
