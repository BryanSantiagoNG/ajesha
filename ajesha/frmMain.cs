using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;

namespace ajesha
{
    public class frmMain : Form
    {
        private readonly int currentUserId;
        private readonly string currentUserRole;

        private Panel panelSidebarContainer; 
        private Panel panelSidebar;
        private Panel panelMainContent;
        private Panel panelHeader;

        private PictureBox picLogo;
        private PictureBox btnToggleSidebar;
        private Label lblSectionTitle;
        private Timer sidebarTransitionTimer;

        private ucProfile profileControl;
        private ucAdminSistema adminSistemaControl;
        private ucAbout aboutControl;
        private ucDashboard dashboardControl;
        private ucCoordinatorPanel coordinatorPanel;
        private ucNotifications notificationsControl;

        private readonly Dictionary<Button, string> navigationButtons = new Dictionary<Button, string>();

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

            this.Text = "Sistema de Gestión Estudiantil - Ajesha";
            this.MinimumSize = new Size(1024, 720);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = colorMainBg;
            this.DoubleBuffered = true;

            InitializeComponents();
            InitializeUserControls();
            InitializeSidebarTimer();

            dashboardControl.BringToFront();
            lblSectionTitle.Text = "Inicio";
            if (navigationButtons.Keys.Any())
            {
                SetActiveButton(navigationButtons.Keys.First(b => b.Tag.ToString() == "Inicio"));
            }
        }

        private void InitializeUserControls()
        {
            profileControl = new ucProfile();
            adminSistemaControl = new ucAdminSistema();
            aboutControl = new ucAbout();
            dashboardControl = new ucDashboard();
            coordinatorPanel = new ucCoordinatorPanel();
            coordinatorPanel.SetCoordinatorId(this.currentUserId);
            notificationsControl = new ucNotifications();

            panelMainContent.Controls.AddRange(new Control[] {
                dashboardControl, profileControl, adminSistemaControl, aboutControl, coordinatorPanel, notificationsControl
            });

            profileControl.LoadProfileData(this.currentUserId, this.currentUserRole);
        }

        private void InitializeComponents()
        {
            panelSidebarContainer = new Panel { Dock = DockStyle.Left, Width = SIDEBAR_WIDTH_EXPANDED, BackColor = colorSidebarBg };
            panelSidebar = new Panel { Dock = DockStyle.Fill }; 

            panelMainContent = new Panel { Dock = DockStyle.Fill, BackColor = colorMainBg };
            panelHeader = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = colorHeaderBg };
            lblSectionTitle = new Label { Text = "", ForeColor = colorFont, Font = new Font("Segoe UI", 14F, FontStyle.Bold), Location = new Point(20, 15), AutoSize = true };
            panelHeader.Controls.Add(lblSectionTitle);

            var navButtonsPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, AutoScroll = true, WrapContents = false, Padding = new Padding(0, 10, 0, 0) };

            var sidebarHeaderPanel = new Panel { Dock = DockStyle.Top, Height = 170 };
            btnToggleSidebar = new PictureBox { Location = new Point(18, 15), Size = new Size(28, 28), SizeMode = PictureBoxSizeMode.Zoom, Cursor = Cursors.Hand, Image = ResizeImage(Properties.Resources.menu, 28, 28) };
            btnToggleSidebar.Click += BtnToggleSidebar_Click;
            picLogo = new PictureBox { Location = new Point((SIDEBAR_WIDTH_EXPANDED - 100) / 2, 60), Size = new Size(100, 100), SizeMode = PictureBoxSizeMode.Zoom, Image = Properties.Resources.logo, BackColor = Color.White };
            sidebarHeaderPanel.Controls.AddRange(new Control[] { btnToggleSidebar, picLogo });

            int iconSize = 24;
            var buttonDefinitions = new List<Tuple<string, Image, string[]>>
            {
                Tuple.Create("Inicio", ResizeImage(Properties.Resources.home, iconSize, iconSize), new[] { "admin", "coordinator", "teacher", "student" }),
                Tuple.Create("Admin Sistema", ResizeImage(Properties.Resources.info, iconSize, iconSize), new[] { "admin" }),
                Tuple.Create("Panel Coordinador", ResizeImage(Properties.Resources.info, iconSize, iconSize), new[] { "coordinator" }),
                Tuple.Create("Notificaciones", ResizeImage(Properties.Resources.notificacion, iconSize, iconSize), new[] { "admin", "coordinator", "teacher", "student" }),
                Tuple.Create("Perfil", ResizeImage(Properties.Resources.perfil, iconSize, iconSize), new[] { "admin", "coordinator", "teacher", "student" })
            };

            foreach (var def in buttonDefinitions)
            {
                if (def.Item3.Contains(this.currentUserRole))
                {
                    navButtonsPanel.Controls.Add(CreateNavButton(def.Item1, def.Item2));
                }
            }

            var aboutButton = CreateNavButton("Acerca de", ResizeImage(Properties.Resources.info, iconSize, iconSize));
            aboutButton.Dock = DockStyle.Bottom;
            panelSidebar.Controls.Add(navButtonsPanel); 
            panelSidebar.Controls.Add(aboutButton);  
            panelSidebar.Controls.Add(sidebarHeaderPanel);

            panelSidebarContainer.Controls.Add(panelSidebar);

            panelMainContent.Controls.Add(panelHeader);
            this.Controls.Add(panelMainContent);
            this.Controls.Add(panelSidebarContainer);
        }

        private Button CreateNavButton(string text, Image icon)
        {
            var button = new Button { Text = "   " + text, Tag = text, ForeColor = colorFont, Font = new Font("Segoe UI", 11F, FontStyle.Regular), Image = icon, ImageAlign = ContentAlignment.MiddleLeft, TextAlign = ContentAlignment.MiddleLeft, TextImageRelation = TextImageRelation.ImageBeforeText, Padding = new Padding(20, 0, 0, 0), FlatStyle = FlatStyle.Flat, Height = 45, Width = SIDEBAR_WIDTH_EXPANDED - 5, Cursor = Cursors.Hand };
            button.FlatAppearance.BorderSize = 0;
            button.Click += NavigationButton_Click;
            navigationButtons.Add(button, text);
            return button;
        }

        private void NavigationButton_Click(object sender, EventArgs e)
        {
            var clickedButton = (Button)sender;
            lblSectionTitle.Text = clickedButton.Tag.ToString();
            SetActiveButton(clickedButton);

            switch (clickedButton.Tag.ToString())
            {
                case "Inicio": dashboardControl.BringToFront(); break;
                case "Admin Sistema": adminSistemaControl.BringToFront(); break;
                case "Panel Coordinador": coordinatorPanel.BringToFront(); break;
                case "Notificaciones": notificationsControl.BringToFront(); break;
                case "Perfil": profileControl.BringToFront(); break;
                case "Acerca de": aboutControl.BringToFront(); break;
                default: dashboardControl.BringToFront(); break;
            }
        }

        private void InitializeSidebarTimer() { sidebarTransitionTimer = new Timer { Interval = 5 }; sidebarTransitionTimer.Tick += SidebarTransitionTimer_Tick; }
        private void BtnToggleSidebar_Click(object sender, EventArgs e) { isSidebarCollapsed = !isSidebarCollapsed; UpdateSidebarState(); sidebarTransitionTimer.Start(); }
        private void UpdateSidebarState() { picLogo.Visible = !isSidebarCollapsed; foreach (var entry in navigationButtons) { Button button = entry.Key; string originalText = entry.Value; button.Text = isSidebarCollapsed ? "" : "   " + originalText; button.ImageAlign = isSidebarCollapsed ? ContentAlignment.MiddleCenter : ContentAlignment.MiddleLeft; button.Padding = isSidebarCollapsed ? new Padding(0) : new Padding(20, 0, 0, 0); } }
        private void SidebarTransitionTimer_Tick(object sender, EventArgs e) { int step = 20; if (isSidebarCollapsed) { if (panelSidebarContainer.Width > SIDEBAR_WIDTH_COLLAPSED) panelSidebarContainer.Width -= step; else { panelSidebarContainer.Width = SIDEBAR_WIDTH_COLLAPSED; sidebarTransitionTimer.Stop(); } } else { if (panelSidebarContainer.Width < SIDEBAR_WIDTH_EXPANDED) panelSidebarContainer.Width += step; else { panelSidebarContainer.Width = SIDEBAR_WIDTH_EXPANDED; sidebarTransitionTimer.Stop(); } } }
        private void SetActiveButton(Button activeButton) { foreach (var button in navigationButtons.Keys) { button.BackColor = colorSidebarBg; } activeButton.BackColor = colorButtonActive; }
        private Image ResizeImage(Image image, int width, int height) { if (image == null) return null; var destRect = new Rectangle(0, 0, width, height); var destImage = new Bitmap(width, height); destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution); using (var graphics = Graphics.FromImage(destImage)) { graphics.CompositingMode = CompositingMode.SourceCopy; graphics.CompositingQuality = CompositingQuality.HighQuality; graphics.InterpolationMode = InterpolationMode.HighQualityBicubic; graphics.SmoothingMode = SmoothingMode.HighQuality; graphics.PixelOffsetMode = PixelOffsetMode.HighQuality; using (var wrapMode = new System.Drawing.Imaging.ImageAttributes()) { wrapMode.SetWrapMode(WrapMode.TileFlipXY); graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode); } } return destImage; }
    }
}