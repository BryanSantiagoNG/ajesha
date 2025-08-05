using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace ajesha
{
    public partial class ucStudentClassroom : UserControl
    {
        private int currentStudentId;
        private ListBox lstMyCourses;
        private DataGridView dgvAssignments;
        private Label lblSchedule;
        private Button btnViewSubmitAssignment;

        // --- Paleta de Colores ---
        private readonly Color colorDarkBg = Color.FromArgb(45, 45, 48);
        private readonly Color colorMidBg = Color.FromArgb(51, 51, 55);
        private readonly Color colorLightBg = Color.FromArgb(63, 63, 70);
        private readonly Color colorFont = Color.White;
        private readonly Color colorBorder = Color.FromArgb(80, 80, 80);

        public ucStudentClassroom()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            this.BackColor = colorDarkBg;
            SetupUI();
            this.VisibleChanged += (s, e) => { if (this.Visible && currentStudentId > 0) LoadMyCourses(); };
        }

        public void SetStudentId(int studentId) { this.currentStudentId = studentId; }

        #region Styling and UI Setup
        private void ApplyDarkTheme(Control control)
        {
            if (control is ListBox || control is ComboBox)
            {
                control.BackColor = colorMidBg;
                control.ForeColor = colorFont;
                if (control is ListBox lb) { lb.BorderStyle = BorderStyle.None; }
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
            var mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(20) };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));

            var leftPanel = new Panel { Dock = DockStyle.Fill };
            var lblMyCourses = new Label 
            { 
                Text = "Mis Cursos", 
                ForeColor = colorFont, 
                Font = new Font("Segoe UI", 12F, FontStyle.Bold), 
                Dock = DockStyle.Top, 
                Margin = new Padding(0, 0, 0, 5) 
            };
            lstMyCourses = new ListBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10F) };
            ApplyDarkTheme(lstMyCourses);
            lstMyCourses.SelectedIndexChanged += LstMyCourses_SelectedIndexChanged;

            var scheduleGroup = new GroupBox { Text = "Horario del Curso", Dock = DockStyle.Bottom, Height = 100, ForeColor = Color.Gainsboro, Padding = new Padding(10) };
            lblSchedule = new Label { Dock = DockStyle.Fill, ForeColor = colorFont, Font = new Font("Segoe UI", 10F), TextAlign = ContentAlignment.MiddleCenter };
            scheduleGroup.Controls.Add(lblSchedule);

            leftPanel.Controls.Add(lstMyCourses);
            leftPanel.Controls.Add(lblMyCourses);
            leftPanel.Controls.Add(scheduleGroup);
            mainLayout.Controls.Add(leftPanel, 0, 0);

            var rightPanel = new Panel { Dock = DockStyle.Fill };
            var lblAssignments = new Label 
            { 
                Text = "Tareas y Actividades", 
                ForeColor = colorFont, 
                Font = new Font("Segoe UI", 12F, FontStyle.Bold), 
                Dock = DockStyle.Top, Margin = new Padding(0, 0, 0, 5) 
            };
            dgvAssignments = new DataGridView 
            { 
                Dock = DockStyle.Fill, 
                ReadOnly = true, 
                AllowUserToAddRows = false, 
                SelectionMode = DataGridViewSelectionMode.FullRowSelect, 
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill 
            };
            ApplyDarkTheme(dgvAssignments);

            btnViewSubmitAssignment = new Button 
            { 
                Text = "Ver / Entregar Tarea Seleccionada", 
                Dock = DockStyle.Bottom, 
                Height = 35, 
                Enabled = false, 
                Margin = new Padding(0, 10, 0, 0) 
            };
            ApplyDarkTheme(btnViewSubmitAssignment);
            btnViewSubmitAssignment.Click += BtnViewSubmitAssignment_Click;

            rightPanel.Controls.Add(dgvAssignments);
            rightPanel.Controls.Add(btnViewSubmitAssignment);
            rightPanel.Controls.Add(lblAssignments);

            dgvAssignments.SelectionChanged += (s, e) => { btnViewSubmitAssignment.Enabled = dgvAssignments.CurrentRow != null; };

            mainLayout.Controls.Add(rightPanel, 1, 0);
            this.Controls.Add(mainLayout);
        }
        #endregion

        #region Data Logic
        private void LoadMyCourses()
        {
            try
            {
                using (var conn = DBConnection.GetConnection())
                {
                    var query = "SELECT c.course_id, c.course_name FROM courses c JOIN student_enrollments se ON c.course_id = se.course_id WHERE se.student_id = " +
                        "@studentId ORDER BY c.course_name";
                    var adapter = new MySqlDataAdapter(query, conn);
                    adapter.SelectCommand.Parameters.AddWithValue("@studentId", this.currentStudentId);
                    var dt = new DataTable();
                    adapter.Fill(dt);
                    lstMyCourses.DataSource = dt;
                    lstMyCourses.DisplayMember = "course_name";
                    lstMyCourses.ValueMember = "course_id";
                }
            }
            catch (Exception ex) { MessageBox.Show("Error al cargar tus cursos: " + ex.Message); }
        }

        private void LstMyCourses_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstMyCourses.SelectedItem == null)
            {
                lblSchedule.Text = "";
                dgvAssignments.DataSource = null;
                return;
            }

            DataRowView drv = (DataRowView)lstMyCourses.SelectedItem;
            int courseId = Convert.ToInt32(drv["course_id"]);

            // Cargar horario
            try
            {
                using (var conn = DBConnection.GetConnection())
                {
                    var query = "SELECT tc.schedule FROM teacher_courses tc JOIN student_enrollments se ON tc.course_id = se.course_id AND " +
                        "tc.teacher_id = se.teacher_id WHERE se.student_id = @studentId AND se.course_id = @courseId LIMIT 1";
                    var cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@studentId", this.currentStudentId);
                    cmd.Parameters.AddWithValue("@courseId", courseId);
                    conn.Open();
                    lblSchedule.Text = cmd.ExecuteScalar()?.ToString() ?? "Horario no definido";
                }
            }
            catch (Exception) { lblSchedule.Text = "Error al cargar horario"; }

            // Cargar tareas y su estado de entrega
            try
            {
                using (var conn = DBConnection.GetConnection())
                {
                    var query = "SELECT a.assignment_id, a.title AS Título, a.due_date AS 'Fecha de Entrega', " +
                                "CASE WHEN s.submission_id IS NOT NULL THEN s.status ELSE 'Pendiente' END AS Estado, " +
                                "s.grade AS Calificación " +
                                "FROM assignments a " +
                                "LEFT JOIN assignment_submissions s ON a.assignment_id = s.assignment_id AND s.student_id = @studentId " +
                                "WHERE a.course_id = @courseId ORDER BY a.due_date ASC";
                    var adapter = new MySqlDataAdapter(query, conn);
                    adapter.SelectCommand.Parameters.AddWithValue("@courseId", courseId);
                    adapter.SelectCommand.Parameters.AddWithValue("@studentId", this.currentStudentId);
                    var dt = new DataTable();
                    adapter.Fill(dt);
                    dgvAssignments.DataSource = dt;
                    if (dgvAssignments.Columns.Contains("assignment_id")) dgvAssignments.Columns["assignment_id"].Visible = false;
                }
            }
            catch (Exception ex) { MessageBox.Show("Error al cargar tareas: " + ex.Message); }
        }

        private void BtnViewSubmitAssignment_Click(object sender, EventArgs e)
        {
            if (dgvAssignments.CurrentRow == null) return;

            int assignmentId = Convert.ToInt32(dgvAssignments.CurrentRow.Cells["assignment_id"].Value);

            frmMain mainForm = this.FindForm() as frmMain;
            if (mainForm != null)
            {
                mainForm.NavigateTo_StudentSubmission(this.currentStudentId, assignmentId);
            }
        }
        #endregion
    }
}