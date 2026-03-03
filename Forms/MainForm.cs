using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using StripMapEditor.Database;
using StripMapEditor.Utils;

namespace stripMap_Editor.Forms
{
    public partial class MainForm : Form
    {
        public string LoggedInUserId { get; set; }
        public string LoggedInUserName { get; set; }
        public string LoggedInUserRole { get; set; }

        private DataTable originalData; // 원본 데이터 저장용 (PCB 원복 탭)
        private DataTable lotIdData; // Lot ID 변경 탭 데이터
        private Dictionary<string, string> modifiedLotIds; // 수정된 Lot ID 저장 (stripNo, newLotId)

        // 관리자 탭 (동적 생성)
        private TabPage _tabPageAdmin;
        
        // 사용자 권한 관련
        private UserRole currentUserRole = UserRole.USER;
        private string currentUserId = string.Empty;
        private HashSet<string> _userPermissions = new HashSet<string>();
        private HashSet<string> _userMenus       = new HashSet<string>();

        // 셀 단위 하이라이트 관련
        private (ListView lv, int row, int col) _hlCell = (null, -1, -1);
        private int _mouseDownColIndex = -1;
        public MainForm()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // 로그인 정보 연결
            currentUserId = LoggedInUserId ?? string.Empty;

            if (Enum.TryParse(LoggedInUserRole, ignoreCase: true, out UserRole parsedRole))
                currentUserRole = parsedRole;
            else
                currentUserRole = UserRole.USER; // 파싱 실패 시 최소 권한 적용

            LoadUserPermissions();
            SetupAdminTab();       // ApplyUserPermissions 전에 탭 생성
            ApplyUserPermissions();
            InitializeForm();
        }

        /// <summary>
        /// DB에서 역할별 기능 권한 및 메뉴 목록 로드
        /// </summary>
        private void LoadUserPermissions()
        {
            try
            {
                _userPermissions = DatabaseHelper.LoadRolePermissions(LoggedInUserRole);
                _userMenus       = DatabaseHelper.LoadRoleMenus(LoggedInUserRole);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"권한 정보를 불러오지 못했습니다. 최소 권한으로 실행됩니다.\n\n{ex.Message}",
                    "권한 로드 실패", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _userPermissions = new HashSet<string>();
                _userMenus       = new HashSet<string>();
            }
        }

        /// <summary>
        /// 사용자 권한에 따른 UI 제어 (DB 메뉴 기반, tblMenu 1:1 매핑)
        /// STRIP_EDIT → tabPageLotId    (Lot ID 수정)
        /// MAP_EDIT   → tabPageMapArray (Map Array 수정)
        /// STRIP_HIST → tabPagePcbRestore (PCB 원복)
        /// </summary>
        private void ApplyUserPermissions()
        {
            tabPageLotId.Parent      = _userMenus.Contains(MenuIds.STRIP_EDIT) ? tabControl_Strip : null;
            tabPageMapArray.Parent   = _userMenus.Contains(MenuIds.MAP_EDIT)   ? tabControl_Strip : null;
            tabPagePcbRestore.Parent = _userMenus.Contains(MenuIds.STRIP_HIST) ? tabControl_Strip : null;
            _tabPageAdmin.Parent     = _userMenus.Contains(MenuIds.ADMIN)      ? tabControl_Strip : null;

            if (tabControl_Strip.TabCount > 0)
                tabControl_Strip.SelectedIndex = 0;
        }

        /// <summary>
        /// 관리자 탭 동적 생성 — 탭 클릭 시 AdminForm 모달 팝업
        /// tblRoleMenu ADMIN 메뉴 권한이 있는 경우에만 표시 (ApplyUserPermissions에서 제어)
        /// </summary>
        private void SetupAdminTab()
        {
            _tabPageAdmin = new TabPage
            {
                Text      = "⚙ 관리자",
                BackColor = Color.FromArgb(240, 240, 240),
                Cursor    = Cursors.Hand,
                Font      = new Font("맑은 고딕", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 129)
            };
            tabControl_Strip.SelectedIndexChanged += TabControl_SelectedIndexChanged;
        }

        private void TabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_tabPageAdmin == null || tabControl_Strip.SelectedTab != _tabPageAdmin)
                return;

            // 관리자 탭 클릭 → 첫 번째 일반 탭으로 되돌린 후 AdminForm 팝업
            for (int i = 0; i < tabControl_Strip.TabCount; i++)
            {
                if (tabControl_Strip.TabPages[i] != _tabPageAdmin)
                {
                    tabControl_Strip.SelectedIndex = i;
                    break;
                }
            }

            using (var adminForm = new AdminForm(currentUserId, LoggedInUserRole, _userPermissions))
                adminForm.ShowDialog(this);
        }

        /// <summary>
        /// 특정 기능 권한 체크 (DB 로드 데이터 기반)
        /// </summary>
        private bool HasPermission(string permission)
        {
            return _userPermissions.Contains(permission);
        }

        /// <summary>
        /// 폼 초기화
        /// </summary>
        private void InitializeForm()
        {
            // 타이틀 바에 사용자 이름 표시
            this.Text = $"StripMap Editor - {LoggedInUserName} ({LoggedInUserRole})";
            // 수정된 데이터 저장용 Dictionary 초기화
            modifiedLotIds = new Dictionary<string, string>();
            modifiedMapArrays = new Dictionary<string, MapArrayModification>();

            // DB 연결 테스트
            if (!TestDatabaseConnection())
            {
                MessageBox.Show("데이터베이스 연결에 실패했습니다.\nconfig.ini 파일을 확인 후 프로그램을 다시 시작해주세요.",
                    "연결 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }

            // Lot ID 변경 탭 이벤트
            btnSearch_LotId.Click += BtnSearch2_Click;
            btnModify_LotId.Click += BtnModify_Click;
            btnUpdate_LotId.Click += BtnSave_Click;
            listViewResult_LotId.ItemChecked += ListViewResult2_ItemChecked;
            listViewResult_LotId.ColumnWidthChanging += ListView_ColumnWidthChanging;

            // MapArray 변경 탭 이벤트
            btnSearch_MapArray.Click += BtnSearchMapArray_Click;
            btnDelete_MapArray.Click += BtnDeleteMapArray_Click;
            btnUpdate_MapArray.Click += BtnUpdateMapArray_Click;
            listViewResult_MapArray.ItemChecked += ListViewResultMapArray_ItemChecked;
            listViewResult_MapArray.ColumnWidthChanging += ListView_ColumnWidthChanging;

            // PCB 2D ID 원복 탭 이벤트
            btnSearch_PCB.Click += BtnSearch_Click;
            btnRestore_PCB.Click += BtnRestore_Click;
            btnPurgeRollback_PCB.Click += BtnPurgeRollback_Click;
            listViewResult_PCB.ItemChecked += ListViewResult_ItemChecked;
            listViewResult_PCB.ColumnWidthChanging += ListView_ColumnWidthChanging;

            // ── 엔터 키 조회 ──────────────────────────────────────
            // Lot ID 탭
            textBox_LOT2.KeyDown += SearchTextBox_LotId_KeyDown;
            textBox_PCB2.KeyDown += SearchTextBox_LotId_KeyDown;
            textBox_MGZ2.KeyDown += SearchTextBox_LotId_KeyDown;
            // MapArray 탭
            textBox_PCB_MapArray.KeyDown += SearchTextBox_MapArray_KeyDown;
            // PCB 원복 탭
            textBox_LOT.KeyDown += SearchTextBox_PCB_KeyDown;
            textBox_PCB.KeyDown += SearchTextBox_PCB_KeyDown;
            textBox_MGZ.KeyDown += SearchTextBox_PCB_KeyDown;

            // ── ListView 복사 (Ctrl+C) / 셀 단위 드래그 ──────────────────
            listViewResult_LotId.KeyDown                  += ListView_CopyKeyDown;
            listViewResult_LotId.MouseDown                += ListView_MouseDown;
            listViewResult_LotId.ItemDrag                 += ListView_ItemDrag;
            listViewResult_MapArray.KeyDown               += ListView_CopyKeyDown;
            listViewResult_MapArray.MouseDown             += ListView_MouseDown;
            listViewResult_MapArray.ItemDrag              += ListView_ItemDrag;
            listViewResult_MapArray_BinCode.KeyDown       += ListView_CopyKeyDown;
            listViewResult_MapArray_BinCode.MouseDown     += ListView_MouseDown;
            listViewResult_MapArray_BinCode.ItemDrag      += ListView_ItemDrag;
            listViewResult_PCB.KeyDown                    += ListView_CopyKeyDown;
            listViewResult_PCB.MouseDown                  += ListView_MouseDown;
            listViewResult_PCB.ItemDrag                   += ListView_ItemDrag;
        }

        /// <summary>
        /// DB 연결 테스트
        /// </summary>
        private bool TestDatabaseConnection()
        {
            try
            {
                string errorMessage;
                bool isConnected = DatabaseHelper.TestConnection(out errorMessage);

                if (!isConnected)
                {
                    MessageBox.Show($"DB 연결 실패: {errorMessage}", "오류",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                return isConnected;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"DB 연결 테스트 중 오류: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// ListView 컬럼 너비 변경 방지
        /// </summary>
        private void ListView_ColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
        {
            // 현재 컬럼의 원래 너비로 되돌림
            e.NewWidth = ((ListView)sender).Columns[e.ColumnIndex].Width;
            e.Cancel = true;
        }
        #region Lot ID 변경 탭

        /// <summary>
        /// 조회 버튼 클릭 (Lot ID 변경 탭)
        /// </summary>
        private void BtnSearch2_Click(object sender, EventArgs e)
        {
            try
            {
                string lotNo = textBox_LOT2.Text.Trim();
                string stripNo = textBox_PCB2.Text.Trim();
                string mgzRf = textBox_MGZ2.Text.Trim();

                // 검색 조건 체크 제거 - 빈 값이어도 전체 조회
                LoadLotIdData(lotNo, stripNo, mgzRf);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"조회 중 오류 발생: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadLotIdData(string lotNo, string stripNo, string mgzRf)
        {
            try
            {
                // 수정 데이터 초기화
                modifiedLotIds.Clear();

                StringBuilder queryBuilder = new StringBuilder();
                queryBuilder.Append(@"
                    SELECT
                        stripNo,
                        process,
                        lotNo,
                        mgzRf,
                        createdTime,
                        userId
                    FROM dbo.[tblStripMap]
                    WHERE active = 1
                ");

                var parameters = new List<SqlParameter>();

                if (!string.IsNullOrEmpty(lotNo))
                {
                    queryBuilder.Append(" AND lotNo LIKE @LotNo");
                    parameters.Add(new SqlParameter("@LotNo", $"%{lotNo}%"));
                }

                if (!string.IsNullOrEmpty(stripNo))
                {
                    queryBuilder.Append(" AND stripNo LIKE @stripNo");
                    parameters.Add(new SqlParameter("@stripNo", $"%{stripNo}%"));
                }

                if (!string.IsNullOrEmpty(mgzRf))
                {
                    queryBuilder.Append(" AND mgzRf LIKE @MgzRf");
                    parameters.Add(new SqlParameter("@MgzRf", $"%{mgzRf}%"));
                }

                queryBuilder.Append(" ORDER BY stripNo ASC");

                DataTable dt = DatabaseHelper.ExecuteQuery(queryBuilder.ToString(), parameters.ToArray());
                lotIdData = dt.Copy();
                DisplayLotIdData(dt);

                labelResultTitle2.Text = $"조회 결과 ({dt.Rows.Count}건)";

                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("조회된 데이터가 없습니다.", "조회 결과",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                labelResultTitle2.Text = "조회 결과";
                MessageBox.Show($"데이터 조회 중 오류: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DisplayLotIdData(DataTable dt)
        {
            listViewResult_LotId.BeginUpdate();
            listViewResult_LotId.Items.Clear();

            foreach (DataRow row in dt.Rows)
            {
                // 첫 번째 컬럼은 체크박스 전용 (빈 값)
                ListViewItem item = new ListViewItem("");

                // LOT ID
                string lotNo = row["lotNo"]?.ToString() ?? "";
                item.SubItems.Add(lotNo);

                // 수정된 LOT ID (저장 전 임시 표시, 초기값은 빈 값)
                item.SubItems.Add(""); // 수정된 LOT ID 컬럼

                // PCB 2D ID
                string stripNo = row["stripNo"]?.ToString() ?? "";
                item.SubItems.Add(stripNo);

                // MGZ ID
                string mgzRf = row["mgzRf"]?.ToString() ?? "";
                item.SubItems.Add(mgzRf);

                item.Tag = row;
                listViewResult_LotId.Items.Add(item);
            }

            listViewResult_LotId.EndUpdate();
        }

        private void ListViewResult2_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            int checkedCount = 0;
            foreach (ListViewItem item in listViewResult_LotId.Items)
            {
                if (item.Checked)
                    checkedCount++;
            }

            int totalCount = listViewResult_LotId.Items.Count;
            labelResultTitle2.Text = $"조회 결과 ({totalCount}건, {checkedCount}개 선택됨)";
        }

        /// <summary>
        /// 수정 버튼 클릭
        /// </summary>
        private void BtnModify_Click(object sender, EventArgs e)
        {
            try
            {
                var checkedItems = new List<ListViewItem>();
                foreach (ListViewItem item in listViewResult_LotId.Items)
                {
                    if (item.Checked)
                        checkedItems.Add(item);
                }

                if (checkedItems.Count == 0)
                {
                    MessageBox.Show("수정할 항목을 선택해주세요.", "알림",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 새로운 Lot ID 입력 받기
                string newLotId = ShowInputDialog("새 LOT ID를 입력하세요:", "LOT ID 수정");

                if (string.IsNullOrWhiteSpace(newLotId))
                {
                    return;
                }

                // LOT ID 형식 검사: 영문자·숫자만 허용 (한글·특수문자·공백 전부 차단)
                if (!Regex.IsMatch(newLotId, @"^[A-Za-z0-9]+$"))
                {
                    MessageBox.Show(
                        "LOT ID에 한글 또는 특수문자를 포함할 수 없습니다.\n\n" +
                        "사용 가능: 영문자(A-Z), 숫자(0-9)",
                        "입력 오류",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 선택된 항목들의 Lot ID 수정 (메모리에만)
                foreach (ListViewItem item in checkedItems)
                {
                    DataRow row = item.Tag as DataRow;
                    if (row != null)
                    {
                        string stripNo = row["stripNo"]?.ToString();

                        // Dictionary에 수정 데이터 저장
                        if (!modifiedLotIds.ContainsKey(stripNo))
                        {
                            modifiedLotIds.Add(stripNo, newLotId);
                        }
                        else
                        {
                            modifiedLotIds[stripNo] = newLotId;
                        }

                        // ListView 표시 업데이트
                        item.SubItems[2].Text = newLotId;
                        item.BackColor = Color.LightYellow; // 수정된 항목 표시
                    }
                }

                MessageBox.Show($"{checkedItems.Count}건의 LOT ID가 수정되었습니다.\n저장 버튼을 눌러 DB에 반영하세요.",
                    "수정 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"수정 중 오류 발생: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 저장 버튼 클릭
        /// </summary>
        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (modifiedLotIds.Count == 0)
                {
                    MessageBox.Show("수정된 항목이 없습니다.", "알림",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                DialogResult result = MessageBox.Show(
                    $"{modifiedLotIds.Count}건의 데이터를 저장하시겠습니까?",
                    "저장 확인",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    this.Cursor = Cursors.WaitCursor;
                    SaveLotIdChanges();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"저장 중 오류 발생: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// Lot ID 변경사항 DB 저장 — SP 'L' (usp_StripMap_Process)
        /// </summary>
        private void SaveLotIdChanges()
        {
            int successCount = 0;
            int failCount = 0;
            StringBuilder errorLog = new StringBuilder();
            string workerIp = GetLocalIPAddress();

            try
            {
                foreach (var kvp in modifiedLotIds)
                {
                    string stripNo = kvp.Key;
                    string newLotId = kvp.Value;

                    try
                    {
                        DataRow[] rows = lotIdData.Select($"stripNo = '{stripNo}'");
                        if (rows.Length == 0)
                        {
                            failCount++;
                            errorLog.AppendLine($"stripNo: {stripNo} - 원본 데이터를 찾을 수 없습니다.");
                            continue;
                        }

                        DataRow originalRow = rows[0];
                        string oldLotId  = originalRow["lotNo"]?.ToString();
                        string process   = originalRow["process"]?.ToString();

                        DatabaseHelper.ExecuteStoredProcedureNonQuery("dbo.usp_StripMap_Process", new SqlParameter[]
                        {
                            new SqlParameter("@actionType",    SqlDbType.Char, 1) { Value = "L" },
                            new SqlParameter("@stripNo",       stripNo),
                            new SqlParameter("@process",       process),
                            new SqlParameter("@mapArray",      DBNull.Value),
                            new SqlParameter("@bincode",       DBNull.Value),
                            new SqlParameter("@lotNo",         newLotId),
                            new SqlParameter("@targetTimekey", DBNull.Value),
                            new SqlParameter("@workerId",      currentUserId),
                            new SqlParameter("@comment",       $"LOT ID 변경: {oldLotId} → {newLotId}"),
                            new SqlParameter("@workerIp",      workerIp)
                        });

                        AppLogger.Info($"[{ActionTypes.LOT_UPDATE}] user={currentUserId} | stripNo={stripNo} | {oldLotId} → {newLotId}");
                        successCount++;
                    }
                    catch (SqlException sqlex)
                    {
                        failCount++;
                        errorLog.AppendLine($"stripNo: {stripNo} - {GetSpErrorMessage(sqlex)}");
                    }
                    catch (Exception ex)
                    {
                        failCount++;
                        errorLog.AppendLine($"stripNo: {stripNo} - {ex.Message}");
                    }
                }

                AppLogger.Info($"[{ActionTypes.LOT_UPDATE}_RESULT] user={currentUserId} | 성공={successCount} 실패={failCount}");
                string resultMessage = $"성공: {successCount}건\n실패: {failCount}건";
                if (errorLog.Length > 0)
                    resultMessage += $"\n\n오류 내역:\n{errorLog}";

                MessageBox.Show(resultMessage, "저장 결과",
                    MessageBoxButtons.OK,
                    failCount > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);

                if (successCount > 0)
                {
                    modifiedLotIds.Clear();
                    BtnSearch2_Click(null, null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"저장 처리 중 오류: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// SP THROW 에러 코드 → 사용자 메시지 변환
        /// </summary>
        private string GetSpErrorMessage(SqlException sqlex)
        {
            switch (sqlex.Number)
            {
                case 50001: return "지원하지 않는 작업 유형입니다.";
                case 50002: return "이 작업에 대한 권한이 없습니다.";
                case 50010: return "Purge 대상 이력을 찾을 수 없습니다. (timekey 불일치)";
                case 50011: return "대상 TimeKey가 지정되지 않았습니다.";
                case 50012: return "히스토리에서 대상 TimeKey를 찾을 수 없습니다.";
                case 50020: return "관리자(ADMIN/SYSADMIN) 전용 작업입니다.";
                case 50021: return "Purge 원복 대상 TimeKey가 지정되지 않았습니다.";
                case 50030: return "지원하지 않는 작업 유형입니다.";
                case 50040: return "targetVersion이 지정되지 않았습니다.";
                case 50041: return "삭제 대상 행을 찾을 수 없습니다.";
                default:    return sqlex.Message;
            }
        }

        /// <summary>
        /// 로컬 IP 주소 가져오기
        /// </summary>
        private string GetLocalIPAddress()
        {
            try
            {
                var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }
                return "127.0.0.1";
            }
            catch
            {
                return "127.0.0.1";
            }
        }

        /// <summary>
        /// 입력 대화상자 표시
        /// </summary>
        private string ShowInputDialog(string message, string title)
        {
            Form inputForm = new Form
            {
                Width = 400,
                Height = 150,
                Text = title,
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            Label label = new Label
            {
                Text = message,
                Location = new Point(20, 20),
                AutoSize = true
            };

            TextBox textBox = new TextBox
            {
                Location = new Point(20, 50),
                Width = 340,
                Font = new Font("맑은 고딕", 10F)
            };

            Button okButton = new Button
            {
                Text = "확인",
                DialogResult = DialogResult.OK,
                Location = new Point(200, 80),
                Width = 80
            };

            Button cancelButton = new Button
            {
                Text = "취소",
                DialogResult = DialogResult.Cancel,
                Location = new Point(280, 80),
                Width = 80
            };

            inputForm.Controls.Add(label);
            inputForm.Controls.Add(textBox);
            inputForm.Controls.Add(okButton);
            inputForm.Controls.Add(cancelButton);
            inputForm.AcceptButton = okButton;
            inputForm.CancelButton = cancelButton;

            return inputForm.ShowDialog() == DialogResult.OK ? textBox.Text : string.Empty;
        }

        #endregion

        #region MapArray 변경 탭

        private DataTable mapArrayData; // MapArray 탭 데이터
        private Dictionary<string, MapArrayModification> modifiedMapArrays; // 수정된 MapArray 저장

        // MapArray 수정 데이터 구조
        private class MapArrayModification
        {
            public string MapArray { get; set; }
            public string BinCode { get; set; }
        }

        /// <summary>
        /// 조회 버튼 클릭 (MapArray 변경 탭)
        /// </summary>
        private void BtnSearchMapArray_Click(object sender, EventArgs e)
        {
            try
            {
                string stripNo = textBox_PCB_MapArray.Text.Trim();

                // 검색 조건 체크 복원 - MapArray 탭만 필수
                if (string.IsNullOrEmpty(stripNo))
                {
                    MessageBox.Show("PCB 2D ID를 입력해주세요.", "알림",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                LoadMapArrayData(stripNo);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"조회 중 오류 발생: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadMapArrayData(string stripNo)
        {
            try
            {
                // 수정 데이터 초기화
                modifiedMapArrays.Clear();
                textBoxMapArray.Clear();
                textBoxBinCode.Clear();

                StringBuilder queryBuilder = new StringBuilder();
                queryBuilder.Append(@"
            SELECT
                stripNo,
                process,
                mapArray,
                bincode
            FROM dbo.[tblStripMap]
            WHERE active = 1
                AND stripNo LIKE @StripNo
            ORDER BY createdTime DESC");

                SqlParameter[] parameters = new SqlParameter[]
                {
            new SqlParameter("@StripNo", $"%{stripNo}%")
                };

                DataTable dt = DatabaseHelper.ExecuteQuery(queryBuilder.ToString(), parameters);
                mapArrayData = dt.Copy();
                DisplayMapArrayData(dt);

                labelResultTitleMapArray.Text = $"조회 결과 ({dt.Rows.Count}건)";

                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("조회된 데이터가 없습니다.", "조회 결과",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                labelResultTitleMapArray.Text = "조회 결과";
                MessageBox.Show($"데이터 조회 중 오류: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DisplayMapArrayData(DataTable dt)
        {
            // 상단 ListView (MapArray)
            listViewResult_MapArray.BeginUpdate();
            listViewResult_MapArray.Items.Clear();

            // 하단 ListView (Bin Code)
            listViewResult_MapArray_BinCode.BeginUpdate();
            listViewResult_MapArray_BinCode.Items.Clear();

            foreach (DataRow row in dt.Rows)
            {
                // 상단 ListView: 체크박스 전용 컬럼 + PCB 2D ID + MapArray
                ListViewItem itemTop = new ListViewItem("");  // 체크박스 전용 컬럼

                string stripNo = row["stripNo"]?.ToString() ?? "";
                itemTop.SubItems.Add(stripNo);

                string mapArray = row["mapArray"]?.ToString() ?? "";
                itemTop.SubItems.Add(mapArray);
                itemTop.Tag = row;

                listViewResult_MapArray.Items.Add(itemTop);

                // 하단 ListView: 체크박스 전용 + 빈 정렬 컬럼 + Bin Code
                ListViewItem itemBottom = new ListViewItem("");  // 체크박스 전용 컬럼
                itemBottom.SubItems.Add("");  // 빈 정렬 컬럼 (MapArray의 PCB 2D ID 컬럼과 정렬)

                string bincode = row["bincode"]?.ToString() ?? "";
                itemBottom.SubItems.Add(bincode);
                itemBottom.Tag = row;

                listViewResult_MapArray_BinCode.Items.Add(itemBottom);
            }

            listViewResult_MapArray.EndUpdate();
            listViewResult_MapArray_BinCode.EndUpdate();
        }

        private void ListViewResultMapArray_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            try
            {
                int checkedCount = 0;
                foreach (ListViewItem item in listViewResult_MapArray.Items)
                {
                    if (item.Checked)
                        checkedCount++;
                }

                int totalCount = listViewResult_MapArray.Items.Count;
                labelResultTitleMapArray.Text = $"조회 결과 ({totalCount}건, {checkedCount}개 선택됨)";

                // 체크된 항목의 데이터를 입력 필드에 표시 (1개만 선택 시)
                if (checkedCount == 1)
                {
                    // CheckedItems 사용 (더 안전)
                    if (listViewResult_MapArray.CheckedItems.Count > 0)
                    {
                        ListViewItem checkedItem = listViewResult_MapArray.CheckedItems[0];
                        int checkedIndex = checkedItem.Index;

                        // MapArray 표시 (SubItems[2]: 체크박스전용[0], PCB 2D ID[1], MapArray[2])
                        if (checkedItem.SubItems.Count > 2)
                        {
                            textBoxMapArray.Text = checkedItem.SubItems[2].Text;
                        }
                        else
                        {
                            textBoxMapArray.Clear();
                        }

                        // 하단 ListView의 같은 인덱스에서 Bin Code 가져오기 (SubItems[2]: 체크박스[0], 정렬[1], BinCode[2])
                        if (checkedIndex >= 0 &&
                            checkedIndex < listViewResult_MapArray_BinCode.Items.Count &&
                            listViewResult_MapArray_BinCode.Items[checkedIndex].SubItems.Count > 2)
                        {
                            textBoxBinCode.Text = listViewResult_MapArray_BinCode.Items[checkedIndex].SubItems[2].Text;
                        }
                        else
                        {
                            textBoxBinCode.Clear();
                        }
                    }
                }
                else
                {
                    textBoxMapArray.Clear();
                    textBoxBinCode.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"체크박스 처리 중 오류: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                textBoxMapArray.Clear();
                textBoxBinCode.Clear();
            }
        }

        /// <summary>
        /// 삭제 버튼 클릭
        /// </summary>
        private void BtnDeleteMapArray_Click(object sender, EventArgs e)
        {
            if (!HasPermission(UserPermissions.STRIP_DELETE))
            {
                MessageBox.Show("이 작업을 수행할 권한이 없습니다.", "권한 없음",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var checkedItems = new List<ListViewItem>();
                foreach (ListViewItem item in listViewResult_MapArray.Items)
                {
                    if (item.Checked)
                        checkedItems.Add(item);
                }

                if (checkedItems.Count == 0)
                {
                    MessageBox.Show("삭제할 항목을 선택해주세요.", "알림",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 사유 입력 다이얼로그
                string deleteComment = string.Empty;
                using (Form prompt = new Form())
                {
                    prompt.Width            = 480;
                    prompt.Height           = 220;
                    prompt.Text             = "삭제 사유 입력";
                    prompt.StartPosition    = FormStartPosition.CenterParent;
                    prompt.FormBorderStyle  = FormBorderStyle.FixedDialog;
                    prompt.MaximizeBox      = false;
                    prompt.MinimizeBox      = false;

                    Label lbl = new Label()
                    {
                        Left = 16, Top = 14, Width = 440, AutoSize = false,
                        Text = $"선택한 {checkedItems.Count}건을 삭제합니다.\n삭제 사유를 입력하세요. (필수)"
                    };
                    TextBox tb = new TextBox()
                    {
                        Left = 16, Top = 50, Width = 440, Height = 80,
                        Multiline = true, ScrollBars = ScrollBars.Vertical
                    };
                    Button btnOk = new Button()
                    {
                        Text = "삭제", Left = 278, Top = 148, Width = 85,
                        DialogResult = DialogResult.OK,
                        BackColor = Color.FromArgb(200, 60, 60), ForeColor = Color.White,
                        FlatStyle = FlatStyle.Flat
                    };
                    Button btnCancel = new Button()
                    {
                        Text = "취소", Left = 372, Top = 148, Width = 85,
                        DialogResult = DialogResult.Cancel
                    };

                    prompt.Controls.AddRange(new Control[] { lbl, tb, btnOk, btnCancel });
                    prompt.AcceptButton = btnOk;
                    prompt.CancelButton = btnCancel;

                    if (prompt.ShowDialog(this) != DialogResult.OK) return;

                    deleteComment = tb.Text.Trim();
                    if (string.IsNullOrEmpty(deleteComment))
                    {
                        MessageBox.Show("삭제 사유를 입력해야 합니다.", "알림",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                this.Cursor = Cursors.WaitCursor;
                DeleteMapArrayData(checkedItems, deleteComment);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"삭제 중 오류 발생: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// MapArray 삭제 — SP 'D' (usp_StripMap_Process)
        /// SP 동작: history 기록 + tblStripMap active=0 설정 (논리 삭제)
        /// 이후 물리 삭제(Purge)는 SP 'P'로 별도 처리
        /// </summary>
        private void DeleteMapArrayData(List<ListViewItem> checkedItems, string comment)
        {
            int successCount = 0;
            int failCount = 0;
            StringBuilder errorLog = new StringBuilder();
            string workerIp = GetLocalIPAddress();

            try
            {
                foreach (ListViewItem item in checkedItems)
                {
                    string stripNo = string.Empty;
                    DataRow row = item.Tag as DataRow;
                    if (row == null) { failCount++; continue; }

                    try
                    {
                        stripNo = row["stripNo"]?.ToString();
                        string process = row["process"]?.ToString();

                        DatabaseHelper.ExecuteStoredProcedureNonQuery("dbo.usp_StripMap_Process", new SqlParameter[]
                        {
                            new SqlParameter("@actionType",    SqlDbType.Char, 1) { Value = "D" },
                            new SqlParameter("@stripNo",       stripNo),
                            new SqlParameter("@process",       process),
                            new SqlParameter("@mapArray",      DBNull.Value),
                            new SqlParameter("@bincode",       DBNull.Value),
                            new SqlParameter("@lotNo",         DBNull.Value),
                            new SqlParameter("@targetTimekey", DBNull.Value),
                            new SqlParameter("@workerId",      currentUserId),
                            new SqlParameter("@comment",       $"Strip 삭제 (논리 삭제: active=0) | 사유: {comment}"),
                            new SqlParameter("@workerIp",      workerIp)
                        });

                        AppLogger.Info($"[{ActionTypes.STRIP_DELETE}] user={currentUserId} | stripNo={stripNo} | 사유={comment}");
                        successCount++;
                    }
                    catch (SqlException sqlex)
                    {
                        failCount++;
                        errorLog.AppendLine($"stripNo: {stripNo} - {GetSpErrorMessage(sqlex)}");
                    }
                    catch (Exception ex)
                    {
                        failCount++;
                        errorLog.AppendLine($"stripNo: {stripNo} - {ex.Message}");
                    }
                }

                AppLogger.Info($"[{ActionTypes.STRIP_DELETE}_RESULT] user={currentUserId} | 성공={successCount} 실패={failCount}");
                string resultMessage = $"삭제 완료\n\n성공: {successCount}건\n실패: {failCount}건";
                if (errorLog.Length > 0)
                    resultMessage += $"\n\n오류 내역:\n{errorLog}";

                MessageBox.Show(resultMessage, "삭제 결과",
                    MessageBoxButtons.OK,
                    failCount > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);

                if (successCount > 0)
                    BtnSearchMapArray_Click(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"삭제 처리 중 오류: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 수정 버튼 클릭
        /// </summary>
        private void BtnUpdateMapArray_Click(object sender, EventArgs e)
        {
            try
            {
                var checkedItems = new List<ListViewItem>();
                foreach (ListViewItem item in listViewResult_MapArray.Items)
                {
                    if (item.Checked)
                        checkedItems.Add(item);
                }

                if (checkedItems.Count == 0)
                {
                    MessageBox.Show("수정할 항목을 선택해주세요.", "알림",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                string newMapArray = textBoxMapArray.Text.Trim();
                string newBinCode = textBoxBinCode.Text.Trim();

                if (string.IsNullOrEmpty(newMapArray) && string.IsNullOrEmpty(newBinCode))
                {
                    MessageBox.Show("MapArray 또는 Bin Code를 입력해주세요.", "알림",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // ── 자릿수 검증: 기존 값과 입력 값의 길이가 다르면 수정 불가 ──
                var lengthErrors = new StringBuilder();

                foreach (ListViewItem chkItem in checkedItems)
                {
                    DataRow chkRow = chkItem.Tag as DataRow;
                    if (chkRow == null) continue;

                    string chkStripNo = chkRow["stripNo"]?.ToString() ?? "";

                    if (!string.IsNullOrEmpty(newMapArray))
                    {
                        string origMapArray = chkRow["mapArray"]?.ToString() ?? "";
                        if (!string.IsNullOrEmpty(origMapArray) && newMapArray.Length != origMapArray.Length)
                        {
                            lengthErrors.AppendLine(
                                $"[{chkStripNo}] MapArray: 기존 {origMapArray.Length}자 → 입력 {newMapArray.Length}자");
                        }
                    }

                    if (!string.IsNullOrEmpty(newBinCode))
                    {
                        string origBinCode = chkRow["bincode"]?.ToString() ?? "";
                        if (!string.IsNullOrEmpty(origBinCode) && newBinCode.Length != origBinCode.Length)
                        {
                            lengthErrors.AppendLine(
                                $"[{chkStripNo}] BinCode: 기존 {origBinCode.Length}자 → 입력 {newBinCode.Length}자");
                        }
                    }
                }

                if (lengthErrors.Length > 0)
                {
                    MessageBox.Show(
                        $"자릿수가 다른 항목이 있어 수정할 수 없습니다.\n\n{lengthErrors}\n기존과 동일한 자릿수로 입력해주세요.",
                        "자릿수 오류",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }
                // ── 자릿수 검증 끝 ──

                DialogResult result = MessageBox.Show(
                    $"{checkedItems.Count}건의 데이터를 수정하시겠습니까?",
                    "수정 확인",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    this.Cursor = Cursors.WaitCursor;
                    UpdateMapArrayData(checkedItems, newMapArray, newBinCode);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"수정 중 오류 발생: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// MapArray 수정 — SP 'U' (usp_StripMap_Process)
        /// SP의 COALESCE(@mapArray, mapArray) 처리: 빈 값이면 NULL 전달 → 기존 값 유지
        /// </summary>
        private void UpdateMapArrayData(List<ListViewItem> checkedItems, string newMapArray, string newBinCode)
        {
            int successCount = 0;
            int failCount = 0;
            StringBuilder errorLog = new StringBuilder();
            string workerIp = GetLocalIPAddress();

            try
            {
                foreach (ListViewItem item in checkedItems)
                {
                    string stripNo = string.Empty;
                    DataRow row = item.Tag as DataRow;
                    if (row == null) { failCount++; continue; }

                    try
                    {
                        stripNo  = row["stripNo"]?.ToString();
                        string process = row["process"]?.ToString();

                        // 빈 값이면 NULL → SP의 COALESCE가 기존 DB 값 유지
                        object mapArrayParam = string.IsNullOrEmpty(newMapArray) ? (object)DBNull.Value : newMapArray;
                        object bincodeParam  = string.IsNullOrEmpty(newBinCode)  ? (object)DBNull.Value : newBinCode;

                        DatabaseHelper.ExecuteStoredProcedureNonQuery("dbo.usp_StripMap_Process", new SqlParameter[]
                        {
                            new SqlParameter("@actionType",    SqlDbType.Char, 1) { Value = "U" },
                            new SqlParameter("@stripNo",       stripNo),
                            new SqlParameter("@process",       process),
                            new SqlParameter("@mapArray",      mapArrayParam),
                            new SqlParameter("@bincode",       bincodeParam),
                            new SqlParameter("@lotNo",         DBNull.Value),
                            new SqlParameter("@targetTimekey", DBNull.Value),
                            new SqlParameter("@workerId",      currentUserId),
                            new SqlParameter("@comment",       "MapArray/BinCode 수정"),
                            new SqlParameter("@workerIp",      workerIp)
                        });

                        AppLogger.Info($"[{ActionTypes.STRIP_UPDATE}] user={currentUserId} | stripNo={stripNo} | mapArray={newMapArray} binCode={newBinCode}");
                        successCount++;
                    }
                    catch (SqlException sqlex)
                    {
                        failCount++;
                        errorLog.AppendLine($"stripNo: {stripNo} - {GetSpErrorMessage(sqlex)}");
                    }
                    catch (Exception ex)
                    {
                        failCount++;
                        errorLog.AppendLine($"stripNo: {stripNo} - {ex.Message}");
                    }
                }

                AppLogger.Info($"[{ActionTypes.STRIP_UPDATE}_RESULT] user={currentUserId} | 성공={successCount} 실패={failCount}");
                string resultMessage = $"수정 완료\n\n성공: {successCount}건\n실패: {failCount}건";
                if (errorLog.Length > 0)
                    resultMessage += $"\n\n오류 내역:\n{errorLog}";

                MessageBox.Show(resultMessage, "수정 결과",
                    MessageBoxButtons.OK,
                    failCount > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);

                if (successCount > 0)
                    BtnSearchMapArray_Click(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"수정 처리 중 오류: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region PCB 2D ID 원복 탭

        /// <summary>
        /// 조회 버튼 클릭 (PCB 원복 탭) - tblStripMapHistory 조회
        /// </summary>
        private void BtnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                string lotNo = textBox_LOT.Text.Trim();
                string stripNo = textBox_PCB.Text.Trim();
                string mgzRf = textBox_MGZ.Text.Trim();

                // 검색 조건 체크 제거 - 빈 값이어도 전체 조회
                LoadHistoryData(lotNo, stripNo, mgzRf);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"조회 중 오류 발생: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadHistoryData(string lotNo, string stripNo, string mgzRf)
        {
            try
            {
                // PCB 원복 탭: tblStripMapHistory 조회 (작업 이력 감사 로그)
                StringBuilder queryBuilder = new StringBuilder();
                queryBuilder.Append(@"
                SELECT
                    timekey,
                    [version],
                    active,
                    stripNo,
                    process,
                    lotNo,
                    mgzRf,
                    comment,
                    actionType,
                    workerId,
                    createdTime
                FROM dbo.[tblStripMapHistory]
                WHERE 1=1
                ");

                var parameters = new List<SqlParameter>();

                if (!string.IsNullOrEmpty(lotNo))
                {
                    queryBuilder.Append(" AND lotNo LIKE @LotNo");
                    parameters.Add(new SqlParameter("@LotNo", $"%{lotNo}%"));
                }

                if (!string.IsNullOrEmpty(stripNo))
                {
                    queryBuilder.Append(" AND stripNo LIKE @StripNo");
                    parameters.Add(new SqlParameter("@StripNo", $"%{stripNo}%"));
                }

                if (!string.IsNullOrEmpty(mgzRf))
                {
                    queryBuilder.Append(" AND mgzRf LIKE @MgzRf");
                    parameters.Add(new SqlParameter("@MgzRf", $"%{mgzRf}%"));
                }

                queryBuilder.Append(" ORDER BY timekey DESC");

                DataTable dt = DatabaseHelper.ExecuteQuery(queryBuilder.ToString(), parameters.ToArray());
                originalData = dt.Copy();
                DisplayHistoryData(dt);

                labelResultTitle.Text = $"조회 결과 ({dt.Rows.Count}건)";

                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("조회된 데이터가 없습니다.", "조회 결과",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                labelResultTitle.Text = "조회 결과";
                MessageBox.Show($"데이터 조회 중 오류: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DisplayHistoryData(DataTable dt)
        {
            // PCB 원복 탭: tblStripMapHistory 이력 표시
            // 컬럼: 버전(INT) | PCB 2D ID | LOT ID | 사유(comment) | 생성 시각(timekey)
            listViewResult_PCB.BeginUpdate();
            listViewResult_PCB.Items.Clear();

            foreach (DataRow row in dt.Rows)
            {
                // 첫 번째 컬럼은 체크박스 전용 (빈 값)
                ListViewItem item = new ListViewItem("");

                // 버전: INT (tblStripMap.[version]과 동일 역할)
                string version = row["version"]?.ToString() ?? "";
                item.SubItems.Add(version);

                string lotNo = row["lotNo"]?.ToString() ?? "";
                item.SubItems.Add(lotNo);

                string stripNo = row["stripNo"]?.ToString() ?? "";
                item.SubItems.Add(stripNo);

                string mgzRf = row["mgzRf"]?.ToString() ?? "";
                item.SubItems.Add(mgzRf);

                string comment = row["comment"]?.ToString() ?? "";
                item.SubItems.Add(comment);

                // 생성 시각: timekey(yyyyMMddHHmmssffffff) → "yyyy-MM-dd HH:mm:ss.ffffff" 형식 표시
                string timekey = row["timekey"]?.ToString() ?? "";
                string displayTime = timekey.Length >= 20
                    ? $"{timekey.Substring(0, 4)}-{timekey.Substring(4, 2)}-{timekey.Substring(6, 2)} {timekey.Substring(8, 2)}:{timekey.Substring(10, 2)}:{timekey.Substring(12, 2)}.{timekey.Substring(14, 6)}"
                    : timekey;
                item.SubItems.Add(displayTime);

                // actionType이 STRIP_PURGE면 다른 색으로 표시
                string actionType = row["actionType"]?.ToString() ?? "";
                bool isPurgeRecord = actionType == ActionTypes.STRIP_PURGE;
                if (isPurgeRecord)
                {
                    item.BackColor = Color.LightCoral;  // Purge 이력은 빨간색

                    // Purge 원복 권한이 없으면 회색 표시
                    if (!HasPermission(UserPermissions.STRIP_PURGE_ROLLBACK))
                    {
                        item.ForeColor = Color.Gray;
                    }
                }

                item.Tag = row;
                listViewResult_PCB.Items.Add(item);
            }
            listViewResult_PCB.EndUpdate();
        }

        private void BtnRestore_Click(object sender, EventArgs e)
        {
            // ① 권한 체크 (가장 먼저)
            if (!HasPermission(UserPermissions.STRIP_ROLLBACK))
            {
                MessageBox.Show("원복 권한이 없습니다.", "권한 없음",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var checkedItems = new List<ListViewItem>();
                foreach (ListViewItem item in listViewResult_PCB.Items)
                {
                    if (item.Checked)
                        checkedItems.Add(item);
                }

                if (checkedItems.Count == 0)
                {
                    MessageBox.Show("원복할 항목을 선택해주세요.", "알림",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // STRIP_PURGE 이력 포함 여부 사전 검증 — 원복 버튼으로는 불가, Purge복원 버튼 전용
                var purgeItems = checkedItems.Where(item =>
                {
                    DataRow r = item.Tag as DataRow;
                    return r != null && (r["actionType"]?.ToString() ?? "") == ActionTypes.STRIP_PURGE;
                }).ToList();

                if (purgeItems.Count > 0)
                {
                    MessageBox.Show(
                        $"선택 항목 중 {purgeItems.Count}건은 STRIP_PURGE 이력입니다.\n\n" +
                        "Purge 이력 복원은 '원복' 버튼이 아닌 'Purge복원' 버튼을 사용하세요.",
                        "선택 오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 동일 stripNo 중복 선택 차단
                var duplicateStripNos = checkedItems
                    .Select(item => (item.Tag as DataRow)?["stripNo"]?.ToString() ?? "")
                    .GroupBy(s => s)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                if (duplicateStripNos.Count > 0)
                {
                    MessageBox.Show(
                        $"동일한 Strip 번호가 여러 건 선택되었습니다.\n" +
                        $"Strip 당 1건의 이력만 선택해주세요.\n\n" +
                        $"중복 Strip:\n" + string.Join("\n", duplicateStripNos.Select(s => $"  • {s}")),
                        "중복 선택 오류",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // ④ 선택 이력 이후 수정 이력 존재 시 경고
                var stripsWithPostChanges = new List<string>();
                foreach (ListViewItem warnItem in checkedItems)
                {
                    DataRow warnRow = warnItem.Tag as DataRow;
                    if (warnRow == null) continue;
                    string sNo = warnRow["stripNo"]?.ToString() ?? "";
                    string tk  = warnRow["timekey"]?.ToString()  ?? "";
                    if (string.IsNullOrEmpty(sNo) || string.IsNullOrEmpty(tk)) continue;

                    DataTable dtChk = DatabaseHelper.ExecuteQuery(
                        "SELECT TOP 1 1 AS chk FROM dbo.tblStripMapHistory WHERE stripNo = @sn AND timekey > @tk",
                        new SqlParameter[] {
                            new SqlParameter("@sn", sNo),
                            new SqlParameter("@tk", tk)
                        });
                    if (dtChk.Rows.Count > 0)
                        stripsWithPostChanges.Add(sNo);
                }

                if (stripsWithPostChanges.Count > 0)
                {
                    DialogResult warnResult = MessageBox.Show(
                        "⚠️ 아래 Strip은 선택한 이력 이후 수정 이력이 존재합니다.\n\n" +
                        string.Join("\n", stripsWithPostChanges.Select(s => $"  • {s}")) +
                        "\n\n원복 이후의 변경 내용은 반영되지 않습니다.\n그래도 계속하시겠습니까?",
                        "이력 이후 수정 이력 경고",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning,
                        MessageBoxDefaultButton.Button2);
                    if (warnResult != DialogResult.Yes) return;
                }

                DialogResult result = MessageBox.Show(
                    $"{checkedItems.Count}건의 데이터를 원복하시겠습니까?\n\n선택한 버전으로 데이터를 되돌립니다.",
                    "원복 확인",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    this.Cursor = Cursors.WaitCursor;
                    RestoreFromHistory(checkedItems);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"원복 중 오류 발생: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// Purge 복원 버튼 클릭 — PURGE 이력 레코드를 tblStripMap으로 복원 (SP 'Q')
        /// 대상: actionType = 'PURGE' 인 이력 레코드만 선택 가능
        /// 권한: STRIP_PURGE_ROLLBACK (Admin 이상)
        /// </summary>
        private void BtnPurgeRollback_Click(object sender, EventArgs e)
        {
            // ① 권한 체크 (가장 먼저 — Admin 이상)
            if (!HasPermission(UserPermissions.STRIP_PURGE_ROLLBACK))
            {
                MessageBox.Show("Purge 복원은 시스템 관리자만 가능합니다.", "권한 없음",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var checkedItems = new List<ListViewItem>();
                foreach (ListViewItem item in listViewResult_PCB.Items)
                {
                    if (item.Checked)
                        checkedItems.Add(item);
                }

                if (checkedItems.Count == 0)
                {
                    MessageBox.Show("복원할 이력을 선택해주세요.", "알림",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // ② PURGE 이력 여부 검증
                var nonPurgeItems = checkedItems.Where(item =>
                {
                    DataRow row = item.Tag as DataRow;
                    if (row == null) return true;
                    string actionType = row["actionType"]?.ToString() ?? "";
                    return actionType != ActionTypes.STRIP_PURGE;
                }).ToList();

                if (nonPurgeItems.Count > 0)
                {
                    MessageBox.Show(
                        $"선택 항목 중 {nonPurgeItems.Count}건은 STRIP_PURGE 이력이 아닙니다.\n\n" +
                        "Purge 복원은 STRIP_PURGE 이력 레코드(빨간색)만 대상으로 합니다.",
                        "선택 오류",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // ③ 동일 stripNo에서 active=1 중복 선택 차단 (active=0은 복수 선택 허용)
                var duplicateActiveOneStripNos = checkedItems
                    .Where(item => { DataRow r = item.Tag as DataRow; return r != null && Convert.ToBoolean(r["active"]); })
                    .Select(item => (item.Tag as DataRow)?["stripNo"]?.ToString() ?? "")
                    .GroupBy(s => s)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                if (duplicateActiveOneStripNos.Count > 0)
                {
                    MessageBox.Show(
                        $"아래 Strip은 active=1 이력이 여러 건 선택되었습니다.\n" +
                        $"active=1은 Strip 당 1건만 복원할 수 있습니다.\n\n" +
                        $"중복 Strip:\n" + string.Join("\n", duplicateActiveOneStripNos.Select(s => $"  • {s}")),
                        "중복 선택 오류",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // ④ Purge 이후 수정 이력 존재 시 경고 (STRIP_PURGE 제외 — 동시 복원 대상 오탐 방지)
                var stripsWithPostChanges = new List<string>();
                foreach (ListViewItem warnItem in checkedItems)
                {
                    DataRow warnRow = warnItem.Tag as DataRow;
                    if (warnRow == null) continue;
                    string sNo = warnRow["stripNo"]?.ToString() ?? "";
                    string tk  = warnRow["timekey"]?.ToString()  ?? "";
                    if (string.IsNullOrEmpty(sNo) || string.IsNullOrEmpty(tk)) continue;

                    DataTable dtChk = DatabaseHelper.ExecuteQuery(
                        "SELECT TOP 1 1 AS chk FROM dbo.tblStripMapHistory WHERE stripNo = @sn AND timekey > @tk AND actionType <> 'STRIP_PURGE'",
                        new SqlParameter[] {
                            new SqlParameter("@sn", sNo),
                            new SqlParameter("@tk", tk)
                        });
                    if (dtChk.Rows.Count > 0)
                        stripsWithPostChanges.Add(sNo);
                }

                if (stripsWithPostChanges.Count > 0)
                {
                    DialogResult warnResult = MessageBox.Show(
                        "⚠️ 아래 Strip은 Purge 이후 수정 이력이 존재합니다.\n\n" +
                        string.Join("\n", stripsWithPostChanges.Distinct().Select(s => $"  • {s}")) +
                        "\n\nPurge 이후 변경된 내용은 반영되지 않습니다.\n그래도 계속하시겠습니까?",
                        "Purge 이후 수정 이력 경고",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning,
                        MessageBoxDefaultButton.Button2);
                    if (warnResult != DialogResult.Yes) return;
                }

                DialogResult result = MessageBox.Show(
                    $"⚠️ Purge 복원 확인 ⚠️\n\n" +
                    $"{checkedItems.Count}건의 PURGE 이력을 복원하시겠습니까?\n\n" +
                    "복원 시 해당 데이터가 tblStripMap에 되살아납니다.",
                    "Purge 복원 확인",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    this.Cursor = Cursors.WaitCursor;
                    PurgeRollbackData(checkedItems);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Purge 복원 중 오류 발생: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// PURGE 이력 복원 — SP 'Q' (usp_StripMap_Process)
        /// </summary>
        private void PurgeRollbackData(List<ListViewItem> checkedItems)
        {
            int successCount = 0;
            int failCount = 0;
            StringBuilder errorLog = new StringBuilder();
            string workerIp = GetLocalIPAddress();

            try
            {
                foreach (ListViewItem item in checkedItems)
                {
                    DataRow row = item.Tag as DataRow;
                    if (row == null) { failCount++; continue; }

                    string stripNo = string.Empty;
                    try
                    {
                        stripNo  = row["stripNo"]?.ToString();
                        string process       = row["process"]?.ToString();
                        string targetTimekey = row["timekey"]?.ToString();  // PURGE 이력 레코드 식별키

                        DatabaseHelper.ExecuteStoredProcedureNonQuery("dbo.usp_StripMap_Process", new SqlParameter[]
                        {
                            new SqlParameter("@actionType",     SqlDbType.Char, 1) { Value = "Q" },
                            new SqlParameter("@stripNo",        stripNo),
                            new SqlParameter("@process",        process),
                            new SqlParameter("@mapArray",       DBNull.Value),
                            new SqlParameter("@bincode",        DBNull.Value),
                            new SqlParameter("@lotNo",          DBNull.Value),
                            new SqlParameter("@targetTimekey",  SqlDbType.VarChar, 20) { Value = targetTimekey },
                            new SqlParameter("@workerId",       currentUserId),
                            new SqlParameter("@comment",        $"Purge 이력 복원 (timekey: {targetTimekey})"),
                            new SqlParameter("@workerIp",       workerIp)
                        });

                        AppLogger.Info($"[{ActionTypes.STRIP_PURGE_ROLLBACK}] user={currentUserId} | stripNo={stripNo} | timekey={targetTimekey}");
                        successCount++;
                    }
                    catch (SqlException sqlex)
                    {
                        failCount++;
                        errorLog.AppendLine($"stripNo: {stripNo} - {GetSpErrorMessage(sqlex)}");
                    }
                    catch (Exception ex)
                    {
                        failCount++;
                        errorLog.AppendLine($"stripNo: {stripNo} - {ex.Message}");
                    }
                }

                AppLogger.Info($"[{ActionTypes.STRIP_PURGE_ROLLBACK}_RESULT] user={currentUserId} | 성공={successCount} 실패={failCount}");
                string resultMessage = $"Purge 복원 완료\n\n성공: {successCount}건\n실패: {failCount}건";
                if (errorLog.Length > 0)
                    resultMessage += $"\n\n오류 내역:\n{errorLog}";

                MessageBox.Show(resultMessage, "Purge 복원 결과",
                    MessageBoxButtons.OK,
                    failCount > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);

                if (successCount > 0)
                    BtnSearch_Click(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Purge 복원 처리 중 오류: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ListViewResult_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            int checkedCount = 0;
            foreach (ListViewItem item in listViewResult_PCB.Items)
            {
                if (item.Checked)
                    checkedCount++;
            }

            int totalCount = listViewResult_PCB.Items.Count;
            labelResultTitle.Text = $"조회 결과 ({totalCount}건, {checkedCount}개 선택됨)";
        }

        /// <summary>
        /// tblStripMapHistory 이력으로부터 원복 — SP 'R' 전용 (일반 이력만)
        /// STRIP_PURGE 이력은 이 경로로 처리 불가 — BtnPurgeRollback_Click(SP 'Q') 전용
        /// </summary>
        private void RestoreFromHistory(List<ListViewItem> checkedItems)
        {
            try
            {
                int successCount = 0;
                int failCount = 0;
                StringBuilder errorLog = new StringBuilder();
                string workerIp = GetLocalIPAddress();

                foreach (ListViewItem item in checkedItems)
                {
                    string stripNo = string.Empty;
                    DataRow row = item.Tag as DataRow;
                    if (row == null) { failCount++; continue; }

                    try
                    {
                        stripNo = row["stripNo"]?.ToString();
                        string process = row["process"]?.ToString();
                        string targetTimekey = row["timekey"]?.ToString();   // 이력 레코드 식별키
                        string actionType = row["actionType"] != DBNull.Value ? row["actionType"].ToString() : string.Empty;

                        // STRIP_PURGE 이력은 원복 버튼 경로로 처리 불가 (Purge복원 버튼 전용)
                        if (actionType == ActionTypes.STRIP_PURGE)
                        {
                            failCount++;
                            errorLog.AppendLine($"stripNo: {stripNo} - STRIP_PURGE 이력은 'Purge복원' 버튼을 사용하세요.");
                            continue;
                        }

                        // 일반 원복 권한 체크
                        if (!HasPermission(UserPermissions.STRIP_ROLLBACK))
                        {
                            failCount++;
                            errorLog.AppendLine($"stripNo: {stripNo} - 원복 권한이 없습니다.");
                            continue;
                        }

                        string comment = $"이력 원복 (timekey: {targetTimekey})";

                        DatabaseHelper.ExecuteStoredProcedureNonQuery("dbo.usp_StripMap_Process", new SqlParameter[]
                        {
                            new SqlParameter("@actionType",    SqlDbType.Char, 1) { Value = "R" },
                            new SqlParameter("@stripNo",       stripNo),
                            new SqlParameter("@process",       process),
                            new SqlParameter("@mapArray",      DBNull.Value),
                            new SqlParameter("@bincode",       DBNull.Value),
                            new SqlParameter("@lotNo",         DBNull.Value),
                            new SqlParameter("@targetTimekey", SqlDbType.VarChar, 20) { Value = targetTimekey },
                            new SqlParameter("@workerId",      currentUserId),
                            new SqlParameter("@comment",       comment),
                            new SqlParameter("@workerIp",      workerIp)
                        });

                        AppLogger.Info($"[{ActionTypes.STRIP_ROLLBACK}] user={currentUserId} | stripNo={stripNo} | timekey={targetTimekey}");
                        successCount++;
                    }
                    catch (SqlException sqlex)
                    {
                        failCount++;
                        errorLog.AppendLine($"stripNo: {stripNo} - {GetSpErrorMessage(sqlex)}");
                    }
                    catch (Exception ex)
                    {
                        failCount++;
                        errorLog.AppendLine($"stripNo: {stripNo} - {ex.Message}");
                    }
                }

                AppLogger.Info($"[{ActionTypes.STRIP_ROLLBACK}_RESULT] user={currentUserId} | 성공={successCount} 실패={failCount}");
                string resultMessage = $"성공: {successCount}건\n실패: {failCount}건";
                if (errorLog.Length > 0)
                    resultMessage += $"\n\n오류 내역:\n{errorLog}";

                MessageBox.Show(resultMessage, "원복 결과",
                    MessageBoxButtons.OK,
                    failCount > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);

                if (successCount > 0)
                    BtnSearch_Click(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"원복 처리 중 오류: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region 엔터 키 조회

        private void SearchTextBox_LotId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true; // 비프음 방지
                BtnSearch2_Click(sender, e);
            }
        }

        private void SearchTextBox_MapArray_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                BtnSearchMapArray_Click(sender, e);
            }
        }

        private void SearchTextBox_PCB_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                BtnSearch_Click(sender, e);
            }
        }

        #endregion

        #region ListView 셀 선택 / Ctrl+C 복사

        /// <summary>
        /// 하이라이트 셀 상태 갱신 및 재그리기 요청
        /// </summary>
        private void SetHighlight(ListView lv, int row, int col)
        {
            var prev = _hlCell.lv;
            _hlCell = (lv, row, col);
            prev?.Invalidate();
            lv?.Invalidate();
        }

        /// <summary>
        /// 마우스 클릭 시 HitTest로 클릭한 셀을 특정하고 하이라이트 설정
        /// </summary>
        private void ListView_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            var lv = sender as ListView;
            if (lv == null) return;

            var hit = lv.HitTest(e.Location);
            if (hit?.Item != null && hit.SubItem != null)
            {
                _mouseDownColIndex = hit.Item.SubItems.IndexOf(hit.SubItem);
                SetHighlight(lv, hit.Item.Index, _mouseDownColIndex);
            }
            else
            {
                _mouseDownColIndex = -1;
            }
        }

        /// <summary>
        /// 드래그 시작 시 MouseDown에서 기록한 컬럼의 셀 하이라이트 설정 (복사 없음)
        /// </summary>
        private void ListView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (_mouseDownColIndex < 0) return;

            var lv = sender as ListView;
            var item = e.Item as ListViewItem;
            if (lv == null || item == null) return;

            if (_mouseDownColIndex < item.SubItems.Count)
                SetHighlight(lv, item.Index, _mouseDownColIndex);
        }

        /// <summary>
        /// Ctrl+C: 하이라이트된 셀 값만 클립보드에 복사
        /// </summary>
        private void ListView_CopyKeyDown(object sender, KeyEventArgs e)
        {
            if (!e.Control || e.KeyCode != Keys.C) return;

            var (lv, row, col) = _hlCell;
            if (lv == null || row < 0 || col < 0 || row >= lv.Items.Count) return;

            var item = lv.Items[row];
            if (col >= item.SubItems.Count) return;

            string text = item.SubItems[col].Text;
            if (!string.IsNullOrEmpty(text))
                Clipboard.SetText(text);
        }

        /// <summary>
        /// DrawSubItem 공통 헬퍼: 기본 셀 그리기 (행 선택 파란 배경 유지)
        /// </summary>
        private void DrawSubItemDefault(DrawListViewSubItemEventArgs e, ListView lv)
        {
            e.DrawDefault = false;

            bool isSelected = e.Item.Selected;

            Color bgColor = isSelected ? SystemColors.Highlight : e.Item.BackColor;
            using (SolidBrush bgBrush = new SolidBrush(bgColor))
                e.Graphics.FillRectangle(bgBrush, e.Bounds);

            Color fgColor = isSelected ? SystemColors.HighlightText : e.Item.ForeColor;
            TextRenderer.DrawText(
                e.Graphics,
                e.SubItem.Text,
                lv.Font,
                e.Bounds,
                fgColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis
            );
        }

        #endregion
    }
}