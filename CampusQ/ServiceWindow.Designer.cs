
namespace CampusQ
{
    partial class ServiceWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            RegNowLabel = new Label();
            RegNextLabel = new Label();
            RegNowLabel2 = new Label();
            RegNextLabel2 = new Label();
            SuspendLayout();
            // 
            // RegNowLabel
            // 
            RegNowLabel.BackColor = Color.Transparent;
            RegNowLabel.Font = new Font("Copperplate Gothic Bold", 65.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            RegNowLabel.ForeColor = Color.Gold;
            RegNowLabel.Location = new Point(308, 470);
            RegNowLabel.Margin = new Padding(2, 0, 2, 0);
            RegNowLabel.Name = "RegNowLabel";
            RegNowLabel.Size = new Size(458, 317);
            RegNowLabel.TabIndex = 0;
            RegNowLabel.Text = "Now: -";
            RegNowLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // RegNextLabel
            // 
            RegNextLabel.BackColor = Color.Transparent;
            RegNextLabel.Font = new Font("Copperplate Gothic Bold", 30F, FontStyle.Regular, GraphicsUnit.Point, 0);
            RegNextLabel.ForeColor = Color.Gold;
            RegNextLabel.Location = new Point(514, 863);
            RegNextLabel.Margin = new Padding(2, 0, 2, 0);
            RegNextLabel.Name = "RegNextLabel";
            RegNextLabel.Size = new Size(343, 60);
            RegNextLabel.TabIndex = 1;
            RegNextLabel.Text = "Next: -";
            RegNextLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // RegNowLabel2
            // 
            RegNowLabel2.BackColor = Color.Transparent;
            RegNowLabel2.Font = new Font("Copperplate Gothic Bold", 65.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            RegNowLabel2.ForeColor = Color.Gold;
            RegNowLabel2.Location = new Point(1134, 470);
            RegNowLabel2.Margin = new Padding(2, 0, 2, 0);
            RegNowLabel2.Name = "RegNowLabel2";
            RegNowLabel2.Size = new Size(456, 317);
            RegNowLabel2.TabIndex = 4;
            RegNowLabel2.Text = "Now: -";
            RegNowLabel2.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // RegNextLabel2
            // 
            RegNextLabel2.BackColor = Color.Transparent;
            RegNextLabel2.Font = new Font("Copperplate Gothic Bold", 30F, FontStyle.Regular, GraphicsUnit.Point, 0);
            RegNextLabel2.ForeColor = Color.Gold;
            RegNextLabel2.Location = new Point(1325, 863);
            RegNextLabel2.Margin = new Padding(2, 0, 2, 0);
            RegNextLabel2.Name = "RegNextLabel2";
            RegNextLabel2.Size = new Size(376, 60);
            RegNextLabel2.TabIndex = 5;
            RegNextLabel2.Text = "Next: -";
            RegNextLabel2.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // ServiceWindow
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = Properties.Resources._14;
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(1904, 1041);
            Controls.Add(RegNextLabel2);
            Controls.Add(RegNowLabel2);
            Controls.Add(RegNextLabel);
            Controls.Add(RegNowLabel);
            Margin = new Padding(2);
            Name = "ServiceWindow";
            Text = "ServiceWindow";
            ResumeLayout(false);

        }

        #endregion

        // Designer-owned labels for registrar window #2
        private System.Windows.Forms.Label RegNowLabel2;
        private System.Windows.Forms.Label RegNextLabel2;
        private System.Windows.Forms.Label RegNowLabel;
        private System.Windows.Forms.Label RegNextLabel;
    }
}