using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace ajesha
{
    public partial class ucAdminSistema : UserControl
    {
        private DataGridView dgvUsers;
        private TextBox txtUsername, txtEmail, txtFullName, txtPassword, txtSpecificInfo, txtSpecificInfo2;
        private ComboBox cmbRole;
        private CheckBox chkIsActive, chkMustChangePassword;
        private Button btnSave, btnDelete, btnClear, btnChangePassword;
        private Label lblSpecificInfo, lblSpecificInfo2;
        private int selectedUserId = 0;

        public ucAdminSistema()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(45, 45, 48);
            SetupUILayout();
            LoadUsers();
        }
        private void SetupUILayout()
        {
            var mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(30) };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            dgvUsers = new DataGridView 
            { 
                Dock = DockStyle.Fill, 
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize, 
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, 
                ReadOnly = true, 
                SelectionMode = DataGridViewSelectionMode.FullRowSelect, 
                AllowUserToAddRows = false, 
                MultiSelect = false 
            };
            dgvUsers.CellClick += DgvUsers_CellClick;
            mainLayout.Controls.Add(dgvUsers, 0, 0);
            var editPanel = new GroupBox { Text = "Detalles del Usuario", ForeColor = Color.White, Dock = DockStyle.Fill, Padding = new Padding(10) };
            mainLayout.Controls.Add(editPanel, 0, 1);
            var fieldsLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 5 };
            fieldsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            fieldsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            fieldsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            fieldsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            fieldsLayout.Controls.Add(new Label { Text = "Username:", ForeColor = Color.White, Anchor = AnchorStyles.Left, AutoSize = true }, 0, 0);
            txtUsername = new TextBox { Dock = DockStyle.Fill };
            fieldsLayout.Controls.Add(txtUsername, 1, 0);
            fieldsLayout.Controls.Add(new Label { Text = "Email:", ForeColor = Color.White, Anchor = AnchorStyles.Left, AutoSize = true }, 2, 0);
            txtEmail = new TextBox { Dock = DockStyle.Fill };
            fieldsLayout.Controls.Add(txtEmail, 3, 0);
            fieldsLayout.Controls.Add(new Label { Text = "Password:", ForeColor = Color.White, Anchor = AnchorStyles.Left, AutoSize = true }, 0, 1);
            txtPassword = new TextBox { Dock = DockStyle.Fill, PasswordChar = '*' };
            fieldsLayout.Controls.Add(txtPassword, 1, 1);
            fieldsLayout.Controls.Add(new Label { Text = "Rol:", ForeColor = Color.White, Anchor = AnchorStyles.Left, AutoSize = true }, 2, 1);
            cmbRole = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbRole.Items.AddRange(new string[] { "admin", "coordinator", "teacher", "student" });
            cmbRole.SelectedIndexChanged += CmbRole_SelectedIndexChanged;
            fieldsLayout.Controls.Add(cmbRole, 3, 1);
            fieldsLayout.Controls.Add(new Label { Text = "Nombre Completo:", ForeColor = Color.White, Anchor = AnchorStyles.Left, AutoSize = true }, 0, 2);
            txtFullName = new TextBox { Dock = DockStyle.Fill };
            fieldsLayout.Controls.Add(txtFullName, 1, 2);
            lblSpecificInfo = new Label { ForeColor = Color.White, Anchor = AnchorStyles.Left, AutoSize = true };
            fieldsLayout.Controls.Add(lblSpecificInfo, 2, 2);
            txtSpecificInfo = new TextBox { Dock = DockStyle.Fill };
            fieldsLayout.Controls.Add(txtSpecificInfo, 3, 2);
            lblSpecificInfo2 = new Label { ForeColor = Color.White, Anchor = AnchorStyles.Left, AutoSize = true };
            fieldsLayout.Controls.Add(lblSpecificInfo2, 0, 3);
            txtSpecificInfo2 = new TextBox { Dock = DockStyle.Fill };
            fieldsLayout.Controls.Add(txtSpecificInfo2, 1, 3);

            chkMustChangePassword = new CheckBox { Text = "Cambio de contraseña", ForeColor = Color.White, AutoSize = true, Checked = false };
            fieldsLayout.Controls.Add(chkMustChangePassword, 2, 3);

            chkIsActive = new CheckBox { Text = "Activo", ForeColor = Color.White, AutoSize = true, Checked = true };
            fieldsLayout.Controls.Add(chkIsActive, 3, 3);

            var buttonsPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
            btnSave = new Button { Text = "Crear Usuario", Size = new Size(120, 35) };
            btnDelete = new Button { Text = "Eliminar", Size = new Size(120, 35) };
            btnClear = new Button { Text = "Limpiar", Size = new Size(120, 35) };
            btnChangePassword = new Button { Text = "Cambiar Pass", Size = new Size(120, 35) };
            buttonsPanel.Controls.AddRange(new Control[] { btnSave, btnDelete, btnClear, btnChangePassword });
            fieldsLayout.Controls.Add(buttonsPanel, 0, 4);
            fieldsLayout.SetColumnSpan(buttonsPanel, 4);
            editPanel.Controls.Add(fieldsLayout);
            this.Controls.Add(mainLayout);
            btnSave.Click += BtnSave_Click;
            btnDelete.Click += BtnDelete_Click;
            btnClear.Click += BtnClear_Click;
            btnChangePassword.Click += BtnChangePassword_Click;
        }

        private void LoadUsers()
        {
            try
            {
                using (var connection = DBConnection.GetConnection())
                {
                    var adapter = new MySqlDataAdapter(
                        "SELECT user_id, username, email, role, full_name, is_active, must_change_password, " +
                        "phone, department, specialization, hire_date, date_of_birth, enrollment_date, current_grade_level " +
                        "FROM users ORDER BY user_id", connection);
                    var dt = new DataTable();
                    adapter.Fill(dt);
                    dgvUsers.DataSource = dt;
                    string[] colsToHide = { "user_id", "password_hash", "phone", "department", "specialization", "hire_date", "date_of_birth", "enrollment_date", "current_grade_level" };
                    foreach (string colName in colsToHide)
                    {
                        if (dgvUsers.Columns.Contains(colName)) dgvUsers.Columns[colName].Visible = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar usuarios: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvUsers_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow row = dgvUsers.Rows[e.RowIndex];
            selectedUserId = Convert.ToInt32(row.Cells["user_id"].Value);

            txtUsername.Text = row.Cells["username"].Value.ToString();
            txtEmail.Text = row.Cells["email"].Value.ToString();
            txtFullName.Text = row.Cells["full_name"].Value.ToString();
            cmbRole.SelectedItem = row.Cells["role"].Value.ToString();
            chkIsActive.Checked = Convert.ToBoolean(row.Cells["is_active"].Value);

            chkMustChangePassword.Checked = Convert.ToBoolean(row.Cells["must_change_password"].Value);

            ClearSpecificFields();
            string role = cmbRole.SelectedItem.ToString();
            switch (role)
            {
                case "admin": txtSpecificInfo.Text = row.Cells["phone"].Value?.ToString(); break;
                case "coordinator": txtSpecificInfo.Text = row.Cells["department"].Value?.ToString(); break;
                case "teacher":
                    txtSpecificInfo.Text = row.Cells["specialization"].Value?.ToString();
                    txtSpecificInfo2.Text = FormatDate(row.Cells["hire_date"].Value);
                    break;
                case "student":
                    txtSpecificInfo.Text = row.Cells["current_grade_level"].Value?.ToString();
                    txtSpecificInfo2.Text = FormatDate(row.Cells["date_of_birth"].Value);
                    break;
            }

            txtPassword.Clear();
            txtPassword.Enabled = false;
            btnSave.Text = "Actualizar";
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtEmail.Text) || string.IsNullOrWhiteSpace(txtFullName.Text) || cmbRole.SelectedItem == null)
            {
                MessageBox.Show("Por favor, complete todos los campos requeridos (Username, Email, Nombre, Rol).", 
                    "Campos Requeridos", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (var connection = DBConnection.GetConnection())
                {
                    connection.Open();
                    MySqlCommand cmd;
                    if (selectedUserId == 0) 
                    {
                        if (string.IsNullOrWhiteSpace(txtPassword.Text))
                        {
                            MessageBox.Show("La contraseña es requerida para nuevos usuarios.", "Contraseña Requerida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        cmd = new MySqlCommand(
                            "INSERT INTO users (username, password_hash, email, role, full_name, is_active, must_change_password, phone, department, " +
                            "specialization, hire_date, date_of_birth, enrollment_date, current_grade_level) " +
                            "VALUES (@username, @password, @email, @role, @full_name, @is_active, @must_change_password, @phone, " +
                            "@department, @specialization, @hire_date, @date_of_birth, @enrollment_date, @current_grade_level)", connection);
                        cmd.Parameters.AddWithValue("@password", txtPassword.Text);
                    }
                    else 
                    {
                        cmd = new MySqlCommand(
                            "UPDATE users SET username=@username, email=@email, role=@role, full_name=@full_name, is_active=@is_active, " +
                            "must_change_password=@must_change_password, phone=@phone, department=@department, " +
                            "specialization=@specialization, hire_date=@hire_date, date_of_birth=@date_of_birth, current_grade_level=@current_grade_level " +
                            "WHERE user_id=@user_id", connection);
                        cmd.Parameters.AddWithValue("@user_id", selectedUserId);
                    }

                    cmd.Parameters.AddWithValue("@username", txtUsername.Text);
                    cmd.Parameters.AddWithValue("@email", txtEmail.Text);
                    cmd.Parameters.AddWithValue("@role", cmbRole.SelectedItem.ToString());
                    cmd.Parameters.AddWithValue("@full_name", txtFullName.Text);
                    cmd.Parameters.AddWithValue("@is_active", chkIsActive.Checked);
                    cmd.Parameters.AddWithValue("@must_change_password", chkMustChangePassword.Checked);

                    string role = cmbRole.SelectedItem.ToString();
                    cmd.Parameters.AddWithValue("@phone", role == "admin" ? (object)txtSpecificInfo.Text : DBNull.Value);
                    cmd.Parameters.AddWithValue("@department", role == "coordinator" ? (object)txtSpecificInfo.Text : DBNull.Value);
                    cmd.Parameters.AddWithValue("@specialization", role == "teacher" ? (object)txtSpecificInfo.Text : DBNull.Value);
                    cmd.Parameters.AddWithValue("@hire_date", (role == "teacher" && !string.IsNullOrWhiteSpace(txtSpecificInfo2.Text)) ? (object)txtSpecificInfo2.Text : DBNull.Value);
                    cmd.Parameters.AddWithValue("@current_grade_level", role == "student" ? (object)txtSpecificInfo.Text : DBNull.Value);
                    cmd.Parameters.AddWithValue("@date_of_birth", (role == "student" && !string.IsNullOrWhiteSpace(txtSpecificInfo2.Text)) ? (object)txtSpecificInfo2.Text : DBNull.Value);
                    cmd.Parameters.AddWithValue("@enrollment_date", role == "student" ? (object)DateTime.Now : DBNull.Value);

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Usuario guardado exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar el usuario: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                LoadUsers();
                ClearForm();
            }
        }

        private void ClearForm()
        {
            selectedUserId = 0;
            txtUsername.Clear();
            txtEmail.Clear();
            txtPassword.Clear();
            txtFullName.Clear();
            ClearSpecificFields();
            cmbRole.SelectedIndex = -1;
            chkIsActive.Checked = true;
            chkMustChangePassword.Checked = false;
            txtPassword.Enabled = true;
            btnSave.Text = "Crear Usuario";
            if (dgvUsers.CurrentRow != null)
            {
                dgvUsers.CurrentRow.Selected = false;
            }
        }
        private string FormatDate(object dbDate) { if (dbDate == null || dbDate == DBNull.Value) return null; return Convert.ToDateTime(dbDate).ToString("yyyy-MM-dd"); }
        private void CmbRole_SelectedIndexChanged(object sender, EventArgs e) 
        { 
            ClearSpecificFields(); 
            if (cmbRole.SelectedItem == null) 
                return; 
            string selectedRole = cmbRole.SelectedItem.ToString(); 
            lblSpecificInfo.Visible = true; 
            txtSpecificInfo.Visible = true; 
            lblSpecificInfo2.Visible = false; 
            txtSpecificInfo2.Visible = false; 
            switch (selectedRole) 
            { 
                case "admin": lblSpecificInfo.Text = "Teléfono:"; break; 
                case "coordinator": lblSpecificInfo.Text = "Departamento:"; break; 
                case "teacher": 
                    lblSpecificInfo.Text = "Especialización:";
                    lblSpecificInfo2.Text = "Fecha Contratación (YYYY-MM-DD):"; 
                    lblSpecificInfo2.Visible = true; 
                    txtSpecificInfo2.Visible = true; 
                    break; 
                case "student": 
                    lblSpecificInfo.Text = "Nivel de Grado:";
                    lblSpecificInfo2.Text = "Fecha Nacimiento (YYYY-MM-DD):"; 
                    lblSpecificInfo2.Visible = true; 
                    txtSpecificInfo2.Visible = true; 
                    break; 
                default: 
                    lblSpecificInfo.Visible = false; 
                    txtSpecificInfo.Visible = false; 
                    break; 
            } 
        }
        private void BtnDelete_Click(object sender, EventArgs e) 
        { 
            if (selectedUserId == 0) 
            {
                MessageBox.Show("Por favor, seleccione un usuario de la lista para eliminar.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning); 
                return; 
            } 
            if (MessageBox.Show("¿Está seguro de que desea eliminar a este usuario? Esta acción no se puede deshacer.", 
                "Confirmar Eliminación", 
                MessageBoxButtons.YesNo, 
                MessageBoxIcon.Question) == DialogResult.Yes) 
            { 
                try 
                { 
                    using (var connection = DBConnection.GetConnection()) 
                    { 
                        connection.Open();
                        var cmd = new MySqlCommand("DELETE FROM users WHERE user_id = @user_id", connection); 
                        cmd.Parameters.AddWithValue("@user_id", selectedUserId); 
                        cmd.ExecuteNonQuery(); 
                        MessageBox.Show("Usuario eliminado exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information); 
                    } 
                }
                catch (Exception ex) 
                { 
                    MessageBox.Show("Error al eliminar el usuario: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); 
                } 
                finally { LoadUsers(); ClearForm(); } 
            } 
        }
        private void BtnChangePassword_Click(object sender, EventArgs e) 
        {
            if (selectedUserId == 0) 
            {
                MessageBox.Show("Por favor, seleccione un usuario de la lista.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning); 
                return; 
            } 
            if (string.IsNullOrWhiteSpace(txtPassword.Text)) 
            { 
                MessageBox.Show("Por favor, ingrese la nueva contraseña en el campo 'Password'.", "Contraseña Vacía", MessageBoxButtons.OK, MessageBoxIcon.Warning); 
                return; 
            } 
            if (MessageBox.Show($"¿Está seguro de que desea cambiar la contraseña para el usuario '{txtUsername.Text}'?", 
                "Confirmar Cambio de Contraseña", 
                MessageBoxButtons.YesNo, 
                MessageBoxIcon.Question) == DialogResult.Yes) 
            { 
                try 
                { 
                    using (var connection = DBConnection.GetConnection()) 
                    { 
                        connection.Open(); 
                        var cmd = new MySqlCommand("UPDATE users SET password_hash = @new_password, must_change_password = FALSE WHERE user_id = @user_id", connection);
                        cmd.Parameters.AddWithValue("@user_id", selectedUserId); 
                        cmd.Parameters.AddWithValue("@new_password", txtPassword.Text); 
                        cmd.ExecuteNonQuery(); 
                        MessageBox.Show("Contraseña actualizada exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information); 
                        txtPassword.Clear(); 
                        txtPassword.Enabled = false;
                    } 
                } 
                catch (Exception ex) { MessageBox.Show("Error al cambiar la contraseña: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); } } }
        private void BtnClear_Click(object sender, EventArgs e) { ClearForm(); }
        private void ClearSpecificFields() 
        { 
            txtSpecificInfo.Clear(); 
            txtSpecificInfo2.Clear(); 
            lblSpecificInfo.Text = ""; 
            lblSpecificInfo2.Text = ""; 
            lblSpecificInfo.Visible = false; 
            txtSpecificInfo.Visible = false; 
            lblSpecificInfo2.Visible = false; 
            txtSpecificInfo2.Visible = false; 
        }
    }
}