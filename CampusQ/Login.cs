using System;
using System.Windows.Forms;
using CampusQ.MVP.Views;
using CampusQ.MVP.Presenters;

namespace CampusQ
{
    public partial class Login : Form, ILoginView
    {
        private readonly LoginPresenter _presenter;

        public Login()
        {
            InitializeComponent();
            _presenter = new LoginPresenter(this);
        }

        private void btn_login_Click(object sender, EventArgs e)
        {
            _presenter.HandleLogin();
        }

        // ILoginView implementation
        public string Username => txt_username.Text ?? string.Empty;
        public string Password => txt_password.Text ?? string.Empty;

        public void ShowMessage(string text, string caption, MessageBoxIcon icon)
        {
            MessageBox.Show(text, caption, MessageBoxButtons.OK, icon);
        }

        public void HideView() => Hide();
        public void CloseView() => Close();
    }
}
