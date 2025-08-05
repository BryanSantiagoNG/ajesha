using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace ajesha
{
    public partial class ucStudentAttendance : UserControl
    {
        private int currentStudentId;
        private DataGridView dgvAttendance;
        private RichTextBox rtbJustification;
        private Button btnSubmitJustification;

        // --- Paleta de Colores ---
        private readonly Color colorDarkBg = Color.FromArgb(45, 45, 48);
        private readonly Color colorMidBg = Color.FromArgb(51, 51, 55);
        private readonly Color colorLightBg = Color.FromArgb(63, 63, 70);
        private readonly Color colorFont = Color.White;
        private readonly Color colorBorder = Color.FromArgb(80, 80, 80);

        public ucStudentAttendance()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            this.BackColor = colorDarkBg;
            SetupUI();
            this.VisibleChanged += (s, e) => { if (this.Visible && currentStudentId > 0) LoadMyAttendance(); };
        }

        public void SetStudentId(int studentId) { this.currentStudentId = studentId; }

        private void ApplyDarkTheme(Control control)
        {
            if (control is RichTextBox || control is ComboBox)
            {
                control.BackColor = colorMidBg;
                control.ForeColor = colorFont;
                if (control is RichTextBox rtb) { rtb.BorderStyle = BorderStyle.FixedSingle; }
            }
            if (control is DataGridView dgv)
            {
                dgv.BackgroundColor = colorMidBg;
                dgv.GridColor = colorDarkBg;
                dgv.BorderStyle = BorderStyle.None;
                dgv.ColumnHeadersDefaultCellStyle.BackColor = colorLightBg;
                dgv.ColumnHeadersDefaultCellStyle.ForeColor = colorFont;
                dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = colorLightBg;
                dgv.EnableHeadersVisualStyles = false;
                dgv.DefaultCellStyle.BackColor = colorMidBg;
                dgv.DefaultCellStyle.ForeColor = colorFont;
                dgv.DefaultCellStyle.SelectionBackColor = Color.SteelBlue;
                dgv.DefaultCellStyle.SelectionForeColor = Color.White;
                dgv.RowHeadersVisible = false;
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
            var mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(20), RowCount = 3 };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            mainLayout.Controls.Add(new Label 
            { 
                Text = "Mi Historial de Asistencia", 
                Font = new Font("Segoe UI", 16F, FontStyle.Bold), 
                ForeColor = colorFont, 
                AutoSize = true, 
                Margin = new Padding(0, 0, 0, 20) 
            }, 0, 0);

            dgvAttendance = new DataGridView 
            { 
                Dock = DockStyle.Fill, 
                ReadOnly = true, 
                AllowUserToAddRows = false, 
                SelectionMode = DataGridViewSelectionMode.FullRowSelect, 
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill 
            };
            ApplyDarkTheme(dgvAttendance);
            mainLayout.Controls.Add(dgvAttendance, 0, 1);

            var justificationGroup = new GroupBox 
            { 
                Dock = DockStyle.Fill, 
                Text = "Justificar Falta", 
                ForeColor = Color.Gainsboro, 
                Padding = new Padding(10), 
                Margin = new Padding(0, 10, 0, 0), 
                AutoSize = true 
            };
            var justificationLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, AutoSize = true };
            justificationLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            justificationLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            rtbJustification = new RichTextBox { Dock = DockStyle.Fill, Height = 70 };
            ApplyDarkTheme(rtbJustification);

            btnSubmitJustification = new Button { Text = "Enviar Justificación", Width = 150, Height = 70, Dock = DockStyle.Fill };
            ApplyDarkTheme(btnSubmitJustification);
            btnSubmitJustification.Click += BtnSubmitJustification_Click;

            justificationLayout.Controls.Add(new Label { Text = "Seleccione una falta ('absent') en la tabla y escriba el motivo:", ForeColor = colorFont, AutoSize = true, Dock = DockStyle.Top }, 0, 0);
            justificationLayout.SetColumnSpan(justificationLayout.GetControlFromPosition(0, 0), 2);
            justificationLayout.Controls.Add(rtbJustification, 0, 1);
            justificationLayout.Controls.Add(btnSubmitJustification, 1, 1);

            justificationGroup.Controls.Add(justificationLayout);
            mainLayout.Controls.Add(justificationGroup, 0, 2);

            this.Controls.Add(mainLayout);
        }
        private void LoadMyAttendance()
        {
            try
            {
                using (var conn = DBConnection.GetConnection())
                {
                    var query = "SELECT a.attendance_id, c.course_name AS Curso, a.class_date AS Fecha, a.status AS Estado, " +
                                "CASE WHEN aj.justification_id IS NOT NULL THEN aj.status ELSE '' END AS 'Justificación' " +
                                "FROM attendance a " +
                                "JOIN student_enrollments se ON a.enrollment_id = se.enrollment_id " +
                                "JOIN courses c ON se.course_id = c.course_id " +
                                "LEFT JOIN attendance_justifications aj ON a.attendance_id = aj.attendance_id " +
                                "WHERE se.student_id = @studentId ORDER BY a.class_date DESC";
                    var adapter = new MySqlDataAdapter(query, conn);
                    adapter.SelectCommand.Parameters.AddWithValue("@studentId", this.currentStudentId);
                    var dt = new DataTable();
                    adapter.Fill(dt);
                    dgvAttendance.DataSource = dt;
                    if (dgvAttendance.Columns.Contains("attendance_id")) dgvAttendance.Columns["attendance_id"].Visible = false;
                }
            }
            catch (Exception ex) { MessageBox.Show("Error al cargar tu historial de asistencia: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void BtnSubmitJustification_Click(object sender, EventArgs e)
        {
            if (dgvAttendance.CurrentRow == null)
            {
                MessageBox.Show("Por favor, seleccione una asistencia de la lista para justificar.", "Selección Requerida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string status = dgvAttendance.CurrentRow.Cells["Estado"].Value.ToString();
            if (status.ToLower() != "absent")
            {
                MessageBox.Show("Solo puedes justificar las faltas ('absent').", "Acción no permitida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string justificationStatus = dgvAttendance.CurrentRow.Cells["Justificación"].Value.ToString();
            if (!string.IsNullOrEmpty(justificationStatus))
            {
                MessageBox.Show("Esta falta ya tiene una justificación en proceso o finalizada.", "Acción no permitida", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (string.IsNullOrWhiteSpace(rtbJustification.Text))
            {
                MessageBox.Show("Por favor, escriba el motivo de la justificación.", "Datos Incompletos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int attendanceId = Convert.ToInt32(dgvAttendance.CurrentRow.Cells["attendance_id"].Value);

            try
            {
                using (var conn = DBConnection.GetConnection())
                {
                    conn.Open();
                    var cmd = new MySqlCommand("INSERT INTO attendance_justifications (attendance_id, student_id, justification_text) VALUES (@attendanceId, @studentId, @text)", conn);
                    cmd.Parameters.AddWithValue("@attendanceId", attendanceId);
                    cmd.Parameters.AddWithValue("@studentId", this.currentStudentId);
                    cmd.Parameters.AddWithValue("@text", rtbJustification.Text);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Tu solicitud de justificación ha sido enviada para revisión.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    rtbJustification.Clear();
                    LoadMyAttendance();
                }
            }
            catch (Exception ex) { MessageBox.Show("Error al enviar la justificación: " + ex.Message, "Error de Base de Datos", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
    }
}