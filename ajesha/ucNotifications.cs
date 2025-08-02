using MySql.Data.MySqlClient;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace ajesha 
{
    public partial class ucNotifications : UserControl
    {
        private FlowLayoutPanel flowLayoutPanel;

        private readonly Color colorDarkBg = Color.FromArgb(45, 45, 48);
        private readonly Color colorMidBg = Color.FromArgb(63, 63, 70);
        private readonly Color colorFont = Color.White;
        private readonly Color colorGrayFont = Color.Gainsboro;

        public ucNotifications()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            this.BackColor = colorDarkBg;
            SetupUI();
            this.VisibleChanged += (sender, e) => { if (this.Visible) LoadNotifications(); };
        }

        private void SetupUI()
        {
            flowLayoutPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                AutoScroll = true, 
                WrapContents = false, 
                Padding = new Padding(30)
            };
            this.Controls.Add(flowLayoutPanel);
        }
        private Panel CreateNotificationPanel(string title, string message, string senderName, DateTime sentAt)
        {
            var panel = new Panel
            {
                BackColor = colorMidBg,
                Width = flowLayoutPanel.ClientSize.Width - 40,
                Margin = new Padding(0, 0, 0, 15), 
                Padding = new Padding(15)
            };

            var lblTitle = new Label
            {
                Text = title,
                ForeColor = colorFont,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                Dock = DockStyle.Top
            };

            var lblMessage = new Label
            {
                Text = message,
                ForeColor = colorGrayFont,
                Font = new Font("Segoe UI", 10F),
                Dock = DockStyle.Fill,
                AutoSize = true
            };

            var lblMeta = new Label
            {
                Text = $"Enviado por: {senderName} - {sentAt:g}",
                ForeColor = Color.DarkGray,
                Font = new Font("Segoe UI", 8F, FontStyle.Italic),
                Dock = DockStyle.Bottom,
                TextAlign = ContentAlignment.MiddleRight
            };

            panel.Controls.Add(lblMessage);
            panel.Controls.Add(lblTitle);
            panel.Controls.Add(lblMeta);
            panel.Height = lblTitle.Height + lblMessage.Height + lblMeta.Height + panel.Padding.Vertical + 10;
            return panel;
        }

        public void LoadNotifications()
        {
            flowLayoutPanel.Controls.Clear();

            try
            {
                using (var connection = DBConnection.GetConnection())
                {
                    var query = "SELECT n.title, n.message, n.sent_at, u.full_name AS sender_name " +
                                "FROM notifications n " +
                                "JOIN users u ON n.sent_by_user_id = u.user_id " +
                                "ORDER BY n.sent_at DESC";

                    var cmd = new MySqlCommand(query, connection);
                    connection.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                        {
                            var noDataLabel = new Label { Text = "No hay notificaciones para mostrar.", ForeColor = colorGrayFont, Font = new Font("Segoe UI", 12F), AutoSize = true };
                            flowLayoutPanel.Controls.Add(noDataLabel);
                        }
                        else
                        {
                            while (reader.Read())
                            {
                                string title = reader["title"].ToString();
                                string message = reader["message"].ToString();
                                string sender = reader["sender_name"].ToString();
                                DateTime sentAt = Convert.ToDateTime(reader["sent_at"]);

                                var notificationPanel = CreateNotificationPanel(title, message, sender, sentAt);
                                flowLayoutPanel.Controls.Add(notificationPanel);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar las notificaciones: " + ex.Message, "Error de Base de Datos", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}