using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace ajesha
{
    public partial class ucCoordinator_AcademicTracking : UserControl
    {
        private ComboBox cmbCourses;
        private DataGridView dgvStudentPerformance;

        // --- Paleta de Colores ---
        private readonly Color colorDarkBg = Color.FromArgb(45, 45, 48);
        private readonly Color colorMidBg = Color.FromArgb(51, 51, 55);
        private readonly Color colorLightBg = Color.FromArgb(63, 63, 70);
        private readonly Color colorFont = Color.White;
        private readonly Color colorWarning = Color.FromArgb(192, 57, 43);
        private readonly Color colorBorder = Color.FromArgb(80, 80, 80);

        public ucCoordinator_AcademicTracking()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            this.BackColor = colorDarkBg;
            SetupUI();
            this.VisibleChanged += (s, e) => { if (this.Visible) LoadAllCourses(); };
        }
        private void ApplyDarkTheme(Control control)
        {
            if (control is ComboBox)
            {
                control.BackColor = colorMidBg;
                control.ForeColor = colorFont;
                ((ComboBox)control).FlatStyle = FlatStyle.Flat;
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
        }
        private void SetupUI()
        {
            var mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(20), RowCount = 3 };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            mainLayout.Controls.Add(new Label 
            { 
                Text = "Seguimiento Académico por Curso", 
                Font = new Font("Segoe UI", 16F, FontStyle.Bold), 
                ForeColor = colorFont,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 20)
            }, 0, 0);

            var topPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, Margin = new Padding(0, 0, 0, 10) };
            topPanel.Controls.Add(new Label { Text = "Seleccionar Curso:", ForeColor = colorFont, AutoSize = true, Margin = new Padding(0, 6, 10, 0) });
            cmbCourses = new ComboBox { Width = 400, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10F) };
            ApplyDarkTheme(cmbCourses);
            cmbCourses.SelectedIndexChanged += CmbCourses_SelectedIndexChanged;
            topPanel.Controls.Add(cmbCourses);
            mainLayout.Controls.Add(topPanel, 0, 1);

            dgvStudentPerformance = new DataGridView 
            { 
                Dock = DockStyle.Fill,
                ReadOnly = true, 
                AllowUserToAddRows = false, 
                SelectionMode = DataGridViewSelectionMode.FullRowSelect, 
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill 
            };
            ApplyDarkTheme(dgvStudentPerformance);
            dgvStudentPerformance.CellFormatting += DgvStudentPerformance_CellFormatting;
            mainLayout.Controls.Add(dgvStudentPerformance, 0, 2);

            this.Controls.Add(mainLayout);
        }
        private void LoadAllCourses()
        {
            try
            {
                using (var conn = DBConnection.GetConnection())
                {
                    var adapter = new MySqlDataAdapter("SELECT course_id, course_name FROM courses WHERE is_active = TRUE ORDER BY course_name", conn);
                    var dt = new DataTable();
                    adapter.Fill(dt);
                    cmbCourses.DataSource = dt;
                    cmbCourses.DisplayMember = "course_name";
                    cmbCourses.ValueMember = "course_id";
                    cmbCourses.SelectedIndex = -1;
                    dgvStudentPerformance.DataSource = null;
                }
            }
            catch (Exception ex) { MessageBox.Show("Error al cargar la lista de cursos: " + ex.Message, "Error de Base de Datos", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void CmbCourses_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbCourses.SelectedValue == null || !(cmbCourses.SelectedValue is int)) return;

            int courseId = Convert.ToInt32(cmbCourses.SelectedValue);

            try
            {
                using (var conn = DBConnection.GetConnection())
                {
                    var query = "SELECT student_name AS Estudiante, course_average AS 'Promedio Actual', " +
                        "status AS Estado FROM vw_student_grades WHERE course_id = @courseId ORDER BY student_name";
                    var adapter = new MySqlDataAdapter(query, conn);
                    adapter.SelectCommand.Parameters.AddWithValue("@courseId", courseId);
                    var dt = new DataTable();
                    adapter.Fill(dt);
                    dgvStudentPerformance.DataSource = dt;
                }
            }
            catch (Exception ex) { MessageBox.Show("Error al cargar el rendimiento de los estudiantes: " + ex.Message, 
                "Error de Base de Datos", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void DgvStudentPerformance_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var row = dgvStudentPerformance.Rows[e.RowIndex];
                object gradeValue = row.Cells["Promedio Actual"].Value;

                if (gradeValue != null && gradeValue != DBNull.Value)
                {
                    if (decimal.TryParse(gradeValue.ToString(), out decimal grade) && grade < 70)
                    {
                        row.DefaultCellStyle.BackColor = colorWarning;
                        row.DefaultCellStyle.ForeColor = Color.White;
                        row.DefaultCellStyle.SelectionBackColor = Color.DarkRed;
                        row.DefaultCellStyle.SelectionForeColor = Color.White;
                    }
                    else
                    {
                        row.DefaultCellStyle.BackColor = colorMidBg;
                        row.DefaultCellStyle.ForeColor = colorFont;
                        row.DefaultCellStyle.SelectionBackColor = Color.SteelBlue;
                        row.DefaultCellStyle.SelectionForeColor = Color.White;
                    }
                }
            }
        }
    }
}