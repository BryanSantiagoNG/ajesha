using System;
using System.Windows.Forms;

namespace ajesha
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            frmLogin loginForm = new frmLogin();

            loginForm.ShowDialog();

            if (loginForm.DialogResult == DialogResult.OK)
            {
                Application.Run(new frmMain(loginForm.LoggedInUserId, loginForm.LoggedInUserRole, loginForm.LoggedInFullName));
            }
        }
    }
}