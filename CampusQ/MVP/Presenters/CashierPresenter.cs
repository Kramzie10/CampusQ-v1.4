using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using CampusQ.MVP.Data;
using CampusQ.MVP.Models;
using CampusQ.MVP.Views;

namespace CampusQ.MVP.Presenters
{
    public class CashierPresenter
    {
        private readonly ICashierView _view;
        private readonly List<QueueEntry> _masterQueue = new();
        private static int _nextTicketNumber = 1;
        private readonly QueueRepository _queueRepo;

        // Keep window count in one place to match Cashier UI (Window 1..4)
        private const int WindowCount = 4;

        public CashierPresenter(ICashierView view)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            DbConfig.EnsureDatabaseAndTables();
            _queue_repo_guard();
            _queueRepo = new QueueRepository(DbConfig.ConnectionString);
            LoadQueue();
            ApplyFilter();
        }

        public void AddToQueue(string purpose, string service)
        {
            var effectiveService = !string.IsNullOrWhiteSpace(service) ? service : (_view.SelectedService ?? "");
            var entry = new QueueEntry
            {
                Purpose = string.IsNullOrWhiteSpace(purpose) ? "Unknown" : purpose,
                Service = NormalizeService(effectiveService),
                TimeAdded = DateTime.Now
            };

            _queueRepo.Add(entry);
            _masterQueue.Add(entry);
            ApplyFilter();
        }

        private static string NormalizeService(string? service)
        {
            if (string.IsNullOrWhiteSpace(service))
                return "Cashier";

            var s = service.Trim();
            var lower = s.ToLowerInvariant();

            if (string.Equals(lower, "all", StringComparison.OrdinalIgnoreCase)) return "Cashier";
            if (lower.Contains("registr")) return "Registrar";
            if (lower.Contains("cashier") || lower.Contains("window") || Regex.IsMatch(lower, @"(^|\s)w\s*\d", RegexOptions.IgnoreCase))
                return "Cashier";

            return s;
        }

        private static bool IsCashierService(string? service)
        {
            if (string.IsNullOrWhiteSpace(service)) return true;

            var s = service.Trim().ToLowerInvariant();

            if (string.Equals(s, "other", StringComparison.OrdinalIgnoreCase)) return true;

            if (string.Equals(s, "all", StringComparison.OrdinalIgnoreCase)) return true;
            if (s.Contains("cashier") || s.Contains("window")) return true;
            if (Regex.IsMatch(s, @"(^|\s)w\s*\d", RegexOptions.IgnoreCase)) return true;

            return false;
        }

        private static int GetAssignedWindow(int sequence)
        {
            if (WindowCount <= 0) return 1;
            return ((sequence - 1) % WindowCount) + 1;
        }

        private static int GetAssignedWindowForEntry(QueueEntry e)
        {
            if (e == null) return 1;
            var seq = e.ServiceTicketNumber > 0 ? e.ServiceTicketNumber : (e.TicketNumber > 0 ? e.TicketNumber : 1);
            return GetAssignedWindow(seq);
        }

        private static int GetSequenceForEntry(QueueEntry e)
        {
            if (e == null) return int.MaxValue;
            return e.ServiceTicketNumber > 0 ? e.ServiceTicketNumber : (e.TicketNumber > 0 ? e.TicketNumber : int.MaxValue);
        }

        private static QueueEntry CloneForDisplay(QueueEntry src, int assignedWindow)
            => new()
            {
                TicketNumber = src.TicketNumber,
                ServiceTicketNumber = src.ServiceTicketNumber,
                Purpose = src.Purpose,
                Service = $"Cashier - Window {assignedWindow}",
                TimeAdded = src.TimeAdded
            };

        private void ApplyFilter()
        {
            // Build list of cashier entries
            var cashierEntries = _masterQueue
                .Where(q => IsCashierService(q.Service))
                .OrderBy(e => GetSequenceForEntry(e))
                .ToList();

            var selected = (_view.SelectedService ?? "").Trim();
            Debug.WriteLine($"[CashierPresenter] ApplyFilter: SelectedService='{selected}', MasterCount={_masterQueue.Count}, CashierCandidateCount={cashierEntries.Count}");

            int? selWindow = null;
            if (selected.IndexOf("Window", StringComparison.OrdinalIgnoreCase) >= 0 || Regex.IsMatch(selected, @"\bW\s*\d", RegexOptions.IgnoreCase))
            {
                var m = Regex.Match(selected, @"\d+");
                if (m.Success && int.TryParse(m.Value, out var v)) selWindow = v;
            }

            IEnumerable<QueueEntry> display;

            if (selWindow.HasValue)
            {
                var w = selWindow.Value;
                display = cashierEntries
                    .Where(e => GetAssignedWindowForEntry(e) == w)
                    .Select(e => CloneForDisplay(e, w));
            }
            else
            {
                display = cashierEntries.Select(e => CloneForDisplay(e, GetAssignedWindowForEntry(e))).OrderBy(e => GetSequenceForEntry(e));
            }

            var view = new BindingList<QueueEntry>(display.ToList());

            Debug.WriteLine($"[CashierPresenter] ApplyFilter: DisplayCount={view.Count}");

            _view.BindQueue(view);
        }

        public void ServeNext()
        {
            LoadQueue();

            var selected = (_view.SelectedService ?? "").Trim();

            var cashierEntries = _masterQueue
                .Where(q => IsCashierService(q.Service))
                .OrderBy(e => GetSequenceForEntry(e))
                .ToList();

            if (!cashierEntries.Any())
            {
                _view.ShowMessage("No one is currently in the cashier queue for the selected window.", "Queue Empty", MessageBoxIcon.Information);
                return;
            }

            // Determine window filter
            int? selWindow = null;
            if (selected.IndexOf("Window", StringComparison.OrdinalIgnoreCase) >= 0 || Regex.IsMatch(selected, @"\bW\s*\d", RegexOptions.IgnoreCase))
            {
                var m = Regex.Match(selected, @"\d+");
                if (m.Success && int.TryParse(m.Value, out var v)) selWindow = v;
            }

            QueueEntry? next = null;
            if (selWindow.HasValue)
            {
                var w = selWindow.Value;
                next = cashierEntries.FirstOrDefault(e => GetAssignedWindowForEntry(e) == w);
                Debug.WriteLine($"[CashierPresenter] Window {w}: candidates={cashierEntries.Count(e => GetAssignedWindowForEntry(e) == w)}");
            }
            else
            {
                next = cashierEntries.FirstOrDefault();
                Debug.WriteLine($"[CashierPresenter] Overall ordered count={cashierEntries.Count}");
            }

            if (next == null)
            {
                _view.ShowMessage("No one is currently in the cashier queue for the selected window.", "Queue Empty", MessageBoxIcon.Information);
                return;
            }

            _masterQueue.Remove(next);
            _queueRepo.Remove(next.TicketNumber);
            ApplyFilter();

            // Inform view via ICashierView helper
            try
            {
                _view.DisplayServedTicket(next);
            }
            catch
            {
                try { _view.ShowMessage($"Now serving {next.ServiceTicketNumber}\nService: {next.Service}\nPurpose: {next.Purpose}", "Serving Next", MessageBoxIcon.Information); } catch { }
            }
        }

        private static int GetNextTicket() => Interlocked.Increment(ref _nextTicketNumber) - 1;

        private void LoadQueue()
        {
            try
            {
                var list = _queue_repo_safe_getall();
                _masterQueue.Clear();
                if (list != null) _masterQueue.AddRange(list);
                _nextTicketNumber = _masterQueue.Any() ? _masterQueue.Max(q => q.TicketNumber) + 1 : 1;

                Debug.WriteLine($"[CashierPresenter] LoadQueue loaded {_masterQueue.Count} rows");
                if (_masterQueue.Any())
                {
                    Debug.WriteLine("[CashierPresenter] Loaded sample:\n" + string.Join("\n", _masterQueue.Take(10).Select(c => $"Ticket={c.TicketNumber}, STN={c.ServiceTicketNumber}, Service='{c.Service}', Purpose='{c.Purpose}', Label={c.TicketLabel}")));
                }
            }
            catch (Exception ex)
            {
                try { _view?.ShowMessage($"Failed to load cashier queue from database:\n{ex.Message}", "Database Error", MessageBoxIcon.Error); } catch { }
                System.Diagnostics.Debug.WriteLine($"Failed to load cashier queue: {ex}");
            }
        }

        private List<QueueEntry>? _queue_repo_safe_getall()
        {
            try
            {
                return _queueRepo.GetAll();
            }
            catch (Exception ex)
            {
                try { _view?.ShowMessage($"Database read failed: {ex.Message}", "DB Error", MessageBoxIcon.Error); } catch { }
                System.Diagnostics.Debug.WriteLine($"QueueRepository.GetAll failed: {ex}");
                return new List<QueueEntry>();
            }
        }

        public void RefreshQueueView()
        {
            LoadQueue();
            ApplyFilter();
        }

        private static void _queue_repo_guard() { var _ = DbConfig.ConnectionString; }
    }
}