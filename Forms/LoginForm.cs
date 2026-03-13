using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using Konscious.Security.Cryptography;
using StripMapEditor.Database;
using StripMapEditor.Utils;
using stripMap_Editor;

namespace stripMap_Editor.Forms
{
    public partial class LoginForm : Form
    {
        public string LoggedInUserId { get; private set; }
        public string LoggedInUserName { get; private set; }
        public string LoggedInUserRole { get; private set; }

        /// <summary>
        /// 로그인 폼의 "RV 메시지 전송" 체크박스 상태.
        /// false이면 Program.cs에서 SimulationMode=true로 설정하여 RV 연결을 건너뜁니다.
        /// </summary>
        public bool RvSendEnabled => checkBox_RvSend.Checked;

        private Bitmap _logoBitmap;

        public LoginForm()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                if (Properties.Resources.SFA_logo != null)
                {
                    _logoBitmap = new Bitmap(Properties.Resources.SFA_logo);
                    _logoBitmap.MakeTransparent(Color.White);
                    pictureBox_Login_Logo.Image = _logoBitmap;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"로고 로드 실패: {ex.Message}");
            }

            textBoxUserId.Focus();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string userId  = textBoxUserId.Text.Trim();
            string password = textBoxPassword.Text.Trim();

            if (string.IsNullOrEmpty(userId))
            {
                MessageBox.Show("아이디를 입력하세요.", "알림",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxUserId.Focus();
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("비밀번호를 입력하세요.", "알림",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxPassword.Focus();
                return;
            }

            if (ValidateLogin(userId, password))
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        // ─────────────────────────────────────────────
        // DB 로그인 검증
        // ─────────────────────────────────────────────
        private bool ValidateLogin(string userId, string password)
        {
            try
            {
                // tblUser + tblUserRole + tblRole JOIN 조회 (isActive 필터 없음 — 비밀번호 검증 후 체크)
                string query = @"
                    SELECT u.userId,
                           u.userName,
                           u.passwordHash,
                           u.isActive,
                           r.roleId
                    FROM   dbo.tblUser     u
                    INNER  JOIN dbo.tblUserRole ur ON u.userId = ur.userId
                    INNER  JOIN dbo.tblRole      r  ON ur.roleId = r.roleId
                    WHERE  u.userId = @userId";

                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@userId", SqlDbType.VarChar, 20) { Value = userId }
                };

                DataTable result = DatabaseHelper.ExecuteQuery(query, parameters);

                // 존재하지 않는 아이디
                if (result.Rows.Count == 0)
                {
                    ShowLoginFailed();
                    return false;
                }

                // Argon2id 비밀번호 검증
                string storedHash = result.Rows[0]["passwordHash"].ToString();
                if (!VerifyPassword(password, storedHash))
                {
                    ShowLoginFailed();
                    return false;
                }

                // 비밀번호 일치 후 isActive 체크
                bool isActive = Convert.ToInt32(result.Rows[0]["isActive"]) == 1;
                if (!isActive)
                {
                    MessageBox.Show("접근이 제한된 계정입니다.", "로그인 실패",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                // 가장 높은 권한 Role 선택 (SUPER > ADMIN > USER)
                LoggedInUserId   = result.Rows[0]["userId"].ToString();
                LoggedInUserName = result.Rows[0]["userName"].ToString();
                LoggedInUserRole = SelectHighestRole(result);

                return true;
            }
            catch (TypeInitializationException ex)
            {
                // DatabaseHelper static 생성자 실패 → config.ini 설정 문제
                string detail = ex.InnerException?.Message ?? ex.Message;
                MessageBox.Show(
                    $"DB 설정 오류가 있습니다.\n\n{detail}\n\nconfig.ini 파일을 확인하세요.",
                    "DB 설정 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            catch (Exception ex) when (HasSqlException(ex))
            {
                // DB 서버 접속 실패 (잘못된 서버/DB명/계정 등)
                string detail = GetInnerMostMessage(ex);
                MessageBox.Show(
                    $"DB 연결에 실패했습니다.\n\nconfig.ini 파일을 확인하세요.\n\n({detail})",
                    "DB 연결 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"로그인 중 오류 발생:\n{ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        // ─────────────────────────────────────────────
        // 예외 체인 분석 헬퍼
        // ─────────────────────────────────────────────

        /// <summary>
        /// 예외 체인에 SqlException이 포함되어 있는지 확인
        /// </summary>
        private static bool HasSqlException(Exception ex)
        {
            while (ex != null)
            {
                if (ex is System.Data.SqlClient.SqlException) return true;
                ex = ex.InnerException;
            }
            return false;
        }

        /// <summary>
        /// 가장 안쪽(근본 원인) 예외 메시지 반환
        /// </summary>
        private static string GetInnerMostMessage(Exception ex)
        {
            while (ex.InnerException != null)
                ex = ex.InnerException;
            return ex.Message;
        }

        // ─────────────────────────────────────────────
        // Argon2id 비밀번호 검증
        // 저장 형식: {salt_base64}:{hash_base64}
        // ─────────────────────────────────────────────
        private bool VerifyPassword(string password, string storedHash)
        {
            try
            {
                string[] parts = storedHash.Split(':');
                if (parts.Length != 2) return false;

                byte[] salt         = Convert.FromBase64String(parts[0]);
                byte[] expectedHash = Convert.FromBase64String(parts[1]);
                byte[] actualHash   = ComputeArgon2id(password, salt);

                return CryptographicEquals(actualHash, expectedHash);
            }
            catch (Exception ex)
            {
                AppLogger.Info($"[WARN] 비밀번호 검증 중 예외 발생: {ex.Message}");
                return false;
            }
        }

        // ─────────────────────────────────────────────
        // Argon2id 해시 계산
        // ─────────────────────────────────────────────
        private byte[] ComputeArgon2id(string password, byte[] salt)
        {
            using (var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password)))
            {
                argon2.Salt                = salt;
                argon2.DegreeOfParallelism = SecurityConstants.ARGON2_PARALLELISM;
                argon2.MemorySize          = SecurityConstants.ARGON2_MEMORY_SIZE;
                argon2.Iterations          = SecurityConstants.ARGON2_ITERATIONS;
                return argon2.GetBytes(SecurityConstants.ARGON2_HASH_LENGTH);
            }
        }

        // ─────────────────────────────────────────────
        // 계정 등록 / 비밀번호 변경 시 해시 생성용 (public static)
        // 반환값을 tblUser.passwordHash 컬럼에 저장
        // ─────────────────────────────────────────────
        public static string CreatePasswordHash(string password)
        {
            byte[] salt = new byte[SecurityConstants.ARGON2_SALT_LENGTH];
            using (var rng = new RNGCryptoServiceProvider())
                rng.GetBytes(salt);

            byte[] hash;
            using (var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password)))
            {
                argon2.Salt                = salt;
                argon2.DegreeOfParallelism = SecurityConstants.ARGON2_PARALLELISM;
                argon2.MemorySize          = SecurityConstants.ARGON2_MEMORY_SIZE;
                argon2.Iterations          = SecurityConstants.ARGON2_ITERATIONS;
                hash = argon2.GetBytes(SecurityConstants.ARGON2_HASH_LENGTH);
            }

            return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
        }

        // ─────────────────────────────────────────────
        // 상수 시간 비교 (타이밍 공격 방지)
        // ─────────────────────────────────────────────
        private static bool CryptographicEquals(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            int diff = 0;
            for (int i = 0; i < a.Length; i++)
                diff |= a[i] ^ b[i];
            return diff == 0;
        }

        // ─────────────────────────────────────────────
        // 다중 Role 중 가장 높은 권한 선택
        // ─────────────────────────────────────────────
        private static string SelectHighestRole(DataTable result)
        {
            var priority = new Dictionary<string, int>
            {
                { "SUPER", 3 },
                { "ADMIN", 2 },
                { "USER",  1 }
            };

            string highest = "USER";
            int highestPriority = 0;

            foreach (DataRow row in result.Rows)
            {
                string role = row["roleId"].ToString().ToUpper();
                if (priority.TryGetValue(role, out int p) && p > highestPriority)
                {
                    highestPriority = p;
                    highest = role;
                }
            }

            return highest;
        }

        // ─────────────────────────────────────────────
        // 로그인 실패 메시지 (아이디/비밀번호 구분 안 함 - 보안)
        // ─────────────────────────────────────────────
        private void ShowLoginFailed()
        {
            MessageBox.Show("아이디 또는 비밀번호가 올바르지 않습니다.", "로그인 실패",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            textBoxPassword.Clear();
            textBoxUserId.Focus();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void textBoxPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                btnLogin_Click(sender, e);
        }

        private void textBoxUserId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                textBoxPassword.Focus();
        }
    }
}
