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
        private TextBox txtUsername, txtEmail, txtFullName, txtPassword, txtSpecificInfo;
        private ComboBox cmbRole;
        private CheckBox chkIsActive;
        private Button btnSave, btnDelete, btnClear, btnChangePassword;
        private Label lblSpecificInfo;
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
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(30)
            };
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
            var editPanel = new GroupBox
            {
                Text = "Detalles del Usuario",
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };
            mainLayout.Controls.Add(editPanel, 0, 1); 

            var fieldsLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4, 
                RowCount = 5
            };

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
                    var adapter = new MySqlDataAdapter("SELECT user_id, username, email, role, is_active AS Activo, full_name AS 'Nombre Completo', created_at AS 'Fecha Creación' FROM vw_user_roles ORDER BY user_id", connection);
                    var dt = new DataTable();
                    adapter.Fill(dt);
                    dgvUsers.DataSource = dt;
                    if (dgvUsers.Columns.Contains("user_id")) dgvUsers.Columns["user_id"].Visible = false;
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
            txtFullName.Text = row.Cells["Nombre Completo"].Value.ToString();
            cmbRole.SelectedItem = row.Cells["role"].Value.ToString();
            chkIsActive.Checked = Convert.ToBoolean(row.Cells["Activo"].Value);
            try
            {
                using (var connection = DBConnection.GetConnection())
                {
                    connection.Open();
                    var cmd = new MySqlCommand("sp_get_user_specific_info", connection) { CommandType = CommandType.StoredProcedure };
                    cmd.Parameters.AddWithValue("@p_user_id", selectedUserId);
                    object result = cmd.ExecuteScalar();
                    txtSpecificInfo.Text = result?.ToString() ?? "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar detalles del rol: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            txtPassword.Text = "";
            txtPassword.Enabled = true;
            btnSave.Text = "Actualizar";
        }

        private void CmbRole_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbRole.SelectedItem == null) return;
            string selectedRole = cmbRole.SelectedItem.ToString();
            switch (selectedRole)
            {
                case "admin": lblSpecificInfo.Text = "Teléfono:"; break;
                case "coordinator": lblSpecificInfo.Text = "Departamento:"; break;
                case "teacher": lblSpecificInfo.Text = "Especialización:"; break;
                case "student": lblSpecificInfo.Text = "Nivel de Grado:"; break;
                default: lblSpecificInfo.Text = ""; break;
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (cmbRole.SelectedItem == null)
            {
                MessageBox.Show("Por favor, seleccione un rol para el usuario.", "Rol Requerido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string roleSpecificJson = $"{{ \"{lblSpecificInfo.Text.Replace(":", "").ToLower().Replace(" ", "_")}\": \"{txtSpecificInfo.Text}\" }}";
            try
            {
                using (var connection = DBConnection.GetConnection())
                {
                    connection.Open();
                    MySqlCommand cmd;
                    if (selectedUserId == 0) 
                    {
                        cmd = new MySqlCommand("sp_register_user", connection);
                        cmd.Parameters.AddWithValue("@p_password", txtPassword.Text);
                        cmd.Parameters.AddWithValue("@p_role", cmbRole.SelectedItem.ToString());
                        cmd.Parameters.AddWithValue("@p_user_id", MySqlDbType.Int32).Direction = ParameterDirection.Output;
                    }
                    else
                    {
                        cmd = new MySqlCommand("sp_update_user", connection);
                        cmd.Parameters.AddWithValue("@p_user_id", selectedUserId);
                        cmd.Parameters.AddWithValue("@p_is_active", chkIsActive.Checked);
                        cmd.Parameters.AddWithValue("@p_new_role", cmbRole.SelectedItem.ToString());
                    }

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@p_username", txtUsername.Text);
                    cmd.Parameters.AddWithValue("@p_email", txtEmail.Text);
                    cmd.Parameters.AddWithValue("@p_full_name", txtFullName.Text);
                    cmd.Parameters.AddWithValue("@p_role_specific_data", roleSpecificJson);
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

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (selectedUserId == 0)
            {
                MessageBox.Show("Por favor, seleccione un usuario de la lista para eliminar.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (MessageBox.Show("¿Está seguro de que desea eliminar a este usuario de forma permanente?", "Confirmar Eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    using (var connection = DBConnection.GetConnection())
                    {
                        connection.Open();
                        var cmd = new MySqlCommand("sp_delete_user", connection) { CommandType = CommandType.StoredProcedure };
                        cmd.Parameters.AddWithValue("@p_user_id", selectedUserId);
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Usuario eliminado exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al eliminar el usuario: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    LoadUsers();
                    ClearForm();
                }
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
            if (MessageBox.Show($"¿Está seguro de que desea cambiar la contraseña para el usuario '{txtUsername.Text}'?", "Confirmar Cambio de Contraseña", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    using (var connection = DBConnection.GetConnection())
                    {
                        connection.Open();
                        var cmd = new MySqlCommand("sp_admin_change_password", connection) { CommandType = CommandType.StoredProcedure };
                        cmd.Parameters.AddWithValue("@p_user_id", selectedUserId);
                        cmd.Parameters.AddWithValue("@p_new_password", txtPassword.Text);
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Contraseña actualizada exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        txtPassword.Clear();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al cambiar la contraseña: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            selectedUserId = 0;
            txtUsername.Clear();
            txtEmail.Clear();
            txtPassword.Clear();
            txtFullName.Clear();
            txtSpecificInfo.Clear();
            cmbRole.SelectedIndex = -1;
            chkIsActive.Checked = true;
            txtPassword.Enabled = true;
            btnSave.Text = "Crear Usuario";
            if (dgvUsers.CurrentRow != null)
            {
                dgvUsers.CurrentRow.Selected = false;
            }
        }
    }
}