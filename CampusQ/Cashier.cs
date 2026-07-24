using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CampusQ.MVP.Views;
using CampusQ.MVP.Models;

namespace CampusQ
{

    public class Cashier : Form
    {
        private readonly CashierView _mvpView;

        public Cashier()
        {
            Text = "Cashier - Staff";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1200, 800);
            BackgroundImage = Properties.Resources.Purple_and_White_Minimalist_Modern_Computer_Repair_Logo__23_386_x_16_535_in___8_5_x_22_cm___500_x_500_px___11_7_x_8_27_in_;

            _mvpView = new CashierView
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill
            };

            Controls.Add(_mvpView);
            _mvpView.Show();
            // Default to show all cashier entries so DB items appear
            _mvpView.SetSelectedService("All");
            _mvpView.RefreshQueueView();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            // Also refresh when the host is shown (handles cases where DB was updated while the app was hidden)
            _mvpView.RefreshQueueView();
        }

        // Removed AddToQueue: cashier no longer exposes the ability to add tickets to the queue.

        public void RefreshQueueView()
            => _mvpView.RefreshQueueView();

        // Expose selection/state operations if other code relies on IStaffView on Cashier.
        public void BindQueue(BindingList<QueueEntry> view)
            => _mvpView.BindQueue(view);

        public string SelectedService => _mvpView.SelectedService;

        public void ShowMessage(string text, string caption, MessageBoxIcon icon)
            => _mvpView.ShowMessage(text, caption, icon);

        private void InitializeComponent()
        {

        }

        public void SetSelectedService(string service)
            => _mvpView.SetSelectedService(service);
    }
}