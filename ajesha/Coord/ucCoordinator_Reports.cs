using MySql.Data.MySqlClient;
using Mysqlx;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace ajesha
{
    public partial class ucCoordinator_Reports : UserControl
    {
        private ComboBox cmbCoursesForReport;
        private Button btnExportAttendance;

        // --- Paleta de Colores ---
        private readonly Color colorDarkBg = Color.FromArgb(45, 45, 48);
        private readonly Color colorMidBg = Color.FromArgb(51, 51, 55);
        private readonly Color colorLightBg = Color.FromArgb(63, 63, 70);
        private readonly Color colorFont = Color.White;
        private readonly Color colorBorder = Color.FromArgb(80, 80, 80);

        public ucCoordinator_Reports()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            this.BackColor = colorDarkBg;
            SetupUI();
            this.VisibleChanged += (s, e) => { if (this.Visible) LoadAllCourses(); };
        }
        private void ApplyDarkTheme(Control control)
        {
            if (control is ComboBox)
            {
                control.BackColor = colorMidBg;
                control.ForeColor = colorFont;
                ((ComboBox)control).FlatStyle = FlatStyle.Flat;
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
            var mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(30), RowCount = 3 };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            mainLayout.Controls.Add(new Label { Text = "Generación de Reportes", Font = new Font("Segoe UI", 16F, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, Margin = new Padding(0, 0, 0, 20) }, 0, 0);

            var reportGroup = new GroupBox { Dock = DockStyle.Top, Text = "Reporte de Asistencia por Curso", ForeColor = Color.Gainsboro, Padding = new Padding(20), AutoSize = true };
            var reportLayout = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, AutoSize = true };

            reportLayout.Controls.Add(new Label { Text = "Seleccionar Curso:", ForeColor = colorFont, AutoSize = true, Margin = new Padding(0, 6, 10, 0) });
            cmbCoursesForReport = new ComboBox { Width = 350, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10F) };
            ApplyDarkTheme(cmbCoursesForReport);

            btnExportAttendance = new Button { Text = "Exportar a CSV (Excel)", Width = 180, Height = 35, Margin = new Padding(20, 0, 0, 0) };
            ApplyDarkTheme(btnExportAttendance);
            btnExportAttendance.Click += BtnExportAttendance_Click;

            reportLayout.Controls.Add(cmbCoursesForReport);
            reportLayout.Controls.Add(btnExportAttendance);
            reportGroup.Controls.Add(reportLayout);
            mainLayout.Controls.Add(reportGroup, 0, 1);

            mainLayout.Controls.Add(new Panel(), 0, 2);

            this.Controls.Add(mainLayout);
        }
        private void LoadAllCourses()
        {
            try
            {
                using (var conn = DBConnection.GetConnection())
                {
                    var adapter = new MySqlDataAdapter("SELECT course_id, course_name FROM courses WHERE is_active = TRUE ORDER BY course_name", conn);
                    var dt = new DataTable();
                    adapter.Fill(dt);
                    cmbCoursesForReport.DataSource = dt;
                    cmbCoursesForReport.DisplayMember = "course_name";
                    cmbCoursesForReport.ValueMember = "course_id";
                    cmbCoursesForReport.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar la lista de cursos: " + ex.Message, "Error de Base de Datos", MessageBoxButtons.OK, MessageBoxIcon.Error); }
         }

        private void BtnExportAttendance_Click(object sender, EventArgs e)
        {
            if (cmbCoursesForReport.SelectedValue == null || !(cmbCoursesForReport.SelectedValue is int))
            {
                MessageBox.Show("Por favor, seleccione un curso para generar el reporte.", "Selección Requerida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            int courseId = Convert.ToInt32(cmbCoursesForReport.SelectedValue);
            string courseName = cmbCoursesForReport.Text;

            try
            {
                DataTable dt = new DataTable();
                using (var conn = DBConnection.GetConnection())
                {
                    var query = "SELECT u.full_name, a.class_date, a.status FROM attendance a " +
                                "JOIN student_enrollments se ON a.enrollment_id = se.enrollment_id " +
                                "JOIN users u ON se.student_id = u.user_id " +
                                "WHERE se.course_id = @courseId ORDER BY u.full_name, a.class_date";
                    var adapter = new MySqlDataAdapter(query, conn);
                    adapter.SelectCommand.Parameters.AddWithValue("@courseId", courseId);
                    adapter.Fill(dt);
                }

                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("No se encontraron registros de asistencia para este curso.", "Sin Datos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Lógica para exportar el DataTable a un archivo CSV
                using (var sfd = new SaveFileDialog())
                {
                    sfd.Filter = "Archivo CSV (*.csv)|*.csv";
                    sfd.FileName = $"Reporte_Asistencia_{courseName.Replace(" ", "_")}.csv";
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        var sb = new StringBuilder();

                        // Escribir los encabezados de las columnas
                        sb.AppendLine("Estudiante,Fecha,Estado");

                        // Escribir los datos de cada fila
                        foreach (DataRow row in dt.Rows)
                        {
                            // Formatear la fecha y escapar comas en los nombres si fuera necesario
                            string studentName = $"\"{row["full_name"]}\"";
                            string classDate = Convert.ToDateTime(row["class_date"]).ToString("yyyy-MM-dd");
                            string status = row["status"].ToString();
                            sb.AppendLine($"{studentName},{classDate},{status}");
                        }

                        // Guardar el archivo con codificación UTF-8 para soportar acentos
                        File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                        MessageBox.Show("Reporte de asistencia exportado exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Ocurrió un error al exportar el reporte: " + ex.Message, "Error de Exportación", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
    }
}