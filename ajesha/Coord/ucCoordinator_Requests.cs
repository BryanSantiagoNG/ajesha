using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace ajesha
{
    public partial class ucCoordinator_Requests : UserControl
    {
        private int currentCoordinatorId;
        private DataGridView dgvRequests;
        private Button btnApprove, btnReject;

        public ucCoordinator_Requests()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(45, 45, 48);
            SetupUI();
            this.VisibleChanged += (s, e) => { if (this.Visible) LoadPendingRequests(); };
        }

        public void SetCoordinatorId(int coordinatorId) { this.currentCoordinatorId = coordinatorId; }

        private void SetupUI()
        {
            var mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(20), RowCount = 3 };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            mainLayout.Controls.Add(new Label 
            { 
                Text = "Solicitudes de Documentos Pendientes", 
                Font = new Font("Segoe UI", 14F, FontStyle.Bold), 
                ForeColor = Color.White, 
                AutoSize = true 
            }, 0, 0);

            dgvRequests = new DataGridView 
            { 
                Dock = DockStyle.Fill, 
                ReadOnly = true, 
                AllowUserToAddRows = false, 
                SelectionMode = DataGridViewSelectionMode.FullRowSelect, 
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill 
            };
            mainLayout.Controls.Add(dgvRequests, 0, 1);

            var buttonsPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, AutoSize = true };
            btnReject = new Button { Text = "Rechazar", Width = 120, Height = 35, BackColor = Color.Maroon };
            btnApprove = new Button { Text = "Aprobar", Width = 120, Height = 35, BackColor = Color.SeaGreen };
            btnApprove.Click += BtnProcessRequest_Click;
            btnReject.Click += BtnProcessRequest_Click;
            buttonsPanel.Controls.AddRange(new Control[] { btnReject, btnApprove });
            mainLayout.Controls.Add(buttonsPanel, 0, 2);

            this.Controls.Add(mainLayout);
        }

        private void LoadPendingRequests()
        {
            try
            {
                using (var conn = DBConnection.GetConnection())
                {
                    var query = "SELECT dr.request_id, u.full_name AS Estudiante, dr.document_type AS Documento, dr.request_date AS Fecha " +
                                "FROM document_requests dr JOIN users u ON dr.student_id = u.user_id " +
                                "WHERE dr.status = 'pending' ORDER BY dr.request_date ASC";
                    var adapter = new MySqlDataAdapter(query, conn);
                    var dt = new DataTable();
                    adapter.Fill(dt);
                    dgvRequests.DataSource = dt;
                    if (dgvRequests.Columns.Contains("request_id")) dgvRequests.Columns["request_id"].Visible = false;
                }
            }
            catch (Exception ex) { MessageBox.Show("Error al cargar solicitudes: " + ex.Message); }
        }

        private void BtnProcessRequest_Click(object sender, EventArgs e)
        {
            if (dgvRequests.CurrentRow == null)
            {
                MessageBox.Show("Por favor, seleccione una solicitud para procesar.", "Selección Requerida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int requestId = Convert.ToInt32(dgvRequests.CurrentRow.Cells["request_id"].Value);
            string newStatus = ((Button)sender == btnApprove) ? "approved" : "rejected";

            try
            {
                using (var conn = DBConnection.GetConnection())
                {
                    var cmd = new MySqlCommand("UPDATE document_requests SET status = @status, processed_by_user_id = @processor, processed_date = NOW() WHERE request_id = @requestId", conn);
                    cmd.Parameters.AddWithValue("@status", newStatus);
                    cmd.Parameters.AddWithValue("@processor", this.currentCoordinatorId);
                    cmd.Parameters.AddWithValue("@requestId", requestId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    MessageBox.Show($"La solicitud ha sido marcada como '{newStatus}'.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadPendingRequests();
                }
            }
            catch (Exception ex) { MessageBox.Show("Error al procesar la solicitud: " + ex.Message); }
        }
    }
}