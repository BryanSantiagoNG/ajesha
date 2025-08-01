using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ajesha
{
    public class frmMain : Form
    {
        private readonly int currentUserId;
        private readonly string currentUserRole;
        private readonly string currentFullName;
        private ucProfile profileControl;
        private ucDashboard dashboardControl;
        private ucAdminSistema adminSistemaControl;
        private Panel panelSidebar;
        private Panel panelMainContent;
        private Panel panelHeader;
        private Panel panelContentArea;
        private PictureBox picLogo;
        private PictureBox btnToggleSidebar;
        private Label lblSectionTitle;
        private Timer sidebarTransitionTimer;
        private Dictionary<Button, string> navigationButtons = new Dictionary<Button, string>();
        private bool isSidebarCollapsed = false;

        private const int SIDEBAR_WIDTH_EXPANDED = 230;
        private const int SIDEBAR_WIDTH_COLLAPSED = 70;
        private readonly Color colorSidebarBg = Color.FromArgb(51, 51, 55);
        private readonly Color colorMainBg = Color.FromArgb(45, 45, 48);
        private readonly Color colorHeaderBg = Color.FromArgb(63, 63, 70);
        private readonly Color colorFont = Color.FromArgb(220, 220, 220);
        private readonly Color colorButtonActive = Color.FromArgb(37, 37, 38);

        public frmMain(int userId, string userRole, string fullName)
        {
            this.currentUserId = userId;
            this.currentUserRole = userRole;
            this.currentFullName = fullName;

            this.Text = "Sistema de Gestión Estudiantil - Ajesha";
            this.MinimumSize = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = colorMainBg;
            this.DoubleBuffered = true;

            InitializeComponents();
            InitializeSidebarTimer();
        }
        private void InitializeComponents()
        {

            panelSidebar = new Panel { Dock = DockStyle.Left, Width = SIDEBAR_WIDTH_EXPANDED, BackColor = colorSidebarBg };
            panelMainContent = new Panel { Dock = DockStyle.Fill, BackColor = colorMainBg };
            panelHeader = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = colorHeaderBg };
            lblSectionTitle = new Label { Text = "Inicio", ForeColor = colorFont, Font = new Font("Segoe UI", 14F, FontStyle.Bold), Location = new Point(20, 15), AutoSize = true };
            panelHeader.Controls.Add(lblSectionTitle);
            panelContentArea = new Panel { Dock = DockStyle.Fill, BackColor = colorMainBg };

            profileControl = new ucProfile();
            profileControl.LoadProfileData(this.currentUserId, this.currentUserRole);

            dashboardControl = new ucDashboard();
            dashboardControl.SetWelcomeMessage(this.currentFullName);

            adminSistemaControl = new ucAdminSistema();

            panelContentArea.Controls.Add(profileControl);
            panelContentArea.Controls.Add(dashboardControl);
            panelContentArea.Controls.Add(adminSistemaControl);

            panelMainContent.Controls.Add(panelContentArea);
            panelMainContent.Controls.Add(panelHeader);

            int iconSize = 24;
            int menuIconSize = 28;
            btnToggleSidebar = new PictureBox
            {
                Location = new Point(18, 15),
                Size = new Size(menuIconSize, menuIconSize),
                SizeMode = PictureBoxSizeMode.Zoom,
                Cursor = Cursors.Hand,
                Image = ResizeImage(Properties.Resources.menu, menuIconSize, menuIconSize)
            };
            btnToggleSidebar.Click += BtnToggleSidebar_Click;

            picLogo = new PictureBox {
                Location = new Point(0, 60),
                Size = new Size(SIDEBAR_WIDTH_EXPANDED, 75),
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = Properties.Resources.logo,
                BackColor = Color.White,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            int topPosition = 180;
            CreateNavButton("Inicio", topPosition, ResizeImage(Properties.Resources.home, iconSize, iconSize));
            CreateNavButton("Classroom", topPosition += 45, ResizeImage(Properties.Resources.classroom, iconSize, iconSize));
            CreateNavButton("Calificaciones", topPosition += 45, ResizeImage(Properties.Resources.calificacion, iconSize, iconSize));
            CreateNavButton("Finanzas", topPosition += 45, ResizeImage(Properties.Resources.finanzas, iconSize, iconSize));
            CreateNavButton("Notificaciones", topPosition += 45, ResizeImage(Properties.Resources.notificacion, iconSize, iconSize));
            CreateNavButton("Perfil", topPosition += 45, ResizeImage(Properties.Resources.perfil, iconSize, iconSize));

            CreateNavButton("Admin Usuarios", 180, ResizeImage(Properties.Resources.perfil, iconSize, iconSize));
            CreateNavButton("Admin Sistema", 225, ResizeImage(Properties.Resources.info, iconSize, iconSize));

            var aboutButton = CreateNavButton("Acerca de", this.Height, ResizeImage(Properties.Resources.info, iconSize, iconSize));
            aboutButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            aboutButton.Location = new Point(0, this.ClientSize.Height - aboutButton.Height);
            this.SizeChanged += (s, e) => { aboutButton.Location = new Point(0, this.ClientSize.Height - aboutButton.Height); };

            panelSidebar.Controls.Add(btnToggleSidebar);
            panelSidebar.Controls.Add(picLogo);
            foreach (var button in navigationButtons.Keys) { panelSidebar.Controls.Add(button); }

            this.Controls.Add(panelMainContent);
            this.Controls.Add(panelSidebar);

            ConfigureSidebarForRole(this.currentUserRole);
            dashboardControl.BringToFront();
            lblSectionTitle.Text = "Inicio";
            SetActiveButton(navigationButtons.Keys.First(b => b.Tag.ToString() == "Inicio"));
        }
        private void ConfigureSidebarForRole(string role)
        {
            foreach (var button in navigationButtons.Keys)
            {
                button.Visible = false;
            }
            navigationButtons.Keys.First(b => b.Tag.ToString() == "Inicio").Visible = true;
            navigationButtons.Keys.First(b => b.Tag.ToString() == "Perfil").Visible = true;
            navigationButtons.Keys.First(b => b.Tag.ToString() == "Acerca de").Visible = true;
            navigationButtons.Keys.First(b => b.Tag.ToString() == "Notificaciones").Visible = true;
            switch (role.ToLower())
            {
                case "admin":
                    navigationButtons.Keys.First(b => b.Tag.ToString() == "Admin Usuarios").Visible = true;
                    navigationButtons.Keys.First(b => b.Tag.ToString() == "Admin Sistema").Visible = true;
                    break;

                case "student":
                    navigationButtons.Keys.First(b => b.Tag.ToString() == "Classroom").Visible = true;
                    navigationButtons.Keys.First(b => b.Tag.ToString() == "Calificaciones").Visible = true;
                    navigationButtons.Keys.First(b => b.Tag.ToString() == "Finanzas").Visible = true;
                    break;

                case "teacher":
                    navigationButtons.Keys.First(b => b.Tag.ToString() == "Classroom").Visible = true;
                    navigationButtons.Keys.First(b => b.Tag.ToString() == "Calificaciones").Visible = true;
                    break;

                case "coordinator":
                    navigationButtons.Keys.First(b => b.Tag.ToString() == "Classroom").Visible = true;
                    navigationButtons.Keys.First(b => b.Tag.ToString() == "Calificaciones").Visible = true;
                    navigationButtons.Keys.First(b => b.Tag.ToString() == "Finanzas").Visible = true;
                    break;
            }
        }

        private void NavigationButton_Click(object sender, EventArgs e)
        {
            var clickedButton = (Button)sender;
            lblSectionTitle.Text = clickedButton.Tag.ToString();
            SetActiveButton(clickedButton);

            switch (clickedButton.Tag.ToString())
            {
                case "Perfil":
                    profileControl.BringToFront();
                    break;

                case "Inicio":
                    dashboardControl.BringToFront();
                    break;

                case "Admin Sistema":
                    adminSistemaControl.BringToFront();
                    break;
            }
        }

        private void SetActiveButton(Button activeButton)
        {
            foreach (var button in navigationButtons.Keys)
            {
                button.BackColor = colorSidebarBg;
            }
            activeButton.BackColor = colorButtonActive;
        }

        private Button CreateNavButton(string text, int topPosition, Image icon)
        {
            var button = new Button
            {
                Text = "   " + text,
                Tag = text,
                ForeColor = colorFont,
                Font = new Font("Segoe UI", 11F, FontStyle.Regular),
                Image = icon,
                ImageAlign = ContentAlignment.MiddleLeft,
                TextAlign = ContentAlignment.MiddleLeft,
                TextImageRelation = TextImageRelation.ImageBeforeText,
                Padding = new Padding(20, 0, 0, 0),
                FlatStyle = FlatStyle.Flat,
                Height = 45,
                Width = SIDEBAR_WIDTH_EXPANDED,
                Location = new Point(0, topPosition),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            button.FlatAppearance.BorderSize = 0;
            button.Click += NavigationButton_Click;
            navigationButtons.Add(button, text);
            return button;
        }
        private void InitializeSidebarTimer()
        {
            sidebarTransitionTimer = new Timer { Interval = 5 };
            sidebarTransitionTimer.Tick += SidebarTransitionTimer_Tick;
        }

        private void BtnToggleSidebar_Click(object sender, EventArgs e)
        {
            isSidebarCollapsed = !isSidebarCollapsed;
            UpdateSidebarState();
            sidebarTransitionTimer.Start();
        }

        private void UpdateSidebarState()
        {
            picLogo.Visible = !isSidebarCollapsed;
            foreach (var entry in navigationButtons)
            {
                Button button = entry.Key;
                string originalText = entry.Value;
                button.Text = isSidebarCollapsed ? "" : "   " + originalText;
                button.ImageAlign = isSidebarCollapsed ? ContentAlignment.MiddleCenter : ContentAlignment.MiddleLeft;
                button.Padding = isSidebarCollapsed ? new Padding(0) : new Padding(20, 0, 0, 0);
            }
        }

        private void SidebarTransitionTimer_Tick(object sender, EventArgs e)
        {
            int step = 20;
            if (isSidebarCollapsed)
            {
                if (panelSidebar.Width > SIDEBAR_WIDTH_COLLAPSED) panelSidebar.Width -= step;
                else { panelSidebar.Width = SIDEBAR_WIDTH_COLLAPSED; sidebarTransitionTimer.Stop(); }
            }
            else
            {
                if (panelSidebar.Width < SIDEBAR_WIDTH_EXPANDED) panelSidebar.Width += step;
                else { panelSidebar.Width = SIDEBAR_WIDTH_EXPANDED; sidebarTransitionTimer.Stop(); }
            }
        }
        private Image ResizeImage(Image image, int width, int height)
        {
            if (image == null) return null;
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);
            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);
            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                using (var wrapMode = new System.Drawing.Imaging.ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }
            return destImage;
        }
    }
}