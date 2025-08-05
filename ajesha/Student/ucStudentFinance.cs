using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace ajesha
{
    public partial class ucStudentFinance : UserControl
    {
        private int currentStudentId;
        private ComboBox cmbDocumentType;
        private Button btnRequestDocument;
        private DataGridView dgvMyRequests;

        public ucStudentFinance()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(45, 45, 48);
            SetupUI();
            this.VisibleChanged += (s, e) => { if (this.Visible) LoadMyRequests(); };
        }

        public void SetStudentId(int studentId) { this.currentStudentId = studentId; }

        private void SetupUI()
        {
            var mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(20), ColumnCount = 2 };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            var financeGroup = new GroupBox { Dock = DockStyle.Fill, Text = "Estado de Cuenta", ForeColor = Color.Gainsboro };
            var lblFinancePlaceholder = new Label { Text = "Módulo de finanzas en desarrollo.", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.White };
            financeGroup.Controls.Add(lblFinancePlaceholder);
            mainLayout.Controls.Add(financeGroup, 0, 0);

            var requestsGroup = new GroupBox { Dock = DockStyle.Fill, Text = "Trámites Escolares", ForeColor = Color.Gainsboro };
            var requestsLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3 };
            requestsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            requestsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            requestsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var requestPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
            cmbDocumentType = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 200 };
            cmbDocumentType.Items.AddRange(new string[] { "Constancia de Estudios", "Historial Académico", "Carta de Buena Conducta" });
            cmbDocumentType.SelectedIndex = 0;
            btnRequestDocument = new Button { Text = "Solicitar Documento", AutoSize = true };
            btnRequestDocument.Click += BtnRequestDocument_Click;
            requestPanel.Controls.AddRange(new Control[] { cmbDocumentType, btnRequestDocument });

            dgvMyRequests = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };

            requestsLayout.Controls.Add(new Label { Text = "Mis Solicitudes", Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = Color.White, AutoSize = true }, 0, 0);
            requestsLayout.Controls.Add(dgvMyRequests, 0, 1);
            requestsLayout.Controls.Add(requestPanel, 0, 2);

            requestsGroup.Controls.Add(requestsLayout);
            mainLayout.Controls.Add(requestsGroup, 1, 0);

            this.Controls.Add(mainLayout);
        }

        private void LoadMyRequests()
        {
            try
            {
                using (var conn = DBConnection.GetConnection())
                {
                    var query = "SELECT document_type AS 'Tipo de Documento', status AS Estado, request_date AS 'Fecha de Solicitud' FROM document_requests WHERE student_id = @studentId ORDER BY request_date DESC";
                    var adapter = new MySqlDataAdapter(query, conn);
                    adapter.SelectCommand.Parameters.AddWithValue("@studentId", this.currentStudentId);
                    var dt = new DataTable();
                    adapter.Fill(dt);
                    dgvMyRequests.DataSource = dt;
                }
            }
            catch (Exception ex) { MessageBox.Show("Error al cargar tus solicitudes: " + ex.Message); }
        }

        private void BtnRequestDocument_Click(object sender, EventArgs e)
        {
            if (cmbDocumentType.SelectedItem == null) return;
            string docType = cmbDocumentType.SelectedItem.ToString();

            try
            {
                using (var conn = DBConnection.GetConnection())
                {
                    var cmd = new MySqlCommand("INSERT INTO document_requests (student_id, document_type) VALUES (@studentId, @docType)", conn);
                    cmd.Parameters.AddWithValue("@studentId", this.currentStudentId);
                    cmd.Parameters.AddWithValue("@docType", docType);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Tu solicitud ha sido enviada.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadMyRequests();
                }
            }
            catch (Exception ex) { MessageBox.Show("Error al enviar la solicitud: " + ex.Message); }
        }
    }
}