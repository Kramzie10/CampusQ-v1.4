namespace CampusQ
{
    partial class CashierWindows
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        // Cashier window labels (current / next) for 4 windows
        private System.Windows.Forms.Label lblWindow1Current;
        private System.Windows.Forms.Label lblWindow1Next;
        private System.Windows.Forms.Label lblWindow2Current;
        private System.Windows.Forms.Label lblWindow2Next;
        private System.Windows.Forms.Label lblWindow3Current;
        private System.Windows.Forms.Label lblWindow3Next;
        private System.Windows.Forms.Label lblWindow4Current;
        private System.Windows.Forms.Label lblWindow4Next;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lblWindow1Current = new Label();
            lblWindow1Next = new Label();
            lblWindow2Current = new Label();
            lblWindow2Next = new Label();
            lblWindow3Current = new Label();
            lblWindow3Next = new Label();
            lblWindow4Current = new Label();
            lblWindow4Next = new Label();
            SuspendLayout();
            // 
            // lblWindow1Current
            // 
            lblWindow1Current.BackColor = Color.Transparent;
            lblWindow1Current.Font = new Font("Segoe UI", 72F, FontStyle.Bold);
            lblWindow1Current.ForeColor = Color.Gold;
            lblWindow1Current.Location = new Point(152, 427);
            lblWindow1Current.Name = "lblWindow1Current";
            lblWindow1Current.Size = new Size(376, 215);
            lblWindow1Current.TabIndex = 0;
            lblWindow1Current.Text = "Now: ---";
            // 
            // lblWindow1Next
            // 
            lblWindow1Next.BackColor = Color.Transparent;
            lblWindow1Next.Font = new Font("Segoe UI", 36F);
            lblWindow1Next.ForeColor = Color.Gold;
            lblWindow1Next.Location = new Point(273, 674);
            lblWindow1Next.Name = "lblWindow1Next";
            lblWindow1Next.Size = new Size(207, 65);
            lblWindow1Next.TabIndex = 1;
            lblWindow1Next.Text = "Next: ---";
            // 
            // lblWindow2Current
            // 
            lblWindow2Current.BackColor = Color.Transparent;
            lblWindow2Current.Font = new Font("Segoe UI", 72F, FontStyle.Bold);
            lblWindow2Current.ForeColor = Color.Gold;
            lblWindow2Current.Location = new Point(593, 427);
            lblWindow2Current.Name = "lblWindow2Current";
            lblWindow2Current.Size = new Size(372, 215);
            lblWindow2Current.TabIndex = 2;
            lblWindow2Current.Text = "Now: ---";
            // 
            // lblWindow2Next
            // 
            lblWindow2Next.BackColor = Color.Transparent;
            lblWindow2Next.Font = new Font("Segoe UI", 36F);
            lblWindow2Next.ForeColor = Color.Gold;
            lblWindow2Next.Location = new Point(703, 674);
            lblWindow2Next.Name = "lblWindow2Next";
            lblWindow2Next.Size = new Size(207, 65);
            lblWindow2Next.TabIndex = 3;
            lblWindow2Next.Text = "Next: ---";
            // 
            // lblWindow3Current
            // 
            lblWindow3Current.BackColor = Color.Transparent;
            lblWindow3Current.Font = new Font("Segoe UI", 72F, FontStyle.Bold);
            lblWindow3Current.ForeColor = Color.Gold;
            lblWindow3Current.Location = new Point(1018, 429);
            lblWindow3Current.Name = "lblWindow3Current";
            lblWindow3Current.Size = new Size(371, 213);
            lblWindow3Current.TabIndex = 4;
            lblWindow3Current.Text = "Now: ---";
            // 
            // lblWindow3Next
            // 
            lblWindow3Next.BackColor = Color.Transparent;
            lblWindow3Next.Font = new Font("Segoe UI", 36F);
            lblWindow3Next.ForeColor = Color.Gold;
            lblWindow3Next.Location = new Point(1138, 674);
            lblWindow3Next.Name = "lblWindow3Next";
            lblWindow3Next.Size = new Size(207, 65);
            lblWindow3Next.TabIndex = 5;
            lblWindow3Next.Text = "Next: ---";
            // 
            // lblWindow4Current
            // 
            lblWindow4Current.BackColor = Color.Transparent;
            lblWindow4Current.Font = new Font("Segoe UI", 72F, FontStyle.Bold);
            lblWindow4Current.ForeColor = Color.Gold;
            lblWindow4Current.Location = new Point(1458, 430);
            lblWindow4Current.Name = "lblWindow4Current";
            lblWindow4Current.Size = new Size(369, 212);
            lblWindow4Current.TabIndex = 6;
            lblWindow4Current.Text = "Now: ---";
            // 
            // lblWindow4Next
            // 
            lblWindow4Next.BackColor = Color.Transparent;
            lblWindow4Next.Font = new Font("Segoe UI", 36F);
            lblWindow4Next.ForeColor = Color.Gold;
            lblWindow4Next.Location = new Point(1628, 674);
            lblWindow4Next.Name = "lblWindow4Next";
            lblWindow4Next.Size = new Size(207, 65);
            lblWindow4Next.TabIndex = 7;
            lblWindow4Next.Text = "Next: ---";
            // 
            // CashierWindows
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = Properties.Resources._25;
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(1904, 1041);
            Controls.Add(lblWindow1Current);
            Controls.Add(lblWindow1Next);
            Controls.Add(lblWindow2Current);
            Controls.Add(lblWindow2Next);
            Controls.Add(lblWindow3Current);
            Controls.Add(lblWindow3Next);
            Controls.Add(lblWindow4Current);
            Controls.Add(lblWindow4Next);
            Name = "CashierWindows";
            Text = "CashierWindows";
            ResumeLayout(false);
        }

        #endregion
    }
}