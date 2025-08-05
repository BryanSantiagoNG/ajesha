using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace ajesha
{
    public partial class ucCoordinator_CourseManagement : UserControl
    {
        // --- Controles de la Interfaz ---
        private DataGridView dgvCourses;
        private TextBox txtCourseName, txtCourseCode, txtCredits, txtDepartment;
        private RichTextBox rtbCourseDescription;
        private CheckBox chkIsActive;
        private Button btnSaveCourse, btnClearForm;
        private int selectedCourseId = 0;

        // --- Paleta de Colores ---
        private readonly Color colorDarkBg = Color.FromArgb(45, 45, 48);
        private readonly Color colorMidBg = Color.FromArgb(51, 51, 55);
        private readonly Color colorLightBg = Color.FromArgb(63, 63, 70);
        private readonly Color colorFont = Color.White;
        private readonly Color colorBorder = Color.FromArgb(80, 80, 80);

        public ucCoordinator_CourseManagement()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            this.BackColor = colorDarkBg;
            SetupUI();
            this.VisibleChanged += (s, e) => { if (this.Visible) LoadCourses(); };
        }
        private void ApplyDarkTheme(Control control)
        {
            if (control is TextBox || control is RichTextBox || control is ComboBox)
            {
                control.BackColor = colorMidBg;
                control.ForeColor = colorFont;
                if (control is TextBox txt) { txt.BorderStyle = BorderStyle.FixedSingle; }
                if (control is RichTextBox rtb) { rtb.BorderStyle = BorderStyle.FixedSingle; }
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
            var mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(20) };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));

            // Columna Izquierda: Lista de Cursos
            var dgvPanel = new Panel { Dock = DockStyle.Fill };
            dgvPanel.Controls.Add(new Label { Text = "Cursos Existentes", Font = new Font("Segoe UI", 14F, FontStyle.Bold), ForeColor = colorFont, AutoSize = true });
            dgvCourses = new DataGridView 
            { 
                Dock = DockStyle.Fill, 
                ReadOnly = true, 
                AllowUserToAddRows = false, 
                SelectionMode = DataGridViewSelectionMode.FullRowSelect, 
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Margin = new Padding(0, 30, 0, 0) 
            };
            ApplyDarkTheme(dgvCourses);
            dgvCourses.CellClick += DgvCourses_CellClick;
            dgvPanel.Controls.Add(dgvCourses);
            mainLayout.Controls.Add(dgvPanel, 0, 0);

            // Columna Derecha: Formulario de Edición/Creación
            var editGroup = new GroupBox { Dock = DockStyle.Fill, Text = "Detalles del Curso", ForeColor = colorFont, Padding = new Padding(15) };
            var fieldsLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 7, ColumnCount = 1 };
            fieldsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Nombre Label
            fieldsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Nombre TextBox
            fieldsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // SubLayout Código/Créditos
            fieldsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // SubLayout Depto/Activo
            fieldsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Descripción Label
            fieldsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Descripción RichTextBox
            fieldsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Botones

            txtCourseName = new TextBox { Dock = DockStyle.Fill }; ApplyDarkTheme(txtCourseName);
            rtbCourseDescription = new RichTextBox { Dock = DockStyle.Fill }; ApplyDarkTheme(rtbCourseDescription);

            var buttonsPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, AutoSize = true };
            btnClearForm = new Button { Text = "Limpiar", Width = 100, Height = 35 }; ApplyDarkTheme(btnClearForm);
            btnClearForm.Click += (s, e) => ClearForm();
            btnSaveCourse = new Button { Text = "Crear Curso", Width = 150, Height = 35 }; ApplyDarkTheme(btnSaveCourse);
            btnSaveCourse.Click += BtnSaveCourse_Click;
            buttonsPanel.Controls.AddRange(new Control[] { btnClearForm, btnSaveCourse });

            fieldsLayout.Controls.Add(new Label { Text = "Nombre del Curso:", ForeColor = colorFont, AutoSize = true, Dock = DockStyle.Bottom }, 0, 0);
            fieldsLayout.Controls.Add(txtCourseName, 0, 1);

            var subLayout1 = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, AutoSize = true };
            subLayout1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            subLayout1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            txtCourseCode = new TextBox { Dock = DockStyle.Fill }; ApplyDarkTheme(txtCourseCode);
            txtCredits = new TextBox { Dock = DockStyle.Fill }; ApplyDarkTheme(txtCredits);
            subLayout1.Controls.Add(new Label { Text = "Código:", ForeColor = colorFont }, 0, 0);
            subLayout1.Controls.Add(txtCourseCode, 0, 1);
            subLayout1.Controls.Add(new Label { Text = "Créditos:", ForeColor = colorFont }, 1, 0);
            subLayout1.Controls.Add(txtCredits, 1, 1);
            fieldsLayout.Controls.Add(subLayout1, 0, 2);

            var subLayout2 = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, AutoSize = true };
            subLayout2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            subLayout2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            txtDepartment = new TextBox { Dock = DockStyle.Fill }; ApplyDarkTheme(txtDepartment);
            chkIsActive = new CheckBox { Text = "Curso Activo", ForeColor = colorFont, Checked = true, Margin = new Padding(0, 25, 0, 0) };
            subLayout2.Controls.Add(new Label { Text = "Departamento:", ForeColor = colorFont }, 0, 0);
            subLayout2.Controls.Add(txtDepartment, 0, 1);
            subLayout2.Controls.Add(chkIsActive, 1, 1);
            fieldsLayout.Controls.Add(subLayout2, 0, 3);

            fieldsLayout.Controls.Add(new Label { Text = "Descripción:", ForeColor = colorFont, AutoSize = true, Margin = new Padding(0, 10, 0, 0) }, 0, 4);
            fieldsLayout.Controls.Add(rtbCourseDescription, 0, 5);
            fieldsLayout.Controls.Add(buttonsPanel, 0, 6);

            editGroup.Controls.Add(fieldsLayout);
            mainLayout.Controls.Add(editGroup, 1, 0);
            this.Controls.Add(mainLayout);
        }
        private void LoadCourses()
        {
            try
            {
                using (var conn = DBConnection.GetConnection())
                {
                    var adapter = new MySqlDataAdapter("SELECT course_id, course_code AS Código, " +
                        "course_name AS Nombre, credits AS Créditos, department AS Departamento, " +
                        "is_active AS Activo, description FROM courses ORDER BY course_name", conn);
                    var dt = new DataTable();
                    adapter.Fill(dt);
                    dgvCourses.DataSource = dt;
                    if (dgvCourses.Columns.Contains("course_id")) dgvCourses.Columns["course_id"].Visible = false;
                    if (dgvCourses.Columns.Contains("description")) dgvCourses.Columns["description"].Visible = false;
                }
            }
            catch (Exception ex) { MessageBox.Show("Error al cargar los cursos: " + ex.Message, "Error de Base de Datos", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
        private void DgvCourses_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            DataGridViewRow row = dgvCourses.Rows[e.RowIndex];
            selectedCourseId = Convert.ToInt32(row.Cells["course_id"].Value);
            txtCourseName.Text = row.Cells["Nombre"].Value.ToString();
            txtCourseCode.Text = row.Cells["Código"].Value.ToString();
            txtCredits.Text = row.Cells["Créditos"].Value.ToString();
            txtDepartment.Text = row.Cells["Departamento"].Value.ToString();
            chkIsActive.Checked = Convert.ToBoolean(row.Cells["Activo"].Value);
            rtbCourseDescription.Text = row.Cells["description"].Value.ToString();
            btnSaveCourse.Text = "Actualizar Curso";
        }
        private void ClearForm()
        {
            selectedCourseId = 0;
            txtCourseName.Clear();
            txtCourseCode.Clear();
            txtCredits.Clear();
            txtDepartment.Clear();
            rtbCourseDescription.Clear();
            chkIsActive.Checked = true;
            btnSaveCourse.Text = "Crear Curso";
            dgvCourses.ClearSelection();
        }
        private void BtnSaveCourse_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCourseName.Text) || string.IsNullOrWhiteSpace(txtCourseCode.Text) || string.IsNullOrWhiteSpace(txtCredits.Text) || string.IsNullOrWhiteSpace(txtDepartment.Text))
            {
                MessageBox.Show("Todos los campos (excepto descripción) son requeridos.", "Datos Incompletos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                using (var conn = DBConnection.GetConnection())
                {
                    conn.Open();
                    MySqlCommand cmd;
                    if (selectedCourseId == 0) // Crear
                    {
                        cmd = new MySqlCommand("INSERT INTO courses (course_name, course_code, credits, department, description, is_active) VALUES (@name, @code, " +
                            "@credits, @dept, @desc, @active)", conn);
                    }
                    else // Actualizar
                    {
                        cmd = new MySqlCommand("UPDATE courses SET course_name = @name, course_code = @code, credits = @credits, department = @dept, " +
                            "description = @desc, is_active = @active WHERE course_id = @id", conn);
                        cmd.Parameters.AddWithValue("@id", selectedCourseId);
                    }
                    cmd.Parameters.AddWithValue("@name", txtCourseName.Text);
                    cmd.Parameters.AddWithValue("@code", txtCourseCode.Text);
                    cmd.Parameters.AddWithValue("@credits", Convert.ToInt32(txtCredits.Text));
                    cmd.Parameters.AddWithValue("@dept", txtDepartment.Text);
                    cmd.Parameters.AddWithValue("@desc", rtbCourseDescription.Text);
                    cmd.Parameters.AddWithValue("@active", chkIsActive.Checked);
                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Curso guardado exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex) { MessageBox.Show("Error al guardar el curso: " + ex.Message, "Error de Base de Datos", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            finally
            {
                LoadCourses();
                ClearForm();
            }
        }
    }
}