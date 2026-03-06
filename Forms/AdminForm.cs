using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using StripMapEditor.Database;
using StripMapEditor.Utils;

namespace stripMap_Editor.Forms
{
    public partial class AdminForm : Form
    {
        private readonly string _userId;
        private readonly string _userRole;
        private readonly HashSet<string> _permissions;
        private readonly RvManager _rv;
        private DataTable _searchResult;

        public AdminForm(string userId, string userRole, HashSet<string> permissions, RvManager rv = null)
        {
            InitializeComponent();
            _userId      = userId;
            _userRole    = userRole ?? string.Empty;
            _permissions = permissions;
            _rv          = rv;
        }

        private void SendMesRvMessage(string frameId, string actionType, string functionId)
        {
            if (_rv == null || !_rv.IsConnected) return;
            try
            {
                string xml =
                    "<message>" +
                      $"<header><messagename>{functionId}</messagename></header>" +
                      "<body>" +
                        $"<FRAME_ID>{frameId}</FRAME_ID>" +
                        $"<ACTIONTYPE>{actionType}</ACTIONTYPE>" +
                        "<FRAME_LOC_XPOS></FRAME_LOC_XPOS>" +
                        "<FRAME_LOC_YPOS></FRAME_LOC_YPOS>" +
                      "</body>" +
                    "</message>";
                _rv.RvSend(_rv.Subject, xml);
            }
            catch (Exception ex)
            {
                AppLogger.Info($"[RV_SEND_FAIL] frameId={frameId} actionType={actionType} | {ex.Message}");
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // ADMIN, SUPER 역할만 Purge 허용
            bool canPurge = _userRole == "ADMIN" || _userRole == "SUPER";
            if (!canPurge)
            {
                btnPurge.Enabled   = false;
                btnPurge.Text      = "권한 없음";
                btnPurge.BackColor = Color.Gray;
            }
        }

        // ── 조회 ─────────────────────────────────────────────────────

        private async void BtnSearch_Click(object sender, EventArgs e)
        {
            string stripNo = textBoxStripNo.Text.Trim();
            if (string.IsNullOrEmpty(stripNo))
            {
                MessageBox.Show("PCB 2D ID를 입력해주세요.", "알림",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            btnSearch.Enabled = false;
            try
            {
                await LoadPurgeDataAsync(stripNo);
            }
            finally
            {
                btnSearch.Enabled = true;
            }
        }

        private void TextBoxStripNo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) BtnSearch_Click(sender, e);
        }

        private async Task LoadPurgeDataAsync(string stripNo)
        {
            try
            {
                // tblStripMap 전체 행 조회 (active=1, active=0 모두)
                string query = @"
                    SELECT
                        m.[version],
                        m.stripNo,
                        m.process,
                        ISNULL(m.lotNo,  '') AS lotNo,
                        ISNULL(m.mgzRf,  '') AS mgzRf,
                        m.active
                    FROM dbo.tblStripMap m
                    WHERE m.stripNo LIKE @stripNo
                    ORDER BY m.stripNo ASC, m.[version] ASC";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@stripNo", $"%{stripNo}%")
                };

                DataTable dt = await Task.Run(() => DatabaseHelper.ExecuteQuery(query, parameters));

                _searchResult = dt.Copy();
                DisplayPurgeData(dt);

                int cnt = dt.Rows.Count;
                labelResultTitle.Text = $"조회 결과 ({cnt}건)";

                // 전체 선택 체크박스 초기화 (이벤트 없이)
                checkBoxAll.CheckedChanged -= CheckBoxAll_CheckedChanged;
                checkBoxAll.Checked = false;
                checkBoxAll.CheckedChanged += CheckBoxAll_CheckedChanged;

                if (cnt == 0)
                    MessageBox.Show("조회된 데이터가 없습니다.", "조회 결과",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                labelResultTitle.Text = "조회 결과";
                MessageBox.Show($"데이터 조회 중 오류: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DisplayPurgeData(DataTable dt)
        {
            listViewPurge.BeginUpdate();
            listViewPurge.Items.Clear();

            foreach (DataRow row in dt.Rows)
            {
                string version    = row["version"]?.ToString() ?? "";
                string lotNo      = row["lotNo"]?.ToString()   ?? "";
                string stripNoVal = row["stripNo"]?.ToString() ?? "";
                string mgzRf      = row["mgzRf"]?.ToString()   ?? "";
                bool   isActive   = row["active"] != DBNull.Value && Convert.ToBoolean(row["active"]);
                string activeText = isActive ? "1" : "0";

                ListViewItem item = new ListViewItem("");  // 체크박스 전용 컬럼
                item.SubItems.Add(version);
                item.SubItems.Add(lotNo);
                item.SubItems.Add(stripNoVal);
                item.SubItems.Add(mgzRf);
                item.SubItems.Add(activeText);
                item.Tag = row;

                // 논리 삭제(비활성) 행은 회색 텍스트
                if (!isActive)
                    item.ForeColor = Color.Gray;

                listViewPurge.Items.Add(item);
            }

            listViewPurge.EndUpdate();
        }

        // ── 전체 선택 체크박스 ────────────────────────────────────────

        private void CheckBoxAll_CheckedChanged(object sender, EventArgs e)
        {
            // ItemChecked 이벤트를 잠시 해제하여 N회 라벨 갱신 방지
            listViewPurge.ItemChecked -= ListViewPurge_ItemChecked;
            bool check = checkBoxAll.Checked;
            foreach (ListViewItem item in listViewPurge.Items)
                item.Checked = check;
            listViewPurge.ItemChecked += ListViewPurge_ItemChecked;
            UpdateResultLabel();
        }

        private void ListViewPurge_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            UpdateResultLabel();
        }

        private void UpdateResultLabel()
        {
            int total        = listViewPurge.Items.Count;
            int checkedCount = listViewPurge.CheckedItems.Count;
            labelResultTitle.Text = checkedCount > 0
                ? $"조회 결과 ({total}건, {checkedCount}개 선택됨)"
                : $"조회 결과 ({total}건)";
        }

        // 컬럼 너비 변경 방지
        private void ListViewPurge_ColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
        {
            e.Cancel    = true;
            e.NewWidth  = listViewPurge.Columns[e.ColumnIndex].Width;
        }

        // ── Purge 실행 ───────────────────────────────────────────────

        private async void BtnPurge_Click(object sender, EventArgs e)
        {
            var checkedItems = listViewPurge.CheckedItems.Cast<ListViewItem>().ToList();
            if (checkedItems.Count == 0)
            {
                MessageBox.Show("Purge할 항목을 선택해주세요.", "알림",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // ① 2중 확인 (위험 작업)
            var confirm = MessageBox.Show(
                $"선택한 {checkedItems.Count}건을 StripMap DB에서 물리 삭제합니다.\n" +
                "이 작업은 시스템 관리자만 되돌릴 수 있습니다.",
                "Purge 실행 확인",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);

            if (confirm != DialogResult.Yes) return;

            // ② 사유 입력 (필수)
            string comment = ShowInputDialog("Purge 사유를 입력하세요 (필수):", "Purge 사유 입력");
            if (string.IsNullOrWhiteSpace(comment))
            {
                MessageBox.Show("Purge 사유를 입력해야 합니다.", "알림",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnPurge.Enabled = false;
            try
            {
                await ExecutePurgeAsync(checkedItems, comment);
            }
            finally
            {
                btnPurge.Enabled = true;
            }
        }

        private async Task ExecutePurgeAsync(List<ListViewItem> items, string comment)
        {
            string workerIp = GetLocalIPAddress();
            string searchStripNo = textBoxStripNo.Text.Trim();

            var result = await Task.Run(() =>
            {
                int successCount = 0;
                int failCount    = 0;
                StringBuilder errorLog = new StringBuilder();

                foreach (ListViewItem item in items)
                {
                    DataRow row = item.Tag as DataRow;
                    if (row == null) { failCount++; continue; }

                    string stripNo = string.Empty;
                    try
                    {
                        stripNo        = row["stripNo"]?.ToString();
                        string process = row["process"]?.ToString();
                        int    version = Convert.ToInt32(row["version"]);

                        DatabaseHelper.ExecuteStoredProcedureNonQuery(
                            "dbo.usp_StripMap_Process",
                            new SqlParameter[]
                            {
                                new SqlParameter("@actionType",    SqlDbType.Char, 1) { Value = "P" },
                                new SqlParameter("@stripNo",       stripNo),
                                new SqlParameter("@process",       process),
                                new SqlParameter("@mapArray",      DBNull.Value),
                                new SqlParameter("@bincode",       DBNull.Value),
                                new SqlParameter("@lotNo",         DBNull.Value),
                                new SqlParameter("@targetTimekey", DBNull.Value),
                                new SqlParameter("@targetVersion", SqlDbType.Int) { Value = version },
                                new SqlParameter("@workerId",      _userId),
                                new SqlParameter("@comment",       comment),
                                new SqlParameter("@workerIp",      workerIp)
                            });

                        AppLogger.Info($"[{ActionTypes.STRIP_PURGE}] user={_userId} | stripNo={stripNo} ver={version} | 사유={comment}");
                        SendMesRvMessage(stripNo, "P", ActionTypes.STRIP_PURGE);
                        successCount++;
                    }
                    catch (SqlException sqlex)
                    {
                        failCount++;
                        errorLog.AppendLine($"stripNo: {stripNo} - {SpErrorCodes.GetMessage(sqlex)}");
                    }
                    catch (Exception ex)
                    {
                        failCount++;
                        errorLog.AppendLine($"stripNo: {stripNo} - {ex.Message}");
                    }
                }

                return (successCount, failCount, errorLog: errorLog.ToString());
            });

            AppLogger.Info($"[{ActionTypes.STRIP_PURGE}_RESULT] user={_userId} | 성공={result.successCount} 실패={result.failCount}");
            // ③ 결과 표시
            if (result.failCount == 0)
                MessageBox.Show($"{result.successCount}건 Purge 완료.", "완료",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show(
                    $"성공: {result.successCount}건 / 실패: {result.failCount}건\n\n실패 목록:\n{result.errorLog}",
                    "결과", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            // ④ 재조회
            if (!string.IsNullOrEmpty(searchStripNo))
                await LoadPurgeDataAsync(searchStripNo);
        }

        // ── ListView 그리기 (파란 헤더) ──────────────────────────────

        private void ListViewPurge_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            using (SolidBrush backBrush = new SolidBrush(Color.Firebrick))
                e.Graphics.FillRectangle(backBrush, e.Bounds);
            e.Graphics.DrawRectangle(Pens.White, e.Bounds);
            TextRenderer.DrawText(e.Graphics, e.Header.Text, e.Font, e.Bounds, Color.White,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        private void ListViewPurge_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            e.DrawDefault = true;
        }

        private void ListViewPurge_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            e.DrawDefault = true;
        }

        // ── 유틸리티 ─────────────────────────────────────────────────

        private string ShowInputDialog(string prompt, string title)
        {
            using (Form inputForm = new Form())
            {
                inputForm.Width            = 480;
                inputForm.Height           = 175;
                inputForm.FormBorderStyle  = FormBorderStyle.FixedDialog;
                inputForm.Text             = title;
                inputForm.StartPosition    = FormStartPosition.CenterParent;
                inputForm.MaximizeBox      = false;
                inputForm.MinimizeBox      = false;
                inputForm.Font             = new Font("맑은 고딕", 9.75f);

                Label  lbl    = new Label  { Left = 12,  Top = 15, Width = 445, Text = prompt, AutoSize = true };
                TextBox tb    = new TextBox { Left = 12,  Top = 42, Width = 445 };
                Button ok     = new Button  { Text = "확인", Left = 265, Width = 90, Top = 85, DialogResult = DialogResult.OK };
                Button cancel = new Button  { Text = "취소", Left = 365, Width = 90, Top = 85, DialogResult = DialogResult.Cancel };

                inputForm.Controls.AddRange(new Control[] { lbl, tb, ok, cancel });
                inputForm.AcceptButton = ok;
                inputForm.CancelButton = cancel;

                return inputForm.ShowDialog(this) == DialogResult.OK ? tb.Text.Trim() : null;
            }
        }

        private string GetLocalIPAddress()
        {
            try
            {
                var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
                foreach (var ip in host.AddressList)
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                        return ip.ToString();
                return "127.0.0.1";
            }
            catch { return "127.0.0.1"; }
        }
    }
}
