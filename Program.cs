using System;
using System.Windows.Forms;
using stripMap_Editor.Forms;
using StripMapEditor.Utils;
using System.IO;

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
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            ////기본 환경 변수
            //string m_DB_IP = "";
            //string m_DB_ID = "";
            //string m_DB_PW = "";
            //string m_DB_DATABASE = "";
            //string m_DB_Timeout = "";
            //string m_DB_Encrypt = "";

            //string basePath = AppDomain.CurrentDomain.BaseDirectory;
            //string m_App_Path_ini = Path.Combine(basePath, "Config.ini");
            //string m_App_Path_Log_Dir = Path.Combine(basePath, "Log");

            ////기본 환경 설정
            //IniFileHelper config = new IniFileHelper(basePath);

            //config.Make_Dir(m_App_Path_Log_Dir);

            //if (File.Exists(m_App_Path_ini) == false)
            //{
            //    config.Write("RV", "SERVICE", "");
            //    config.Write("RV", "NETWORK", "");
            //    config.Write("RV", "DAEMON", "");
            //    config.Write("RV", "SUBJECT", "");

            //    config.Write("DB_INFO", "IP", "192.168.10.79");
            //    config.Write("DB_INFO", "ID", "sfa_test_login");
            //    config.Write("DB_INFO", "PW", "sfa_test_login");
            //    config.Write("DB_INFO", "DATABASE", "SFA_TEST_DB");

            //}
            //else
            //{
            //    m_DB_IP = config.Read("DB_INFO", "IP", m_App_Path_ini);
            //    m_DB_ID = config.Read("DB_INFO", "ID", m_App_Path_ini);
            //    m_DB_PW = config.Read("DB_INFO", "PW", m_App_Path_ini);
            //    m_DB_DATABASE = config.Read("DB_INFO", "DATABASE", m_App_Path_ini);
            //}


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
        }
    }
}
