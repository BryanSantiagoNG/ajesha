using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ajesha
{
    public partial class ucCoordinatorPanel : UserControl
    {
        private int currentCoordinatorId;
        private TabControl mainTabControl;

        private DataGridView dgvStudents;
        private TextBox txtStudentName, txtStudentEmail, txtStudentPassword, txtStudentDob, txtStudentGrade;
        private Button btnSaveStudent, btnClearStudentForm;
        private int selectedStudentId = 0;

        private ComboBox cmbStudentsForEnrollment;
        private ListBox lstAvailableCourses, lstEnrolledCourses;
        private Button btnEnroll, btnUnenroll;

        private TextBox txtNotificationTitle;
        private RichTextBox rtbNotificationMessage;
        private ComboBox cmbNotificationAudience;
        private Button btnSendNotification;

        private Button btnSelectFile, btnUploadFile;
        private TextBox txtFileDescription, txtSelectedFilePath;
        private DataGridView dgvFiles;
        private const string FileDescriptionPlaceholder = "Descripción del archivo...";

        private readonly Color colorDarkBg = Color.FromArgb(45, 45, 48);
        private readonly Color colorMidBg = Color.FromArgb(51, 51, 55);
        private readonly Color colorLightBg = Color.FromArgb(63, 63, 70);
        private readonly Color colorFont = Color.White;
        private readonly Color colorGrayFont = Color.Gainsboro;
        private readonly Color colorBorder = Color.FromArgb(80, 80, 80);

        public ucCoordinatorPanel()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            this.BackColor = colorDarkBg;
            SetupUI();
            this.VisibleChanged += (sender, e) => { if (this.Visible) LoadInitialData(); };
        }
        private void ApplyDarkTheme(Control control)
        {
            if (control is TextBox || control is RichTextBox || control is ComboBox || control is ListBox)
            {
                control.BackColor = colorMidBg;
                control.ForeColor = colorFont;
            }
            if (control is DataGridView dgv)
            {
                dgv.BackgroundColor = colorMidBg;
                dgv.ForeColor = Color.Black;
                dgv.GridColor = colorDarkBg;
                dgv.BorderStyle = BorderStyle.FixedSingle;
                dgv.ColumnHeadersDefaultCellStyle.BackColor = colorLightBg;
                dgv.ColumnHeadersDefaultCellStyle.ForeColor = colorFont;
                dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = colorLightBg;
                dgv.EnableHeadersVisualStyles = false;
                dgv.DefaultCellStyle.BackColor = colorMidBg;
                dgv.DefaultCellStyle.ForeColor = colorFont;
                dgv.DefaultCellStyle.SelectionBackColor = Color.SteelBlue;
                dgv.DefaultCellStyle.SelectionForeColor = Color.White;
            }
            if (control is Button btn)
            {
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 1;
                btn.FlatAppearance.BorderColor = colorBorder;
                btn.BackColor = colorLightBg;
                btn.ForeColor = colorFont;
            }
            if (control is Label lbl)
            {
                lbl.ForeColor = colorFont;
            }
            if (control is GroupBox gb)
            {
                gb.ForeColor = colorFont;
            }
            if (control is TabControl tc)
            {
                tc.Appearance = TabAppearance.Normal;
                tc.DrawMode = TabDrawMode.OwnerDrawFixed;
                tc.DrawItem += TabControl_DrawItem;
            }
        }
        public void SetCoordinatorId(int coordinatorId)
        {
            this.currentCoordinatorId = coordinatorId;
        }
        private void TabControl_DrawItem(object sender, DrawItemEventArgs e)
        {
            TabPage page = mainTabControl.TabPages[e.Index];
            e.Graphics.FillRectangle(new SolidBrush(colorMidBg), e.Bounds);

            using (var brush = new SolidBrush(colorFont))
            {
                e.Graphics.DrawString(page.Text, this.Font, brush, e.Bounds.X + 3, e.Bounds.Y + 3);
            }
        }

        private void SetupUI()
        {
            mainTabControl = new TabControl { Dock = DockStyle.Fill };
            ApplyDarkTheme(mainTabControl);

            var tabStudents = new TabPage("Gestión de Estudiantes") { BackColor = colorDarkBg };
            SetupStudentManagementTab(tabStudents);

            var tabEnrollments = new TabPage("Inscripciones") { BackColor = colorDarkBg };
            SetupEnrollmentTab(tabEnrollments);

            var tabNotifications = new TabPage("Enviar Notificaciones") { BackColor = colorDarkBg };
            SetupNotificationsTab(tabNotifications);

            var tabFiles = new TabPage("Subir Archivos") { BackColor = colorDarkBg };
            SetupFilesTab(tabFiles);

            mainTabControl.TabPages.AddRange(new TabPage[] { tabStudents, tabEnrollments, tabNotifications, tabFiles });
            this.Controls.Add(mainTabControl);
        }
        private void SetupStudentManagementTab(TabPage parentTab)
        {
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(10) };
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 60F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));

            dgvStudents = new DataGridView { Dock = DockStyle.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AllowUserToAddRows = false };
            ApplyDarkTheme(dgvStudents);
            dgvStudents.CellClick += DgvStudents_CellClick;

            var editGroup = new GroupBox { Dock = DockStyle.Fill, Text = "Detalles del Estudiante" };
            ApplyDarkTheme(editGroup);
            var fieldsLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 3, Padding = new Padding(10) };

            fieldsLayout.Controls.Add(new Label { Text = "Nombre:", AutoSize = true, ForeColor = colorFont }, 0, 0);
            txtStudentName = new TextBox { Dock = DockStyle.Fill }; ApplyDarkTheme(txtStudentName);
            fieldsLayout.Controls.Add(txtStudentName, 1, 0);
            fieldsLayout.Controls.Add(new Label { Text = "Email:", AutoSize = true, ForeColor = colorFont }, 2, 0);
            txtStudentEmail = new TextBox { Dock = DockStyle.Fill }; ApplyDarkTheme(txtStudentEmail);
            fieldsLayout.Controls.Add(txtStudentEmail, 3, 0);
            fieldsLayout.Controls.Add(new Label { Text = "Contraseña:", AutoSize = true, ForeColor = colorFont }, 0, 1);
            txtStudentPassword = new TextBox { Dock = DockStyle.Fill }; ApplyDarkTheme(txtStudentPassword);
            fieldsLayout.Controls.Add(txtStudentPassword, 1, 1);
            fieldsLayout.Controls.Add(new Label { Text = "Nivel Grado:", AutoSize = true, ForeColor = colorFont }, 2, 1);
            txtStudentGrade = new TextBox { Dock = DockStyle.Fill }; ApplyDarkTheme(txtStudentGrade);
            fieldsLayout.Controls.Add(txtStudentGrade, 3, 1);
            fieldsLayout.Controls.Add(new Label { Text = "Fecha Nac. (YYYY-MM-DD):", AutoSize = true, ForeColor = colorFont }, 0, 2);
            txtStudentDob = new TextBox { Dock = DockStyle.Fill }; ApplyDarkTheme(txtStudentDob);
            fieldsLayout.Controls.Add(txtStudentDob, 1, 2);

            var buttonsPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            btnClearStudentForm = new Button { Text = "Limpiar", Width = 100, Height = 35 }; ApplyDarkTheme(btnClearStudentForm);
            btnClearStudentForm.Click += (s, e) => ClearStudentForm();
            btnSaveStudent = new Button { Text = "Crear Estudiante", Width = 150, Height = 35 }; ApplyDarkTheme(btnSaveStudent);
            btnSaveStudent.Click += BtnSaveStudent_Click;
            buttonsPanel.Controls.AddRange(new Control[] { btnClearStudentForm, btnSaveStudent });
            fieldsLayout.Controls.Add(buttonsPanel, 3, 2);

            editGroup.Controls.Add(fieldsLayout);
            layout.Controls.Add(dgvStudents, 0, 0);
            layout.Controls.Add(editGroup, 0, 1);
            parentTab.Controls.Add(layout);
        }
        private void SetupEnrollmentTab(TabPage parentTab) 
        {
            var layout = new TableLayoutPanel 
            { 
                Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 2, Padding = new Padding(20) 
            }; 
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F)); 
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); 
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F)); 
            layout.Controls.Add(new Label { Text = "Seleccionar Estudiante:", Dock = DockStyle.Top, AutoSize = true, ForeColor = Color.White }, 0, 0); 
            cmbStudentsForEnrollment = new ComboBox { Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList }; 
            cmbStudentsForEnrollment.SelectedIndexChanged += CmbStudentsForEnrollment_SelectedIndexChanged; 
            layout.Controls.Add(cmbStudentsForEnrollment, 0, 1); 
            var buttonsPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, Size = new Size(60, 150), Padding = new Padding(10) }; 
            btnEnroll = new Button { Text = ">", Width = 40 }; 
            btnEnroll.Click += BtnEnroll_Click; 
            btnUnenroll = new Button { Text = "<", Width = 40 }; 
            btnUnenroll.Click += BtnUnenroll_Click; 
            buttonsPanel.Controls.AddRange(new Control[] { btnEnroll, btnUnenroll }); 
            layout.Controls.Add(buttonsPanel, 1, 1); 
            layout.Controls.Add(new Label { Text = "Cursos Disponibles / Cursos Inscritos", Dock = DockStyle.Top, AutoSize = true, ForeColor = Color.White }, 2, 0); 
            var coursesLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            coursesLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F)); 
            coursesLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F)); 
            lstAvailableCourses = new ListBox { Dock = DockStyle.Fill, SelectionMode = SelectionMode.MultiExtended }; 
            lstEnrolledCourses = new ListBox { Dock = DockStyle.Fill }; 
            coursesLayout.Controls.Add(lstAvailableCourses, 0, 0); 
            coursesLayout.Controls.Add(lstEnrolledCourses, 1, 0); 
            layout.Controls.Add(coursesLayout, 2, 1); 
            parentTab.Controls.Add(layout); }
        private void SetupNotificationsTab(TabPage parentTab) 
        { 
            var layout = new TableLayoutPanel 
            { 
                Dock = DockStyle.Fill, Padding = new Padding(20), ColumnCount = 2, RowCount = 4 
            }; 
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); 
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); 
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); 
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); 
            layout.Controls.Add(new Label { Text = "Título:", AutoSize = true, ForeColor = Color.White }, 0, 0); 
            txtNotificationTitle = new TextBox { Dock = DockStyle.Fill }; layout.SetColumnSpan(txtNotificationTitle, 2); 
            layout.Controls.Add(txtNotificationTitle, 0, 1); 
            layout.Controls.Add(new Label { Text = "Mensaje:", AutoSize = true, ForeColor = Color.White }, 0, 2); 
            rtbNotificationMessage = new RichTextBox { Dock = DockStyle.Fill }; 
            layout.SetRowSpan(rtbNotificationMessage, 2); 
            layout.Controls.Add(rtbNotificationMessage, 0, 3); 
            layout.Controls.Add(new Label { Text = "Enviar a:", AutoSize = true, ForeColor = Color.White }, 1, 2); 
            var rightPanel = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 }; 
            rightPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F)); 
            rightPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F)); 
            cmbNotificationAudience = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList }; 
            cmbNotificationAudience.Items.AddRange(new string[] { "all", "students", "teachers" }); 
            cmbNotificationAudience.SelectedIndex = 0; btnSendNotification = new Button { Text = "Enviar Notificación", Height = 40, Dock = DockStyle.Fill }; 
            btnSendNotification.Click += BtnSendNotification_Click; 
            rightPanel.Controls.Add(cmbNotificationAudience, 0, 0); 
            rightPanel.Controls.Add(btnSendNotification, 0, 1); 
            layout.Controls.Add(rightPanel, 1, 3); 
            parentTab.Controls.Add(layout); 
        }
        private void SetupFilesTab(TabPage parentTab) 
        { 
            var layout = new TableLayoutPanel 
            {
                Dock = DockStyle.Fill, Padding = new Padding(20), RowCount = 3 
            }; 
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); 
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            var uploadPanel = new TableLayoutPanel 
            { 
                Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2 
            };
            uploadPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); 
            uploadPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            txtSelectedFilePath = new TextBox 
            { 
                Dock = DockStyle.Fill, ReadOnly = true
            }; 
            btnSelectFile = new Button 
            {
                Text = "Seleccionar Archivo...", AutoSize = true 
            }; 
            btnSelectFile.Click += BtnSelectFile_Click;
            uploadPanel.Controls.Add(txtSelectedFilePath, 0, 0); 
            uploadPanel.Controls.Add(btnSelectFile, 1, 0); 
            txtFileDescription = new TextBox 
            {
                Dock = DockStyle.Fill 
            }; 
            SetPlaceholder(); 
            txtFileDescription.Enter += TxtFileDescription_Enter; 
            txtFileDescription.Leave += TxtFileDescription_Leave; 
            btnUploadFile = new Button 
            { 
                Text = "Subir Archivo", AutoSize = true 
            }; 
            btnUploadFile.Click += BtnUploadFile_Click; 
            uploadPanel.Controls.Add(txtFileDescription, 0, 1); 
            uploadPanel.Controls.Add(btnUploadFile, 1, 1); 
            dgvFiles = new DataGridView 
            {
                Dock = DockStyle.Fill, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, AllowUserToAddRows = false 
            };
            layout.Controls.Add(uploadPanel, 0, 0); 
            layout.Controls.Add(dgvFiles, 0, 2); 
            parentTab.Controls.Add(layout); 
        }
        private void SetPlaceholder() 
        {
            txtFileDescription.Text = FileDescriptionPlaceholder; 
            txtFileDescription.ForeColor = Color.Gray;
        }
        private void TxtFileDescription_Enter(object sender, EventArgs e) 
        { 
            if (txtFileDescription.Text == FileDescriptionPlaceholder) 
            { 
                txtFileDescription.Text = ""; 
                txtFileDescription.ForeColor = Color.White; 
            }
        }
        private void TxtFileDescription_Leave(object sender, EventArgs e) 
        { 
            if (string.IsNullOrWhiteSpace(txtFileDescription.Text)) 
            { 
                SetPlaceholder(); 
            }
        }

        private void LoadInitialData() { LoadStudents(); LoadStudentsForEnrollment(); LoadAvailableCourses(); LoadUploadedFiles(); }
        private void LoadStudents() 
        { 
            try 
            { 
                using (var connection = DBConnection.GetConnection()) 
                { 
                    var adapter = new MySqlDataAdapter("SELECT user_id, full_name, email, current_grade_level, date_of_birth FROM users WHERE role = 'student' ORDER BY full_name", connection);
                    var dt = new DataTable(); 
                    adapter.Fill(dt); 
                    dgvStudents.DataSource = dt; 
                    if (dgvStudents.Columns.Contains("user_id")) dgvStudents.Columns["user_id"].Visible = false;
                } 
            } 
            catch (Exception ex) 
            {
                MessageBox.Show("Error al cargar estudiantes: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); 
            }
        }
        private void BtnSaveStudent_Click(object sender, EventArgs e) 
        { 
            if (string.IsNullOrWhiteSpace(txtStudentName.Text) || string.IsNullOrWhiteSpace(txtStudentEmail.Text)) 
            { 
                MessageBox.Show("El nombre y el email del estudiante son requeridos.", "Datos Incompletos", MessageBoxButtons.OK, MessageBoxIcon.Warning); 
                return; 
            } 
            if (selectedStudentId == 0 && string.IsNullOrWhiteSpace(txtStudentPassword.Text)) 
            { 
                MessageBox.Show("La contraseña es requerida para nuevos estudiantes.", "Datos Incompletos", MessageBoxButtons.OK, MessageBoxIcon.Warning); 
                return; 
            } 
            try 
            { 
                using (var connection = DBConnection.GetConnection()) 
                { 
                    connection.Open(); 
                    MySqlCommand cmd; 
                    if (selectedStudentId == 0) 
                    { 
                        cmd = new MySqlCommand("sp_register_user", connection); 
                        cmd.CommandType = CommandType.StoredProcedure; 
                        string json = $"{{ \"date_of_birth\": \"{txtStudentDob.Text}\", \"grade_level\": \"{txtStudentGrade.Text}\" }}"; 
                        cmd.Parameters.AddWithValue("@p_username", txtStudentEmail.Text); 
                        cmd.Parameters.AddWithValue("@p_password", txtStudentPassword.Text); 
                        cmd.Parameters.AddWithValue("@p_email", txtStudentEmail.Text); 
                        cmd.Parameters.AddWithValue("@p_role", "student"); 
                        cmd.Parameters.AddWithValue("@p_full_name", txtStudentName.Text); 
                        cmd.Parameters.AddWithValue("@p_role_specific_data", json); 
                        cmd.Parameters.AddWithValue("@p_user_id", MySqlDbType.Int32).Direction = ParameterDirection.Output; 
                    } 
                    else 
                    {
                        cmd = new MySqlCommand("UPDATE users SET full_name = @name, email = @email, current_grade_level = @grade, date_of_birth = @dob WHERE user_id = @id", connection); 
                        cmd.Parameters.AddWithValue("@name", txtStudentName.Text); 
                        cmd.Parameters.AddWithValue("@email", txtStudentEmail.Text); 
                        cmd.Parameters.AddWithValue("@grade", txtStudentGrade.Text); 
                        cmd.Parameters.AddWithValue("@dob", txtStudentDob.Text); 
                        cmd.Parameters.AddWithValue("@id", selectedStudentId); 
                    } 
                    cmd.ExecuteNonQuery(); 
                    MessageBox.Show("Estudiante guardado exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information); 
                } 
            } 
            catch (Exception ex) 
            { 
                MessageBox.Show("Error al guardar estudiante: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); 
            } 
            finally 
            {
                LoadStudents(); 
                ClearStudentForm(); 
            } 
        }
        private void DgvStudents_CellClick(object sender, DataGridViewCellEventArgs e) 
        { 
            if (e.RowIndex < 0) 
                return; 
            DataGridViewRow row = dgvStudents.Rows[e.RowIndex]; 
            selectedStudentId = Convert.ToInt32(row.Cells["user_id"].Value); 
            txtStudentName.Text = row.Cells["full_name"].Value.ToString(); 
            txtStudentEmail.Text = row.Cells["email"].Value.ToString(); 
            txtStudentGrade.Text = row.Cells["current_grade_level"].Value?.ToString(); 
            txtStudentDob.Text = Convert.ToDateTime(row.Cells["date_of_birth"].Value).ToString("yyyy-MM-dd"); 
            txtStudentPassword.Enabled = false;
            btnSaveStudent.Text = "Actualizar Estudiante"; 
        }
        private void ClearStudentForm() 
        {
            selectedStudentId = 0; 
            txtStudentName.Clear(); 
            txtStudentEmail.Clear();
            txtStudentPassword.Clear(); 
            txtStudentDob.Clear(); 
            txtStudentGrade.Clear(); 
            txtStudentPassword.Enabled = true; 
            btnSaveStudent.Text = "Crear Estudiante"; 
            dgvStudents.ClearSelection(); 
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
                }
            } 
            catch (Exception ex) 
            { 
                MessageBox.Show("Error al cargar lista de estudiantes: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); 
            }
        }
        private void LoadAvailableCourses() 
        { 
            try 
            { 
                using (var connection = DBConnection.GetConnection()) 
                { 
                    var adapter = new MySqlDataAdapter("SELECT course_id, course_name FROM courses WHERE is_active = TRUE ORDER BY course_name", connection); 
                    var dt = new DataTable(); 
                    adapter.Fill(dt); 
                    lstAvailableCourses.DataSource = dt; 
                    lstAvailableCourses.DisplayMember = "course_name"; 
                    lstAvailableCourses.ValueMember = "course_id"; 
                } 
            } 
            catch (Exception ex) 
            {
                MessageBox.Show("Error al cargar cursos disponibles: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); 
            } 
        }
        private void CmbStudentsForEnrollment_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbStudentsForEnrollment.SelectedItem == null)
            {
                lstEnrolledCourses.DataSource = null;
                return;
            }
            DataRowView drv = (DataRowView)cmbStudentsForEnrollment.SelectedItem;
            int studentId = Convert.ToInt32(drv["user_id"]);

            try
            {
                using (var connection = DBConnection.GetConnection())
                {
                    var query = "SELECT c.course_id, c.course_name FROM courses c JOIN student_enrollments se ON c.course_id = se.course_id WHERE se.student_id = @student_id";
                    var adapter = new MySqlDataAdapter(query, connection);
                    adapter.SelectCommand.Parameters.AddWithValue("@student_id", studentId);
                    var dt = new DataTable();
                    adapter.Fill(dt);
                    lstEnrolledCourses.DataSource = dt;
                    lstEnrolledCourses.DisplayMember = "course_name";
                    lstEnrolledCourses.ValueMember = "course_id";
                }
            }
            catch (Exception ex) { MessageBox.Show("Error al cargar cursos inscritos: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
        private void BtnEnroll_Click(object sender, EventArgs e)
        {
            if (cmbStudentsForEnrollment.SelectedItem == null || lstAvailableCourses.SelectedItems.Count == 0)
            {
                MessageBox.Show("Seleccione un estudiante y al menos un curso para inscribir.", "Selección Requerida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                        var cmd = new MySqlCommand("INSERT IGNORE INTO student_enrollments (student_id, course_id, teacher_id, academic_year, semester, enrollment_date) VALUES (@student_id, @course_id, 1, '2025', '1', CURDATE())", connection);
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
            if (cmbStudentsForEnrollment.SelectedItem == null || lstEnrolledCourses.SelectedItem == null) return;
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
                }
            }
            catch (Exception ex) 
            { 
                MessageBox.Show("Error al anular inscripción: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); 
            }
            finally 
            { 
                CmbStudentsForEnrollment_SelectedIndexChanged(null, null); 
            }
        }

        private void BtnSendNotification_Click(object sender, EventArgs e) 
        { 
            if (string.IsNullOrWhiteSpace(txtNotificationTitle.Text) || string.IsNullOrWhiteSpace(rtbNotificationMessage.Text)) 
            {
                MessageBox.Show("El título y el mensaje son requeridos.", "Datos Incompletos", MessageBoxButtons.OK, MessageBoxIcon.Warning); 
                return; 
            } try { 
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
                    txtNotificationTitle.Clear(); 
                    rtbNotificationMessage.Clear(); 
                } 
            } 
            catch (Exception ex) 
            { 
                MessageBox.Show("Error al enviar notificación: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); 
            } 
        }
        private void LoadUploadedFiles() 
        { 
            try 
            { using (var connection = DBConnection.GetConnection()) 
                { var adapter = new MySqlDataAdapter("SELECT file_id, file_name, description, file_type, uploaded_at FROM uploaded_files ORDER BY uploaded_at DESC", connection); 
                    var dt = new DataTable(); 
                    adapter.Fill(dt); 
                    dgvFiles.DataSource = dt; 
                } 
            } 
            catch (Exception ex) { MessageBox.Show("Error al cargar archivos: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); 
            } 
        }
        private void BtnSelectFile_Click(object sender, EventArgs e) 
        { 
            using (var ofd = new OpenFileDialog()) 
            { 
                ofd.Filter = "Todos los archivos (*.*)|*.*|Documentos PDF (*.pdf)|*.pdf|Imágenes (*.jpg;*.png)|*.jpg;*.png"; 
                if (ofd.ShowDialog() == DialogResult.OK) 
                { 
                    txtSelectedFilePath.Text = ofd.FileName; 
                } 
            } 
        }
        private void BtnUploadFile_Click(object sender, EventArgs e) { string filePath = txtSelectedFilePath.Text; string description = txtFileDescription.Text; 
            if (string.IsNullOrEmpty(filePath)) 
            { 
                MessageBox.Show("Por favor, seleccione un archivo para subir.", "Archivo no seleccionado", MessageBoxButtons.OK, MessageBoxIcon.Warning); 
                return; 
            } 
            if (description == FileDescriptionPlaceholder) 
            { 
                description = "";
            } 
            try 
            { 
                byte[] fileData = File.ReadAllBytes(filePath); 
                using (var connection = DBConnection.GetConnection()) 
                { 
                    connection.Open(); 
                    var cmd = new MySqlCommand("INSERT INTO uploaded_files (file_name, description, file_type, file_data, uploaded_by_user_id) VALUES (@name, @desc, @type, @data, @uploader)", connection); 
                    cmd.Parameters.AddWithValue("@name", Path.GetFileName(filePath)); 
                    cmd.Parameters.AddWithValue("@desc", description); 
                    cmd.Parameters.AddWithValue("@type", Path.GetExtension(filePath)); 
                    cmd.Parameters.Add("@data", MySqlDbType.LongBlob).Value = fileData; 
                    cmd.Parameters.AddWithValue("@uploader", this.currentCoordinatorId); 
                    cmd.ExecuteNonQuery(); MessageBox.Show("Archivo subido exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information); 
                } 
            } 
            catch (Exception ex) 
            { 
                MessageBox.Show("Error al subir archivo: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); 
            } 
            finally 
            {
                LoadUploadedFiles(); 
                txtSelectedFilePath.Clear(); 
                txtFileDescription.Clear(); 
                SetPlaceholder(); 
            } 
        }
    }
}