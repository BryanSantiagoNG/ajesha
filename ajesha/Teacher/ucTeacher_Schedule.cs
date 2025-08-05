using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace ajesha
{
    public partial class ucTeacher_Schedule : UserControl
    {
        private int currentTeacherId;
        private DataGridView dgvSchedule;

        public ucTeacher_Schedule()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(45, 45, 48);
            SetupUI();
            this.VisibleChanged += (s, e) => { if (this.Visible) LoadSchedule(); };
        }

        public void SetTeacherId(int teacherId) { this.currentTeacherId = teacherId; }

        private void SetupUI()
        {
            var mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(20), RowCount = 2 };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            mainLayout.Controls.Add(new Label 
            { 
                Text = "Mi Horario y Cursos Asignados", 
                Font = new Font("Segoe UI", 16F, FontStyle.Bold), 
                ForeColor = Color.White, 
                AutoSize = true, 
                Margin = new Padding(0, 0, 0, 20) 
            }, 0, 0);

            dgvSchedule = new DataGridView 
            { 
                Dock = DockStyle.Fill,
                ReadOnly = true, 
                AllowUserToAddRows = false, 
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill 
            };

            mainLayout.Controls.Add(dgvSchedule, 0, 1);
            this.Controls.Add(mainLayout);
        }

        private void LoadSchedule()
        {
            try
            {
                using (var conn = DBConnection.GetConnection())
                {
                    var query = "SELECT course_name AS Curso, academic_year AS 'Año Académico', semester AS Semestre, schedule AS Horario, " +
                        "classroom AS Salón FROM vw_courses_with_teachers WHERE teacher_id = @teacherId";
                    var adapter = new MySqlDataAdapter(query, conn);
                    adapter.SelectCommand.Parameters.AddWithValue("@teacherId", this.currentTeacherId);
                    var dt = new DataTable();
                    adapter.Fill(dt);
                    dgvSchedule.DataSource = dt;
                }
            }
            catch (Exception ex) { MessageBox.Show("Error al cargar el horario: " + ex.Message); }
        }
    }
}