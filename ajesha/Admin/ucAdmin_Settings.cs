using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace ajesha
{
    public partial class ucAdmin_Settings : UserControl
    {
        // --- Controles de la Interfaz ---
        private DataGridView dgvCycles;
        private TextBox txtCycleName;
        private DateTimePicker dtpStartDate, dtpEndDate;
        private CheckBox chkIsActive;
        private Button btnSaveCycle, btnClearForm;
        private int selectedCycleId = 0;

        // --- Paleta de Colores ---
        private readonly Color colorDarkBg = Color.FromArgb(45, 45, 48);
        private readonly Color colorMidBg = Color.FromArgb(51, 51, 55);
        private readonly Color colorLightBg = Color.FromArgb(63, 63, 70);
        private readonly Color colorFont = Color.White;
        private readonly Color colorBorder = Color.FromArgb(80, 80, 80);

        public ucAdmin_Settings()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            this.BackColor = colorDarkBg;
            SetupUI();
            this.VisibleChanged += (s, e) => { if (this.Visible) LoadCycles(); };
        }
        private void ApplyDarkTheme(Control control)
        {
            if (control is TextBox || control is ComboBox)
            {
                control.BackColor = colorMidBg;
                control.ForeColor = colorFont;
                if (control is TextBox txt) { txt.BorderStyle = BorderStyle.FixedSingle; }
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
            if (control is DateTimePicker dtp)
            {
                dtp.CalendarForeColor = colorFont;
                dtp.CalendarMonthBackground = colorMidBg;
                dtp.CalendarTitleBackColor = colorLightBg;
            }
        }
        private void SetupUI()
        {
            var mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(20) };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            var leftPanelLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
            leftPanelLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Fila para el título
            leftPanelLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Fila para la tabla

            var lblTitle = new Label { Text = "Ciclos Escolares Existentes", Font = new Font("Segoe UI", 14F, FontStyle.Bold), ForeColor = colorFont, AutoSize = true, Margin = new Padding(0, 0, 0, 10) };

            dgvCycles = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            ApplyDarkTheme(dgvCycles);
            dgvCycles.CellClick += DgvCycles_CellClick;
            dgvCycles.Paint += DgvCycles_Paint;

            leftPanelLayout.Controls.Add(lblTitle, 0, 0);
            leftPanelLayout.Controls.Add(dgvCycles, 0, 1);
            mainLayout.Controls.Add(leftPanelLayout, 0, 0);

            var editGroup = new GroupBox { Dock = DockStyle.Fill, Text = "Detalles del Ciclo Escolar", ForeColor = colorFont, Padding = new Padding(15) };
            var fieldsLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 5, ColumnCount = 1 };
            fieldsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            fieldsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            fieldsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            fieldsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            fieldsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            txtCycleName = new TextBox { Dock = DockStyle.Fill }; ApplyDarkTheme(txtCycleName);
            dtpStartDate = new DateTimePicker { Dock = DockStyle.Fill }; ApplyDarkTheme(dtpStartDate);
            dtpEndDate = new DateTimePicker { Dock = DockStyle.Fill }; ApplyDarkTheme(dtpEndDate);
            chkIsActive = new CheckBox { Text = "Establecer como ciclo activo", ForeColor = colorFont, Checked = false, AutoSize = true };

            var buttonsPanel = new FlowLayoutPanel { Dock = DockStyle.Bottom, FlowDirection = FlowDirection.RightToLeft, AutoSize = true };
            btnClearForm = new Button { Text = "Limpiar", Width = 100, Height = 35 }; ApplyDarkTheme(btnClearForm);
            btnClearForm.Click += (s, e) => ClearForm();
            btnSaveCycle = new Button { Text = "Crear Ciclo", Width = 150, Height = 35 }; ApplyDarkTheme(btnSaveCycle);
            btnSaveCycle.Click += BtnSaveCycle_Click;
            buttonsPanel.Controls.AddRange(new Control[] { btnClearForm, btnSaveCycle });

            fieldsLayout.Controls.Add(new Label { Text = "Nombre del Ciclo (Ej: Semestre 2025-1):", ForeColor = colorFont, AutoSize = true }, 0, 0);
            fieldsLayout.Controls.Add(txtCycleName, 0, 1);

            var datesLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, AutoSize = true, Margin = new Padding(0, 10, 0, 0) };
            datesLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            datesLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            datesLayout.Controls.Add(new Label { Text = "Fecha de Inicio:", ForeColor = colorFont }, 0, 0);
            datesLayout.Controls.Add(dtpStartDate, 0, 1);
            datesLayout.Controls.Add(new Label { Text = "Fecha de Fin:", ForeColor = colorFont }, 1, 0);
            datesLayout.Controls.Add(dtpEndDate, 1, 1);
            fieldsLayout.Controls.Add(datesLayout, 0, 2);

            fieldsLayout.Controls.Add(chkIsActive, 0, 3);
            fieldsLayout.Controls.Add(buttonsPanel, 0, 4);

            editGroup.Controls.Add(fieldsLayout);
            mainLayout.Controls.Add(editGroup, 1, 0);
            this.Controls.Add(mainLayout);
        }
        private void DgvCycles_Paint(object sender, PaintEventArgs e)
        {
            if (dgvCycles.Rows.Count == 0)
            {
                TextRenderer.DrawText(e.Graphics, "No hay ciclos escolares registrados. Cree uno nuevo en el panel de la derecha.",
                    dgvCycles.Font, dgvCycles.ClientRectangle, dgvCycles.ForeColor, dgvCycles.BackgroundColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }
        private void LoadCycles()
        {
            try
            {
                using (var conn = DBConnection.GetConnection())
                {
                    var adapter = new MySqlDataAdapter("SELECT cycle_id, cycle_name AS 'Nombre del Ciclo', start_date AS 'Fecha de Inicio', " +
                        "end_date AS 'Fecha de Fin', is_active AS Activo FROM academic_cycles ORDER BY start_date DESC", conn);
                    var dt = new DataTable();
                    adapter.Fill(dt);
                    dgvCycles.DataSource = dt;
                    if (dgvCycles.Columns.Contains("cycle_id")) dgvCycles.Columns["cycle_id"].Visible = false;
                }
            }
            catch (Exception ex) 
            { 
                MessageBox.Show("Error al cargar los ciclos escolares: " + ex.Message, 
                "Error de Base de Datos", 
                MessageBoxButtons.OK, 
                MessageBoxIcon.Error); 
            }
        }
        private void DgvCycles_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            DataGridViewRow row = dgvCycles.Rows[e.RowIndex];
            selectedCycleId = Convert.ToInt32(row.Cells["cycle_id"].Value);
            txtCycleName.Text = row.Cells["Nombre del Ciclo"].Value.ToString();
            dtpStartDate.Value = Convert.ToDateTime(row.Cells["Fecha de Inicio"].Value);
            dtpEndDate.Value = Convert.ToDateTime(row.Cells["Fecha de Fin"].Value);
            chkIsActive.Checked = Convert.ToBoolean(row.Cells["Activo"].Value);
            btnSaveCycle.Text = "Actualizar Ciclo";
        }
        private void ClearForm()
        {
            selectedCycleId = 0;
            txtCycleName.Clear();
            dtpStartDate.Value = DateTime.Now;
            dtpEndDate.Value = DateTime.Now.AddMonths(6);
            chkIsActive.Checked = false;
            btnSaveCycle.Text = "Crear Ciclo";
            dgvCycles.ClearSelection();
        }
        private void BtnSaveCycle_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCycleName.Text))
            {
                MessageBox.Show("El nombre del ciclo es requerido.", "Datos Incompletos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var conn = DBConnection.GetConnection())
            {
                MySqlTransaction transaction = null;
                try
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    if (chkIsActive.Checked)
                    {
                        var cmdDeactivate = new MySqlCommand("UPDATE academic_cycles SET is_active = FALSE WHERE is_active = TRUE", conn, transaction);
                        cmdDeactivate.ExecuteNonQuery();
                    }

                    MySqlCommand cmd;
                    if (selectedCycleId == 0) // Crear
                    {
                        cmd = new MySqlCommand("INSERT INTO academic_cycles (cycle_name, start_date, end_date, is_active) VALUES (@name, @start, @end, @active)", conn, transaction);
                    }
                    else // Actualizar
                    {
                        cmd = new MySqlCommand("UPDATE academic_cycles SET cycle_name = @name, start_date = @start, end_date = @end, is_active = @active WHERE cycle_id = @id", conn, transaction);
                        cmd.Parameters.AddWithValue("@id", selectedCycleId);
                    }
                    cmd.Parameters.AddWithValue("@name", txtCycleName.Text);
                    cmd.Parameters.AddWithValue("@start", dtpStartDate.Value);
                    cmd.Parameters.AddWithValue("@end", dtpEndDate.Value);
                    cmd.Parameters.AddWithValue("@active", chkIsActive.Checked);
                    cmd.ExecuteNonQuery();

                    transaction.Commit();
                    MessageBox.Show("Ciclo escolar guardado exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    transaction?.Rollback();
                    MessageBox.Show("Error al guardar el ciclo escolar: " + ex.Message, "Error de Base de Datos", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    LoadCycles();
                    ClearForm();
                }
            }
        }
    }
}