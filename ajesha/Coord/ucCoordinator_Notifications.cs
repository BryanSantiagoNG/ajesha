using MySql.Data.MySqlClient;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace ajesha
{
    public partial class ucCoordinator_Notifications : UserControl
    {
        // --- ID del coordinador que está usando el panel ---
        private int currentCoordinatorId;

        // --- Controles de la Interfaz ---
        private TextBox txtNotificationTitle;
        private RichTextBox rtbNotificationMessage;
        private ComboBox cmbNotificationAudience;
        private Button btnSendNotification;

        // --- Paleta de Colores para el Tema Oscuro ---
        private readonly Color colorDarkBg = Color.FromArgb(45, 45, 48);
        private readonly Color colorMidBg = Color.FromArgb(51, 51, 55);
        private readonly Color colorLightBg = Color.FromArgb(63, 63, 70);
        private readonly Color colorFont = Color.White;
        private readonly Color colorBorder = Color.FromArgb(80, 80, 80);

        public ucCoordinator_Notifications()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            this.BackColor = colorDarkBg;
            SetupUI();
        }
        public void SetCoordinatorId(int coordinatorId)
        {
            this.currentCoordinatorId = coordinatorId;
        }

        private void ApplyDarkTheme(Control control)
        {
            if (control is TextBox || control is RichTextBox || control is ComboBox)
            {
                control.BackColor = colorMidBg;
                control.ForeColor = colorFont;
                if (control is TextBox txt) { txt.BorderStyle = BorderStyle.FixedSingle; }
                if (control is RichTextBox rtb) { rtb.BorderStyle = BorderStyle.FixedSingle; }
                if (control is ComboBox cmb) { cmb.FlatStyle = FlatStyle.Flat; }
            }
            if (control is Button btn)
            {
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 1;
                btn.FlatAppearance.BorderColor = colorBorder;
                btn.BackColor = colorLightBg;
                btn.ForeColor = colorFont;
            }
        }

        private void SetupUI()
        {
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(30),
                ColumnCount = 1,
                RowCount = 7
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Título principal
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Etiqueta "Título"
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // TextBox "Título"
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Etiqueta "Mensaje"
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // RichTextBox "Mensaje" (ocupa el espacio restante)
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Panel de envío
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F)); // Espacio al final

            // --- Título Principal ---
            var lblTitle = new Label { Text = "Crear Nueva Notificación", Font = new Font("Segoe UI", 16F, FontStyle.Bold), ForeColor = colorFont, AutoSize = true, Margin = new Padding(0, 0, 0, 20) };
            mainLayout.Controls.Add(lblTitle, 0, 0);

            // --- Campo de Título ---
            var lblNotificationTitle = new Label { Text = "Título:", ForeColor = colorFont, AutoSize = true, Margin = new Padding(0, 10, 0, 5) };
            txtNotificationTitle = new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 11F) };
            ApplyDarkTheme(txtNotificationTitle);
            mainLayout.Controls.Add(lblNotificationTitle, 0, 1);
            mainLayout.Controls.Add(txtNotificationTitle, 0, 2);

            // --- Campo de Mensaje ---
            var lblMessage = new Label { Text = "Mensaje:", ForeColor = colorFont, AutoSize = true, Margin = new Padding(0, 15, 0, 5) };
            rtbNotificationMessage = new RichTextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 11F) };
            ApplyDarkTheme(rtbNotificationMessage);
            mainLayout.Controls.Add(lblMessage, 0, 3);
            mainLayout.Controls.Add(rtbNotificationMessage, 0, 4);

            // --- Panel de Envío ---
            var sendPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                Margin = new Padding(0, 20, 0, 0)
            };
            var lblAudience = new Label { Text = "Enviar a:", ForeColor = colorFont, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 6, 10, 0) };
            cmbNotificationAudience = new ComboBox { Width = 150, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10F) };
            ApplyDarkTheme(cmbNotificationAudience);
            cmbNotificationAudience.Items.AddRange(new string[] { "all", "students", "teachers" });
            cmbNotificationAudience.SelectedIndex = 0;

            btnSendNotification = new Button { Text = "Enviar Notificación", Height = 35, Width = 150, Margin = new Padding(20, 0, 0, 0) };
            ApplyDarkTheme(btnSendNotification);
            btnSendNotification.Click += BtnSendNotification_Click;

            sendPanel.Controls.AddRange(new Control[] { lblAudience, cmbNotificationAudience, btnSendNotification });
            mainLayout.Controls.Add(sendPanel, 0, 5);

            this.Controls.Add(mainLayout);
        }
        private void BtnSendNotification_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNotificationTitle.Text) || string.IsNullOrWhiteSpace(rtbNotificationMessage.Text))
            {
                MessageBox.Show("El título y el mensaje de la notificación son requeridos.", "Datos Incompletos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbNotificationAudience.SelectedItem == null)
            {
                MessageBox.Show("Por favor, seleccione una audiencia para la notificación.", "Audiencia Requerida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (var connection = DBConnection.GetConnection())
                {
                    connection.Open();
                    var cmd = new MySqlCommand("INSERT INTO notifications (title, message, sent_by_user_id, audience) VALUES (@title, @msg, @sender, @audience)", connection);
                    cmd.Parameters.AddWithValue("@title", txtNotificationTitle.Text);
                    cmd.Parameters.AddWithValue("@msg", rtbNotificationMessage.Text);
                    cmd.Parameters.AddWithValue("@sender", this.currentCoordinatorId);
                    cmd.Parameters.AddWithValue("@audience", cmbNotificationAudience.SelectedItem.ToString());
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Notificación enviada exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Limpiar el formulario
                    txtNotificationTitle.Clear();
                    rtbNotificationMessage.Clear();
                    cmbNotificationAudience.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al enviar la notificación: " + ex.Message, "Error de Base de Datos", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}