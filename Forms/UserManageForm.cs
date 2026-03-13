using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using StripMapEditor.Database;
using StripMapEditor.Utils;

namespace stripMap_Editor.Forms
{
    /// <summary>
    /// 사용자 관리 팝업 — SUPER 권한 전용
    /// 사용자 등록(INSERT) 및 권한/활성 수정(UPDATE) 기능 제공
    /// </summary>
    public partial class UserManageForm : Form
    {
        private readonly string _adminUserId;   // 현재 로그인한 관리자 userId
        private readonly string _adminUserName; // 현재 로그인한 관리자 userName

        // 현재 목록에서 선택된 userId
        private string _selectedUserId = null;

        public UserManageForm(string adminUserId, string adminUserName)
        {
            InitializeComponent();
            _adminUserId   = adminUserId   ?? string.Empty;
            _adminUserName = adminUserName ?? string.Empty;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            comboBoxRoleReg.SelectedIndex  = 0; // 기본값: USER
            SetEditPanelEnabled(false);
            LoadUserList();
        }

        // ── 사용자 목록 조회 ─────────────────────────────────────────

        private void LoadUserList()
        {
            try
            {
                string sql = @"
                    SELECT u.userId,
                           u.userName,
                           r.roleId,
                           u.isActive,
                           CONVERT(VARCHAR(19), u.createdTime, 120) AS createdTime
                    FROM   dbo.tblUser     u
                    INNER  JOIN dbo.tblUserRole ur ON u.userId = ur.userId
                    INNER  JOIN dbo.tblRole      r  ON ur.roleId = r.roleId
                    ORDER  BY u.createdTime DESC";

                DataTable dt = DatabaseHelper.ExecuteQuery(sql, null);

                listViewUsers.Items.Clear();
                foreach (DataRow row in dt.Rows)
                {
                    var item = new ListViewItem(row["userId"].ToString());
                    item.SubItems.Add(row["userName"].ToString());
                    item.SubItems.Add(row["roleId"].ToString());
                    item.SubItems.Add(row["isActive"].ToString());
                    item.SubItems.Add(row["createdTime"].ToString());
                    item.Tag = row["userId"].ToString();
                    listViewUsers.Items.Add(item);
                }
                AutoResizeColumnsToContent();
            }
            catch (Exception ex)
            {
                AppLogger.Info($"[USER_LIST_FAIL] {ex.Message}");
                MessageBox.Show($"사용자 목록 조회 실패:\n{ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── ListView 커스텀 그리기 (파란 헤더, AdminForm 스타일) ────────

        private void ListViewUsers_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            using (var brush = new SolidBrush(Color.SteelBlue))
                e.Graphics.FillRectangle(brush, e.Bounds);

            TextRenderer.DrawText(
                e.Graphics, e.Header.Text, e.Font,
                e.Bounds, Color.White,
                TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
        }

        private void ListViewUsers_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            e.DrawDefault = true;
        }

        private void ListViewUsers_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            // 선택 행 배경
            if (e.Item.Selected && listViewUsers.Focused)
                e.Graphics.FillRectangle(SystemBrushes.Highlight, e.Bounds);
            else if (e.Item.Selected)
                e.Graphics.FillRectangle(SystemBrushes.ButtonFace, e.Bounds);
            else
                e.DrawBackground();

            Color textColor = (e.Item.Selected && listViewUsers.Focused)
                ? SystemColors.HighlightText
                : SystemColors.WindowText;

            TextRenderer.DrawText(
                e.Graphics, e.SubItem.Text, listViewUsers.Font,
                e.Bounds, textColor,
                TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter | TextFormatFlags.EndEllipsis);
        }

        // ── 컬럼 너비 자동 조정 (내용/헤더 중 더 넓은 쪽) ──────────────
        private void AutoResizeColumnsToContent()
        {
            for (int i = 0; i < listViewUsers.Columns.Count; i++)
            {
                listViewUsers.Columns[i].AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
                int contentWidth = listViewUsers.Columns[i].Width;
                listViewUsers.Columns[i].AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize);
                int headerWidth = listViewUsers.Columns[i].Width;
                listViewUsers.Columns[i].Width = Math.Max(contentWidth, headerWidth) + 16;
            }
        }

        // ── 컬럼 너비 사용자 변경 방지 ──────────────────────────────────
        private void ListViewUsers_ColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
        {
            e.Cancel    = true;
            e.NewWidth  = listViewUsers.Columns[e.ColumnIndex].Width;
        }

        // ── 행 선택 → 하단 편집 패널 자동 채우기 ────────────────────────

        private void ListViewUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewUsers.SelectedItems.Count == 0)
            {
                _selectedUserId = null;
                labelSelectedUser.Text = "(없음)";
                SetEditPanelEnabled(false);
                return;
            }

            var item = listViewUsers.SelectedItems[0];
            _selectedUserId = item.Tag?.ToString();

            string uid      = item.SubItems[0].Text;
            string uname    = item.SubItems[1].Text;
            string role     = item.SubItems[2].Text;
            string isActive = item.SubItems[3].Text;

            labelSelectedUser.Text = $"{uid} ({uname})";

            int roleIdx = comboBoxRoleEdit.Items.IndexOf(role);
            comboBoxRoleEdit.SelectedIndex = roleIdx >= 0 ? roleIdx : 0;

            int activeIdx = comboBoxIsActive.Items.IndexOf(isActive);
            comboBoxIsActive.SelectedIndex = activeIdx >= 0 ? activeIdx : 0;

            SetEditPanelEnabled(true);
        }

        private void SetEditPanelEnabled(bool enabled)
        {
            comboBoxRoleEdit.Enabled  = enabled;
            comboBoxIsActive.Enabled  = enabled;
            btnUpdate.Enabled         = enabled;
        }

        // ── 사용자 등록 ──────────────────────────────────────────────

        private void BtnRegister_Click(object sender, EventArgs e)
        {
            string userId   = textBoxUserId.Text.Trim();
            string userName = textBoxUserName.Text.Trim();
            string password = textBoxPassword.Text.Trim();
            string roleId   = comboBoxRoleReg.SelectedItem?.ToString() ?? "USER";

            if (string.IsNullOrEmpty(userId))
            {
                MessageBox.Show("ID를 입력하세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxUserId.Focus();
                return;
            }
            if (string.IsNullOrEmpty(userName))
            {
                MessageBox.Show("이름을 입력하세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxUserName.Focus();
                return;
            }
            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("비밀번호를 입력하세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxPassword.Focus();
                return;
            }
            if (password.Length < 4)
            {
                MessageBox.Show("비밀번호는 4자 이상이어야 합니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxPassword.Focus();
                return;
            }

            try
            {
                string passwordHash = LoginForm.CreatePasswordHash(password);

                DatabaseHelper.ExecuteTransaction((conn, tx) =>
                {
                    // tblUser INSERT
                    string sqlUser = @"
                        INSERT INTO dbo.tblUser (userId, userName, passwordHash, isActive, createdTime, createdBy)
                        VALUES (@userId, @userName, @passwordHash, 1, GETDATE(), @createdBy)";

                    using (var cmd = new SqlCommand(sqlUser, conn, tx))
                    {
                        cmd.Parameters.Add(new SqlParameter("@userId",       SqlDbType.VarChar, 20)  { Value = userId });
                        cmd.Parameters.Add(new SqlParameter("@userName",     SqlDbType.NVarChar, 50) { Value = userName });
                        cmd.Parameters.Add(new SqlParameter("@passwordHash", SqlDbType.VarChar, 200) { Value = passwordHash });
                        cmd.Parameters.Add(new SqlParameter("@createdBy",    SqlDbType.VarChar, 20)  { Value = _adminUserName });
                        cmd.ExecuteNonQuery();
                    }

                    // tblUserRole INSERT
                    string sqlRole = @"
                        INSERT INTO dbo.tblUserRole (userId, roleId)
                        VALUES (@userId, @roleId)";

                    using (var cmd = new SqlCommand(sqlRole, conn, tx))
                    {
                        cmd.Parameters.Add(new SqlParameter("@userId", SqlDbType.VarChar, 20) { Value = userId });
                        cmd.Parameters.Add(new SqlParameter("@roleId", SqlDbType.VarChar, 20) { Value = roleId });
                        cmd.ExecuteNonQuery();
                    }
                }, out string regError);

                if (!string.IsNullOrEmpty(regError))
                    throw new Exception(regError);

                AppLogger.Info($"[USER_CREATE] adminId={_adminUserId} newUserId={userId} userName={userName} role={roleId}");

                MessageBox.Show($"사용자 '{userId}' ({userName}) 등록 완료.", "알림",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                // 입력 필드 초기화
                textBoxUserId.Clear();
                textBoxUserName.Clear();
                textBoxPassword.Clear();
                comboBoxRoleReg.SelectedIndex = 0;

                LoadUserList();
            }
            catch (SqlException sqlex) when (sqlex.Number == 2627 || sqlex.Number == 2601)
            {
                // PRIMARY KEY 또는 UNIQUE 제약 위반 → 중복 userId
                MessageBox.Show($"이미 존재하는 ID입니다: {userId}", "등록 실패",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                AppLogger.Info($"[USER_CREATE_FAIL] adminId={_adminUserId} newUserId={userId} | {ex.Message}");
                MessageBox.Show($"사용자 등록 실패:\n{ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── 사용자 수정 (권한/활성 변경) ────────────────────────────────

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedUserId)) return;

            string roleId   = comboBoxRoleEdit.SelectedItem?.ToString() ?? "USER";
            int    isActive = comboBoxIsActive.SelectedItem?.ToString() == "1" ? 1 : 0;

            var confirm = MessageBox.Show(
                $"사용자 '{_selectedUserId}' 를 수정합니다.\n권한: {roleId}  활성: {isActive}\n계속하시겠습니까?",
                "수정 확인", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            try
            {
                DatabaseHelper.ExecuteTransaction((conn, tx) =>
                {
                    // tblUser UPDATE
                    string sqlUser = @"
                        UPDATE dbo.tblUser
                        SET    isActive    = @isActive,
                               updatedTime = GETDATE(),
                               updatedBy   = @updatedBy
                        WHERE  userId = @userId";

                    using (var cmd = new SqlCommand(sqlUser, conn, tx))
                    {
                        cmd.Parameters.Add(new SqlParameter("@isActive",   SqlDbType.TinyInt)        { Value = isActive });
                        cmd.Parameters.Add(new SqlParameter("@updatedBy",  SqlDbType.VarChar, 20)    { Value = _adminUserName });
                        cmd.Parameters.Add(new SqlParameter("@userId",     SqlDbType.VarChar, 20)    { Value = _selectedUserId });
                        cmd.ExecuteNonQuery();
                    }

                    // tblUserRole UPDATE
                    string sqlRole = @"
                        UPDATE dbo.tblUserRole
                        SET    roleId = @roleId
                        WHERE  userId = @userId";

                    using (var cmd = new SqlCommand(sqlRole, conn, tx))
                    {
                        cmd.Parameters.Add(new SqlParameter("@roleId", SqlDbType.VarChar, 20) { Value = roleId });
                        cmd.Parameters.Add(new SqlParameter("@userId", SqlDbType.VarChar, 20) { Value = _selectedUserId });
                        cmd.ExecuteNonQuery();
                    }
                }, out string updError);

                if (!string.IsNullOrEmpty(updError))
                    throw new Exception(updError);

                AppLogger.Info($"[USER_UPDATE] adminId={_adminUserId} targetUserId={_selectedUserId} role={roleId} isActive={isActive}");

                MessageBox.Show($"사용자 '{_selectedUserId}' 수정 완료.", "알림",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadUserList();
            }
            catch (Exception ex)
            {
                AppLogger.Info($"[USER_UPDATE_FAIL] adminId={_adminUserId} targetUserId={_selectedUserId} | {ex.Message}");
                MessageBox.Show($"사용자 수정 실패:\n{ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
