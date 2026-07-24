using System;
using System.Windows.Forms;

namespace CampusQ
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            Application.Run(new AppContext());
        }

        private class AppContext : ApplicationContext
        {
            public AppContext()
            {
                ShowLogin();
            }

            private void ShowLogin()
            {
                var login = new Login();
                // Make login the context's main form so closing it doesn't terminate the app unexpectedly.
                MainForm = login;
                login.FormClosed += Login_FormClosed;
                login.Show();
            }

            private void Login_FormClosed(object? sender, FormClosedEventArgs e)
            {
                if (sender is Login login)
                {
                    login.FormClosed -= Login_FormClosed;

                    // If presenter stored the next form in login.Tag, transfer ownership to ApplicationContext.
                    if (login.Tag is Form next)
                    {
                        MainForm = next;
                        next.FormClosed += (s, args) => ExitThread();
                        next.Show();
                        return;
                    }
                }

                // No follow-up form -> exit the app.
                ExitThread();
            }
        }
    }
}