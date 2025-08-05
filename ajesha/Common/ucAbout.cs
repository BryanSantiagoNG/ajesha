using System.Drawing;
using System.Windows.Forms;

namespace ajesha
{
    public partial class ucAbout : UserControl
    {
        public ucAbout()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(45, 45, 48);
            SetupUI();
        }
        private void SetupUI()
        {
            var contentPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(50),
                AutoScroll = true
            };
            var lblTitle = new Label
            {
                Text = "Sistema de Gestión Estudiantil - Ajesha",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 22F, FontStyle.Bold),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 20) 
            };

            var lblVersion = new Label
            {
                Text = "Versión 1.0.0",
                ForeColor = Color.Gainsboro,
                Font = new Font("Segoe UI", 12F, FontStyle.Italic),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 40)
            };

            var lblDescription = new RichTextBox
            {
                Text = "Este software es una solución integral para la gestión académica, " +
                "diseñada para facilitar la administración de usuarios, cursos, calificaciones y asistencias. Permite a administradores, " +
                "coordinadores, maestros y estudiantes interactuar con el sistema de una manera eficiente y segura.",
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                BackColor = this.BackColor, 
                ForeColor = Color.WhiteSmoke,
                Font = new Font("Segoe UI", 11F),
                Width = 600,
                Height = 100
            };

            var lblCopyright = new Label
            {
                Text = "© 2025 Ajesha Systems. Todos los derechos reservados.\n" +
                "Desarrollado por: \nBryan Santiago Navarro Godinez, \n" +
                "Diego Alfonso Vaca Moreno, \n" +
                "Esequiel Espejo Salon, \n" +
                "Heriberto López Velázquez, \n" +
                "Jesus Manuel Sanchez Quiñonez, \n" +
                "Eduardo Antonio Valenzuela Hernandez",
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 9F),
                AutoSize = true,
                Margin = new Padding(0, 60, 0, 0)
            };
            contentPanel.Controls.Add(lblTitle);
            contentPanel.Controls.Add(lblVersion);
            contentPanel.Controls.Add(lblDescription);
            contentPanel.Controls.Add(lblCopyright);

            this.Controls.Add(contentPanel);
        }
    }
}