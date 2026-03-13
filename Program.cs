using System;
using System.IO;
using System.Threading;
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

            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            LoginForm loginForm = new LoginForm();

            if (loginForm.ShowDialog() == DialogResult.OK)
            {
                RvManager rv = CreateRvManager(loginForm.RvSendEnabled);
                if (rv == null) return;

                using (rv)
                {
                    MainForm mainForm = new MainForm();
                    mainForm.LoggedInUserId   = loginForm.LoggedInUserId;
                    mainForm.LoggedInUserName = loginForm.LoggedInUserName;
                    mainForm.LoggedInUserRole = loginForm.LoggedInUserRole;
                    mainForm.Rv = rv;

                    Application.Run(mainForm);
                }
            }

            AppLogger.Close();
        }

        /// <summary>
        /// UI 스레드 미처리 예외 핸들러
        /// </summary>
        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            AppLogger.Info($"[UNHANDLED_EXCEPTION] UI Thread: {e.Exception}");
            MessageBox.Show(
                $"예기치 않은 오류가 발생했습니다.\n\n{e.Exception.Message}",
                "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// 비-UI 스레드 미처리 예외 핸들러
        /// </summary>
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            AppLogger.Info($"[UNHANDLED_EXCEPTION] Domain: {ex?.ToString() ?? e.ExceptionObject.ToString()}");
            AppLogger.Close();

            MessageBox.Show(
                $"치명적인 오류가 발생하여 프로그램을 종료합니다.\n\n{ex?.Message ?? "알 수 없는 오류"}",
                "치명적 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// config.ini [RV] 섹션을 읽어 RvManager를 생성하고 초기화합니다.
        /// Service/Network/Daemon/Subject 중 하나라도 비어 있으면 RV 기능을 비활성화합니다.
        /// </summary>
        private static RvManager CreateRvManager(bool rvSendEnabled)
        {
            string iniPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.ini");
            var ini = new IniFileHelper(iniPath);

            string service = ini.Read("RV", "Service", "");
            string network = ini.Read("RV", "Network", "");
            string daemon  = ini.Read("RV", "Daemon",  "");
            string subject = ini.Read("RV", "Subject", "");
            var rv = new RvManager
            {
                Service        = service,
                Network        = network,
                Daemon         = daemon,
                Subject        = subject,
                SimulationMode = !rvSendEnabled
            };

            if (!rvSendEnabled)
            {
                AppLogger.Info("[RV] 시뮬레이션 모드 — 실제 TIBCO 연결 없이 로그만 기록합니다.");
                rv.RvInit();
                rv.RvConnect();
                return rv;
            }

            if (string.IsNullOrWhiteSpace(service)
                || string.IsNullOrWhiteSpace(daemon) || string.IsNullOrWhiteSpace(subject))
            {
                AppLogger.Info("[RV] config.ini [RV] 설정이 비어 있어 프로그램을 종료합니다.");
                MessageBox.Show(
                    "RV 연결 정보가 설정되지 않았습니다.\nconfig.ini [RV] 섹션의 Service / Daemon / Subject를 입력 후 재시작하세요.",
                    "RV 연결 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            if (!rv.RvInit())
            {
                AppLogger.Info("[RV] 초기화 실패 — 프로그램을 종료합니다.");
                MessageBox.Show(
                    "TIBCO Rendezvous 초기화에 실패했습니다.\nTIBCO RV 설치 여부를 확인하세요.",
                    "RV 연결 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            if (!rv.RvConnect())
            {
                AppLogger.Info("[RV] 연결 실패 — 프로그램을 종료합니다.");
                MessageBox.Show(
                    "TIBCO Rendezvous 연결에 실패했습니다.\nconfig.ini [RV] 설정값을 확인하세요.",
                    "RV 연결 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            return rv;
        }
    }
}
