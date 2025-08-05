using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace ajesha
{
    public partial class ucCoordinator_AssignCourses : UserControl
    {
        // --- Controles de la Interfaz ---
        private ComboBox cmbTeachers, cmbAvailableCourses, cmbSemester;
        private DataGridView dgvAssignedCourses;
        private TextBox txtAcademicYear, txtSchedule, txtClassroom;
        private Button btnAssignCourse, btnRemoveAssignment;

        // --- Paleta de Colores ---
        private readonly Color colorDarkBg = Color.FromArgb(45, 45, 48);
        private readonly Color colorMidBg = Color.FromArgb(51, 51, 55);
        private readonly Color colorLightBg = Color.FromArgb(63, 63, 70);
        private readonly Color colorFont = Color.White;
        private readonly Color colorBorder = Color.FromArgb(80, 80, 80);

        public ucCoordinator_AssignCourses()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            this.BackColor = colorDarkBg;
            SetupUI();
            this.VisibleChanged += (s, e) => { if (this.Visible) LoadTeachers(); };
        }
        private void ApplyDarkTheme(Control control) {  }

        private void SetupUI()
        {
            var mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(20), RowCount = 3 };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            var topPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, Margin = new Padding(0, 0, 0, 20) };
            topPanel.Controls.Add(new Label { Text = "Seleccionar Maestro:", ForeColor = colorFont, AutoSize = true, Margin = new Padding(0, 6, 10, 0) });
            cmbTeachers = new ComboBox { Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
            ApplyDarkTheme(cmbTeachers);
            cmbTeachers.SelectedIndexChanged += CmbTeachers_SelectedIndexChanged;
            topPanel.Controls.Add(cmbTeachers);
            mainLayout.Controls.Add(topPanel, 0, 0);

            var assignedGroup = new GroupBox { Dock = DockStyle.Fill, Text = "Cursos Asignados a este Maestro", ForeColor = colorFont, Padding = new Padding(10) };
            dgvAssignedCourses = new DataGridView 
            { 
                Dock = DockStyle.Fill, 
                ReadOnly = true, 
                AllowUserToAddRows = false, 
                SelectionMode = DataGridViewSelectionMode.FullRowSelect, 
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill 
            };
            ApplyDarkTheme(dgvAssignedCourses);
            btnRemoveAssignment = new Button { Text = "Quitar Asignación Seleccionada", Dock = DockStyle.Bottom, Height = 30 };
            ApplyDarkTheme(btnRemoveAssignment);
            btnRemoveAssignment.Click += BtnRemoveAssignment_Click;
            assignedGroup.Controls.Add(dgvAssignedCourses);
            assignedGroup.Controls.Add(btnRemoveAssignment);
            mainLayout.Controls.Add(assignedGroup, 0, 1);

            var newAssignmentGroup = new GroupBox { Dock = DockStyle.Fill, Text = "Nueva Asignación", ForeColor = colorFont, Padding = new Padding(10) };
            var newAssignmentLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 4 };
            newAssignmentLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            newAssignmentLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            cmbAvailableCourses = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList }; ApplyDarkTheme(cmbAvailableCourses);
            txtAcademicYear = new TextBox { Dock = DockStyle.Fill, Text = DateTime.Now.Year.ToString() }; ApplyDarkTheme(txtAcademicYear);
            cmbSemester = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList }; cmbSemester.Items.AddRange(new[] { "1", "2", "summer" }); ApplyDarkTheme(cmbSemester);
            txtSchedule = new TextBox { Dock = DockStyle.Fill }; ApplyDarkTheme(txtSchedule);
            txtClassroom = new TextBox { Dock = DockStyle.Fill }; ApplyDarkTheme(txtClassroom);
            btnAssignCourse = new Button { Text = "Asignar Curso", Dock = DockStyle.Fill, Height = 40 }; ApplyDarkTheme(btnAssignCourse);
            btnAssignCourse.Click += BtnAssignCourse_Click;

            newAssignmentLayout.Controls.Add(new Label { Text = "Curso Disponible:", ForeColor = colorFont, AutoSize = true }, 0, 0);
            newAssignmentLayout.Controls.Add(cmbAvailableCourses, 1, 0);
            newAssignmentLayout.Controls.Add(new Label { Text = "Año Académico / Semestre:", ForeColor = colorFont, AutoSize = true }, 0, 1);
            var yearSemesterPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            yearSemesterPanel.Controls.Add(txtAcademicYear, 0, 0);
            yearSemesterPanel.Controls.Add(cmbSemester, 1, 0);
            newAssignmentLayout.Controls.Add(yearSemesterPanel, 1, 1);
            newAssignmentLayout.Controls.Add(new Label { Text = "Horario (Ej: Lu-Mi 10:00-12:00):", ForeColor = colorFont, AutoSize = true }, 0, 2);
            newAssignmentLayout.Controls.Add(txtSchedule, 1, 2);
            newAssignmentLayout.Controls.Add(new Label { Text = "Salón:", ForeColor = colorFont, AutoSize = true }, 0, 3);
            newAssignmentLayout.Controls.Add(txtClassroom, 1, 3);
            newAssignmentLayout.SetColumnSpan(btnAssignCourse, 2);
            newAssignmentLayout.Controls.Add(btnAssignCourse, 0, 4);

            newAssignmentGroup.Controls.Add(newAssignmentLayout);
            mainLayout.Controls.Add(newAssignmentGroup, 0, 2);
            this.Controls.Add(mainLayout);
        }
        private void LoadTeachers()
        {
            try
            {
                using (var conn = DBConnection.GetConnection())
                {
                    var adapter = new MySqlDataAdapter("SELECT user_id, full_name FROM users WHERE role = 'teacher' ORDER BY full_name", conn);
                    var dt = new DataTable();
                    adapter.Fill(dt);
                    cmbTeachers.DataSource = dt;
                    cmbTeachers.DisplayMember = "full_name";
                    cmbTeachers.ValueMember = "user_id";
                    cmbTeachers.SelectedIndex = -1;
                }
            }
            catch (Exception ex) { MessageBox.Show("Error al cargar maestros: " + ex.Message); }
        }

        private void CmbTeachers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbTeachers.SelectedValue == null) return;
            LoadAssignedCourses();
            LoadAvailableCourses();
        }

        private void LoadAssignedCourses()
        {
            try
            {
                using (var conn = DBConnection.GetConnection())
                {
                    var adapter = new MySqlDataAdapter("SELECT tc.teacher_course_id, c.course_name AS Curso, tc.academic_year AS Año, tc.semester AS Sem, " +
                        "tc.schedule AS Horario, tc.classroom AS Salón FROM teacher_courses tc JOIN courses c ON tc.course_id = c.course_id WHERE tc.teacher_id = @teacherId", conn);
                    adapter.SelectCommand.Parameters.AddWithValue("@teacherId", cmbTeachers.SelectedValue);
                    var dt = new DataTable();
                    adapter.Fill(dt);
                    dgvAssignedCourses.DataSource = dt;
                    if (dgvAssignedCourses.Columns.Contains("teacher_course_id")) dgvAssignedCourses.Columns["teacher_course_id"].Visible = false;
                }
            }
            catch (Exception ex) { MessageBox.Show("Error al cargar cursos asignados: " + ex.Message); }
        }

        private void LoadAvailableCourses()
        {
            try
            {
                using (var conn = DBConnection.GetConnection())
                {
                    var adapter = new MySqlDataAdapter("SELECT course_id, course_name FROM courses WHERE is_active = TRUE AND course_id NOT IN (SELECT course_id FROM teacher_courses WHERE teacher_id = @teacherId)", conn);
                    adapter.SelectCommand.Parameters.AddWithValue("@teacherId", cmbTeachers.SelectedValue);
                    var dt = new DataTable();
                    adapter.Fill(dt);
                    cmbAvailableCourses.DataSource = dt;
                    cmbAvailableCourses.DisplayMember = "course_name";
                    cmbAvailableCourses.ValueMember = "course_id";
                }
            }
            catch (Exception ex) { MessageBox.Show("Error al cargar cursos disponibles: " + ex.Message); }
        }

        private void BtnAssignCourse_Click(object sender, EventArgs e)
        {
            if (cmbTeachers.SelectedValue == null || cmbAvailableCourses.SelectedValue == null || cmbSemester.SelectedItem == null || string.IsNullOrWhiteSpace(txtSchedule.Text))
            {
                MessageBox.Show("Por favor, seleccione un maestro, un curso y complete todos los campos de la nueva asignación.", "Datos Incompletos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (var conn = DBConnection.GetConnection())
                {
                    conn.Open();
                    var cmd = new MySqlCommand("INSERT INTO teacher_courses (teacher_id, course_id, academic_year, semester, schedule, classroom) VALUES (@teacherId, @courseId, @year, @semester, @schedule, @classroom)", conn);
                    cmd.Parameters.AddWithValue("@teacherId", cmbTeachers.SelectedValue);
                    cmd.Parameters.AddWithValue("@courseId", cmbAvailableCourses.SelectedValue);
                    cmd.Parameters.AddWithValue("@year", txtAcademicYear.Text);
                    cmd.Parameters.AddWithValue("@semester", cmbSemester.SelectedItem.ToString());
                    cmd.Parameters.AddWithValue("@schedule", txtSchedule.Text);
                    cmd.Parameters.AddWithValue("@classroom", txtClassroom.Text);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Curso asignado exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    CmbTeachers_SelectedIndexChanged(null, null);
                }
            }
            catch (Exception ex) { MessageBox.Show("Error al asignar el curso: " + ex.Message); }
        }

        private void BtnRemoveAssignment_Click(object sender, EventArgs e)
        {
            if (dgvAssignedCourses.CurrentRow == null)
            {
                MessageBox.Show("Por favor, seleccione una asignación de la tabla para eliminar.", "Selección Requerida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("¿Está seguro de que desea quitar esta asignación?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;

            int assignmentId = Convert.ToInt32(dgvAssignedCourses.CurrentRow.Cells["teacher_course_id"].Value);

            try
            {
                using (var conn = DBConnection.GetConnection())
                {
                    conn.Open();
                    var cmd = new MySqlCommand("DELETE FROM teacher_courses WHERE teacher_course_id = @id", conn);
                    cmd.Parameters.AddWithValue("@id", assignmentId);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Asignación eliminada.", "Éxito");
                    CmbTeachers_SelectedIndexChanged(null, null);
                }
            }
            catch (Exception ex) { MessageBox.Show("Error al eliminar la asignación: " + ex.Message); }
        }
    }
}