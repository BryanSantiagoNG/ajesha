using MySql.Data.MySqlClient;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ajesha
{
    public partial class ucTeacherGradeSubmission : UserControl
    {
        // --- Evento para la navegación hacia atrás ---
        public event EventHandler BackButtonClicked;

        // --- Campos de la clase ---
        private readonly int submissionId;
        private byte[] fileData;
        private string fileName;

        // --- Controles de la Interfaz ---
        private Label lblStudentName, lblAssignmentTitle, lblFileName;
        private Button btnDownloadFile, btnSaveGrade, btnBack;
        private TextBox txtGrade;
        private RichTextBox rtbComments;

        // --- Paleta de Colores ---
        private readonly Color colorDarkBg = Color.FromArgb(45, 45, 48);
        private readonly Color colorMidBg = Color.FromArgb(51, 51, 55);
        private readonly Color colorLightBg = Color.FromArgb(63, 63, 70);
        private readonly Color colorFont = Color.White;
        private readonly Color colorBorder = Color.FromArgb(80, 80, 80);

        public ucTeacherGradeSubmission()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            this.BackColor = colorDarkBg;
        }
        public void LoadSubmission(int submissionId)
        {
            this.Controls.Clear();
            SetupUI();
            LoadSubmissionDetails(submissionId);
        }
        private void ApplyDarkTheme(Control control)
        {
            if (control is TextBox || control is RichTextBox)
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
            var mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(30), RowCount = 7 };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Botón Volver
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Título
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Archivo entregado
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Calificación
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Comentarios Label
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Comentarios RichTextBox
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Botón Guardar

            btnBack = new Button { Text = "< Volver a Gestión de Cursos", AutoSize = true, Font = new Font("Segoe UI", 9F) };
            ApplyDarkTheme(btnBack);
            btnBack.Click += (s, e) => BackButtonClicked?.Invoke(this, EventArgs.Empty);

            lblStudentName = new Label { Dock = DockStyle.Fill, ForeColor = colorFont, Font = new Font("Segoe UI", 16F, FontStyle.Bold), Margin = new Padding(0, 10, 0, 0) };
            lblAssignmentTitle = new Label { Dock = DockStyle.Fill, ForeColor = Color.Gainsboro, Font = new Font("Segoe UI", 10F, FontStyle.Italic), Margin = new Padding(0, 0, 0, 20) };

            var filePanel = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, Margin = new Padding(0, 0, 0, 20) };
            lblFileName = new Label { ForeColor = colorFont, Font = new Font("Segoe UI", 10F), AutoSize = true, Text = "Archivo adjunto: " };
            btnDownloadFile = new Button { Text = "Descargar Archivo", AutoSize = true, Enabled = false };
            ApplyDarkTheme(btnDownloadFile);
            btnDownloadFile.Click += BtnDownloadFile_Click;
            filePanel.Controls.AddRange(new Control[] { lblFileName, btnDownloadFile });

            var gradePanel = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
            gradePanel.Controls.Add(new Label { Text = "Calificación (0-100):", ForeColor = colorFont, Font = new Font("Segoe UI", 10F), AutoSize = true, Margin = new Padding(0, 5, 10, 0) });
            txtGrade = new TextBox { Width = 100 }; ApplyDarkTheme(txtGrade);
            gradePanel.Controls.Add(txtGrade);

            rtbComments = new RichTextBox { Dock = DockStyle.Fill }; ApplyDarkTheme(rtbComments);

            btnSaveGrade = new Button { Text = "Guardar Calificación y Comentarios", Dock = DockStyle.Fill, Height = 40, Margin = new Padding(0, 20, 0, 0) };
            ApplyDarkTheme(btnSaveGrade);
            btnSaveGrade.Click += BtnSaveGrade_Click;

            mainLayout.Controls.Add(btnBack, 0, 0);
            mainLayout.Controls.Add(lblStudentName, 0, 1);
            mainLayout.Controls.Add(lblAssignmentTitle, 0, 2);
            mainLayout.Controls.Add(filePanel, 0, 3);
            mainLayout.Controls.Add(gradePanel, 0, 4);
            mainLayout.Controls.Add(new Label { Text = "Comentarios / Retroalimentación:", ForeColor = colorFont, Font = new Font("Segoe UI", 10F), AutoSize = true, Margin = new Padding(0, 15, 0, 5) }, 0, 5);
            mainLayout.Controls.Add(rtbComments, 0, 6);
            mainLayout.Controls.Add(btnSaveGrade, 0, 7);

            this.Controls.Add(mainLayout);
        }
        private void LoadSubmissionDetails(int submissionId)
        {
            try
            {
                using (var conn = DBConnection.GetConnection())
                {
                    conn.Open();
                    var query = "SELECT s.file_name, s.file_data, s.grade, s.teacher_comments, u.full_name, a.title " +
                                "FROM assignment_submissions s " +
                                "JOIN users u ON s.student_id = u.user_id " +
                                "JOIN assignments a ON s.assignment_id = a.assignment_id " +
                                "WHERE s.submission_id = @id";
                    var cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", submissionId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            lblStudentName.Text = reader["full_name"].ToString();
                            lblAssignmentTitle.Text = $"Tarea: {reader["title"]}";
                            txtGrade.Text = reader["grade"]?.ToString();
                            rtbComments.Text = reader["teacher_comments"]?.ToString();

                            if (reader["file_data"] != DBNull.Value)
                            {
                                fileName = reader["file_name"].ToString();
                                fileData = (byte[])reader["file_data"];
                                lblFileName.Text = $"Archivo adjunto: {fileName}";
                                btnDownloadFile.Enabled = true;
                            }
                            else
                            {
                                lblFileName.Text = "No se adjuntó ningún archivo.";
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error al cargar los detalles de la entrega: " + ex.Message); }
        }

        private void BtnDownloadFile_Click(object sender, EventArgs e)
        {
            if (fileData == null || string.IsNullOrEmpty(fileName))
            {
                MessageBox.Show("No hay archivo para descargar.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (var sfd = new SaveFileDialog())
            {
                sfd.FileName = fileName;
                sfd.Filter = "Todos los archivos (*.*)|*.*";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        File.WriteAllBytes(sfd.FileName, fileData);
                        MessageBox.Show("Archivo descargado exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error al guardar el archivo: " + ex.Message, "Error de Guardado", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnSaveGrade_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtGrade.Text))
            {
                MessageBox.Show("Por favor, ingrese una calificación.", "Dato Requerido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                decimal grade = Convert.ToDecimal(txtGrade.Text);

                using (var conn = DBConnection.GetConnection())
                {
                    conn.Open();
                    var cmd = new MySqlCommand("UPDATE assignment_submissions SET grade = @grade, teacher_comments = @comments, graded_at = NOW() WHERE submission_id = @id", conn);
                    cmd.Parameters.AddWithValue("@grade", grade);
                    cmd.Parameters.AddWithValue("@comments", rtbComments.Text);
                    cmd.Parameters.AddWithValue("@id", submissionId);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Calificación guardada exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Disparar el evento para volver atrás
                    BackButtonClicked?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("Por favor, ingrese un valor numérico válido para la calificación.", "Formato Inválido", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar la calificación: " + ex.Message, "Error de Base de Datos", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}