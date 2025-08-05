using MySql.Data.MySqlClient;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ajesha
{
    public partial class ucStudentSubmitAssignment : UserControl
    {
        // --- Evento para la navegación hacia atrás ---
        public event EventHandler BackButtonClicked;

        // --- Campos de la clase ---
        private int studentId;
        private int assignmentId;
        private byte[] fileData;
        private string fileName;

        // --- Controles de la Interfaz ---
        private Label lblTitle, lblDueDate, lblDescription, lblStatusTitle, lblStatus, lblGradeTitle, lblGrade, lblTeacherCommentsTitle;
        private RichTextBox rtbTeacherComments;
        private TextBox txtSelectedFile;
        private Button btnChooseFile, btnSubmit, btnBack;
        private Panel submissionPanel, gradePanel;

        // --- Paleta de Colores ---
        private readonly Color colorDarkBg = Color.FromArgb(45, 45, 48);
        private readonly Color colorMidBg = Color.FromArgb(51, 51, 55);
        private readonly Color colorLightBg = Color.FromArgb(63, 63, 70);
        private readonly Color colorFont = Color.White;
        private readonly Color colorGrayFont = Color.Gainsboro;
        private readonly Color colorBorder = Color.FromArgb(80, 80, 80);

        public ucStudentSubmitAssignment()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            this.BackColor = colorDarkBg;
        }

        // Método público para cargar la información de la tarea
        public void LoadAssignment(int studentId, int assignmentId)
        {
            this.studentId = studentId;
            this.assignmentId = assignmentId;
            // Limpiar datos anteriores antes de cargar nuevos
            this.Controls.Clear();
            SetupUI();
            LoadAssignmentDetails();
        }

        #region Styling and UI Setup
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
            var mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(30), RowCount = 8 };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Botón Volver
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Título
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Fecha Entrega
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 40F)); // Descripción
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Panel de Entrega
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Separador
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Panel de Calificación
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 60F)); // Comentarios

            btnBack = new Button { Text = "< Volver a Classroom", AutoSize = true, Font = new Font("Segoe UI", 9F) };
            ApplyDarkTheme(btnBack);
            btnBack.Click += (s, e) => BackButtonClicked?.Invoke(this, EventArgs.Empty);

            lblTitle = new Label { Dock = DockStyle.Fill, ForeColor = colorFont, Font = new Font("Segoe UI", 16F, FontStyle.Bold), Margin = new Padding(0, 10, 0, 5) };
            lblDueDate = new Label { Dock = DockStyle.Fill, ForeColor = colorGrayFont, Font = new Font("Segoe UI", 9F, FontStyle.Italic), Margin = new Padding(0, 0, 0, 15) };
            lblDescription = new Label { Dock = DockStyle.Fill, ForeColor = colorGrayFont, Font = new Font("Segoe UI", 10F), AutoSize = true };

            submissionPanel = new Panel { Dock = DockStyle.Fill, AutoSize = true };
            var submissionLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, AutoSize = true };
            submissionLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            submissionLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            txtSelectedFile = new TextBox { Dock = DockStyle.Fill, ReadOnly = true }; ApplyDarkTheme(txtSelectedFile);
            btnChooseFile = new Button { Text = "Elegir Archivo...", AutoSize = true, Height = 30 }; ApplyDarkTheme(btnChooseFile);
            btnSubmit = new Button { Text = "Entregar Tarea", Dock = DockStyle.Fill, Height = 35, Margin = new Padding(0, 10, 0, 0) }; ApplyDarkTheme(btnSubmit);
            submissionLayout.Controls.Add(txtSelectedFile, 0, 0);
            submissionLayout.Controls.Add(btnChooseFile, 1, 0);
            submissionLayout.SetColumnSpan(btnSubmit, 2);
            submissionLayout.Controls.Add(btnSubmit, 0, 1);
            submissionPanel.Controls.Add(submissionLayout);

            var separator = new Label { Dock = DockStyle.Fill, Height = 2, BorderStyle = BorderStyle.Fixed3D, Margin = new Padding(0, 20, 0, 20) };

            gradePanel = new Panel { Dock = DockStyle.Fill, AutoSize = true, Visible = false };
            var gradeLayout = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
            lblStatusTitle = new Label { Text = "Estado:", ForeColor = colorFont, Font = new Font("Segoe UI", 10F, FontStyle.Bold), AutoSize = true };
            lblStatus = new Label { Text = "N/A", ForeColor = colorGrayFont, Font = new Font("Segoe UI", 10F), AutoSize = true, Margin = new Padding(5, 0, 20, 0) };
            lblGradeTitle = new Label { Text = "Calificación:", ForeColor = colorFont, Font = new Font("Segoe UI", 10F, FontStyle.Bold), AutoSize = true };
            lblGrade = new Label { Text = "N/A", ForeColor = colorGrayFont, Font = new Font("Segoe UI", 10F), AutoSize = true, Margin = new Padding(5, 0, 0, 0) };
            gradeLayout.Controls.AddRange(new Control[] { lblStatusTitle, lblStatus, lblGradeTitle, lblGrade });
            gradePanel.Controls.Add(gradeLayout);

            lblTeacherCommentsTitle = new Label { Dock = DockStyle.Fill, Text = "Comentarios del Maestro:", ForeColor = colorFont, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Visible = false, Margin = new Padding(0, 5, 0, 5) };
            rtbTeacherComments = new RichTextBox { Dock = DockStyle.Fill, ReadOnly = true, Visible = false }; ApplyDarkTheme(rtbTeacherComments);

            mainLayout.Controls.Add(btnBack, 0, 0);
            mainLayout.Controls.Add(lblTitle, 0, 1);
            mainLayout.Controls.Add(lblDueDate, 0, 2);
            mainLayout.Controls.Add(lblDescription, 0, 3);
            mainLayout.Controls.Add(submissionPanel, 0, 4);
            mainLayout.Controls.Add(separator, 0, 5);
            mainLayout.Controls.Add(gradePanel, 0, 6);
            mainLayout.Controls.Add(new TableLayoutPanel { Controls = { lblTeacherCommentsTitle, rtbTeacherComments }, Dock = DockStyle.Fill, RowCount = 2, RowStyles = { new RowStyle(SizeType.AutoSize), new RowStyle(SizeType.Percent, 100F) } }, 0, 7);

            btnChooseFile.Click += BtnChooseFile_Click;
            btnSubmit.Click += BtnSubmit_Click;

            this.Controls.Add(mainLayout);
        }
        #endregion

        #region Data Logic
        private void LoadAssignmentDetails()
        {
            try
            {
                using (var conn = DBConnection.GetConnection())
                {
                    conn.Open();
                    var cmdAssignment = new MySqlCommand("SELECT title, description, due_date FROM assignments WHERE assignment_id = @id", conn);
                    cmdAssignment.Parameters.AddWithValue("@id", this.assignmentId);
                    using (var reader = cmdAssignment.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            lblTitle.Text = reader["title"].ToString();
                            lblDescription.Text = reader["description"].ToString();
                            lblDueDate.Text = $"Fecha de entrega: {Convert.ToDateTime(reader["due_date"]):g}";
                        }
                    }

                    var cmdSubmission = new MySqlCommand("SELECT status, grade, teacher_comments FROM assignment_submissions WHERE assignment_id = @assignmentId AND student_id = @studentId", conn);
                    cmdSubmission.Parameters.AddWithValue("@assignmentId", this.assignmentId);
                    cmdSubmission.Parameters.AddWithValue("@studentId", this.studentId);
                    using (var reader = cmdSubmission.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            submissionPanel.Visible = false;
                            gradePanel.Visible = true;
                            lblTeacherCommentsTitle.Visible = true;
                            rtbTeacherComments.Visible = true;
                            lblStatus.Text = reader["status"].ToString();
                            lblGrade.Text = reader["grade"] == DBNull.Value ? "Sin calificar" : reader["grade"].ToString();
                            rtbTeacherComments.Text = reader["teacher_comments"]?.ToString() ?? "Sin comentarios.";
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error al cargar los detalles de la tarea: " + ex.Message); }
        }

        private void BtnChooseFile_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Title = "Seleccione el archivo de su tarea";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtSelectedFile.Text = ofd.FileName;
                    fileName = ofd.SafeFileName;
                    fileData = File.ReadAllBytes(ofd.FileName);
                }
            }
        }

        private void BtnSubmit_Click(object sender, EventArgs e)
        {
            if (fileData == null)
            {
                MessageBox.Show("Por favor, seleccione un archivo para entregar.", "Archivo Requerido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (var conn = DBConnection.GetConnection())
                {
                    conn.Open();
                    var cmd = new MySqlCommand("INSERT INTO assignment_submissions (assignment_id, student_id, status, file_name, file_data) VALUES (@assignmentId, @studentId, 'entregada', @fileName, @fileData) ON DUPLICATE KEY UPDATE status='entregada', file_name=@fileName, file_data=@fileData, submission_date=NOW()", conn);
                    cmd.Parameters.AddWithValue("@assignmentId", this.assignmentId);
                    cmd.Parameters.AddWithValue("@studentId", this.studentId);
                    cmd.Parameters.AddWithValue("@fileName", this.fileName);
                    cmd.Parameters.Add("@fileData", MySqlDbType.LongBlob).Value = this.fileData;
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Tarea entregada exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Disparar el evento para volver atrás
                    BackButtonClicked?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (Exception ex) { MessageBox.Show("Error al entregar la tarea: " + ex.Message); }
        }
        #endregion
    }
}