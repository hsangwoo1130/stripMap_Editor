using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using StripMapEditor.Utils;

namespace StripMapEditor.Database
{
    /// <summary>
    /// 데이터베이스 연결 및 쿼리 실행을 담당하는 헬퍼 클래스 (INI 파일 사용)
    /// </summary>
    public class DatabaseHelper
    {
        private static string _connectionString;
        private static IniFileHelper _iniFile;
        private static string _iniFilePath;

        /// <summary>
        /// 연결 문자열 초기화 (INI 파일에서 읽기)
        /// </summary>
        static DatabaseHelper()
        {
            _iniFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.ini");
            _iniFile = new IniFileHelper(_iniFilePath);

            if (!_iniFile.FileExists())
                CreateDefaultIniFile();

            LoadConnectionString();
        }

        // ─────────────────────────────────────────────
        // 내부 초기화
        // ─────────────────────────────────────────────

        /// <summary>
        /// 기본 INI 파일 생성 (config.ini 없을 때)
        /// </summary>
        private static void CreateDefaultIniFile()
        {
            _iniFile.Write("Database", "Server",   "");
            _iniFile.Write("Database", "Database", "");
            _iniFile.Write("Database", "UserId",   "");
            _iniFile.Write("Database", "Password", "");
            _iniFile.Write("Database", "Timeout",  "5");
            _iniFile.Write("Database", "Encrypt",  "false");
        }

        /// <summary>
        /// INI 파일에서 연결 문자열 로드 (내부 전용)
        /// </summary>
        private static void LoadConnectionString()
        {
            try
            {
                string server   = _iniFile.Read("Database", "Server",   "");
                string database = _iniFile.Read("Database", "Database", "");
                string userId   = _iniFile.Read("Database", "UserId",   "");
                string password = _iniFile.Read("Database", "Password", "");
                string timeout  = _iniFile.Read("Database", "Timeout",  "5");
                string encrypt  = _iniFile.Read("Database", "Encrypt",  "false");

                // 필수 값 유효성 검사
                if (string.IsNullOrWhiteSpace(server))
                    throw new InvalidOperationException("config.ini [Database] Server 값이 비어 있습니다.");
                if (string.IsNullOrWhiteSpace(database))
                    throw new InvalidOperationException("config.ini [Database] Database 값이 비어 있습니다.");
                if (string.IsNullOrWhiteSpace(userId))
                    throw new InvalidOperationException("config.ini [Database] UserId 값이 비어 있습니다.");

                _connectionString = $"Server={server};Database={database};User Id={userId};Password={password};Connect Timeout={timeout};Encrypt={encrypt};";
            }
            catch (Exception ex)
            {
                throw new Exception($"INI 파일 로드 중 오류 발생: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 새로운 데이터베이스 연결 반환 (내부 전용)
        /// </summary>
        private static SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        // ─────────────────────────────────────────────
        // 연결 테스트
        // ─────────────────────────────────────────────

        /// <summary>
        /// 연결 테스트
        /// </summary>
        public static bool TestConnection(out string errorMessage)
        {
            errorMessage = string.Empty;
            try
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    return true;
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        // ─────────────────────────────────────────────
        // 쿼리 실행
        // ─────────────────────────────────────────────

        /// <summary>
        /// SELECT 쿼리 실행 (DataTable 반환)
        /// </summary>
        public static DataTable ExecuteQuery(string query, SqlParameter[] parameters = null)
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection conn = GetConnection())
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (parameters != null)
                        cmd.Parameters.AddRange(parameters);

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        conn.Open();
                        adapter.Fill(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"쿼리 실행 중 오류 발생: {ex.Message}", ex);
            }
            return dt;
        }

        /// <summary>
        /// INSERT / UPDATE / DELETE 쿼리 실행 (영향받은 행 수 반환)
        /// </summary>
        public static int ExecuteNonQuery(string query, SqlParameter[] parameters = null)
        {
            try
            {
                using (SqlConnection conn = GetConnection())
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (parameters != null)
                        cmd.Parameters.AddRange(parameters);

                    conn.Open();
                    return cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"쿼리 실행 중 오류 발생: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 단일 값 반환 (COUNT, MAX 등)
        /// </summary>
        public static object ExecuteScalar(string query, SqlParameter[] parameters = null)
        {
            try
            {
                using (SqlConnection conn = GetConnection())
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (parameters != null)
                        cmd.Parameters.AddRange(parameters);

                    conn.Open();
                    return cmd.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"쿼리 실행 중 오류 발생: {ex.Message}", ex);
            }
        }

        // ─────────────────────────────────────────────
        // Stored Procedure 실행
        // ─────────────────────────────────────────────

        /// <summary>
        /// Stored Procedure 실행 (DataTable 반환)
        /// </summary>
        public static DataTable ExecuteStoredProcedure(string procedureName, SqlParameter[] parameters = null)
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection conn = GetConnection())
                using (SqlCommand cmd = new SqlCommand(procedureName, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    if (parameters != null)
                        cmd.Parameters.AddRange(parameters);

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        conn.Open();
                        adapter.Fill(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Stored Procedure 실행 중 오류 발생: {ex.Message}", ex);
            }
            return dt;
        }

        /// <summary>
        /// Stored Procedure 실행 (영향받은 행 수 반환)
        /// SqlException (SP의 THROW 에러 50001~50030 등)은 직접 rethrow하여 호출부에서 Number로 분기 가능
        /// </summary>
        public static int ExecuteStoredProcedureNonQuery(string procedureName, SqlParameter[] parameters = null)
        {
            try
            {
                using (SqlConnection conn = GetConnection())
                using (SqlCommand cmd = new SqlCommand(procedureName, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    if (parameters != null)
                        cmd.Parameters.AddRange(parameters);

                    conn.Open();
                    return cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException)
            {
                throw;  // SP THROW 에러 코드 보존 (50001/50002/50010 등)
            }
            catch (Exception ex)
            {
                throw new Exception($"Stored Procedure 실행 중 오류 발생: {ex.Message}", ex);
            }
        }

        // ─────────────────────────────────────────────
        // 트랜잭션
        // ─────────────────────────────────────────────

        /// <summary>
        /// 트랜잭션 처리 (여러 쿼리를 원자적으로 실행)
        /// </summary>
        public static bool ExecuteTransaction(Action<SqlConnection, SqlTransaction> action, out string errorMessage)
        {
            errorMessage = string.Empty;
            try
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    using (SqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            action(conn, transaction);
                            transaction.Commit();
                            return true;
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        // ─────────────────────────────────────────────
        // 권한 / 메뉴 로드
        // ─────────────────────────────────────────────

        /// <summary>
        /// 역할 계층 반환 (상위 역할 포함)
        /// SUPER → ["SUPER", "ADMIN"]  (ADMIN 권한 상속)
        /// ADMIN → ["ADMIN"]
        /// USER  → ["USER"]
        /// </summary>
        private static string[] GetEffectiveRoles(string roleId)
        {
            if (string.Equals(roleId, "SUPER", StringComparison.OrdinalIgnoreCase))
                return new[] { "SUPER", "ADMIN" };
            return new[] { roleId };
        }

        /// <summary>
        /// 역할별 기능 권한 로드 (상위 역할 상속 포함)
        /// </summary>
        public static HashSet<string> LoadRolePermissions(string roleId)
        {
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            string[] roles = GetEffectiveRoles(roleId);

            var prms = roles.Select((r, i) => new SqlParameter($"@r{i}", r)).ToArray();
            string inClause = string.Join(",", prms.Select(p => p.ParameterName));

            string sql = $@"
                SELECT DISTINCT rf.functionId
                FROM   dbo.tblRoleFunction rf
                INNER  JOIN dbo.tblFunction f ON f.functionId = rf.functionId
                WHERE  rf.roleId IN ({inClause})
                AND    f.isActive = 1";

            DataTable dt = ExecuteQuery(sql, prms);
            foreach (DataRow row in dt.Rows)
                result.Add(row["functionId"].ToString());
            return result;
        }

        /// <summary>
        /// 전체 메뉴 정보 로드 (menuId → menuName, menuUrl)
        /// </summary>
        public static Dictionary<string, (string menuName, string menuUrl)> LoadMenuInfo()
        {
            var result = new Dictionary<string, (string, string)>(StringComparer.OrdinalIgnoreCase);

            string sql = @"
                SELECT menuId, menuName, ISNULL(menuUrl, '') AS menuUrl
                FROM   dbo.tblMenu
                WHERE  isActive = 1";

            DataTable dt = ExecuteQuery(sql);
            foreach (DataRow row in dt.Rows)
                result[row["menuId"].ToString()] = (row["menuName"].ToString(), row["menuUrl"].ToString());

            return result;
        }

        /// <summary>
        /// 역할별 메뉴 권한 로드 (상위 역할 상속 포함, canView=1)
        /// </summary>
        public static HashSet<string> LoadRoleMenus(string roleId)
        {
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            string[] roles = GetEffectiveRoles(roleId);

            var prms = roles.Select((r, i) => new SqlParameter($"@r{i}", r)).ToArray();
            string inClause = string.Join(",", prms.Select(p => p.ParameterName));

            string sql = $@"
                SELECT DISTINCT rm.menuId
                FROM   dbo.tblRoleMenu rm
                INNER  JOIN dbo.tblMenu m ON m.menuId = rm.menuId
                WHERE  rm.roleId IN ({inClause})
                AND    rm.canView = 1
                AND    m.isActive = 1";

            DataTable dt = ExecuteQuery(sql, prms);
            foreach (DataRow row in dt.Rows)
                result.Add(row["menuId"].ToString());
            return result;
        }
    }
}
