using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace ajesha
{
    public partial class ucCoordinator_StudentManagement : UserControl
    {
        // --- Controles de la Interfaz ---
        private DataGridView dgvStudents;
        private TextBox txtStudentName, txtStudentEmail, txtStudentPassword, txtStudentDob, txtStudentGrade;
        private Button btnSaveStudent, btnClearStudentForm;
        private TextBox txtSearchStudent;
        private int selectedStudentId = 0;

        // --- Paleta de Colores para el Tema Oscuro ---
        private readonly Color colorDarkBg = Color.FromArgb(45, 45, 48);
        private readonly Color colorMidBg = Color.FromArgb(51, 51, 55);
        private readonly Color colorLightBg = Color.FromArgb(63, 63, 70);
        private readonly Color colorFont = Color.White;
        private readonly Color colorBorder = Color.FromArgb(80, 80, 80);

        public ucCoordinator_StudentManagement()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            this.BackColor = colorDarkBg;
            SetupUI();
            this.VisibleChanged += (s, e) => { if (this.Visible) LoadStudents(); };
        }
        private void ApplyDarkTheme(Control control)
        {
            if (control is TextBox || control is RichTextBox || control is ComboBox || control is ListBox)
            {
                control.BackColor = colorMidBg;
                control.ForeColor = colorFont;
                if (control is TextBox txt) { txt.BorderStyle = BorderStyle.FixedSingle; }
                if (control is ComboBox cmb) { cmb.FlatStyle = FlatStyle.Flat; }
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
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(20) };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 55F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 45F));

            // --- Panel de Búsqueda ---
            var searchPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, AutoSize = true, Margin = new Padding(0, 0, 0, 10) };
            var lblSearch = new Label { Text = "Buscar Estudiante:", ForeColor = colorFont, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 5, 10, 0) };
            txtSearchStudent = new TextBox { Width = 300, Font = new Font("Segoe UI", 10F) };
            ApplyDarkTheme(txtSearchStudent);
            txtSearchStudent.TextChanged += TxtSearchStudent_TextChanged;
            searchPanel.Controls.AddRange(new Control[] { lblSearch, txtSearchStudent });
            layout.Controls.Add(searchPanel, 0, 0);

            // --- Tabla de Estudiantes ---
            dgvStudents = new DataGridView 
            { 
                Dock = DockStyle.Fill, 
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true, 
                SelectionMode = DataGridViewSelectionMode.FullRowSelect, 
                AllowUserToAddRows = false 
            };
            ApplyDarkTheme(dgvStudents);
            dgvStudents.CellClick += DgvStudents_CellClick;
            layout.Controls.Add(dgvStudents, 0, 1);

            // --- Panel de Edición ---
            var editGroup = new GroupBox { Dock = DockStyle.Fill, Text = "Detalles del Estudiante", ForeColor = Color.Gainsboro, Padding = new Padding(15) };
            var fieldsLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 3 };
            fieldsLayout.Controls.Add(new Label { Text = "Nombre:", AutoSize = true, ForeColor = colorFont, Anchor = AnchorStyles.Left, Margin = new Padding(0, 5, 0, 0) }, 0, 0);
            txtStudentName = new TextBox { Dock = DockStyle.Fill }; ApplyDarkTheme(txtStudentName);
            fieldsLayout.Controls.Add(txtStudentName, 1, 0);
            fieldsLayout.Controls.Add(new Label { Text = "Email:", AutoSize = true, ForeColor = colorFont, Anchor = AnchorStyles.Left, Margin = new Padding(10, 5, 0, 0) }, 2, 0);
            txtStudentEmail = new TextBox { Dock = DockStyle.Fill }; ApplyDarkTheme(txtStudentEmail);
            fieldsLayout.Controls.Add(txtStudentEmail, 3, 0);
            fieldsLayout.Controls.Add(new Label { Text = "Contraseña:", AutoSize = true, ForeColor = colorFont, Anchor = AnchorStyles.Left, Margin = new Padding(0, 5, 0, 0) }, 0, 1);
            txtStudentPassword = new TextBox { Dock = DockStyle.Fill, PasswordChar = '*' }; ApplyDarkTheme(txtStudentPassword);
            fieldsLayout.Controls.Add(txtStudentPassword, 1, 1);
            fieldsLayout.Controls.Add(new Label { Text = "Nivel Grado:", AutoSize = true, ForeColor = colorFont, Anchor = AnchorStyles.Left, Margin = new Padding(10, 5, 0, 0) }, 2, 1);
            txtStudentGrade = new TextBox { Dock = DockStyle.Fill }; ApplyDarkTheme(txtStudentGrade);
            fieldsLayout.Controls.Add(txtStudentGrade, 3, 1);
            fieldsLayout.Controls.Add(new Label { Text = "Fecha Nac. (YYYY-MM-DD):", AutoSize = true, ForeColor = colorFont, Anchor = AnchorStyles.Left, Margin = new Padding(0, 5, 0, 0) }, 0, 2);
            txtStudentDob = new TextBox { Dock = DockStyle.Fill }; ApplyDarkTheme(txtStudentDob);
            fieldsLayout.Controls.Add(txtStudentDob, 1, 2);
            var buttonsPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 5, 0, 0) };
            btnClearStudentForm = new Button { Text = "Limpiar", Width = 100, Height = 35 }; ApplyDarkTheme(btnClearStudentForm);
            btnClearStudentForm.Click += (s, e) => ClearStudentForm();
            btnSaveStudent = new Button { Text = "Crear Estudiante", Width = 150, Height = 35 }; ApplyDarkTheme(btnSaveStudent);
            btnSaveStudent.Click += BtnSaveStudent_Click;
            buttonsPanel.Controls.AddRange(new Control[] { btnClearStudentForm, btnSaveStudent });
            fieldsLayout.Controls.Add(buttonsPanel, 3, 2);
            editGroup.Controls.Add(fieldsLayout);
            layout.Controls.Add(editGroup, 0, 2);
            this.Controls.Add(layout);
        }
        private void TxtSearchStudent_TextChanged(object sender, EventArgs e)
        {
            LoadStudents();
        }

        private void LoadStudents()
        {
            try
            {
                using (var connection = DBConnection.GetConnection())
                {
                    string searchTerm = txtSearchStudent.Text.Trim();
                    string query = "SELECT user_id, full_name AS 'Nombre Completo', email AS Email, current_grade_level AS 'Nivel', date_of_birth AS 'Fecha de Nacimiento' FROM users WHERE role = 'student'";

                    if (!string.IsNullOrEmpty(searchTerm))
                    {
                        query += " AND (full_name LIKE @search OR email LIKE @search)";
                    }
                    query += " ORDER BY full_name";

                    var adapter = new MySqlDataAdapter(query, connection);
                    if (!string.IsNullOrEmpty(searchTerm))
                    {
                        adapter.SelectCommand.Parameters.AddWithValue("@search", $"%{searchTerm}%");
                    }

                    var dt = new DataTable();
                    adapter.Fill(dt);
                    dgvStudents.DataSource = dt;
                    if (dgvStudents.Columns.Contains("user_id")) dgvStudents.Columns["user_id"].Visible = false;
                }
            }
            catch (Exception ex) { MessageBox.Show("Error al cargar estudiantes: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
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
            string email = txtStudentEmail.Text.Trim();
            string username = email.Split('@')[0];
            username = System.Text.RegularExpressions.Regex.Replace(username, "[^a-zA-Z0-9_]", "");

            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show("El email proporcionado no es válido para generar un nombre de usuario.", "Email Inválido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

                        cmd.Parameters.AddWithValue("@p_username", username);
                        cmd.Parameters.AddWithValue("@p_password", txtStudentPassword.Text);
                        cmd.Parameters.AddWithValue("@p_email", email);
                        cmd.Parameters.AddWithValue("@p_role", "student");
                        cmd.Parameters.AddWithValue("@p_full_name", txtStudentName.Text);
                        cmd.Parameters.AddWithValue("@p_role_specific_data", json);
                        cmd.Parameters.AddWithValue("@p_user_id", MySqlDbType.Int32).Direction = ParameterDirection.Output;
                    }
                    else 
                    {
                        cmd = new MySqlCommand("UPDATE users SET full_name = @name, email = @email, username = @username, current_grade_level = @grade, date_of_birth = @dob WHERE user_id = @id", connection);
                        cmd.Parameters.AddWithValue("@name", txtStudentName.Text);
                        cmd.Parameters.AddWithValue("@email", email);
                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.Parameters.AddWithValue("@grade", txtStudentGrade.Text);
                        cmd.Parameters.AddWithValue("@dob", txtStudentDob.Text);
                        cmd.Parameters.AddWithValue("@id", selectedStudentId);
                    }
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Estudiante guardado exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex) { MessageBox.Show("Error al guardar estudiante: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            finally
            {
                LoadStudents();
                ClearStudentForm();
            }
        }


        private void DgvStudents_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow row = dgvStudents.Rows[e.RowIndex];
            selectedStudentId = Convert.ToInt32(row.Cells["user_id"].Value);
            txtStudentName.Text = row.Cells["Nombre Completo"].Value.ToString();
            txtStudentEmail.Text = row.Cells["Email"].Value.ToString();
            txtStudentGrade.Text = row.Cells["Nivel"].Value?.ToString();

            object dateValue = row.Cells["Fecha de Nacimiento"].Value;
            if (dateValue is DateTime)
            {
                DateTime dob = (DateTime)dateValue;
                txtStudentDob.Text = dob.ToString("yyyy-MM-dd");
            }
            else
            {
                txtStudentDob.Text = "";
            }

            txtStudentPassword.Enabled = false;
            txtStudentPassword.Clear();
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
            if (dgvStudents.CurrentRow != null)
            {
                dgvStudents.CurrentRow.Selected = false;
            }
        }

    }
}