using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ajesha
{
    public partial class ucProfile : UserControl
    {
        private Label lblFullNameTitle, lblFullNameValue;
        private Label lblEmailTitle, lblEmailValue;
        private Label lblRoleTitle, lblRoleValue;

        private readonly Dictionary<string, Label> specificLabels = new Dictionary<string, Label>();

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
                Size = new Size(700, 500),
                AutoScroll = true 
            };

            int currentTop = 0;
            lblFullNameTitle = CreateTitleLabel("Nombre Completo:", currentTop);
            lblFullNameValue = CreateValueLabel("", currentTop);

            currentTop += 50;
            lblEmailTitle = CreateTitleLabel("Email:", currentTop);
            lblEmailValue = CreateValueLabel("", currentTop);

            currentTop += 50;
            lblRoleTitle = CreateTitleLabel("Rol de Usuario:", currentTop);
            lblRoleValue = CreateValueLabel("", currentTop);

            contentPanel.Controls.AddRange(new Control[] {
                lblFullNameTitle, lblFullNameValue, lblEmailTitle, lblEmailValue, lblRoleTitle, lblRoleValue
            });

            currentTop += 60;
            CreateSpecificRow("Teléfono:", "phone", currentTop, contentPanel);
            CreateSpecificRow("Departamento:", "department", currentTop, contentPanel);
            CreateSpecificRow("Especialización:", "specialization", currentTop, contentPanel);
            CreateSpecificRow("Fecha de Nacimiento:", "date_of_birth", currentTop, contentPanel);

            currentTop += 50;
            CreateSpecificRow("Fecha de Contratación:", "hire_date", currentTop, contentPanel);
            CreateSpecificRow("Nivel de Grado:", "current_grade_level", currentTop, contentPanel);

            this.Controls.Add(contentPanel);
        }
        private Label CreateTitleLabel(string text, int top) => new Label { Text = text, ForeColor = Color.White, Font = new Font("Segoe UI", 12F, FontStyle.Bold), Location = new Point(0, top), AutoSize = true };
        private Label CreateValueLabel(string text, int top) => new Label { Text = text, ForeColor = Color.Gainsboro, Font = new Font("Segoe UI", 12F, FontStyle.Regular), Location = new Point(250, top), AutoSize = true };

        private void CreateSpecificRow(string titleText, string key, int top, Panel parent)
        {
            var titleLabel = CreateTitleLabel(titleText, top);
            var valueLabel = CreateValueLabel("", top);
            titleLabel.Visible = false;
            valueLabel.Visible = false;

            specificLabels[key + "_title"] = titleLabel;
            specificLabels[key + "_value"] = valueLabel;

            parent.Controls.AddRange(new Control[] { titleLabel, valueLabel });
        }
        public void LoadProfileData(int userId, string userRole)
        {
            foreach (var label in specificLabels.Values) { label.Visible = false; }

            string query = "SELECT * FROM users WHERE user_id = @id";

            using (var connection = DBConnection.GetConnection())
            using (var cmd = new MySqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@id", userId);
                try
                {
                    connection.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {

                            lblFullNameValue.Text = reader["full_name"].ToString();
                            lblEmailValue.Text = reader["email"].ToString();
                            lblRoleValue.Text = char.ToUpper(userRole[0]) + userRole.Substring(1); 

                            switch (userRole.ToLower())
                            {
                                case "admin":
                                    ShowSpecificRow("phone", reader["phone"]?.ToString());
                                    break;
                                case "coordinator":
                                    ShowSpecificRow("department", reader["department"]?.ToString());
                                    break;
                                case "teacher":
                                    ShowSpecificRow("specialization", reader["specialization"]?.ToString());
                                    ShowSpecificRow("hire_date", FormatDate(reader["hire_date"]));
                                    break;
                                case "student":
                                    ShowSpecificRow("date_of_birth", FormatDate(reader["date_of_birth"]));
                                    ShowSpecificRow("current_grade_level", reader["current_grade_level"]?.ToString());
                                    break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al cargar los datos del perfil:\n" + ex.Message, "Error de Base de Datos", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void ShowSpecificRow(string key, string value)
        {
            if (string.IsNullOrEmpty(value)) return;

            if (specificLabels.TryGetValue(key + "_title", out Label titleLabel) &&
                specificLabels.TryGetValue(key + "_value", out Label valueLabel))
            {
                valueLabel.Text = value;
                titleLabel.Visible = true;
                valueLabel.Visible = true;
            }
        }
        private string FormatDate(object dbDate)
        {
            if (dbDate == null || dbDate == DBNull.Value) return null;
            return Convert.ToDateTime(dbDate).ToString("dd/MM/yyyy");
        }
    }
}