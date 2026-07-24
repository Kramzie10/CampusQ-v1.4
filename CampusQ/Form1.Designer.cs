namespace CampusQ
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btn_cashier = new Button();
            btn_registrar = new Button();
            btn_admission = new Button();
            SuspendLayout();
            // 
            // btn_cashier
            // 
            btn_cashier.BackColor = Color.Transparent;
            btn_cashier.BackgroundImageLayout = ImageLayout.Stretch;
            btn_cashier.FlatAppearance.BorderSize = 0;
            btn_cashier.FlatStyle = FlatStyle.Popup;
            btn_cashier.ForeColor = Color.Transparent;
            btn_cashier.Location = new Point(97, 626);
            btn_cashier.Name = "btn_cashier";
            btn_cashier.Size = new Size(352, 76);
            btn_cashier.TabIndex = 1;
            btn_cashier.UseVisualStyleBackColor = false;
            btn_cashier.Click += btn_cashier_Click;
            // 
            // btn_registrar
            // 
            btn_registrar.BackColor = Color.Transparent;
            btn_registrar.BackgroundImageLayout = ImageLayout.Zoom;
            btn_registrar.FlatStyle = FlatStyle.Popup;
            btn_registrar.ForeColor = Color.Transparent;
            btn_registrar.Location = new Point(97, 507);
            btn_registrar.Name = "btn_registrar";
            btn_registrar.Size = new Size(381, 94);
            btn_registrar.TabIndex = 3;
            btn_registrar.UseVisualStyleBackColor = false;
            btn_registrar.Click += btn_registrar_Click_1;
            // 
            // btn_admission
            // 
            btn_admission.BackColor = Color.Transparent;
            btn_admission.BackgroundImageLayout = ImageLayout.Stretch;
            btn_admission.FlatAppearance.BorderSize = 0;
            btn_admission.FlatStyle = FlatStyle.Popup;
            btn_admission.ForeColor = Color.Transparent;
            btn_admission.Location = new Point(65, 735);
            btn_admission.Name = "btn_admission";
            btn_admission.Size = new Size(413, 90);
            btn_admission.TabIndex = 4;
            btn_admission.TabStop = false;
            btn_admission.UseVisualStyleBackColor = false;
            btn_admission.Click += btn_admission_Click_1;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = Properties.Resources.Kiosk_Idle;
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(1920, 1080);
            Controls.Add(btn_admission);
            Controls.Add(btn_registrar);
            Controls.Add(btn_cashier);
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.None;
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Form1";
            Load += Form1_Load;
            ResumeLayout(false);
        }

        #endregion
        private Button btn_cashier;
        private Button btn_registrar;
        private Button btn_adm;
        private Button btn_admission;
    }
}
