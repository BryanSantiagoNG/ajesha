using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace ajesha
{
    public partial class ucTeacher_CourseManagement : UserControl
    {
        private int currentTeacherId;
        private ComboBox cmbCourses;
        private TabControl tabCourseActions;

        // Pestaña Asistencia
        private DataGridView dgvAttendance;
        private DateTimePicker dtpAttendanceDate;
        private Button btnSaveAttendance;

        // Pestaña Calificaciones
        private DataGridView dgvStudentsForGrades, dgvIndividualGrades;
        private ComboBox cmbAssessmentType;
        private TextBox txtGradeValue, txtMaxGrade, txtWeight;
        private Button btnSaveGrade;
        private int selectedEnrollmentId = 0;

        // Pestaña Tareas
        private DataGridView dgvAssignments, dgvSubmissions;
        private TextBox txtAssignmentTitle;
        private RichTextBox rtbAssignmentDescription;
        private DateTimePicker dtpDueDate;
        private Button btnCreateAssignment;

        // Pestaña Observaciones
        private DataGridView dgvStudentsForObs;
        private RichTextBox rtbObservation;
        private Button btnSaveObservation;
        private int selectedEnrollmentIdForObs = 0;

        // Placeholders para .NET Framework
        private const string GradeValuePlaceholder = "Valor";
        private const string MaxGradePlaceholder = "Máximo";
        private const string WeightPlaceholder = "Peso (ej: 0.25)";
        private const string AssignmentTitlePlaceholder = "Título de la Tarea";
        private const string AssignmentDescPlaceholder = "Descripción...";

        // Paleta de Colores
        private readonly Color colorDarkBg = Color.FromArgb(45, 45, 48);
        private readonly Color colorMidBg = Color.FromArgb(51, 51, 55);
        private readonly Color colorLightBg = Color.FromArgb(63, 63, 70);
        private readonly Color colorFont = Color.White;
        private readonly Color colorBorder = Color.FromArgb(80, 80, 80);

        public ucTeacher_CourseManagement()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            this.BackColor = colorDarkBg;
            SetupUI();
            this.VisibleChanged += (s, e) => { if (this.Visible) LoadCourses(); };
        }

        public void SetTeacherId(int teacherId) { this.currentTeacherId = teacherId; }

        private void ApplyDarkTheme(Control control) 
        { 
            if (control is TextBox || control is RichTextBox || control is ComboBox || control is ListBox) 
            { 
                control.BackColor = colorMidBg; 
                control.ForeColor = colorFont; 
                if (control is TextBox txt) 
                { 
                    txt.BorderStyle = BorderStyle.FixedSingle; 
                } 
                if (control is RichTextBox rtb) 
                { 
                    rtb.BorderStyle = BorderStyle.FixedSingle; 
                } 
                if (control is ComboBox cmb) 
                { 
                    cmb.FlatStyle = FlatStyle.Flat; 
                } 
            } 
            if (control is DataGridView dgv) { 
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
            if (control is DateTimePicker dtp) 
            { 
                dtp.CalendarForeColor = colorFont; 
                dtp.CalendarMonthBackground = colorMidBg; 
                dtp.CalendarTitleBackColor = colorLightBg; 
            } 
        }
        private void TabControl_DrawItem(object sender, DrawItemEventArgs e) { TabPage page = tabCourseActions.TabPages[e.Index]; e.Graphics.FillRectangle(new SolidBrush(colorDarkBg), e.Bounds); using (var brush = new SolidBrush(colorFont)) { e.Graphics.DrawString(page.Text, this.Font, brush, e.Bounds.X + 3, e.Bounds.Y + 3); } }
        private void SetupPlaceholder(Control control) { if (control.Tag is string placeholder) { control.Text = placeholder; control.ForeColor = Color.Gray; control.Enter += (s, e) => { if (control.Text == placeholder) { control.Text = ""; control.ForeColor = colorFont; } }; control.Leave += (s, e) => { if (string.IsNullOrWhiteSpace(control.Text)) { control.Text = placeholder; control.ForeColor = Color.Gray; } }; } }

        private void SetupUI()
        {
            var mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(20), RowCount = 2 };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            var topPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
            topPanel.Controls.Add(new Label { Text = "Seleccionar Curso:", ForeColor = colorFont, AutoSize = true, Margin = new Padding(0, 6, 10, 0) });
            cmbCourses = new ComboBox { Width = 400, DropDownStyle = ComboBoxStyle.DropDownList };
            ApplyDarkTheme(cmbCourses);
            cmbCourses.SelectedIndexChanged += CmbCourses_SelectedIndexChanged;
            topPanel.Controls.Add(cmbCourses);
            mainLayout.Controls.Add(topPanel, 0, 0);
            tabCourseActions = new TabControl { Dock = DockStyle.Fill, Enabled = false, Margin = new Padding(0, 10, 0, 0) };
            ApplyDarkTheme(tabCourseActions);
            var tabAttendance = new TabPage("Asistencia") { BackColor = colorDarkBg, Padding = new Padding(10) };
            SetupAttendanceTab(tabAttendance);
            var tabGrades = new TabPage("Calificaciones") { BackColor = colorDarkBg, Padding = new Padding(10) };
            SetupGradesTab(tabGrades);
            var tabAssignments = new TabPage("Tareas") { BackColor = colorDarkBg, Padding = new Padding(10) };
            SetupAssignmentsTab(tabAssignments);
            var tabObservations = new TabPage("Observaciones") { BackColor = colorDarkBg, Padding = new Padding(10) };
            SetupObservationsTab(tabObservations);
            tabCourseActions.TabPages.AddRange(new TabPage[] { tabAttendance, tabGrades, tabAssignments, tabObservations });
            tabCourseActions.SelectedIndexChanged += (s, e) => LoadDataForCurrentTab();
            mainLayout.Controls.Add(tabCourseActions, 0, 1);
            this.Controls.Add(mainLayout);
        }

        private void SetupAttendanceTab(TabPage parent) 
        { 
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 }; 
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); 
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); 
            var topPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, Margin = new Padding(0, 0, 0, 10) }; 
            topPanel.Controls.Add(new Label { Text = "Fecha:", ForeColor = colorFont, AutoSize = true, Margin = new Padding(0, 6, 10, 0) }); 
            dtpAttendanceDate = new DateTimePicker(); 
            ApplyDarkTheme(dtpAttendanceDate); 
            btnSaveAttendance = new Button { Text = "Guardar Asistencia", Width = 150, Height = 30, Margin = new Padding(20, 0, 0, 0) }; 
            ApplyDarkTheme(btnSaveAttendance); 
            btnSaveAttendance.Click += BtnSaveAttendance_Click; 
            topPanel.Controls.AddRange(new Control[] { dtpAttendanceDate, btnSaveAttendance }); 
            layout.Controls.Add(topPanel, 0, 0); 
            dgvAttendance = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false }; 
            ApplyDarkTheme(dgvAttendance);
            layout.Controls.Add(dgvAttendance, 0, 1); 
            parent.Controls.Add(layout); 
        }
        private void SetupGradesTab(TabPage parent) 
        { 
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 }; 
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F)); 
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F)); 
            dgvStudentsForGrades = new DataGridView 
            { 
                Dock = DockStyle.Fill, 
                ReadOnly = true, 
                AllowUserToAddRows = false, 
                SelectionMode = DataGridViewSelectionMode.FullRowSelect, 
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill 
            }; 
            ApplyDarkTheme(dgvStudentsForGrades); 
            dgvStudentsForGrades.CellClick += DgvStudentsForGrades_CellClick; 
            layout.Controls.Add(dgvStudentsForGrades, 0, 0); 
            var rightPanel = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 }; 
            rightPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F)); 
            rightPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F)); 
            dgvIndividualGrades = new DataGridView 
            { 
                Dock = DockStyle.Fill, 
                ReadOnly = true, 
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill 
            }; 
            ApplyDarkTheme(dgvIndividualGrades); 
            var addGradeGroup = new GroupBox 
            { 
                Dock = DockStyle.Fill, 
                Text = "Añadir Nueva Calificación", 
                ForeColor = colorFont, 
                Padding = new Padding(10) 
            }; 
            var addGradeLayout = new TableLayoutPanel 
            { 
                Dock = DockStyle.Fill, 
                ColumnCount = 4, 
                RowCount = 2 
            }; 
            cmbAssessmentType = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList }; 
            cmbAssessmentType.Items.AddRange(new[] { "exam", "quiz", "homework", "project", "participation" }); 
            ApplyDarkTheme(cmbAssessmentType); 
            txtGradeValue = new TextBox { Dock = DockStyle.Fill, Tag = GradeValuePlaceholder }; 
            ApplyDarkTheme(txtGradeValue); 
            txtMaxGrade = new TextBox { Dock = DockStyle.Fill, Tag = MaxGradePlaceholder }; 
            ApplyDarkTheme(txtMaxGrade); 
            txtWeight = new TextBox { Dock = DockStyle.Fill, Tag = WeightPlaceholder }; 
            ApplyDarkTheme(txtWeight); 
            SetupPlaceholder(txtGradeValue); 
            SetupPlaceholder(txtMaxGrade); 
            SetupPlaceholder(txtWeight); 
            btnSaveGrade = new Button { Text = "Guardar Calificación", Dock = DockStyle.Fill }; 
            ApplyDarkTheme(btnSaveGrade); 
            btnSaveGrade.Click += BtnSaveGrade_Click; 
            addGradeLayout.Controls.Add(cmbAssessmentType, 0, 0); 
            addGradeLayout.Controls.Add(txtGradeValue, 1, 0); 
            addGradeLayout.Controls.Add(txtMaxGrade, 2, 0); 
            addGradeLayout.Controls.Add(txtWeight, 3, 0); 
            addGradeLayout.SetColumnSpan(btnSaveGrade, 4); 
            addGradeLayout.Controls.Add(btnSaveGrade, 0, 1); 
            addGradeGroup.Controls.Add(addGradeLayout); 
            rightPanel.Controls.Add(dgvIndividualGrades, 0, 0); 
            rightPanel.Controls.Add(addGradeGroup, 0, 1); 
            layout.Controls.Add(rightPanel, 1, 0); 
            parent.Controls.Add(layout); 
        }
        private void SetupAssignmentsTab(TabPage parent) 
        { 
            var layout = new TableLayoutPanel 
            { 
                Dock = DockStyle.Fill, 
                ColumnCount = 2 
            }; 
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F)); 
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F)); 
            var leftPanel = new TableLayoutPanel 
            { Dock = DockStyle.Fill, RowCount = 2 }; 
            leftPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F)); 
            leftPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F)); 
            dgvAssignments = new DataGridView 
            { 
                Dock = DockStyle.Fill, 
                ReadOnly = true, 
                AllowUserToAddRows = false, 
                SelectionMode = DataGridViewSelectionMode.FullRowSelect, 
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill 
            }; 
            ApplyDarkTheme(dgvAssignments); 
            dgvAssignments.SelectionChanged += DgvAssignments_SelectionChanged; 
            var createAssignmentGroup = new GroupBox 
            { 
                Dock = DockStyle.Fill, 
                Text = "Crear Nueva Tarea", 
                ForeColor = colorFont, 
                Padding = new Padding(10) 
            }; 
            var createLayout = new TableLayoutPanel 
            { 
                Dock = DockStyle.Fill, 
                RowCount = 6 
            }; 
            createLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            createLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); 
            createLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); 
            createLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); 
            createLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); 
            createLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); 
            txtAssignmentTitle = new TextBox { Dock = DockStyle.Fill, Tag = AssignmentTitlePlaceholder }; 
            ApplyDarkTheme(txtAssignmentTitle); 
            rtbAssignmentDescription = new RichTextBox { Dock = DockStyle.Fill, Tag = AssignmentDescPlaceholder }; 
            ApplyDarkTheme(rtbAssignmentDescription); 
            SetupPlaceholder(txtAssignmentTitle); 
            SetupPlaceholder(rtbAssignmentDescription); 
            dtpDueDate = new DateTimePicker 
            { 
                Dock = DockStyle.Fill, 
                Format = DateTimePickerFormat.Custom, 
                CustomFormat = "yyyy-MM-dd HH:mm:ss" 
            }; 
            ApplyDarkTheme(dtpDueDate); 
            btnCreateAssignment = new Button 
            { 
                Text = "Crear Tarea", 
                Dock = DockStyle.Fill, 
                Height = 40 
            }; 
            ApplyDarkTheme(btnCreateAssignment); 
            btnCreateAssignment.Click += BtnCreateAssignment_Click; 
            createLayout.Controls.Add(new Label { Text = "Título:", ForeColor = colorFont, AutoSize = true }, 0, 0); 
            createLayout.Controls.Add(txtAssignmentTitle, 0, 1); 
            createLayout.Controls.Add(rtbAssignmentDescription, 0, 2); 
            createLayout.Controls.Add(new Label { Text = "Fecha de Entrega:", ForeColor = colorFont, AutoSize = true }, 0, 3);
            createLayout.Controls.Add(dtpDueDate, 0, 4); 
            createLayout.Controls.Add(btnCreateAssignment, 0, 5); 
            createAssignmentGroup.Controls.Add(createLayout); 
            leftPanel.Controls.Add(dgvAssignments, 0, 0); 
            leftPanel.Controls.Add(createAssignmentGroup, 0, 1); 
            layout.Controls.Add(leftPanel, 0, 0); 
            var rightPanel = new Panel { Dock = DockStyle.Fill }; 
            rightPanel.Controls.Add(new Label 
            { 
                Text = "Entregas de la Tarea Seleccionada", 
                Font = new Font("Segoe UI", 12F, FontStyle.Bold), 
                ForeColor = Color.White, 
                Dock = DockStyle.Top, 
                AutoSize = true 
            }); 
            dgvSubmissions = new DataGridView 
            { 
                Dock = DockStyle.Fill, 
                ReadOnly = true, 
                AllowUserToAddRows = false, 
                SelectionMode = DataGridViewSelectionMode.FullRowSelect, 
                Margin = new Padding(0, 30, 0, 0), 
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill 
            }; 
            ApplyDarkTheme(dgvSubmissions); 
            dgvSubmissions.CellDoubleClick += DgvSubmissions_CellDoubleClick; 
            rightPanel.Controls.Add(dgvSubmissions); 
            layout.Controls.Add(rightPanel, 1, 0); 
            parent.Controls.Add(layout); 
        }
        private void SetupObservationsTab(TabPage parent) 
        { 
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(10) }; 
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F)); 
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F)); 
            dgvStudentsForObs = new DataGridView 
            { 
                Dock = DockStyle.Fill, 
                ReadOnly = true, 
                AllowUserToAddRows = false, 
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill 
            }; 
            ApplyDarkTheme(dgvStudentsForObs); 
            dgvStudentsForObs.CellClick += DgvStudentsForObs_CellClick; 
            layout.Controls.Add(dgvStudentsForObs, 0, 0); 
            var rightPanel = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 }; 
            rightPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            rightPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); 
            rtbObservation = new RichTextBox { Dock = DockStyle.Fill, Text = "Seleccione un estudiante para ver o añadir observaciones..." };
            ApplyDarkTheme(rtbObservation); 
            btnSaveObservation = new Button { Text = "Guardar Observación", Dock = DockStyle.Fill, Height = 40, Enabled = false }; 
            ApplyDarkTheme(btnSaveObservation); 
            btnSaveObservation.Click += BtnSaveObservation_Click; 
            rightPanel.Controls.Add(rtbObservation, 0, 0); 
            rightPanel.Controls.Add(btnSaveObservation, 0, 1); 
            layout.Controls.Add(rightPanel, 1, 0); 
            parent.Controls.Add(layout); 
        }
        
        private void LoadCourses() 
        { 
            try
            { 
                using (var conn = DBConnection.GetConnection()) 
                { 
                    var adapter = new MySqlDataAdapter("SELECT course_id, course_name FROM vw_courses_with_teachers WHERE teacher_id = @teacherId", conn); 
                    adapter.SelectCommand.Parameters.AddWithValue("@teacherId", this.currentTeacherId); 
                    var dt = new DataTable(); 
                    adapter.Fill(dt); 
                    cmbCourses.DataSource = dt; 
                    cmbCourses.DisplayMember = "course_name"; 
                    cmbCourses.ValueMember = "course_id"; 
                    cmbCourses.SelectedIndex = -1; 
                    tabCourseActions.Enabled = false; 
                } 
            } 
            catch (Exception ex) { MessageBox.Show("Error al cargar cursos: " + ex.Message); } 
        }
        private void CmbCourses_SelectedIndexChanged(object sender, EventArgs e) 
        { 
            if (cmbCourses.SelectedValue != null) 
            { 
                tabCourseActions.Enabled = true; LoadDataForCurrentTab(); 
            } 
        }
        private void LoadDataForCurrentTab() 
        { 
            if (cmbCourses.SelectedValue == null) return; 
            switch (tabCourseActions.SelectedTab.Text) 
            { 
                case "Asistencia": LoadStudentsForAttendance(); break; 
                case "Calificaciones": LoadStudentsForGrades(); break; 
                case "Tareas": LoadAssignments(); break; 
                case "Observaciones": LoadStudentsForObservations(); break; 
            } 
        }
        private void LoadStudentsForAttendance() 
        { 
            if (cmbCourses.SelectedValue == null) return; 
            try 
            { 
                using (var conn = DBConnection.GetConnection()) 
                { 
                    var query = "SELECT se.enrollment_id, u.full_name AS Estudiante FROM users u JOIN student_enrollments se ON u.user_id = se.student_id " +
                        "WHERE se.course_id = @courseId"; var adapter = new MySqlDataAdapter(query, conn); 
                    adapter.SelectCommand.Parameters.AddWithValue("@courseId", cmbCourses.SelectedValue); 
                    var dt = new DataTable(); 
                    adapter.Fill(dt); 
                    dgvAttendance.DataSource = dt; 
                    if (dgvAttendance.Columns["enrollment_id"] != null) dgvAttendance.Columns["enrollment_id"].Visible = false; 
                    if (!dgvAttendance.Columns.Contains("Status")) 
                    { 
                        var statusColumn = new DataGridViewComboBoxColumn(); 
                        statusColumn.Name = "Status"; 
                        statusColumn.HeaderText = "Estado"; 
                        statusColumn.DataSource = new[] { "present", "absent", "late", "excused" }; 
                        dgvAttendance.Columns.Add(statusColumn); 
                    } 
                } 
            } 
            catch (Exception ex) { MessageBox.Show("Error al cargar estudiantes para asistencia: " + ex.Message); 
            } 
        }
        private void LoadStudentsForGrades() 
        { 
            if (cmbCourses.SelectedValue == null) return; 
            try 
            { 
                using (var conn = DBConnection.GetConnection()) 
                { 
                    var query = "SELECT se.enrollment_id, u.full_name AS Estudiante, se.final_grade AS 'Promedio' FROM users u JOIN student_enrollments se ON " +
                        "u.user_id = se.student_id WHERE se.course_id = @courseId"; 
                    var adapter = new MySqlDataAdapter(query, conn); 
                    adapter.SelectCommand.Parameters.AddWithValue("@courseId", cmbCourses.SelectedValue); 
                    var dt = new DataTable(); 
                    adapter.Fill(dt); 
                    dgvStudentsForGrades.DataSource = dt; 
                    if (dgvStudentsForGrades.Columns["enrollment_id"] != null) dgvStudentsForGrades.Columns["enrollment_id"].Visible = false; 
                    dgvIndividualGrades.DataSource = null; 
                } 
            } 
            catch (Exception ex) { MessageBox.Show("Error al cargar estudiantes para calificaciones: " + ex.Message); 
            } 
        }
        private void LoadIndividualGrades() 
        { 
            if (selectedEnrollmentId == 0) return; 
            try 
            { 
                using (var conn = DBConnection.GetConnection()) 
                { 
                    var adapter = new MySqlDataAdapter("SELECT assessment_type AS Tipo, grade_value AS Nota, max_grade AS 'Nota Max', weight AS Peso, " +
                        "assessment_date AS Fecha FROM grades WHERE enrollment_id = @enrollmentId ORDER BY assessment_date DESC", conn); 
                    adapter.SelectCommand.Parameters.AddWithValue("@enrollmentId", selectedEnrollmentId); 
                    var dt = new DataTable(); 
                    adapter.Fill(dt); 
                    dgvIndividualGrades.DataSource = dt; 
                }
            } 
            catch (Exception ex) { MessageBox.Show("Error al cargar calificaciones individuales: " + ex.Message);
            } 
        }
        private void LoadAssignments() 
        { 
            if (cmbCourses.SelectedValue == null) return; 
            try 
            { 
                using (var conn = DBConnection.GetConnection()) 
                { 
                    var adapter = new MySqlDataAdapter("SELECT assignment_id, title AS Título, description AS Descripción, due_date AS 'Fecha de Entrega' FROM assignments " +
                        "WHERE course_id = @courseId ORDER BY due_date DESC", conn); 
                    adapter.SelectCommand.Parameters.AddWithValue("@courseId", cmbCourses.SelectedValue); 
                    var dt = new DataTable(); 
                    adapter.Fill(dt); 
                    dgvAssignments.DataSource = dt; 
                    if (dgvAssignments.Columns.Contains("assignment_id")) dgvAssignments.Columns["assignment_id"].Visible = false; 
                    if (dgvAssignments.Columns.Contains("description")) dgvAssignments.Columns["description"].Visible = false; 
                } 
            } 
            catch (Exception ex) { MessageBox.Show("Error al cargar tareas: " + ex.Message); 
            } 
        }
        private void DgvAssignments_SelectionChanged(object sender, EventArgs e) 
        { 
            if (dgvAssignments.CurrentRow == null) { dgvSubmissions.DataSource = null; return; } 
            int assignmentId = Convert.ToInt32(dgvAssignments.CurrentRow.Cells["assignment_id"].Value); 
            LoadSubmissionsForAssignment(assignmentId); 
        }
        private void LoadSubmissionsForAssignment(int assignmentId) 
        { 
            try 
            { 
                using (var conn = DBConnection.GetConnection()) 
                { 
                    var query = "SELECT s.submission_id, u.full_name AS Estudiante, s.status AS Estado, s.submission_date AS 'Fecha Entrega', s.grade AS Calificación " +
                        "FROM student_enrollments se JOIN users u ON se.student_id = u.user_id LEFT JOIN assignment_submissions s ON se.student_id = s.student_id AND " +
                        "s.assignment_id = @assignmentId WHERE se.course_id = @courseId ORDER BY u.full_name"; 
                    var adapter = new MySqlDataAdapter(query, conn); 
                    adapter.SelectCommand.Parameters.AddWithValue("@assignmentId", assignmentId); 
                    adapter.SelectCommand.Parameters.AddWithValue("@courseId", cmbCourses.SelectedValue);
                    var dt = new DataTable(); 
                    adapter.Fill(dt); 
                    dgvSubmissions.DataSource = dt; 
                    if (dgvSubmissions.Columns.Contains("submission_id")) dgvSubmissions.Columns["submission_id"].Visible = false; 
                } 
            } 
            catch (Exception ex) { MessageBox.Show("Error al cargar entregas: " + ex.Message); 
            } 
        }
        private void LoadStudentsForObservations() 
        { 
            if (cmbCourses.SelectedValue == null) return; 
            try 
            { 
                using (var conn = DBConnection.GetConnection()) 
                { 
                    var query = "SELECT se.enrollment_id, u.full_name AS Estudiante FROM users u JOIN student_enrollments se ON u.user_id = se.student_id WHERE " +
                        "se.course_id = @courseId"; 
                    var adapter = new MySqlDataAdapter(query, conn); 
                    adapter.SelectCommand.Parameters.AddWithValue("@courseId", cmbCourses.SelectedValue); 
                    var dt = new DataTable(); 
                    adapter.Fill(dt); 
                    dgvStudentsForObs.DataSource = dt; 
                    if (dgvStudentsForObs.Columns["enrollment_id"] != null) dgvStudentsForObs.Columns["enrollment_id"].Visible = false; 
                } 
            } 
            catch (Exception ex) { MessageBox.Show("Error al cargar estudiantes: " + ex.Message); 
            } 
        }
        
        private void BtnSaveAttendance_Click(object sender, EventArgs e) 
        { 
            DateTime selectedDate = dtpAttendanceDate.Value.Date; 
            try 
            { 
                using (var conn = DBConnection.GetConnection()) 
                { 
                    conn.Open(); 
                    foreach (DataGridViewRow row in dgvAttendance.Rows) 
                    { 
                        int enrollmentId = Convert.ToInt32(row.Cells["enrollment_id"].Value); 
                        string status = row.Cells["Status"].Value?.ToString() ?? "absent"; 
                        var cmd = new MySqlCommand("INSERT INTO attendance (enrollment_id, class_date, status, recorded_by) VALUES (@enrollmentId, @date, @status, " + conn); 
                        cmd.Parameters.AddWithValue("@enrollmentId", enrollmentId); 
                        cmd.Parameters.AddWithValue("@date", selectedDate); 
                        cmd.Parameters.AddWithValue("@status", status); 
                        cmd.Parameters.AddWithValue("@teacherId", this.currentTeacherId);
                        cmd.ExecuteNonQuery(); 
                    } 
                    MessageBox.Show("Asistencia guardada para la fecha " + selectedDate.ToShortDateString(), "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information); 
                } 
            } 
            catch (Exception ex) { MessageBox.Show("Error al guardar la asistencia: " + ex.Message); 
            } 
        }
        private void BtnSaveGrade_Click(object sender, EventArgs e) 
        { 
            if (selectedEnrollmentId == 0) { MessageBox.Show("Seleccione un estudiante primero.", "Error"); return; } 
            try 
            { 
                using (var conn = DBConnection.GetConnection()) 
                { 
                    conn.Open(); 
                    var cmd = new MySqlCommand("INSERT INTO grades (enrollment_id, assessment_type, grade_value, max_grade, weight, assessment_date, " +
                        "recorded_by) VALUES (@enrollmentId, @type, @value, @max, @weight, CURDATE(), @teacherId)", conn); 
                    cmd.Parameters.AddWithValue("@enrollmentId", selectedEnrollmentId); 
                    cmd.Parameters.AddWithValue("@type", cmbAssessmentType.SelectedItem.ToString()); 
                    cmd.Parameters.AddWithValue("@value", Convert.ToDecimal(txtGradeValue.Text)); 
                    cmd.Parameters.AddWithValue("@max", Convert.ToDecimal(txtMaxGrade.Text)); 
                    cmd.Parameters.AddWithValue("@weight", Convert.ToDecimal(txtWeight.Text)); 
                    cmd.Parameters.AddWithValue("@teacherId", this.currentTeacherId); 
                    cmd.ExecuteNonQuery(); 
                    MessageBox.Show("Calificación guardada.", "Éxito"); 
                    LoadIndividualGrades();
                    LoadStudentsForGrades(); 
                } 
            } 
            catch (Exception ex) { MessageBox.Show("Error al guardar calificación: " + ex.Message); } 
        }
        private void DgvStudentsForGrades_CellClick(object sender, DataGridViewCellEventArgs e) 
        { 
            if (e.RowIndex < 0) return; 
            selectedEnrollmentId = Convert.ToInt32(dgvStudentsForGrades.Rows[e.RowIndex].Cells["enrollment_id"].Value); 
            LoadIndividualGrades(); 
        }
        private void BtnCreateAssignment_Click(object sender, EventArgs e) 
        { 
            if (cmbCourses.SelectedValue == null) return; 
            try 
            { 
                using (var conn = DBConnection.GetConnection()) 
                { 
                    conn.Open(); 
                    var cmd = new MySqlCommand("INSERT INTO assignments (course_id, teacher_id, title, description, due_date) VALUES (@courseId, @teacherId, " +
                        "@title, @desc, @due)", conn);
                    cmd.Parameters.AddWithValue("@courseId", cmbCourses.SelectedValue); 
                    cmd.Parameters.AddWithValue("@teacherId", this.currentTeacherId);
                    cmd.Parameters.AddWithValue("@title", txtAssignmentTitle.Text); 
                    cmd.Parameters.AddWithValue("@desc", rtbAssignmentDescription.Text); 
                    cmd.Parameters.AddWithValue("@due", dtpDueDate.Value); 
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Tarea creada exitosamente.", "Éxito"); 
                    LoadAssignments(); 
                } 
            } 
            catch (Exception ex) { MessageBox.Show("Error al crear la tarea: " + ex.Message); } 
        }
        private void DgvStudentsForObs_CellClick(object sender, DataGridViewCellEventArgs e) 
        { 
            if (e.RowIndex < 0) return; 
            selectedEnrollmentIdForObs = Convert.ToInt32(dgvStudentsForObs.Rows[e.RowIndex].Cells["enrollment_id"].Value); 
            rtbObservation.Clear(); 
            btnSaveObservation.Enabled = true;
            try 
            { 
                using (var conn = DBConnection.GetConnection()) 
                { 
                    var cmd = new MySqlCommand("SELECT observation_text, observation_date FROM teacher_observations WHERE enrollment_id = @enrollmentId ORDER " +
                        "BY observation_date DESC", conn); 
                    cmd.Parameters.AddWithValue("@enrollmentId", selectedEnrollmentIdForObs); 
                    conn.Open(); 
                    using (var reader = cmd.ExecuteReader()) 
                    { 
                        while (reader.Read()) 
                        { 
                            rtbObservation.AppendText($"[{reader.GetDateTime("observation_date"):g}]\n{reader.GetString("observation_text")}\n\n---\n\n"); 
                        } 
                    } 
                } 
            } 
            catch (Exception ex) { MessageBox.Show("Error al cargar observaciones: " + ex.Message); 
            } 
        }
        private void BtnSaveObservation_Click(object sender, EventArgs e) 
        { 
            if (selectedEnrollmentIdForObs == 0 || string.IsNullOrWhiteSpace(rtbObservation.Text)) 
            { MessageBox.Show("Seleccione un estudiante y escriba una observación.", "Error"); return; } 
            try 
            { 
                using (var conn = DBConnection.GetConnection()) 
                { 
                    conn.Open(); 
                    var cmd = new MySqlCommand("INSERT INTO teacher_observations (enrollment_id, teacher_id, observation_text) VALUES (@enrollmentId, @teacherId, @text)", conn);
                    cmd.Parameters.AddWithValue("@enrollmentId", selectedEnrollmentIdForObs); 
                    cmd.Parameters.AddWithValue("@teacherId", this.currentTeacherId); 
                    cmd.Parameters.AddWithValue("@text", rtbObservation.Text); 
                    cmd.ExecuteNonQuery(); 
                    MessageBox.Show("Observación guardada."); 
                    DgvStudentsForObs_CellClick(dgvStudentsForObs, new DataGridViewCellEventArgs(dgvStudentsForObs.CurrentCell.ColumnIndex, dgvStudentsForObs.CurrentRow.Index));
                }
            } 
            catch (Exception ex) { MessageBox.Show("Error al guardar la observación: " + ex.Message); } 
        }
        private void DgvSubmissions_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        { 
            if (e.RowIndex < 0 || dgvSubmissions.CurrentRow.Cells["submission_id"].Value == DBNull.Value) 
            { 
                MessageBox.Show("Este estudiante aún no ha entregado la tarea.", "Sin Entrega", MessageBoxButtons.OK, MessageBoxIcon.Information); return; 
            } 
            int submissionId = Convert.ToInt32(dgvSubmissions.CurrentRow.Cells["submission_id"].Value); 
            frmMain mainForm = this.FindForm() as frmMain; 
            if (mainForm != null) { mainForm.NavigateTo_TeacherGrading(submissionId); } 
        }
    }
}