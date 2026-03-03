using System.IO;
using Serilog;

namespace StripMapEditor.Utils
{
    /// <summary>
    /// 애플리케이션 전역 로거 (Serilog 래퍼)
    /// 로그 파일 위치: &lt;실행경로&gt;\logs\stripmap_YYYYMMDD.txt (일별 롤링, 90일 보관)
    /// 모든 작업 이력은 Information 레벨로 기록
    /// </summary>
    internal static class AppLogger
    {
        /// <summary>
        /// 프로그램 시작 시 1회 호출 — 로그 파일 초기화
        /// </summary>
        public static void Initialize()
        {
            string logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            Directory.CreateDirectory(logDir);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File(
                    path: Path.Combine(logDir, "stripmap_.txt"),
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                    shared: true,
                    retainedFileCountLimit: 90)
                .CreateLogger();
        }

        /// <summary>
        /// 프로그램 종료 시 1회 호출 — 로그 버퍼 플러시
        /// </summary>
        public static void Close() => Log.CloseAndFlush();

        /// <summary>
        /// Information 레벨 로그 기록
        /// </summary>
        public static void Info(string message) => Log.Information(message);

    }
}
