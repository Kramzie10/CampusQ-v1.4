using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;
using CampusQ.MVP.Views;
using CampusQ.MVP.Models;
using CampusQ.MVP.Data;

namespace CampusQ.MVP.Presenters
{
    public class LoginPresenter
    {
        private readonly ILoginView _view;
        private readonly UserRepository _userRepo;
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        public LoginPresenter(ILoginView view)
        {
            _view = view;
            DbConfig.EnsureDatabaseAndTables();
            _userRepo = new UserRepository(DbConfig.ConnectionString);
        }

        public void HandleLogin()
        {
            var username = _view.Username?.Trim() ?? "";
            var password = _view.Password ?? "";

            if (TryValidatePersistedUser(username, password, out var role))
            {
                OpenFormForRole(role);
                return;
            }

            if (username == "staffReg" && password == "staffReg")
            {
                OpenFormForRole("Staff");
            }
            else if (username == "staffCash" && password == "staffCash")
            {
                OpenFormForRole("Cashier");
            }
            else if (username == "user" && password == "user")
            {
                OpenFormForRole("User");
            }
            else if (username == "admin" && password == "admin")
            {
                OpenFormForRole("Admin");
            }
            else
            {
                _view.ShowMessage("Invalid username or password", "Authentication Failed", MessageBoxIcon.Warning);
            }
        }

        private static void OpenFormForRole(string role)
        {
            Form toOpen = role?.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true
                ? new AdminDashboard()
                : role?.Equals("Staff", StringComparison.OrdinalIgnoreCase) == true
                    ? new Staff()
                    : role?.Equals("Cashier", StringComparison.OrdinalIgnoreCase) == true
                        ? new Cashier()
                        : (Form)new Form1();


            var login = Application.OpenForms.OfType<Login>().FirstOrDefault();

            if (login != null && !login.IsDisposed)
            {
                try
                {
                    login.Tag = toOpen;
                    login.Close();
                    return;
                }
                catch
                {
                }
            }

            toOpen.Show();

            toOpen.FormClosed += (s, e) =>
            {
                try
                {
                    if (login != null && !login.IsDisposed)
                    {
                        login.Show();
                    }
                }
                catch
                {
                    // ignore exceptions during restore
                }
            };
        }

        private bool TryValidatePersistedUser(string username, string password, out string role)
        {
            role = "";

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
                return false;

            try
            {
                var users = _userRepo.GetAll();
                var user = users.FirstOrDefault(u => string.Equals(u.Username, username, StringComparison.Ordinal));
                if (user == null)
                    return false;

                var salt = Convert.FromBase64String(user.Salt ?? "");
                var expectedHash = Convert.FromBase64String(user.PasswordHash ?? "");

                var computed = HashPassword(password, salt);

                if (CryptographicOperations.FixedTimeEquals(computed, expectedHash))
                {
                    role = user.Role ?? "Staff";
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private static byte[] HashPassword(string password, byte[] salt, int iterations = 100_000, int keyBytes = 32)
        {
            using var derive = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            return derive.GetBytes(keyBytes);
        }
    }
}