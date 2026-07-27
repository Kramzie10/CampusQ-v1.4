using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Windows.Forms;
using CampusQ.MVP.Views;
using CampusQ.MVP.Models;
using CampusQ.MVP.Data;

namespace CampusQ.MVP.Presenters
{

    public class AdminPresenter
    {
        private readonly IAdminView _view;
        private readonly UserRepository _userRepo;
        private readonly QueueRepository _queue_repo;

        private readonly List<UserAccount> _accounts = new();

        public AdminPresenter(IAdminView view)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));

            DbConfig.EnsureDatabaseAndTables();

            _userRepo = new UserRepository(DbConfig.ConnectionString);
            _queue_repo = new QueueRepository(DbConfig.ConnectionString);

            LoadAccounts();
            RefreshQueueView();
            RefreshAccountsView();
            RefreshActiveQueueView();
        }
        public void RefreshQueueView()
        {
            try
            {
                var items = _queue_repo.GetHistoryAll();
                _view.ShowQueue(items.ToList());
            }
            catch (Exception ex)
            {
                _view.ShowMessage($"Failed to load persisted queue: {ex.Message}", "Error", MessageBoxIcon.Error);
                _view.ShowQueue(new List<QueuePersistDto>());
            }
        }
        public void RefreshActiveQueueView()
        {
            try
            {
                var entries = _queue_repo.GetAll() ?? new List<QueueEntry>();
                var dtos = entries.Select(e => new QueuePersistDto
                {
                    TicketNumber = e.TicketNumber,
                    ServiceTicketNumber = e.ServiceTicketNumber,
                    Purpose = e.Purpose,
                    Service = e.Service,
                    TimeAdded = e.TimeAdded
                }).ToList();
                _view.ShowQueue(dtos);
            }
            catch (Exception ex)
            {
                _view.ShowMessage($"Failed to load active queue: {ex.Message}", "Error", MessageBoxIcon.Error);
                _view.ShowQueue(new List<QueuePersistDto>());
            }
        }

        public void ClearPersistedQueue()
        {
            try
            {
                _queue_repo.ClearHistory();
                RefreshQueueView();
                _view.ShowMessage("Persisted queue history cleared.", "Cleared", MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                _view.ShowMessage($"Failed to clear persisted history: {ex.Message}", "Error", MessageBoxIcon.Error);
            }
        }

        public void RefreshAccountsView()
        {
            _view.ShowAccountsView(_accounts.OrderBy(a => a.Username).ToList());
        }

        public void LoadAccounts()
        {
            _accounts.Clear();
            try
            {
                var list = _userRepo.GetAll();
                if (list != null)
                _accounts.AddRange(list);
            }
        catch (Exception ex)
        {
            _view.ShowMessage($"Failed to load accounts: {ex.Message}", "Error", MessageBoxIcon.Error);
            }
        }

 public void SaveAccounts()
 {

 try
 {

 }
 catch (Exception ex)
 {
 _view.ShowMessage($"Failed to save accounts: {ex.Message}", "Error", MessageBoxIcon.Error);
 }
 }

 public void CreateAccount(string username, string password, string confirm, string role)
 {
 if (!IsValidUsername(username))
 {
 _view.ShowMessage("Username must be3-20 characters and contain only letters, digits, or underscore.", "Validation", MessageBoxIcon.Warning);
 return;
 }

 if (!IsValidPassword(password))
 {
 _view.ShowMessage("Password must be at least8 characters and include at least one letter and one digit.", "Validation", MessageBoxIcon.Warning);
 return;
 }

 if (password != confirm)
 {
 _view.ShowMessage("Password and confirmation do not match.", "Validation", MessageBoxIcon.Warning);
 return;
 }

 if (_accounts.Any(a => string.Equals(a.Username, username, StringComparison.OrdinalIgnoreCase)))
 {
 _view.ShowMessage("An account with this username already exists.", "Validation", MessageBoxIcon.Warning);
 return;
 }

 var salt = RandomNumberGenerator.GetBytes(16);
 var hash = HashPassword(password, salt);
 var account = new UserAccount
 {
 Username = username,
 PasswordHash = Convert.ToBase64String(hash),
 Salt = Convert.ToBase64String(salt),
 Role = role,
 CreatedAt = DateTime.UtcNow
 };

 _userRepo.Add(account);
 _accounts.Add(account);
 RefreshAccountsView();
 _view.ShowMessage("Account created successfully.", "Success", MessageBoxIcon.Information);
 }

 public void DeleteAccount(string username)
 {
 var removed = _accounts.RemoveAll(a => string.Equals(a.Username, username, StringComparison.OrdinalIgnoreCase));
 if (removed >0)
 {
 _userRepo.Remove(username);
 RefreshAccountsView();
 _view.ShowMessage("Account deleted.", "Deleted", MessageBoxIcon.Information);
 }
 else
 {
 _view.ShowMessage("Account not found.", "Error", MessageBoxIcon.Error);
 }
 }
 private static bool IsValidUsername(string? username)
 {
 if (string.IsNullOrWhiteSpace(username)) return false;
 return Regex.IsMatch(username, @"^[A-Za-z0-9_]{3,20}$");
 }

 private static bool IsValidPassword(string? password)
 {
 if (string.IsNullOrEmpty(password) || password.Length <8) return false;
 return Regex.IsMatch(password, @"(?=.*[A-Za-z])(?=.*\d)");
 }

 private static byte[] HashPassword(string password, byte[] salt, int iterations =100_000, int keyBytes =32)
 {
 using var derive = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
 return derive.GetBytes(keyBytes);
 }
 }
}