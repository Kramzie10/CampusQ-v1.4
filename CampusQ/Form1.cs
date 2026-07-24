using Microsoft.Win32;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Xml.Linq;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Linq;
using System.Windows.Forms;
using CampusQ.MVP.Views;
using CampusQ.MVP.Presenters;
using System.Drawing;
using System;
using CampusQ.MVP.Data;
using CampusQ.MVP.Models;
using System.Drawing.Printing;
using System.Collections.Generic;
using QRCoder;

namespace CampusQ
{
    public partial class Form1 : Form, IMainView
    {
        private Staff staff;
        private readonly MainPresenter _presenter;

        public Form1()
        {
            InitializeComponent();
            _presenter = new MainPresenter(this);
        }
        private void CreateDepartmentPanel(string backgroundImage, string[] purposes)
        {
            Panel panel = new Panel()
            {
                Size = new Size(1920, 1080),
                Location = new Point(0, 0),
                BackgroundImageLayout = ImageLayout.Stretch,
            };

            if (backgroundImage == "cash")
            {
                panel.BackgroundImage = Properties.Resources.Kiosk_C;
                panel.Tag = "Cashier";
            }
            else if (backgroundImage == "reg")
            {
                panel.BackgroundImage = Properties.Resources.Kiosk_R;
                panel.Tag = "Registrar";
            }
            else if (backgroundImage == "adm")
            {
                panel.BackgroundImage = Properties.Resources.Kiosk_A;
                panel.Tag = "Admission";
            }

            var controlFont = new Font("Segoe UI", 16F, FontStyle.Regular, GraphicsUnit.Point);

            // Create purpose buttons dynamically
            int startY = 300;
            int buttonHeight = 120;
            int buttonWidth = 500;
            int buttonSpacing = 150;
            int startX = 750;

            for (int i = 0; i < purposes.Length; i++)
            {
                Button purposeBtn = new()
                {
                    Size = new(buttonWidth, buttonHeight),
                    Location = new(startX, startY + (i * buttonSpacing)),
                    FlatStyle = FlatStyle.Popup,
                    BackColor = Color.FromArgb(41, 128, 185),
                    ForeColor = Color.White,
                    Font = controlFont,
                    Text = purposes[i],
                    Name = $"btnPurpose_{i}",
                    Tag = purposes[i]
                };

                purposeBtn.Click += (sender, e) => PurposeButton_Click(sender, e, panel);
                panel.Controls.Add(purposeBtn);
            }

            foreach (Control c in this.Controls.OfType<Panel>()) c.Visible = false;
            this.Controls.Add(panel);
        }

        private void btn_cashier_Click(object sender, EventArgs e)
        {
            CreateDepartmentPanel("cash", new[] { "Tuition Fee", "Miscellaneous Fee", "Other Payments" });
        }

        private void btn_registrar_Click_1(object sender, EventArgs e)
        {
            CreateDepartmentPanel("reg", new[] { "Enrollment", "Credentials", "Other Inquiries" });
        }

        private void btn_admission_Click_1(object sender, EventArgs e)
        {
            CreateDepartmentPanel("adm", new[] { "Application Status", "Document Verification", "General Inquiry" });
        }

        private void PurposeButton_Click(object sender, EventArgs e, Panel activePanel)
        {
            if (sender is not Button button)
                return;

            string purpose = button.Tag?.ToString() ?? "";
            string service = activePanel.Tag?.ToString() ?? "Other";

            if (string.IsNullOrWhiteSpace(purpose) || string.IsNullOrWhiteSpace(service))
            {
                MessageBox.Show("Invalid selection. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Add to queue
            AddToQueue(purpose, service);

            try
            {
                PrintLastInsertedTicket();
                MessageBox.Show("Thank you for using CampusQ!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Printing failed: {ex.Message}", "Print Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void PrintLastInsertedTicket()
        {
            DbConfig.EnsureDatabaseAndTables();
            var repo = new QueueRepository(DbConfig.ConnectionString);
            var all = repo.GetAll() ?? new List<QueueEntry>();
            if (!all.Any()) return;

            var last = all.OrderByDescending(x => x.TicketNumber).First();
            string assignedWindow = DetermineAssignedWindow(last, all);

            // PRINT LAYOUT - Simplified format without name/year
            var lines = new List<string>
            {
                "CampusQ - Queue Ticket",
                "======================",
                $"Ticket #: {last.TicketNumber}",
                $"Office: {last.Service} {assignedWindow}",
                $"Purpose: {last.Purpose}",
                $"Time: {last.TimeAdded:g}",
                "",
                "Please wait for your ticket to be called.",
            };

            // Generate QR code with ticket info URL
            string ticketUrl = GenerateTicketInfoUrl(last.TicketNumber);
            Bitmap? qrCodeBitmap = GenerateQRCode(ticketUrl);

            PrintDocument pd = new PrintDocument();
            pd.DocumentName = $"Ticket_{last.TicketNumber}";

            PaperSize paperSize = new PaperSize("58mm Thermal", 219, 350);
            PaperSource paperSource = new PaperSource { RawKind = (int)PaperSourceKind.Custom };

            pd.DefaultPageSettings.PaperSize = paperSize;
            pd.DefaultPageSettings.PaperSource = paperSource;
            pd.DefaultPageSettings.Margins = new Margins(5, 5, 5, 5);

            pd.PrintPage += (s, e) =>
            {
                var g = e.Graphics;
                var fontTitle = new Font("Arial", 12, FontStyle.Bold);
                var font = new Font("Arial", 9);
                var fontSmall = new Font("Arial", 7);

                float x = 10;
                float y = 10;
                float maxWidth = 199; // 219 - margins

                // Center align text for receipt style
                var centerFormat = new StringFormat { Alignment = StringAlignment.Center };

                // Draw ticket title and info
                g.DrawString(lines[0], fontTitle, Brushes.Black, x + maxWidth / 2, y, centerFormat);
                y += 25;

                g.DrawString(lines[1], font, Brushes.Black, x + maxWidth / 2, y, centerFormat);
                y += 18;

                foreach (var ln in lines.Skip(2))
                {
                    if (string.IsNullOrWhiteSpace(ln))
                    {
                        y += 8;
                    }
                    else
                    {
                        g.DrawString(ln, font, Brushes.Black, x, y);
                        y += 18;
                    }
                }

                // Draw QR code at the bottom if generated successfully
                if (qrCodeBitmap != null)
                {
                    y += 10;
                    int qrSize = 80; // Smaller for 58mm width
                    int qrX = (int)(x + (maxWidth - qrSize) / 2);
                    int qrY = (int)y;

                    g.DrawImage(qrCodeBitmap, qrX, qrY, qrSize, qrSize);

                    // Add label below QR code
                    y += qrSize + 5;
                    g.DrawString("Scan for info", fontSmall, Brushes.Black, x + maxWidth / 2, y, centerFormat);
                }
            };

            using (var dlg = new PrintDialog())
            {
                dlg.Document = pd;
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    pd.Print();
                }
            }

            // Clean up QR code bitmap
            qrCodeBitmap?.Dispose();
        }

        private static string DetermineAssignedWindow(QueueEntry entry, List<QueueEntry> all)
        {
            if (entry == null) return string.Empty;
            if (string.Equals(entry.Service, "Cashier", StringComparison.OrdinalIgnoreCase))
            {
                var stn = entry.ServiceTicketNumber > 0 ? entry.ServiceTicketNumber : entry.TicketNumber;
                var idx = ((stn - 1) % 4) + 1;
                return $"- Window {idx}";
            }

            if (string.Equals(entry.Service, "Registrar", StringComparison.OrdinalIgnoreCase))
            {
                var reg = all.Where(q => string.Equals(q.Service, "Registrar", StringComparison.OrdinalIgnoreCase)).OrderBy(q => q.TicketNumber).ToList();
                if (!reg.Any()) return string.Empty;
                int baseParity = reg[0].TicketNumber % 2;

                bool isRC = IsRCRequest(entry);

                if ((entry.TicketNumber % 2) == baseParity && !isRC)
                    return "- W1";
                return "- W2";
            }

            return string.Empty;
        }

        private static bool IsRCRequest(QueueEntry? entry)
        {
            if (entry == null) return false;
            var label = entry.TicketLabel; if (!string.IsNullOrWhiteSpace(label) && label.StartsWith("RC", StringComparison.OrdinalIgnoreCase)) return true;
            var purpose = entry.Purpose ?? string.Empty;
            if (purpose.IndexOf("credential", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            var p = purpose.ToLowerInvariant();
            var separators = new[] { ' ', '\t', '/', '\\', ',', ';', '-', '_', '.', '(', ')', '[', ']' };
            var tokens = p.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Any(t => t == "rc")) return true;
            return false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }


        public void ShowMessage(string text, string caption, MessageBoxIcon icon)
        {
            MessageBox.Show(text, caption, MessageBoxButtons.OK, icon);
        }

        public void AddToQueue(string purpose, string service)
        {
            if (staff == null || staff.IsDisposed)
            {
                staff = new Staff();
            }
            staff.AddToQueue(purpose, service);

        }

        private string GenerateTicketInfoUrl(int ticketNumber)
        {
            var baseUrl = WebAppConfig.BaseUrl.TrimEnd('/');
            return $"{baseUrl}/Ticket/{ticketNumber}";
        }

        private Bitmap? GenerateQRCode(string text, int size = 200)
        {
            try
            {
                using (var qrGenerator = new QRCoder.QRCodeGenerator())
                {
                    var qrCodeData = qrGenerator.CreateQrCode(text, QRCoder.QRCodeGenerator.ECCLevel.Q);
                    using (var qrCode = new QRCoder.QRCode(qrCodeData))
                    {
                        return qrCode.GetGraphic(10);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating QR code: {ex.Message}", "QR Code Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }
        }

    }
}
