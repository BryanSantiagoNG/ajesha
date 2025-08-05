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
        // --- Datos del Usuario Logueado ---
        private readonly int currentUserId;
        private readonly string currentUserRole;

        // --- Controles de la Interfaz ---
        private Panel panelSidebar;
        private Panel panelMainContent;
        private Panel panelHeader;
        private PictureBox picLogo;
        private PictureBox btnToggleSidebar;
        private Label lblSectionTitle;
        private Timer sidebarTransitionTimer;

        // --- Controles de Usuario para cada sección ---
        private ucProfile profileControl;
        private ucAdminSistema adminSistemaControl;
        private ucAdmin_Settings adminSettingsControl;
        private ucAdmin_Reports adminReportsControl;
        private ucAbout aboutControl;
        private ucDashboard dashboardControl;
        private ucNotifications notificationsControl;
        private ucCoordinator_StudentManagement coordStudentControl;
        private ucCoordinator_Enrollments coordEnrollmentControl;
        private ucCoordinator_CourseManagement coordCourseControl;
        private ucCoordinator_AssignCourses coordAssignControl;
        private ucCoordinator_Notifications coordNotificationControl;
        private ucCoordinator_Files coordFileControl;
        private ucCoordinator_Requests coordRequestControl;
        private ucCoordinator_AcademicTracking coordTrackingControl;
        private ucCoordinator_Reports coordReportControl;
        private ucTeacher_Schedule teacherScheduleControl;
        private ucTeacher_CourseManagement teacherCourseControl;
        private ucTeacher_Announcements teacherAnnounceControl;
        private ucTeacherGradeSubmission teacherGradeControl;
        private ucStudentClassroom studentClassroomControl;
        private ucStudentGrades studentGradesControl;
        private ucStudentFinance studentFinanceControl;
        private ucStudentAttendance studentAttendanceControl;
        private ucStudentSubmitAssignment studentSubmitControl;

        // Diccionario para gestionar los botones de navegación
        private readonly Dictionary<Button, string> navigationButtons = new Dictionary<Button, string>();

        // --- Estado y Constantes de Diseño ---
        private bool isSidebarCollapsed = false;
        private const int SIDEBAR_WIDTH_EXPANDED = 230;
        private const int SIDEBAR_WIDTH_COLLAPSED = 70;

        // --- Paleta de Colores ---
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
            if (navigationButtons.Keys.Any(b => b.Tag.ToString() == "Inicio"))
            {
                SetActiveButton(navigationButtons.Keys.First(b => b.Tag.ToString() == "Inicio"));
            }
        }

        private void InitializeUserControls()
        {
            profileControl = new ucProfile();
            adminSistemaControl = new ucAdminSistema();
            adminSettingsControl = new ucAdmin_Settings();
            adminReportsControl = new ucAdmin_Reports();
            aboutControl = new ucAbout();
            dashboardControl = new ucDashboard();
            notificationsControl = new ucNotifications();
            coordStudentControl = new ucCoordinator_StudentManagement();
            coordEnrollmentControl = new ucCoordinator_Enrollments();
            coordCourseControl = new ucCoordinator_CourseManagement();
            coordAssignControl = new ucCoordinator_AssignCourses();
            coordNotificationControl = new ucCoordinator_Notifications();
            coordFileControl = new ucCoordinator_Files();
            coordRequestControl = new ucCoordinator_Requests();
            coordTrackingControl = new ucCoordinator_AcademicTracking();
            coordReportControl = new ucCoordinator_Reports();
            teacherScheduleControl = new ucTeacher_Schedule();
            teacherCourseControl = new ucTeacher_CourseManagement();
            teacherAnnounceControl = new ucTeacher_Announcements();
            teacherGradeControl = new ucTeacherGradeSubmission();
            studentClassroomControl = new ucStudentClassroom();
            studentGradesControl = new ucStudentGrades();
            studentFinanceControl = new ucStudentFinance();
            studentAttendanceControl = new ucStudentAttendance();
            studentSubmitControl = new ucStudentSubmitAssignment();

            studentSubmitControl.BackButtonClicked += (s, e) => { studentClassroomControl.BringToFront(); lblSectionTitle.Text = "Classroom"; };
            teacherGradeControl.BackButtonClicked += (s, e) => { teacherCourseControl.BringToFront(); lblSectionTitle.Text = "Gestión de Cursos"; };

            InitializePanelData();

            panelMainContent.Controls.AddRange(new Control[] {
                dashboardControl, profileControl, adminSistemaControl, adminSettingsControl, adminReportsControl, aboutControl, notificationsControl,
                coordStudentControl, coordEnrollmentControl, coordCourseControl, coordAssignControl, coordNotificationControl, coordFileControl, coordRequestControl, coordTrackingControl, coordReportControl,
                teacherScheduleControl, teacherCourseControl, teacherAnnounceControl, teacherGradeControl,
                studentClassroomControl, studentGradesControl, studentFinanceControl, studentAttendanceControl, studentSubmitControl
            });
        }

        private void InitializePanelData()
        {
            profileControl.LoadProfileData(this.currentUserId, this.currentUserRole);
            coordRequestControl.SetCoordinatorId(this.currentUserId);
            coordNotificationControl.SetCoordinatorId(this.currentUserId);
            coordFileControl.SetCoordinatorId(this.currentUserId);
            teacherScheduleControl.SetTeacherId(this.currentUserId);
            teacherCourseControl.SetTeacherId(this.currentUserId);
            teacherAnnounceControl.SetTeacherId(this.currentUserId);
            studentClassroomControl.SetStudentId(this.currentUserId);
            studentGradesControl.SetStudentId(this.currentUserId);
            studentFinanceControl.SetStudentId(this.currentUserId);
            studentAttendanceControl.SetStudentId(this.currentUserId);
        }

        private void InitializeComponents()
        {
            panelSidebar = new Panel { Dock = DockStyle.Left, Width = SIDEBAR_WIDTH_EXPANDED, BackColor = colorSidebarBg };
            panelMainContent = new Panel { Dock = DockStyle.Fill, BackColor = colorMainBg };
            this.Controls.Add(panelMainContent);
            this.Controls.Add(panelSidebar);

            panelHeader = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = colorHeaderBg };
            lblSectionTitle = new Label { Text = "", ForeColor = colorFont, Font = new Font("Segoe UI", 14F, FontStyle.Bold), Location = new Point(20, 15), AutoSize = true };
            panelHeader.Controls.Add(lblSectionTitle);
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
            picLogo = new PictureBox 
            { 
                Location = new Point((SIDEBAR_WIDTH_EXPANDED - 100) / 2, 60), 
                Size = new Size(100, 100), 
                SizeMode = PictureBoxSizeMode.Zoom, 
                Image = Properties.Resources.logo, 
                BackColor = Color.White 
            };

            var buttonDefinitions = new List<Tuple<string, Image, string[]>>
            {
                Tuple.Create("Inicio", ResizeImage(Properties.Resources.home, iconSize, iconSize), new[] { "admin", "coordinator", "teacher", "student" }),
                Tuple.Create("Notificaciones", ResizeImage(Properties.Resources.notificacion, iconSize, iconSize), new[] { "admin", "coordinator", "teacher", "student" }),
                Tuple.Create("Perfil", ResizeImage(Properties.Resources.perfil, iconSize, iconSize), new[] { "admin", "coordinator", "teacher", "student" }),
                Tuple.Create("Admin Sistema", ResizeImage(Properties.Resources.admin, iconSize, iconSize), new[] { "admin" }),
                Tuple.Create("Configuración", ResizeImage(Properties.Resources.settings, iconSize, iconSize), new[] { "admin" }),
                Tuple.Create("Reportes Globales", ResizeImage(Properties.Resources.reports, iconSize, iconSize), new[] { "admin" }),
                Tuple.Create("Gestión Estudiantes", ResizeImage(Properties.Resources.admin, iconSize, iconSize), new[] { "coordinator" }),
                Tuple.Create("Gestión de Cursos", ResizeImage(Properties.Resources.courses, iconSize, iconSize), new[] { "coordinator" }),
                Tuple.Create("Seguimiento Académico", ResizeImage(Properties.Resources.tracking, iconSize, iconSize), new[] { "coordinator" }),
                Tuple.Create("Reportes", ResizeImage(Properties.Resources.reports, iconSize, iconSize), new[] { "coordinator" }),
                Tuple.Create("Inscripciones", ResizeImage(Properties.Resources.classroom, iconSize, iconSize), new[] { "coordinator" }),
                Tuple.Create("Asignar Cursos", ResizeImage(Properties.Resources.assign, iconSize, iconSize), new[] { "coordinator" }),
                Tuple.Create("Enviar Notificación", ResizeImage(Properties.Resources.notificacion, iconSize, iconSize), new[] { "coordinator" }),
                Tuple.Create("Gestionar Archivos", ResizeImage(Properties.Resources.files, iconSize, iconSize), new[] { "coordinator" }),
                Tuple.Create("Solicitudes", ResizeImage(Properties.Resources.requests, iconSize, iconSize), new[] { "coordinator" }),
                Tuple.Create("Mi Horario", ResizeImage(Properties.Resources.schedule, iconSize, iconSize), new[] { "teacher" }),
                Tuple.Create("Gestión de Materias", ResizeImage(Properties.Resources.courses, iconSize, iconSize), new[] { "teacher" }),
                Tuple.Create("Enviar Anuncios", ResizeImage(Properties.Resources.notificacion, iconSize, iconSize), new[] { "teacher" }),
                Tuple.Create("Classroom", ResizeImage(Properties.Resources.classroom, iconSize, iconSize), new[] { "student" }),
                Tuple.Create("Calificaciones", ResizeImage(Properties.Resources.calificacion, iconSize, iconSize), new[] { "student" }),
                Tuple.Create("Mi Asistencia", ResizeImage(Properties.Resources.attendance, iconSize, iconSize), new[] { "student" }),
                Tuple.Create("Finanzas y Trámites", ResizeImage(Properties.Resources.finanzas, iconSize, iconSize), new[] { "student" })
            };

            int topPosition = 180;
            foreach (var def in buttonDefinitions)
            {
                if (def.Item3.Contains(this.currentUserRole))
                {
                    var button = CreateNavButton(def.Item1, topPosition, def.Item2);
                    panelSidebar.Controls.Add(button);
                    topPosition += 45;
                }
            }

            var aboutButton = CreateNavButton("Acerca de", 0, ResizeImage(Properties.Resources.info, iconSize, iconSize));
            aboutButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            aboutButton.Location = new Point(0, this.ClientSize.Height - aboutButton.Height);
            this.SizeChanged += (s, e) => { aboutButton.Location = new Point(0, this.ClientSize.Height - aboutButton.Height); };

            panelSidebar.Controls.Add(btnToggleSidebar);
            panelSidebar.Controls.Add(picLogo);
            panelSidebar.Controls.Add(aboutButton);
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

        private void NavigationButton_Click(object sender, EventArgs e)
        {
            var clickedButton = (Button)sender;
            lblSectionTitle.Text = clickedButton.Tag.ToString();
            SetActiveButton(clickedButton);

            switch (clickedButton.Tag.ToString())
            {
                // Casual
                case "Inicio": dashboardControl.BringToFront(); break;
                case "Notificaciones": notificationsControl.BringToFront(); break;
                case "Perfil": profileControl.BringToFront(); break;
                case "Acerca de": aboutControl.BringToFront(); break;
                // Admin
                case "Admin Sistema": adminSistemaControl.BringToFront(); break;
                case "Configuración": adminSettingsControl.BringToFront(); break;
                case "Reportes Globales": adminReportsControl.BringToFront(); break;
                // Coordinador
                case "Gestión Estudiantes": coordStudentControl.BringToFront(); break;
                case "Gestión de Cursos": coordCourseControl.BringToFront(); break;
                case "Seguimiento Académico": coordTrackingControl.BringToFront(); break;
                case "Reportes": coordReportControl.BringToFront(); break;
                case "Inscripciones": coordEnrollmentControl.BringToFront(); break;
                case "Asignar Cursos": coordAssignControl.BringToFront(); break;
                case "Enviar Notificación": coordNotificationControl.BringToFront(); break;
                case "Gestionar Archivos": coordFileControl.BringToFront(); break;
                case "Solicitudes": coordRequestControl.BringToFront(); break;
                // Maestro
                case "Mi Horario": teacherScheduleControl.BringToFront(); break;
                case "Gestión de Materias": teacherCourseControl.BringToFront(); break;
                case "Enviar Anuncios": teacherAnnounceControl.BringToFront(); break;
                // Estudiante
                case "Classroom": studentClassroomControl.BringToFront(); break;
                case "Calificaciones": studentGradesControl.BringToFront(); break;
                case "Mi Asistencia": studentAttendanceControl.BringToFront(); break;
                case "Finanzas y Trámites": studentFinanceControl.BringToFront(); break;
                default: dashboardControl.BringToFront(); break;
            }
        }

        public void NavigateTo_StudentSubmission(int studentId, int assignmentId) 
        { 
            studentSubmitControl.LoadAssignment(studentId, assignmentId); 
            studentSubmitControl.BringToFront(); 
            lblSectionTitle.Text = "Entregar Tarea"; 
        }
        public void NavigateTo_TeacherGrading(int submissionId) 
        { 
            teacherGradeControl.LoadSubmission(submissionId);
            teacherGradeControl.BringToFront();
            lblSectionTitle.Text = "Calificar Entrega"; 
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
        private void SetActiveButton(Button activeButton) 
        { 
            foreach (var button in navigationButtons.Keys) 
            { 
                button.BackColor = colorSidebarBg; 
            } 
            activeButton.BackColor = colorButtonActive; 
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