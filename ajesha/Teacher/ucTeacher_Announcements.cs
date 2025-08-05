using MySql.Data.MySqlClient;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace ajesha
{
    public partial class ucTeacher_Announcements : UserControl
    {
        // --- ID del maestro que está usando el panel ---
        private int currentTeacherId;

        // --- Controles de la Interfaz ---
        private TextBox txtAnnouncementTitle;
        private RichTextBox rtbAnnouncementMessage;
        private Button btnSendAnnouncement;

        // --- Paleta de Colores para el Tema Oscuro ---
        private readonly Color colorDarkBg = Color.FromArgb(45, 45, 48);
        private readonly Color colorMidBg = Color.FromArgb(51, 51, 55);
        private readonly Color colorLightBg = Color.FromArgb(63, 63, 70);
        private readonly Color colorFont = Color.White;
        private readonly Color colorBorder = Color.FromArgb(80, 80, 80);

        public ucTeacher_Announcements()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            this.BackColor = colorDarkBg;
            SetupUI();
        }

        // Método para que frmMain pueda pasar el ID del maestro
        public void SetTeacherId(int teacherId)
        {
            this.currentTeacherId = teacherId;
        }
        private void ApplyDarkTheme(Control control)
        {
            if (control is TextBox || control is RichTextBox || control is ComboBox)
            {
                control.BackColor = colorMidBg;
                control.ForeColor = colorFont;
                if (control is TextBox txt) { txt.BorderStyle = BorderStyle.FixedSingle; }
                if (control is RichTextBox rtb) { rtb.BorderStyle = BorderStyle.FixedSingle; }
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
                RowCount = 6
            };

            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Título principal
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Etiqueta "Título"
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // TextBox "Título"
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Etiqueta "Mensaje"
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // RichTextBox "Mensaje"
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Botón de envío

            // Título Principal
            var lblTitle = new Label { Text = "Enviar Anuncio a Estudiantes", Font = new Font("Segoe UI", 16F, FontStyle.Bold), ForeColor = colorFont, AutoSize = true, Margin = new Padding(0, 0, 0, 20) };
            mainLayout.Controls.Add(lblTitle, 0, 0);

            // Campo de Título
            var lblAnnouncementTitle = new Label { Text = "Título:", ForeColor = colorFont, AutoSize = true, Margin = new Padding(0, 10, 0, 5) };
            txtAnnouncementTitle = new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 11F) };
            ApplyDarkTheme(txtAnnouncementTitle);
            mainLayout.Controls.Add(lblAnnouncementTitle, 0, 1);
            mainLayout.Controls.Add(txtAnnouncementTitle, 0, 2);

            // Campo de Mensaje
            var lblMessage = new Label { Text = "Anuncio:", ForeColor = colorFont, AutoSize = true, Margin = new Padding(0, 15, 0, 5) };
            rtbAnnouncementMessage = new RichTextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 11F) };
            ApplyDarkTheme(rtbAnnouncementMessage);
            mainLayout.Controls.Add(lblMessage, 0, 3);
            mainLayout.Controls.Add(rtbAnnouncementMessage, 0, 4);

            // Botón de Envío
            btnSendAnnouncement = new Button { Text = "Enviar Anuncio", Height = 40, Width = 200, Anchor = AnchorStyles.Right, Margin = new Padding(0, 20, 0, 0) };
            ApplyDarkTheme(btnSendAnnouncement);
            btnSendAnnouncement.Click += BtnSendAnnouncement_Click;
            mainLayout.Controls.Add(btnSendAnnouncement, 0, 5);

            this.Controls.Add(mainLayout);
        }
        private void BtnSendAnnouncement_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtAnnouncementTitle.Text) || string.IsNullOrWhiteSpace(rtbAnnouncementMessage.Text))
            {
                MessageBox.Show("El título y el mensaje del anuncio son requeridos.", "Datos Incompletos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (var connection = DBConnection.GetConnection())
                {
                    connection.Open();
                    var cmd = new MySqlCommand("INSERT INTO notifications (title, message, sent_by_user_id, audience) VALUES (@title, @msg, @sender, 'students')", connection);
                    cmd.Parameters.AddWithValue("@title", txtAnnouncementTitle.Text);
                    cmd.Parameters.AddWithValue("@msg", rtbAnnouncementMessage.Text);
                    cmd.Parameters.AddWithValue("@sender", this.currentTeacherId);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Anuncio enviado exitosamente a todos los estudiantes.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Limpiar el formulario
                    txtAnnouncementTitle.Clear();
                    rtbAnnouncementMessage.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al enviar el anuncio: " + ex.Message, "Error de Base de Datos", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}