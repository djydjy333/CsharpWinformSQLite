using System;
using System.IO;
using System.Windows.Forms;

namespace TaskManager
{
    static class Program
    {
        public static string DbPath { get; private set; }
        public static string ConnectionString { get; private set; }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            DbPath = Path.Combine(Application.StartupPath, "tasks.db");
            ConnectionString = "Data Source=" + DbPath + ";Version=3;";

            while (true)
            {
                using (LoginForm loginForm = new LoginForm())
                {
                    DialogResult loginResult = loginForm.ShowDialog();

                    if (loginResult != DialogResult.OK)
                        break;

                    using (MainForm mainForm = new MainForm(loginForm.LoggedInUser))
                    {
                        DialogResult mainResult = mainForm.ShowDialog();

                        if (mainResult != DialogResult.Retry)
                            break;
                    }
                }
            }
        }
    }
}
