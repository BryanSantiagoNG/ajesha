using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ajesha
{
    public partial class ucCoordinator_Files : UserControl
    {
        // --- ID del coordinador que está usando el panel ---
        private int currentCoordinatorId;

        // --- Controles de la Interfaz ---
        private Button btnSelectFile, btnUploadFile;
        private TextBox txtFileDescription, txtSelectedFilePath;
        private DataGridView dgvFiles;
        private const string FileDescriptionPlaceholder = "Añadir una descripción (opcional)...";

        // --- Paleta de Colores para el Tema Oscuro ---
        private readonly Color colorDarkBg = Color.FromArgb(45, 45, 48);
        private readonly Color colorMidBg = Color.FromArgb(51, 51, 55);
        private readonly Color colorLightBg = Color.FromArgb(63, 63, 70);
        private readonly Color colorFont = Color.White;
        private readonly Color colorGrayFont = Color.Gainsboro;
        private readonly Color colorBorder = Color.FromArgb(80, 80, 80);

        public ucCoordinator_Files()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            this.BackColor = colorDarkBg;
            SetupUI();
            this.VisibleChanged += (sender, e) => { if (this.Visible) LoadUploadedFiles(); };
        }
        public void SetCoordinatorId(int coordinatorId)
        {
            this.currentCoordinatorId = coordinatorId;
        }
        private void ApplyDarkTheme(Control control)
        {
            if (control is TextBox)
            {
                control.BackColor = colorMidBg;
                control.ForeColor = colorFont;
                ((TextBox)control).BorderStyle = BorderStyle.FixedSingle;
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
            var mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(20), RowCount = 3 };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            mainLayout.Controls.Add(new Label { Text = "Gestión de Archivos", Font = new Font("Segoe UI", 16F, FontStyle.Bold), ForeColor = colorFont, AutoSize = true, Margin = new Padding(0, 0, 0, 20) }, 0, 0);

            var uploadPanel = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 2, RowCount = 2, Margin = new Padding(0, 0, 0, 20) };
            uploadPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            uploadPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            txtSelectedFilePath = new TextBox { Dock = DockStyle.Fill, ReadOnly = true, Font = new Font("Segoe UI", 9F) };
            ApplyDarkTheme(txtSelectedFilePath);
            btnSelectFile = new Button { Text = "Seleccionar Archivo...", AutoSize = true, Height = 30 };
            ApplyDarkTheme(btnSelectFile);
            btnSelectFile.Click += BtnSelectFile_Click;
            uploadPanel.Controls.Add(txtSelectedFilePath, 0, 0);
            uploadPanel.Controls.Add(btnSelectFile, 1, 0);

            txtFileDescription = new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9F) };
            ApplyDarkTheme(txtFileDescription);
            SetPlaceholder();
            txtFileDescription.Enter += TxtFileDescription_Enter;
            txtFileDescription.Leave += TxtFileDescription_Leave;
            btnUploadFile = new Button { Text = "Subir Archivo", AutoSize = true, Height = 30 };
            ApplyDarkTheme(btnUploadFile);
            btnUploadFile.Click += BtnUploadFile_Click;
            uploadPanel.Controls.Add(txtFileDescription, 0, 1);
            uploadPanel.Controls.Add(btnUploadFile, 1, 1);

            dgvFiles = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, AllowUserToAddRows = false };
            ApplyDarkTheme(dgvFiles);

            mainLayout.Controls.Add(uploadPanel, 0, 1);
            mainLayout.Controls.Add(dgvFiles, 0, 2);
            this.Controls.Add(mainLayout);
        }

        private void SetPlaceholder() { txtFileDescription.Text = FileDescriptionPlaceholder; txtFileDescription.ForeColor = Color.Gray; }
        private void TxtFileDescription_Enter(object sender, EventArgs e) { if (txtFileDescription.Text == FileDescriptionPlaceholder) { txtFileDescription.Text = ""; txtFileDescription.ForeColor = colorFont; } }
        private void TxtFileDescription_Leave(object sender, EventArgs e) { if (string.IsNullOrWhiteSpace(txtFileDescription.Text)) { SetPlaceholder(); } }
        
        private void LoadUploadedFiles()
        {
            try
            {
                using (var connection = DBConnection.GetConnection())
                {
                    // No traemos el file_data para no sobrecargar la memoria de la aplicación
                    var adapter = new MySqlDataAdapter("SELECT file_id, file_name AS 'Nombre de Archivo', description AS 'Descripción', file_type AS 'Tipo', uploaded_at AS 'Fecha de Subida' FROM uploaded_files ORDER BY uploaded_at DESC", connection);
                    var dt = new DataTable();
                    adapter.Fill(dt);
                    dgvFiles.DataSource = dt;
                    if (dgvFiles.Columns.Contains("file_id")) dgvFiles.Columns["file_id"].Visible = false;
                }
            }
            catch (Exception ex) { MessageBox.Show("Error al cargar la lista de archivos: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void BtnSelectFile_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Title = "Seleccione un archivo para subir";
                ofd.Filter = "Todos los archivos (*.*)|*.*|Documentos PDF (*.pdf)|*.pdf|Imágenes (*.jpg;*.png)|*.jpg;*.png|Documentos de Word (*.doc;*.docx)|*.doc;*.docx";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtSelectedFilePath.Text = ofd.FileName;
                }
            }
        }

        private void BtnUploadFile_Click(object sender, EventArgs e)
        {
            string filePath = txtSelectedFilePath.Text;
            string description = txtFileDescription.Text;

            if (string.IsNullOrEmpty(filePath))
            {
                MessageBox.Show("Por favor, seleccione un archivo para subir.", "Archivo no seleccionado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (description == FileDescriptionPlaceholder)
            {
                description = "";
            }

            try
            {
                byte[] fileData = File.ReadAllBytes(filePath); // Leer el archivo en memoria

                using (var connection = DBConnection.GetConnection())
                {
                    connection.Open();
                    var cmd = new MySqlCommand("INSERT INTO uploaded_files (file_name, description, file_type, file_data, uploaded_by_user_id) VALUES (@name, @desc, @type, @data, @uploader)", connection);
                    cmd.Parameters.AddWithValue("@name", Path.GetFileName(filePath));
                    cmd.Parameters.AddWithValue("@desc", description);
                    cmd.Parameters.AddWithValue("@type", Path.GetExtension(filePath));
                    cmd.Parameters.Add("@data", MySqlDbType.LongBlob).Value = fileData;
                    cmd.Parameters.AddWithValue("@uploader", this.currentCoordinatorId);
                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Archivo subido exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al subir el archivo: " + ex.Message, "Error de Subida", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Limpiar y recargar
                LoadUploadedFiles();
                txtSelectedFilePath.Clear();
                txtFileDescription.Clear();
                SetPlaceholder();
            }
        }
    }
}