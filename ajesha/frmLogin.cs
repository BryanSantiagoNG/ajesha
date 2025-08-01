using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace ajesha
{
    public class frmLogin : Form
    {
        private PictureBox picLogo; 
        private Label lblTitle;
        private Label lblUsername;
        private Label lblPassword;
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;
        public int LoggedInUserId { get; private set; }
        public string LoggedInUserRole { get; private set; }
        public string LoggedInFullName { get; private set; }

        public frmLogin()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Text = "Inicio de Sesión - Sistema Ajesha";
            this.Size = new Size(400, 360);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            picLogo = new PictureBox
            {
                Location = new Point((this.ClientSize.Width - 100) / 2, 20),
                Size = new Size(100, 100),
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = Properties.Resources.logo
            };

            lblTitle = new Label
            {
                Text = "Control de Acceso",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point((this.ClientSize.Width - 180) / 2, 130)
            };

            lblUsername = new Label
            {
                Text = "Usuario:",
                Location = new Point(50, 180),
                AutoSize = true
            };

            txtUsername = new TextBox
            {
                Location = new Point(150, 177),
                Size = new Size(180, 20)
            };

            lblPassword = new Label
            {
                Text = "Contraseña:",
                Location = new Point(50, 220),
                AutoSize = true
            };

            txtPassword = new TextBox
            {
                Location = new Point(150, 217),
                Size = new Size(180, 20),
                PasswordChar = '*'
            };

            btnLogin = new Button
            {
                Text = "Iniciar Sesión",
                Location = new Point(150, 270),
                Size = new Size(120, 35),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                Cursor = Cursors.Hand
            };

            btnLogin.Click += new EventHandler(btnLogin_Click);
            this.AcceptButton = btnLogin;

            this.Controls.Add(picLogo);
            this.Controls.Add(lblTitle);
            this.Controls.Add(lblUsername);
            this.Controls.Add(txtUsername);
            this.Controls.Add(lblPassword);
            this.Controls.Add(txtPassword);
            this.Controls.Add(btnLogin);
        }
        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Por favor, ingrese el usuario y la contraseña.", "Campos Vacíos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (MySqlConnection connection = DBConnection.GetConnection())
            {
                using (MySqlCommand cmd = new MySqlCommand("sp_user_login", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@p_username", username);
                    cmd.Parameters.AddWithValue("@p_password", password);

                    try
                    {
                        connection.Open();
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                if (reader.FieldCount > 2)
                                {
                                    this.LoggedInUserId = reader.GetInt32("user_id");
                                    this.LoggedInUserRole = reader.GetString("role");
                                    this.LoggedInFullName = reader.GetString("full_name");
                                    this.DialogResult = DialogResult.OK;
                                    this.Close();
                                }
                                else
                                {
                                    string errorMessage = reader["message"].ToString();
                                    MessageBox.Show(errorMessage, "Error de Inicio de Sesión", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                            else
                            {
                                MessageBox.Show("Credenciales inválidas.", "Error de Inicio de Sesión", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                    catch (MySqlException ex)
                    {
                        MessageBox.Show("No se pudo conectar a la base de datos. Por favor, verifique la conexión.\n\nError: " + ex.Message, "Error de Conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ha ocurrido un error inesperado: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}