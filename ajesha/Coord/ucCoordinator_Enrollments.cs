using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace ajesha
{
    public partial class ucCoordinator_Enrollments : UserControl
    {
        private ComboBox cmbStudentsForEnrollment;
        private ListBox lstAvailableCourses, lstEnrolledCourses;
        private Button btnEnroll, btnUnenroll;

        private readonly Color colorDarkBg = Color.FromArgb(45, 45, 48);
        private readonly Color colorMidBg = Color.FromArgb(51, 51, 55);
        private readonly Color colorLightBg = Color.FromArgb(63, 63, 70);
        private readonly Color colorFont = Color.White;
        private readonly Color colorBorder = Color.FromArgb(80, 80, 80);

        public ucCoordinator_Enrollments()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            this.BackColor = colorDarkBg;
            SetupUI();
            this.VisibleChanged += (sender, e) => { if (this.Visible) LoadInitialData(); };
        }
        private void ApplyDarkTheme(Control control)
        {
            if (control is ComboBox || control is ListBox)
            {
                control.BackColor = colorMidBg;
                control.ForeColor = colorFont;
                if (control is ListBox lb) { lb.BorderStyle = BorderStyle.None; }
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
            var mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 3, Padding = new Padding(20) };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            mainLayout.Controls.Add(new Label 
            { 
                Text = "Gestión de Inscripciones", 
                Font = new Font("Segoe UI", 16F, FontStyle.Bold), 
                ForeColor = colorFont, 
                AutoSize = true, 
                Margin = new Padding(0, 0, 0, 20) 
            }, 0, 0);
            mainLayout.SetColumnSpan(mainLayout.GetControlFromPosition(0, 0), 3);

            mainLayout.Controls.Add(new Label { Text = "Seleccionar Estudiante:", Dock = DockStyle.Top, AutoSize = true, ForeColor = colorFont }, 0, 1);
            cmbStudentsForEnrollment = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10F) };
            ApplyDarkTheme(cmbStudentsForEnrollment);
            cmbStudentsForEnrollment.SelectedIndexChanged += CmbStudentsForEnrollment_SelectedIndexChanged;
            mainLayout.Controls.Add(cmbStudentsForEnrollment, 0, 2);

            var buttonsPanel = new FlowLayoutPanel 
            { 
                Dock = DockStyle.Fill, 
                FlowDirection = FlowDirection.TopDown,
                Size = new Size(60, 150), 
                Padding = new Padding(10), 
                Anchor = AnchorStyles.None 
            };
            btnEnroll = new Button { Text = ">>", Width = 40, Font = new Font("Segoe UI", 6F, FontStyle.Bold) };
            ApplyDarkTheme(btnEnroll);
            btnEnroll.Click += BtnEnroll_Click;
            btnUnenroll = new Button { Text = "<<", Width = 40, Font = new Font("Segoe UI", 6F, FontStyle.Bold) };
            ApplyDarkTheme(btnUnenroll);
            btnUnenroll.Click += BtnUnenroll_Click;
            buttonsPanel.Controls.AddRange(new Control[] { btnEnroll, btnUnenroll });
            mainLayout.Controls.Add(buttonsPanel, 1, 2);

            var coursesPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2 };
            coursesPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            coursesPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            coursesPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            coursesPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            coursesPanel.Controls.Add(new Label { Text = "Cursos Disponibles", Dock = DockStyle.Fill, ForeColor = colorFont }, 0, 0);
            coursesPanel.Controls.Add(new Label { Text = "Cursos Inscritos", Dock = DockStyle.Fill, ForeColor = colorFont }, 1, 0);

            lstAvailableCourses = new ListBox { Dock = DockStyle.Fill, SelectionMode = SelectionMode.MultiExtended, Font = new Font("Segoe UI", 10F) };
            ApplyDarkTheme(lstAvailableCourses);
            lstEnrolledCourses = new ListBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10F) };
            ApplyDarkTheme(lstEnrolledCourses);
            coursesPanel.Controls.Add(lstAvailableCourses, 0, 1);
            coursesPanel.Controls.Add(lstEnrolledCourses, 1, 1);
            mainLayout.Controls.Add(coursesPanel, 2, 2);

            this.Controls.Add(mainLayout);
        }
        private void LoadInitialData()
        {
            LoadStudentsForEnrollment();
        }

        private void LoadStudentsForEnrollment()
        {
            try
            {
                using (var connection = DBConnection.GetConnection())
                {
                    var adapter = new MySqlDataAdapter("SELECT user_id, full_name FROM users WHERE role = 'student' ORDER BY full_name", connection);
                    var dt = new DataTable();
                    adapter.Fill(dt);
                    cmbStudentsForEnrollment.DataSource = dt;
                    cmbStudentsForEnrollment.DisplayMember = "full_name";
                    cmbStudentsForEnrollment.ValueMember = "user_id";
                    cmbStudentsForEnrollment.SelectedIndex = -1;
                }
            }
            catch (Exception ex) { MessageBox.Show("Error al cargar la lista de estudiantes: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void CmbStudentsForEnrollment_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbStudentsForEnrollment.SelectedItem == null)
            {
                lstAvailableCourses.DataSource = null;
                lstEnrolledCourses.DataSource = null;
                return;
            }

            DataRowView drv = (DataRowView)cmbStudentsForEnrollment.SelectedItem;
            int studentId = Convert.ToInt32(drv["user_id"]);

            try
            {
                using (var connection = DBConnection.GetConnection())
                {
                    var availableQuery = "SELECT course_id, course_name FROM courses WHERE is_active = TRUE AND course_id NOT IN (SELECT course_id " +
                        "FROM student_enrollments WHERE student_id = @studentId) ORDER BY course_name";
                    var availableAdapter = new MySqlDataAdapter(availableQuery, connection);
                    availableAdapter.SelectCommand.Parameters.AddWithValue("@studentId", studentId);
                    var dtAvailable = new DataTable();
                    availableAdapter.Fill(dtAvailable);
                    lstAvailableCourses.DataSource = dtAvailable;
                    lstAvailableCourses.DisplayMember = "course_name";
                    lstAvailableCourses.ValueMember = "course_id";

                    // Cargar cursos inscritos
                    var enrolledQuery = "SELECT c.course_id, c.course_name FROM courses c JOIN student_enrollments se ON c.course_id = se.course_id WHERE " +
                        "se.student_id = @studentId ORDER BY c.course_name";
                    var enrolledAdapter = new MySqlDataAdapter(enrolledQuery, connection);
                    enrolledAdapter.SelectCommand.Parameters.AddWithValue("@studentId", studentId);
                    var dtEnrolled = new DataTable();
                    enrolledAdapter.Fill(dtEnrolled);
                    lstEnrolledCourses.DataSource = dtEnrolled;
                    lstEnrolledCourses.DisplayMember = "course_name";
                    lstEnrolledCourses.ValueMember = "course_id";
                }
            }
            catch (Exception ex) { MessageBox.Show("Error al cargar los cursos del estudiante: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void BtnEnroll_Click(object sender, EventArgs e)
        {
            if (cmbStudentsForEnrollment.SelectedItem == null || lstAvailableCourses.SelectedItems.Count == 0)
            {
                MessageBox.Show("Seleccione un estudiante y al menos un curso disponible para inscribir.", "Selección Requerida", MessageBoxButtons.OK, 
                    MessageBoxIcon.Warning);
                return;
            }

            DataRowView drv = (DataRowView)cmbStudentsForEnrollment.SelectedItem;
            int studentId = Convert.ToInt32(drv["user_id"]);

            try
            {
                using (var connection = DBConnection.GetConnection())
                {
                    connection.Open();
                    foreach (DataRowView selectedItem in lstAvailableCourses.SelectedItems)
                    {
                        int courseId = Convert.ToInt32(selectedItem["course_id"]);
                        var cmd = new MySqlCommand("INSERT IGNORE INTO student_enrollments (student_id, course_id, teacher_id, academic_year, semester, " +
                            "enrollment_date) VALUES (@student_id, @course_id, 1, '2025', '1', CURDATE())", connection);
                        cmd.Parameters.AddWithValue("@student_id", studentId);
                        cmd.Parameters.AddWithValue("@course_id", courseId);
                        cmd.ExecuteNonQuery();
                    }
                    MessageBox.Show("Inscripción(es) realizada(s) exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex) { MessageBox.Show("Error durante la inscripción: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            finally { CmbStudentsForEnrollment_SelectedIndexChanged(null, null); }
        }

        private void BtnUnenroll_Click(object sender, EventArgs e)
        {
            if (cmbStudentsForEnrollment.SelectedItem == null || lstEnrolledCourses.SelectedItem == null)
            {
                MessageBox.Show("Seleccione un estudiante y un curso inscrito para anular la inscripción.", "Selección Requerida", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DataRowView studentDrv = (DataRowView)cmbStudentsForEnrollment.SelectedItem;
            int studentId = Convert.ToInt32(studentDrv["user_id"]);

            DataRowView courseDrv = (DataRowView)lstEnrolledCourses.SelectedItem;
            int courseId = Convert.ToInt32(courseDrv["course_id"]);

            try
            {
                using (var connection = DBConnection.GetConnection())
                {
                    connection.Open();
                    var cmd = new MySqlCommand("DELETE FROM student_enrollments WHERE student_id = @student_id AND course_id = @course_id", connection);
                    cmd.Parameters.AddWithValue("@student_id", studentId);
                    cmd.Parameters.AddWithValue("@course_id", courseId);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Inscripción anulada exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex) { MessageBox.Show("Error al anular la inscripción: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            finally { CmbStudentsForEnrollment_SelectedIndexChanged(null, null); }
        }
    }
}