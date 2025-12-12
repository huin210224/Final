using System;
using System.Windows.Forms;

namespace StaffApp
{
    public static class Program
    {
        public static StaffLogin LoginForm;
        public static Dashboard DashboardForm;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            LoginForm = new StaffLogin();
            Application.Run(LoginForm);
        }

        // LOGIN SUCCESS
        public static void LoginSuccess(string staffName)
        {
            LoginForm.Hide();
            DashboardForm = new Dashboard(staffName);
            DashboardForm.Show();
        }

        // LOGOUT
        public static void Logout()
        {
            DashboardForm.Hide();
            LoginForm.ClearFields();   // Reset textboxes
            LoginForm.Show();
        }
    }
}
