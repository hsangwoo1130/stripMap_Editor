using System;
using System.Windows.Forms;
using stripMap_Editor.Forms;
using StripMapEditor.Utils;

namespace StripMapEditor
{
    static class Program
    {
        /// <summary>
        /// 애플리케이션의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            AppLogger.Initialize();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            LoginForm loginForm = new LoginForm();

            if (loginForm.ShowDialog() == DialogResult.OK)
            {
                // 로그인 성공 시 메인 폼 실행
                MainForm mainForm = new MainForm();
                mainForm.LoggedInUserId = loginForm.LoggedInUserId;
                mainForm.LoggedInUserName = loginForm.LoggedInUserName;
                mainForm.LoggedInUserRole = loginForm.LoggedInUserRole;

                Application.Run(mainForm);
            }

            AppLogger.Close();
        }
    }
}
