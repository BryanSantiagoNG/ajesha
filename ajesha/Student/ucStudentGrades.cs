using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace ajesha
{
    public partial class ucStudentGrades : UserControl
    {
        private int currentStudentId;
        private DataGridView dgvGradesSummary;

        public ucStudentGrades()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(45, 45, 48);
            SetupUI();
            this.VisibleChanged += (s, e) => { if (this.Visible) LoadGradesSummary(); };
        }

        public void SetStudentId(int studentId) { this.currentStudentId = studentId; }

        private void SetupUI()
        {
            var mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(20), RowCount = 2 };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var lblTitle = new Label { Text = "Resumen de Calificaciones", ForeColor = Color.White, Font = new Font("Segoe UI", 14F, FontStyle.Bold), Dock = DockStyle.Top, AutoSize = true };
            dgvGradesSummary = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };

            mainLayout.Controls.Add(lblTitle, 0, 0);
            mainLayout.Controls.Add(dgvGradesSummary, 0, 1);
            this.Controls.Add(mainLayout);
        }

        private void LoadGradesSummary()
        {
            try
            {
                using (var conn = DBConnection.GetConnection())
                {
                    var query = "SELECT course_name AS Curso, course_average AS 'Promedio Actual', final_grade AS 'Calificación Final' FROM vw_student_grades WHERE student_id = @studentId";
                    var adapter = new MySqlDataAdapter(query, conn);
                    adapter.SelectCommand.Parameters.AddWithValue("@studentId", this.currentStudentId);
                    var dt = new DataTable();
                    adapter.Fill(dt);
                    dgvGradesSummary.DataSource = dt;
                }
            }
            catch (Exception ex) { MessageBox.Show("Error al cargar calificaciones: " + ex.Message); }
        }
    }
}