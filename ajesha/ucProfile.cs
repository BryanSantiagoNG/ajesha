using MySql.Data.MySqlClient;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace ajesha
{
    public partial class ucProfile : UserControl
    {
        private Label lblFullNameValue;
        private Label lblEmailValue;
        private Label lblRoleValue;
        private Label lblSpecificInfoTitle;
        private Label lblSpecificInfoValue;

        public ucProfile()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(45, 45, 48);
            SetupUI();
        }
        private void SetupUI()
        {
            var contentPanel = new Panel
            {
                Location = new Point(40, 40),
                Size = new Size(700, 400),
                BackColor = this.BackColor
            };

            int currentTop = 0;
            int spacing = 40;

            var lblTitle = new Label { Text = "Información del Perfil", ForeColor = Color.White, Font = new Font("Segoe UI", 16F, FontStyle.Bold), Location = new Point(0, currentTop), AutoSize = true };
            currentTop += spacing + 10;

            var lblFullNameTitle = CreateTitleLabel("Nombre Completo:", currentTop);
            lblFullNameValue = CreateValueLabel("", currentTop);
            currentTop += spacing;

            var lblEmailTitle = CreateTitleLabel("Correo Electrónico:", currentTop);
            lblEmailValue = CreateValueLabel("", currentTop);
            currentTop += spacing;

            var lblRoleTitle = CreateTitleLabel("Rol de Usuario:", currentTop);
            lblRoleValue = CreateValueLabel("", currentTop);
            currentTop += spacing;

            lblSpecificInfoTitle = CreateTitleLabel("", currentTop);
            lblSpecificInfoValue = CreateValueLabel("", currentTop);

            contentPanel.Controls.AddRange(new Control[] {
                lblTitle, lblFullNameTitle, lblFullNameValue, lblEmailTitle, lblEmailValue,
                lblRoleTitle, lblRoleValue, lblSpecificInfoTitle, lblSpecificInfoValue
            });

            this.Controls.Add(contentPanel);
        }
        private Label CreateTitleLabel(string text, int top)
        {
            return new Label { Text = text, ForeColor = Color.White, Font = new Font("Segoe UI", 12F, FontStyle.Bold), Location = new Point(0, top), AutoSize = true };
        }
        private Label CreateValueLabel(string text, int top)
        {
            return new Label { Text = text, ForeColor = Color.Gainsboro, Font = new Font("Segoe UI", 12F, FontStyle.Regular), Location = new Point(250, top), AutoSize = true };
        }
        public void LoadProfileData(int userId, string userRole)
        {
            try
            {
                using (var connection = DBConnection.GetConnection())
                {
                    connection.Open();

                    var cmdBase = new MySqlCommand("SELECT full_name, email FROM vw_user_roles WHERE user_id = @id", connection);
                    cmdBase.Parameters.AddWithValue("@id", userId);

                    using (var reader = cmdBase.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            lblFullNameValue.Text = reader["full_name"].ToString();
                            lblEmailValue.Text = reader["email"].ToString();
                            lblRoleValue.Text = char.ToUpper(userRole[0]) + userRole.Substring(1);
                        }
                    }

                    string specificQuery = "";
                    string specificTitle = "";

                    switch (userRole.ToLower())
                    {
                        case "admin":
                            specificQuery = "SELECT phone FROM administrator WHERE user_id = @id";
                            specificTitle = "Teléfono:";
                            break;
                        case "teacher":
                            specificQuery = "SELECT specialization FROM teachers WHERE user_id = @id";
                            specificTitle = "Especialización:";
                            break;
                        case "student":
                            specificQuery = "SELECT current_grade_level FROM students WHERE user_id = @id";
                            specificTitle = "Nivel de Grado:";
                            break;
                        case "coordinator":
                            specificQuery = "SELECT department FROM coordinators WHERE user_id = @id";
                            specificTitle = "Departamento:";
                            break;
                    }

                    lblSpecificInfoTitle.Text = specificTitle;

                    if (!string.IsNullOrEmpty(specificQuery))
                    {
                        var cmdSpecific = new MySqlCommand(specificQuery, connection);
                        cmdSpecific.Parameters.AddWithValue("@id", userId);

                        object result = cmdSpecific.ExecuteScalar();

                        lblSpecificInfoValue.Text = (result != null && result != DBNull.Value) ? result.ToString() : "No disponible";
                    }
                    else
                    {
                        lblSpecificInfoValue.Text = "";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudieron cargar los datos del perfil. Error: " + ex.Message,
                    "Error de Base de Datos",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }
}