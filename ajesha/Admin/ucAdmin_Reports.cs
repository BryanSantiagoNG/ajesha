using MySql.Data.MySqlClient;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace ajesha
{
    public partial class ucAdmin_Reports : UserControl
    {
        // --- Controles para mostrar las estadísticas ---
        private Label lblTotalUsers, lblTotalAdmins, lblTotalCoordinators, lblTotalTeachers, lblTotalStudents;
        private Label lblTotalCourses, lblActiveCourses;

        // --- Paleta de Colores ---
        private readonly Color colorDarkBg = Color.FromArgb(45, 45, 48);
        private readonly Color colorMidBg = Color.FromArgb(51, 51, 55);
        private readonly Color colorFont = Color.White;
        private readonly Color colorAccent = Color.SteelBlue;

        public ucAdmin_Reports()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            this.BackColor = colorDarkBg;
            SetupUI();
            this.VisibleChanged += (s, e) => { if (this.Visible) LoadGlobalStats(); };
        }
        private Panel CreateStatPanel(string title, out Label valueLabel)
        {
            var panel = new Panel
            {
                BackColor = colorMidBg,
                Size = new Size(200, 100),
                Margin = new Padding(15)
            };

            var lblTitle = new Label
            {
                Text = title,
                ForeColor = Color.Gainsboro,
                Font = new Font("Segoe UI", 10F),
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.TopCenter,
                Padding = new Padding(5)
            };

            valueLabel = new Label
            {
                Text = "0",
                ForeColor = colorAccent,
                Font = new Font("Segoe UI", 24F, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };

            panel.Controls.Add(valueLabel);
            panel.Controls.Add(lblTitle);
            return panel;
        }
        private void SetupUI()
        {
            var mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(30), RowCount = 2 };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            mainLayout.Controls.Add(new Label 
            { 
                Text = "Reportes y Estadísticas Globales", 
                Font = new Font("Segoe UI", 16F, FontStyle.Bold), 
                ForeColor = colorFont, 
                AutoSize = true, 
                Margin = new Padding(0, 0, 0, 20) 
            }, 0, 0);

            var statsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoScroll = true
            };

            // Crear widgets de estadísticas de usuarios
            var usersGroup = new GroupBox { Text = "Usuarios del Sistema", ForeColor = colorFont, Width = 700, Height = 150, Padding = new Padding(10) };
            var usersFlow = new FlowLayoutPanel { Dock = DockStyle.Fill };
            usersFlow.Controls.Add(CreateStatPanel("Usuarios Totales", out lblTotalUsers));
            usersFlow.Controls.Add(CreateStatPanel("Administradores", out lblTotalAdmins));
            usersFlow.Controls.Add(CreateStatPanel("Coordinadores", out lblTotalCoordinators));
            usersFlow.Controls.Add(CreateStatPanel("Maestros", out lblTotalTeachers));
            usersFlow.Controls.Add(CreateStatPanel("Estudiantes", out lblTotalStudents));
            usersGroup.Controls.Add(usersFlow);

            // Crear widgets de estadísticas de cursos
            var coursesGroup = new GroupBox { Text = "Información Académica", ForeColor = colorFont, Width = 450, Height = 150, Padding = new Padding(10) };
            var coursesFlow = new FlowLayoutPanel { Dock = DockStyle.Fill };
            coursesFlow.Controls.Add(CreateStatPanel("Cursos Totales", out lblTotalCourses));
            coursesFlow.Controls.Add(CreateStatPanel("Cursos Activos", out lblActiveCourses));
            coursesGroup.Controls.Add(coursesFlow);

            statsPanel.Controls.Add(usersGroup);
            statsPanel.Controls.Add(coursesGroup);

            mainLayout.Controls.Add(statsPanel, 0, 1);
            this.Controls.Add(mainLayout);
        }
        private void LoadGlobalStats()
        {
            try
            {
                using (var conn = DBConnection.GetConnection())
                {
                    conn.Open();

                    // Resetear contadores a cero antes de consultar
                    int total = 0, admins = 0, coords = 0, teachers = 0, students = 0;
                    lblTotalUsers.Text = "0";
                    lblTotalAdmins.Text = "0";
                    lblTotalCoordinators.Text = "0";
                    lblTotalTeachers.Text = "0";
                    lblTotalStudents.Text = "0";

                    // Contar usuarios por rol
                    var cmdUsers = new MySqlCommand("SELECT role, COUNT(*) as count FROM users GROUP BY role", conn);
                    using (var reader = cmdUsers.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string role = reader["role"].ToString();
                            int count = Convert.ToInt32(reader["count"]);
                            total += count;
                            switch (role)
                            {
                                case "admin": admins = count; break;
                                case "coordinator": coords = count; break;
                                case "teacher": teachers = count; break;
                                case "student": students = count; break;
                            }
                        }
                    } // El reader se cierra aquí

                    // Actualizar las etiquetas de usuarios
                    lblTotalUsers.Text = total.ToString();
                    lblTotalAdmins.Text = admins.ToString();
                    lblTotalCoordinators.Text = coords.ToString();
                    lblTotalTeachers.Text = teachers.ToString();
                    lblTotalStudents.Text = students.ToString();

                    // Contar cursos
                    var cmdCourses = new MySqlCommand("SELECT COUNT(*) FROM courses", conn);
                    lblTotalCourses.Text = cmdCourses.ExecuteScalar().ToString();

                    cmdCourses.CommandText = "SELECT COUNT(*) FROM courses WHERE is_active = TRUE";
                    lblActiveCourses.Text = cmdCourses.ExecuteScalar().ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar las estadísticas globales: " + ex.Message, "Error de Base de Datos", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}