using System.Drawing;
using System.Windows.Forms;

namespace ajesha
{
    public partial class ucDashboard : UserControl
    {
        private Label lblWelcome;
        private Label lblUserName;
        private PictureBox picAppIcon;

        public ucDashboard()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(45, 45, 48);
            SetupUI();
        }

        private void SetupUI()
        {
            picAppIcon = new PictureBox
            {
                Image = Properties.Resources.logo,
                Size = new Size(120, 120),
                SizeMode = PictureBoxSizeMode.Zoom,
                Location = new Point((this.Width - 120) / 2, (this.Height / 2) - 120)
            };
            lblWelcome = new Label
            {
                Text = "Bienvenido al Sistema de Gestión",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                AutoSize = true
            };
            lblWelcome.Location = new Point((this.Width - lblWelcome.Width) / 2, picAppIcon.Bottom + 10);

            lblUserName = new Label
            {
                Text = "",
                ForeColor = Color.Gainsboro,
                Font = new Font("Segoe UI", 16F, FontStyle.Regular),
                AutoSize = true
            };
            this.SizeChanged += (s, e) => {
                picAppIcon.Location = new Point((this.Width - picAppIcon.Width) / 2, (this.Height / 2) - 120);
                lblWelcome.Location = new Point((this.Width - lblWelcome.Width) / 2, picAppIcon.Bottom + 10);
                lblUserName.Location = new Point((this.Width - lblUserName.Width) / 2, lblWelcome.Bottom + 5);
            };

            this.Controls.Add(picAppIcon);
            this.Controls.Add(lblWelcome);
            this.Controls.Add(lblUserName);
        }
        public void SetWelcomeMessage(string fullName)
        {
            lblUserName.Text = fullName;
            lblUserName.Location = new Point((this.Width - lblUserName.Width) / 2, lblWelcome.Bottom + 5);
        }
    }
}