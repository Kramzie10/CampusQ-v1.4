using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;
using CampusQ.MVP.Models;
using CampusQ.MVP.Views;
using CampusQ.MVP.Data;
using System.Text.RegularExpressions;

namespace CampusQ.MVP.Presenters
{
    public class StaffPresenter
    {
        private readonly IStaffView _view;
        private readonly List<QueueEntry> _masterQueue = new();
        private static int _nextTicketNumber = 1;
        private readonly QueueRepository _queueRepo;

        public StaffPresenter(IStaffView view)
        {
            _view = view;
            DbConfig.EnsureDatabaseAndTables();
            _queueRepo = new QueueRepository(DbConfig.ConnectionString);
            LoadQueue();
            ApplyFilter();
        }

        public void AddToQueue(string purpose, string service)
        {
            // If caller didn't supply a service, try to use the currently selected service from the view
            var effectiveService = !string.IsNullOrWhiteSpace(service)
                ? service
                : (_view.SelectedService ?? "");

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

        private static bool IsRCRequest(QueueEntry? entry)
        {
            if (entry == null) return false;

            var label = entry.TicketLabel;
            if (!string.IsNullOrWhiteSpace(label) && label.StartsWith("RC", StringComparison.OrdinalIgnoreCase))
                return true;
            var purpose = entry.Purpose ?? string.Empty;
            if (purpose.IndexOf("credential", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            // Treat "rc" as a standalone token in purpose
            var p = purpose.ToLowerInvariant();
            var separators = new[] { ' ', '\t', '/', '\\', ',', ';', '-', '_', '.', '(', ')', '[', ']' };
            var tokens = p.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Any(t => t == "rc"))
                return true;

            return false;
        }

        private static string NormalizeService(string? service)
        {
            if (string.IsNullOrWhiteSpace(service))
                return "Other";

            var s = service.Trim();
            var lower = s.ToLowerInvariant();

            if (lower.Contains("cash") || lower.Contains("window") || Regex.IsMatch(lower, @"(^|\s)w\s*\d", RegexOptions.IgnoreCase))
                return "Cashier";
            if (lower.Contains("registr") || lower.Equals("reg", StringComparison.OrdinalIgnoreCase) || lower.StartsWith("reg ") || lower.Contains(" reg"))
                return "Registrar";

            if (string.Equals(s, "Registrar", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(s, "Other", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(s, "Cashier", StringComparison.OrdinalIgnoreCase))
            {
                return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLowerInvariant());
            }

            return "Other";
        }

        private void ApplyFilter()
        {
            var reg = _masterQueue
                .Where(q => string.Equals(q.Service, "Registrar", StringComparison.OrdinalIgnoreCase))
                .OrderBy(q => q.TicketNumber)
                .ToList();

            IEnumerable<QueueEntry> filtered = reg;

            var selected = (_view.SelectedService ?? string.Empty).Trim();

            if (selected.StartsWith("Registrar", StringComparison.OrdinalIgnoreCase) && reg.Any())
            {
                int? window = null;
                if (selected.IndexOf("W1", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    selected.IndexOf("W 1", StringComparison.OrdinalIgnoreCase) >= 0)
                    window = 1;
                else if (selected.IndexOf("W2", StringComparison.OrdinalIgnoreCase) >= 0 ||
                         selected.IndexOf("W 2", StringComparison.OrdinalIgnoreCase) >= 0)
                    window = 2;

                if (window.HasValue)
                {
                    // Window 1: only non-credential registrar entries
                    if (window == 1)
                    {
                        filtered = reg
                            .Where(q => !IsRCRequest(q))
                            .OrderBy(q => q.TicketNumber);
                    }
                    else // Window 2: only credential (RC) requests
                    {
                        filtered = reg
                            .Where(q => IsRCRequest(q))
                            .OrderBy(q => q.TicketNumber);
                    }
                }
            }

            var view = new BindingList<QueueEntry>(filtered.ToList());
            _view.BindQueue(view);
        }

        public void ServeNext()
        {
            string selected = _view.SelectedService ?? "Registrar - W1";
            QueueEntry? next = null;

            var reg = _masterQueue
                .Where(q => string.Equals(q.Service, "Registrar", StringComparison.OrdinalIgnoreCase))
                .OrderBy(q => q.TicketNumber)
                .ToList();

            if (!reg.Any())
            {
                next = null;
            }
            else
            {
                int? window = null;
                if (selected.IndexOf("W1", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    selected.IndexOf("W 1", StringComparison.OrdinalIgnoreCase) >= 0)
                    window = 1;
                else if (selected.IndexOf("W2", StringComparison.OrdinalIgnoreCase) >= 0 ||
                         selected.IndexOf("W 2", StringComparison.OrdinalIgnoreCase) >= 0)
                    window = 2;

                if (window == 1)
                {
                    // pick first non-RC registrar
                    next = reg.FirstOrDefault(q => !IsRCRequest(q));
                }
                else if (window == 2)
                {
                    // pick first RC (credential) request only
                    next = reg.FirstOrDefault(q => IsRCRequest(q));
                }
                else
                {
                    next = reg.FirstOrDefault();
                }
            }

            if (next == null)
            {
                _view.ShowMessage("No one is currently in the registrar queue for the selected window.", "Queue Empty", MessageBoxIcon.Information);
                return;
            }

            _masterQueue.Remove(next);
            _queueRepo.Remove(next.TicketNumber);
            ApplyFilter();

            _view.ShowMessage($"Now serving Ticket #{next.ServiceTicketNumber}\nService: {next.Service}\nPurpose: {next.Purpose}", "Serving Next", MessageBoxIcon.Information);
        }

        private static int GetNextTicket() => System.Threading.Interlocked.Increment(ref _nextTicketNumber) - 1;



        private void LoadQueue()
        {
            try
            {
                var list = _queueRepo.GetAll();
                _masterQueue.Clear();
                if (list != null) _masterQueue.AddRange(list);
                _nextTicketNumber = _masterQueue.Any() ? _masterQueue.Max(q => q.TicketNumber) + 1 : 1;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load queue: {ex}");
            }
        }

        public void RefreshQueueView()
        {
            LoadQueue();
            ApplyFilter();
        }

        public void ServiceWindow()
        {
            Form Service = new ServiceWindow();
            Service.Show();
        }
    }
}