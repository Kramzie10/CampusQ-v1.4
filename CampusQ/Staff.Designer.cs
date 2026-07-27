namespace CampusQ
{
    partial class Staff
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Staff));
            comboBoxService = new ComboBox();
            labelService = new Label();
            buttonRefresh = new Button();
            buttonServeNext = new Button();
            labelTotal = new Label();
            dataGridViewQueue = new DataGridView();
            trayIcon = new NotifyIcon(components);
            btn_service = new Button();
            ((System.ComponentModel.ISupportInitialize)dataGridViewQueue).BeginInit();
            SuspendLayout();
            // 
            // comboBoxService
            // 
            comboBoxService.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxService.FormattingEnabled = true;
            comboBoxService.Location = new Point(60, 12);
            comboBoxService.Name = "comboBoxService";
            comboBoxService.Size = new Size(121, 23);
            comboBoxService.TabIndex = 0;
            // 
            // labelService
            // 
            labelService.AutoSize = true;
            labelService.BackColor = Color.Transparent;
            labelService.Location = new Point(12, 15);
            labelService.Name = "labelService";
            labelService.Size = new Size(47, 15);
            labelService.TabIndex = 1;
            labelService.Text = "Service:";
            // 
            // buttonRefresh
            // 
            buttonRefresh.Location = new Point(200, 11);
            buttonRefresh.Name = "buttonRefresh";
            buttonRefresh.Size = new Size(75, 25);
            buttonRefresh.TabIndex = 2;
            buttonRefresh.Text = "Refresh";
            buttonRefresh.UseVisualStyleBackColor = true;
            // 
            // buttonServeNext
            // 
            buttonServeNext.Location = new Point(285, 11);
            buttonServeNext.Name = "buttonServeNext";
            buttonServeNext.Size = new Size(90, 25);
            buttonServeNext.TabIndex = 3;
            buttonServeNext.Text = "Serve Next";
            buttonServeNext.UseVisualStyleBackColor = true;
            // 
            // labelTotal
            // 
            labelTotal.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            labelTotal.AutoSize = true;
            labelTotal.BackColor = Color.Transparent;
            labelTotal.Location = new Point(700, 15);
            labelTotal.Name = "labelTotal";
            labelTotal.Size = new Size(45, 15);
            labelTotal.TabIndex = 4;
            labelTotal.Text = "Total: 1";
            // 
            // dataGridViewQueue
            // 
            dataGridViewQueue.AllowUserToAddRows = false;
            dataGridViewQueue.AllowUserToDeleteRows = false;
            dataGridViewQueue.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridViewQueue.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewQueue.Location = new Point(12, 45);
            dataGridViewQueue.Name = "dataGridViewQueue";
            dataGridViewQueue.ReadOnly = true;
            dataGridViewQueue.RowHeadersWidth = 62;
            dataGridViewQueue.Size = new Size(760, 350);
            dataGridViewQueue.TabIndex = 5;
            // 
            // trayIcon
            // 
            try
            {
                var iconObj = resources.GetObject("trayIcon.Icon");
                trayIcon.Icon = iconObj as Icon ?? System.Drawing.SystemIcons.Application;
            }
            catch
            {
                trayIcon.Icon = System.Drawing.SystemIcons.Application;
            }

            trayIcon.Text = "CampusQ Staff Dashboard";
            trayIcon.MouseDoubleClick += trayIcon_MouseDoubleClick;
            // 
            // btn_service
            // 
            btn_service.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btn_service.Location = new Point(603, 11);
            btn_service.Name = "btn_service";
            btn_service.Size = new Size(75, 23);
            btn_service.TabIndex = 6;
            btn_service.Text = "Service R";
            btn_service.UseVisualStyleBackColor = true;
            btn_service.Click += btn_service_Click;
            // 
            // Staff
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = Properties.Resources.Purple_and_White_Minimalist_Modern_Computer_Repair_Logo__23_386_x_16_535_in___8_5_x_22_cm___500_x_500_px___11_7_x_8_27_in_;
            ClientSize = new Size(784, 411);
            Controls.Add(btn_service);
            Controls.Add(dataGridViewQueue);
            Controls.Add(labelTotal);
            Controls.Add(buttonServeNext);
            Controls.Add(buttonRefresh);
            Controls.Add(labelService);
            Controls.Add(comboBoxService);
            Name = "Staff";
            Text = "Staff Dashboard";
            Load += Staff_Load;
            ((System.ComponentModel.ISupportInitialize)dataGridViewQueue).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ComboBox comboBoxService;
        private System.Windows.Forms.Label labelService;
        private System.Windows.Forms.Button buttonRefresh;
        private System.Windows.Forms.Button buttonServeNext;
        private System.Windows.Forms.Label labelTotal;
        private DataGridView dataGridViewQueue;
        private NotifyIcon trayIcon;
        private Button btn_service;
    }
}