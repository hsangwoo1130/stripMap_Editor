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
        public string LoggedInUserId   { get; set; }
        public string LoggedInUserName { get; set; }
        public string LoggedInUserRole { get; set; }
        public RvManager Rv            { get; set; }

        private DataTable originalData; // 원본 데이터 저장용 (PCB 원복 탭)
        private int _periodOffset = 0;  // 0=현재 기간, 1=1개월 전, ...
        private DataTable lotIdData; // Lot ID 변경 탭 데이터
        private Dictionary<string, string> modifiedLotIds; // 수정된 Lot ID 저장 (stripNo, newLotId)

        // 관리자 탭 (동적 생성)
        private TabPage _tabPageAdmin;
        private TabPage _prevTabPage;   // 관리자 탭 클릭 직전에 보던 탭
        
        // 사용자 권한 관련
        private UserRole currentUserRole = UserRole.USER;
        private string currentUserId = string.Empty;
        private HashSet<string> _userPermissions = new HashSet<string>();
        private HashSet<string> _userMenus       = new HashSet<string>();
        private Dictionary<string, (string menuName, string menuUrl)> _menuInfo = new Dictionary<string, (string, string)>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, TabPage> _menuUrlMap = new Dictionary<string, TabPage>(StringComparer.OrdinalIgnoreCase);

        // 셀 단위 하이라이트 관련
        private (ListView lv, int row, int col) _hlCell = (null, -1, -1);
        private int _mouseDownColIndex = -1;

        // MapArray 상하 ListView 선택 동기화 재진입 방지
        private bool _syncingSelection = false;
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
                _menuInfo        = DatabaseHelper.LoadMenuInfo();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"권한 정보를 불러오지 못했습니다. 최소 권한으로 실행됩니다.\n\n{ex.Message}",
                    "권한 로드 실패", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _userPermissions = new HashSet<string>();
                _userMenus       = new HashSet<string>();
                _menuInfo        = new Dictionary<string, (string, string)>(StringComparer.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// 사용자 권한에 따른 UI 제어 (DB 메뉴 기반, tblMenu 1:1 매핑)
        /// STRIP_EDIT → tabPageLotId      (menuUrl=lotidedit)
        /// MAP_EDIT   → tabPageMapArray   (menuUrl=mapedit)
        /// STRIP_HIST → tabPagePcbRestore (menuUrl=striphistory)
        /// PURGE      → _tabPageAdmin     (menuUrl=purge)
        /// </summary>
        private void ApplyUserPermissions()
        {
            tabPageLotId.Parent      = _userMenus.Contains(MenuIds.STRIP_EDIT) ? tabControl_Strip : null;
            tabPageMapArray.Parent   = _userMenus.Contains(MenuIds.MAP_EDIT)   ? tabControl_Strip : null;
            tabPagePcbRestore.Parent = _userMenus.Contains(MenuIds.STRIP_HIST) ? tabControl_Strip : null;
            _tabPageAdmin.Parent     = _userMenus.Contains(MenuIds.PURGE)      ? tabControl_Strip : null;

            // tblMenu.menuName으로 탭 텍스트 동적 설정
            tabPageLotId.Text      = GetMenuName(MenuIds.STRIP_EDIT, tabPageLotId.Text);
            tabPageMapArray.Text   = GetMenuName(MenuIds.MAP_EDIT,   tabPageMapArray.Text);
            tabPagePcbRestore.Text = GetMenuName(MenuIds.STRIP_HIST, tabPagePcbRestore.Text);
            _tabPageAdmin.Text     = " " + GetMenuName(MenuIds.PURGE, _tabPageAdmin.Text) + " ";

            // menuUrl → TabPage 매핑 구성
            _menuUrlMap.Clear();
            RegisterMenuUrl(MenuIds.STRIP_EDIT, tabPageLotId);
            RegisterMenuUrl(MenuIds.MAP_EDIT,   tabPageMapArray);
            RegisterMenuUrl(MenuIds.STRIP_HIST, tabPagePcbRestore);
            RegisterMenuUrl(MenuIds.PURGE,      _tabPageAdmin);

            if (tabControl_Strip.TabCount > 0)
                tabControl_Strip.SelectedIndex = 0;
        }

        private string GetMenuName(string menuId, string fallback)
            => _menuInfo.TryGetValue(menuId, out var info) && !string.IsNullOrEmpty(info.menuName)
               ? info.menuName : fallback;

        private void RegisterMenuUrl(string menuId, TabPage tab)
        {
            if (_menuInfo.TryGetValue(menuId, out var info) && !string.IsNullOrEmpty(info.menuUrl))
                _menuUrlMap[info.menuUrl] = tab;
        }

        /// <summary>
        /// menuUrl로 탭 이동 (예: NavigateTo("mapedit"))
        /// </summary>
        public void NavigateTo(string menuUrl)
        {
            if (string.IsNullOrEmpty(menuUrl)) return;
            if (!_menuUrlMap.TryGetValue(menuUrl, out TabPage target)) return;
            if (target.Parent == null) return;  // 권한 없어 숨겨진 탭
            tabControl_Strip.SelectedTab = target;
        }

        /// <summary>
        /// Purge 탭 동적 생성 — 탭 클릭 시 AdminForm 모달 팝업
        /// tblRoleMenu PURGE 메뉴 권한이 있는 경우에만 표시 (ApplyUserPermissions에서 제어)
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
            {
                // 관리자 탭이 아닐 때 → 현재 탭을 기억
                _prevTabPage = tabControl_Strip.SelectedTab;
                return;
            }

            // 관리자 탭 클릭 → 보던 탭으로 복원 후 AdminForm 팝업
            if (_prevTabPage != null && tabControl_Strip.TabPages.Contains(_prevTabPage))
                tabControl_Strip.SelectedTab = _prevTabPage;
            else
            {
                for (int i = 0; i < tabControl_Strip.TabCount; i++)
                {
                    if (tabControl_Strip.TabPages[i] != _tabPageAdmin)
                    {
                        tabControl_Strip.SelectedIndex = i;
                        break;
                    }
                }
            }

            using (var adminForm = new AdminForm(currentUserId, LoggedInUserRole, _userPermissions, Rv))
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
            listViewResult_MapArray_BinCode.MouseDoubleClick += ListViewResultMapArrayBinCode_MouseDoubleClick;
            listViewResult_MapArray.SelectedIndexChanged          += ListViewResultMapArray_SelectedIndexChanged;
            listViewResult_MapArray_BinCode.SelectedIndexChanged  += ListViewResultMapArrayBinCode_SelectedIndexChanged;
            this.checkBoxVFlip.CheckedChanged  += CheckBoxFlip_CheckedChanged;
            this.checkBoxHFlip.CheckedChanged  += CheckBoxFlip_CheckedChanged;
            btnGridOriginal.Click      += BtnGridOriginal_Click;
            btnGridPreview.Click       += BtnGridPreview_Click;
            btnRefreshGrid.Click       += BtnRefreshGrid_Click;
            textBoxMapArray.TextChanged += TextBoxMapArray_TextChanged;

            // PCB 2D ID 원복 탭 이벤트
            btnSearch_PCB.Click += BtnSearch_Click;
            btnRestore_PCB.Click += BtnRestore_Click;
            btnPurgeRollback_PCB.Click += BtnPurgeRollback_Click;
            btnPrevPeriod.Click += BtnPrevPeriod_Click;
            btnNextPeriod.Click += BtnNextPeriod_Click;
            // 기간 라벨 초기값: 오늘 기준 한 달 표시
            var (periodStart, periodEnd) = GetPeriodRange(0);
            labelPeriod.Text = $"{periodStart:yyyy-MM-dd} ~ {periodEnd:yyyy-MM-dd}";
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

            this.FormClosing += MainForm_FormClosing;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Lot ID 탭
            btnSearch_LotId.Click        -= BtnSearch2_Click;
            btnModify_LotId.Click        -= BtnModify_Click;
            btnUpdate_LotId.Click        -= BtnSave_Click;
            listViewResult_LotId.ItemChecked        -= ListViewResult2_ItemChecked;
            listViewResult_LotId.ColumnWidthChanging -= ListView_ColumnWidthChanging;

            // MapArray 탭
            btnSearch_MapArray.Click     -= BtnSearchMapArray_Click;
            btnDelete_MapArray.Click     -= BtnDeleteMapArray_Click;
            btnUpdate_MapArray.Click     -= BtnUpdateMapArray_Click;
            listViewResult_MapArray.ItemChecked          -= ListViewResultMapArray_ItemChecked;
            listViewResult_MapArray.ColumnWidthChanging  -= ListView_ColumnWidthChanging;
            listViewResult_MapArray_BinCode.MouseDoubleClick -= ListViewResultMapArrayBinCode_MouseDoubleClick;
            listViewResult_MapArray.SelectedIndexChanged         -= ListViewResultMapArray_SelectedIndexChanged;
            listViewResult_MapArray_BinCode.SelectedIndexChanged -= ListViewResultMapArrayBinCode_SelectedIndexChanged;
            checkBoxVFlip.CheckedChanged -= CheckBoxFlip_CheckedChanged;
            checkBoxHFlip.CheckedChanged -= CheckBoxFlip_CheckedChanged;
            btnGridOriginal.Click      -= BtnGridOriginal_Click;
            btnGridPreview.Click       -= BtnGridPreview_Click;
            btnRefreshGrid.Click       -= BtnRefreshGrid_Click;
            textBoxMapArray.TextChanged -= TextBoxMapArray_TextChanged;

            // PCB 원복 탭
            btnSearch_PCB.Click          -= BtnSearch_Click;
            btnRestore_PCB.Click         -= BtnRestore_Click;
            btnPurgeRollback_PCB.Click   -= BtnPurgeRollback_Click;
            btnPrevPeriod.Click          -= BtnPrevPeriod_Click;
            btnNextPeriod.Click          -= BtnNextPeriod_Click;
            listViewResult_PCB.ItemChecked        -= ListViewResult_ItemChecked;
            listViewResult_PCB.ColumnWidthChanging -= ListView_ColumnWidthChanging;

            // 엔터 키 조회
            textBox_LOT2.KeyDown          -= SearchTextBox_LotId_KeyDown;
            textBox_PCB2.KeyDown          -= SearchTextBox_LotId_KeyDown;
            textBox_MGZ2.KeyDown          -= SearchTextBox_LotId_KeyDown;
            textBox_PCB_MapArray.KeyDown  -= SearchTextBox_MapArray_KeyDown;
            textBox_LOT.KeyDown           -= SearchTextBox_PCB_KeyDown;
            textBox_PCB.KeyDown           -= SearchTextBox_PCB_KeyDown;
            textBox_MGZ.KeyDown           -= SearchTextBox_PCB_KeyDown;

            // ListView 복사/드래그
            listViewResult_LotId.KeyDown  -= ListView_CopyKeyDown;
            listViewResult_LotId.MouseDown -= ListView_MouseDown;
            listViewResult_LotId.ItemDrag  -= ListView_ItemDrag;
            listViewResult_MapArray.KeyDown  -= ListView_CopyKeyDown;
            listViewResult_MapArray.MouseDown -= ListView_MouseDown;
            listViewResult_MapArray.ItemDrag  -= ListView_ItemDrag;
            listViewResult_MapArray_BinCode.KeyDown  -= ListView_CopyKeyDown;
            listViewResult_MapArray_BinCode.MouseDown -= ListView_MouseDown;
            listViewResult_MapArray_BinCode.ItemDrag  -= ListView_ItemDrag;
            listViewResult_PCB.KeyDown  -= ListView_CopyKeyDown;
            listViewResult_PCB.MouseDown -= ListView_MouseDown;
            listViewResult_PCB.ItemDrag  -= ListView_ItemDrag;

            // 탭 컨트롤
            tabControl_Strip.SelectedIndexChanged -= TabControl_SelectedIndexChanged;

            // DataTable 리소스 정리
            originalData?.Dispose();
            lotIdData?.Dispose();
            mapArrayData?.Dispose();
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
        private async void BtnSearch2_Click(object sender, EventArgs e)
        {
            try
            {
                btnSearch_LotId.Enabled = false;
                string lotNo = textBox_LOT2.Text.Trim();
                string stripNo = textBox_PCB2.Text.Trim();
                string mgzRf = textBox_MGZ2.Text.Trim();

                // 검색 조건 체크 제거 - 빈 값이어도 전체 조회
                await LoadLotIdDataAsync(lotNo, stripNo, mgzRf);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"조회 중 오류 발생: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnSearch_LotId.Enabled = true;
            }
        }

        private async Task LoadLotIdDataAsync(string lotNo, string stripNo, string mgzRf)
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

                string query = queryBuilder.ToString();
                SqlParameter[] paramArray = parameters.ToArray();
                DataTable dt = await Task.Run(() => DatabaseHelper.ExecuteQuery(query, paramArray));
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
        private async void BtnSave_Click(object sender, EventArgs e)
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
                    btnUpdate_LotId.Enabled = false;
                    await SaveLotIdChangesAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"저장 중 오류 발생: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnUpdate_LotId.Enabled = true;
            }
        }

        /// <summary>
        /// Lot ID 변경사항 DB 저장 — SP 'L' (usp_StripMap_Process)
        /// </summary>
        private async Task SaveLotIdChangesAsync()
        {
            string workerIp = GetLocalIPAddress();
            // 현재 검색 조건 캡처 (재검색용)
            string searchLotNo = textBox_LOT2.Text.Trim();
            string searchStripNo = textBox_PCB2.Text.Trim();
            string searchMgzRf = textBox_MGZ2.Text.Trim();

            try
            {
                var result = await Task.Run(() =>
                {
                    int successCount = 0;
                    int failCount = 0;
                    StringBuilder errorLog = new StringBuilder();

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
                            SendMesRvMessage(stripNo, "L", ActionTypes.LOT_UPDATE);
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

                AppLogger.Info($"[{ActionTypes.LOT_UPDATE}_RESULT] user={currentUserId} | 성공={result.successCount} 실패={result.failCount}");
                string resultMessage = $"성공: {result.successCount}건\n실패: {result.failCount}건";
                if (!string.IsNullOrEmpty(result.errorLog))
                    resultMessage += $"\n\n오류 내역:\n{result.errorLog}";

                MessageBox.Show(resultMessage, "저장 결과",
                    MessageBoxButtons.OK,
                    result.failCount > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);

                if (result.successCount > 0)
                {
                    modifiedLotIds.Clear();
                    await LoadLotIdDataAsync(searchLotNo, searchStripNo, searchMgzRf);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"저장 처리 중 오류: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            catch (Exception ex)
            {
                AppLogger.Info($"[WARN] 로컬 IP 주소 조회 실패: {ex.Message}");
                return "127.0.0.1";
            }
        }

        /// <summary>
        /// 입력 대화상자 표시
        /// </summary>
        private string ShowInputDialog(string message, string title)
        {
            using (Form inputForm = new Form
            {
                Width = 400,
                Height = 150,
                Text = title,
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            })
            {
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

                bool ok = inputForm.ShowDialog() == DialogResult.OK;
                string result = ok ? textBox.Text : string.Empty;
                return result;
            }
        }

        #endregion

        #region MapArray 변경 탭

        private DataTable mapArrayData; // MapArray 탭 데이터
        private Dictionary<string, MapArrayModification> modifiedMapArrays; // 수정된 MapArray 저장

        // 그리드 시각화용 현재 선택 데이터
        private string _currentMapArray = "";
        private string _currentBinCode   = "";
        private bool   _isPreviewMode    = false;
        private int    _currentColCnt   = 0;
        private int    _currentRowCnt   = 0;

        // MapArray 수정 데이터 구조
        private class MapArrayModification
        {
            public string MapArray { get; set; }
            public string BinCode { get; set; }
        }

        /// <summary>
        /// 조회 버튼 클릭 (MapArray 변경 탭)
        /// </summary>
        private async void BtnSearchMapArray_Click(object sender, EventArgs e)
        {
            try
            {
                btnSearch_MapArray.Enabled = false;
                string stripNo = textBox_PCB_MapArray.Text.Trim();

                // 검색 조건 체크 복원 - MapArray 탭만 필수
                if (string.IsNullOrEmpty(stripNo))
                {
                    MessageBox.Show("PCB 2D ID를 입력해주세요.", "알림",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                await LoadMapArrayDataAsync(stripNo);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"조회 중 오류 발생: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnSearch_MapArray.Enabled = true;
            }
        }

        private async Task LoadMapArrayDataAsync(string stripNo)
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
                bincode,
                colCnt,
                rowCnt
            FROM dbo.[tblStripMap]
            WHERE active = 1
                AND stripNo LIKE @StripNo
            ORDER BY createdTime DESC");

                string query = queryBuilder.ToString();
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@StripNo", $"%{stripNo}%")
                };

                DataTable dt = await Task.Run(() => DatabaseHelper.ExecuteQuery(query, parameters));
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

        /// <summary>
        /// MapArray 문자열을 2D 그리드 텍스트로 변환하여 richTextBoxGrid에 출력한다.
        /// 좌표 규칙: 오른쪽→왼쪽 채움 (pos=1 → Col=colCnt)
        /// </summary>
        private void DrawGrid(string mapArray, int colCnt, int rowCnt, bool flipV, bool flipH)
        {
            if (string.IsNullOrEmpty(mapArray) || colCnt <= 0)
            {
                richTextBoxGrid.Text = "데이터 없음";
                return;
            }

            int len = mapArray.Length;
            if (rowCnt <= 0) rowCnt = len / colCnt;
            if (rowCnt <= 0)
            {
                richTextBoxGrid.Text = "데이터 없음";
                return;
            }

            char[,] grid = new char[rowCnt + 1, colCnt + 1];

            // 초기화
            for (int r = 1; r <= rowCnt; r++)
                for (int c = 1; c <= colCnt; c++)
                    grid[r, c] = '0';

            // 좌표 변환 (자리수 기준, 오른쪽→왼쪽)
            for (int pos = 1; pos <= len && pos <= rowCnt * colCnt; pos++)
            {
                int r = ((pos - 1) / colCnt) + 1;
                int c = colCnt - ((pos - 1) % colCnt);

                if (flipV) r = rowCnt - r + 1;
                if (flipH) c = colCnt - c + 1;

                if (r >= 1 && r <= rowCnt && c >= 1 && c <= colCnt)
                    grid[r, c] = mapArray[pos - 1];
            }

            // 텍스트 생성
            var sb = new System.Text.StringBuilder();

            // 헤더 행
            sb.Append("      ");
            for (int c = 1; c <= colCnt; c++)
                sb.Append($" C{c:00}");
            sb.AppendLine();

            // 데이터 행
            for (int r = 1; r <= rowCnt; r++)
            {
                sb.Append($"R{r:00}   ");
                for (int c = 1; c <= colCnt; c++)
                {
                    char val = grid[r, c];
                    sb.Append(val == '2' ? "  ■ " : "  · ");
                }
                sb.AppendLine();
            }

            richTextBoxGrid.Text = sb.ToString();
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

        private void ListViewResultMapArrayBinCode_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var hit = listViewResult_MapArray_BinCode.HitTest(e.Location);
            if (hit?.Item == null) return;

            int index = hit.Item.Index;
            if (index < 0 || index >= listViewResult_MapArray.Items.Count) return;

            listViewResult_MapArray.Items[index].Checked = !listViewResult_MapArray.Items[index].Checked;
        }

        private void ListViewResultMapArray_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_syncingSelection) return;
            _syncingSelection = true;
            try { SyncListViewSelection(listViewResult_MapArray, listViewResult_MapArray_BinCode); }
            finally { _syncingSelection = false; }

            // 선택 행 그리드 렌더링
            if (listViewResult_MapArray.SelectedItems.Count == 0) return;
            var row = listViewResult_MapArray.SelectedItems[0].Tag as System.Data.DataRow;
            if (row == null) return;

            _currentMapArray = row["mapArray"]?.ToString() ?? "";
            SetGridMode(false);
            _currentBinCode  = row["bincode"]?.ToString() ?? "";
            _currentColCnt   = row.Table.Columns.Contains("colCnt") && row["colCnt"] != DBNull.Value
                               ? Convert.ToInt32(row["colCnt"]) : 0;
            _currentRowCnt   = row.Table.Columns.Contains("rowCnt") && row["rowCnt"] != DBNull.Value
                               ? Convert.ToInt32(row["rowCnt"]) : 0;

            DrawGrid(_currentMapArray, _currentColCnt, _currentRowCnt,
                     checkBoxVFlip.Checked, checkBoxHFlip.Checked);
        }

        private void CheckBoxFlip_CheckedChanged(object sender, EventArgs e)
        {
            string mapArray = _isPreviewMode ? textBoxMapArray.Text.Trim() : _currentMapArray;
            DrawGrid(mapArray, _currentColCnt, _currentRowCnt,
                     checkBoxVFlip.Checked, checkBoxHFlip.Checked);
        }

        /// <summary>
        /// newMapArray의 각 위치를 기준으로 binCode를 자동 계산한다.
        /// '0' → '1', '2' → 'D', 그 외 → origBinCode[i] 유지
        /// </summary>
        private string ComputeBinCode(string newMapArray, string origBinCode)
        {
            if (string.IsNullOrEmpty(newMapArray) || string.IsNullOrEmpty(origBinCode)
                || newMapArray.Length != origBinCode.Length)
                return origBinCode;

            var sb = new StringBuilder(origBinCode);
            for (int i = 0; i < newMapArray.Length; i++)
            {
                if      (newMapArray[i] == '0') sb[i] = '1';
                else if (newMapArray[i] == '2') sb[i] = 'D';
                // 그 외: origBinCode[i] 그대로 유지
            }
            return sb.ToString();
        }

        private void SetGridMode(bool previewMode)
        {
            _isPreviewMode = previewMode;
            btnGridOriginal.BackColor = previewMode ? SystemColors.Control     : Color.CornflowerBlue;
            btnGridOriginal.ForeColor = previewMode ? SystemColors.ControlText : Color.White;
            btnGridPreview.BackColor  = previewMode ? Color.CornflowerBlue     : SystemColors.Control;
            btnGridPreview.ForeColor  = previewMode ? Color.White              : SystemColors.ControlText;
        }

        private void RefreshPreviewGrid()
        {
            if (_currentColCnt <= 0)
            {
                MessageBox.Show("먼저 조회 결과에서 항목을 선택해 주세요.", "알림",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            string previewMapArray = textBoxMapArray.Text.Trim();
            SetGridMode(true);
            DrawGrid(previewMapArray, _currentColCnt, _currentRowCnt,
                     checkBoxVFlip.Checked, checkBoxHFlip.Checked);
        }

        private void BtnGridOriginal_Click(object sender, EventArgs e)
        {
            SetGridMode(false);
            DrawGrid(_currentMapArray, _currentColCnt, _currentRowCnt,
                     checkBoxVFlip.Checked, checkBoxHFlip.Checked);
        }

        private void BtnGridPreview_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBoxMapArray.Text.Trim())) return;
            RefreshPreviewGrid();
        }

        private void BtnRefreshGrid_Click(object sender, EventArgs e)
        {
            RefreshPreviewGrid();
        }

        private void TextBoxMapArray_TextChanged(object sender, EventArgs e)
        {
            bool hasValue = !string.IsNullOrEmpty(textBoxMapArray.Text.Trim());
            btnRefreshGrid.Enabled = hasValue;
            btnGridPreview.Enabled = hasValue;

            // binCode 자동 계산
            if (hasValue && !string.IsNullOrEmpty(_currentBinCode))
                textBoxBinCode.Text = ComputeBinCode(textBoxMapArray.Text.Trim(), _currentBinCode);
            else if (!hasValue)
                textBoxBinCode.Text = _currentBinCode;  // mapArray 지우면 원본 복원

            if (!hasValue && _isPreviewMode)
            {
                SetGridMode(false);
                DrawGrid(_currentMapArray, _currentColCnt, _currentRowCnt,
                         checkBoxVFlip.Checked, checkBoxHFlip.Checked);
            }
        }

        private void ListViewResultMapArrayBinCode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_syncingSelection) return;
            _syncingSelection = true;
            try { SyncListViewSelection(listViewResult_MapArray_BinCode, listViewResult_MapArray); }
            finally { _syncingSelection = false; }
        }

        private void SyncListViewSelection(ListView source, ListView target)
        {
            target.BeginUpdate();
            foreach (ListViewItem item in target.Items)
                item.Selected = false;
            foreach (ListViewItem item in source.SelectedItems)
            {
                if (item.Index < target.Items.Count)
                    target.Items[item.Index].Selected = true;
            }
            target.EndUpdate();
        }

        /// <summary>
        /// 삭제 버튼 클릭
        /// </summary>
        private async void BtnDeleteMapArray_Click(object sender, EventArgs e)
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

                btnDelete_MapArray.Enabled = false;
                await DeleteMapArrayDataAsync(checkedItems, deleteComment);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"삭제 중 오류 발생: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnDelete_MapArray.Enabled = true;
            }
        }

        /// <summary>
        /// MapArray 삭제 — SP 'D' (usp_StripMap_Process)
        /// SP 동작: history 기록 + tblStripMap active=0 설정 (논리 삭제)
        /// 이후 물리 삭제(Purge)는 SP 'P'로 별도 처리
        /// </summary>
        private async Task DeleteMapArrayDataAsync(List<ListViewItem> checkedItems, string comment)
        {
            string workerIp = GetLocalIPAddress();
            string searchStripNo = textBox_PCB_MapArray.Text.Trim();

            try
            {
                var result = await Task.Run(() =>
                {
                    int successCount = 0;
                    int failCount = 0;
                    StringBuilder errorLog = new StringBuilder();

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
                            SendMesRvMessage(stripNo, "D", ActionTypes.STRIP_DELETE);
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

                AppLogger.Info($"[{ActionTypes.STRIP_DELETE}_RESULT] user={currentUserId} | 성공={result.successCount} 실패={result.failCount}");
                string resultMessage = $"성공: {result.successCount}건\n실패: {result.failCount}건";
                if (!string.IsNullOrEmpty(result.errorLog))
                    resultMessage += $"\n\n오류 내역:\n{result.errorLog}";

                MessageBox.Show(resultMessage, "삭제 결과",
                    MessageBoxButtons.OK,
                    result.failCount > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);

                if (result.successCount > 0)
                    await LoadMapArrayDataAsync(searchStripNo);
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
        private async void BtnUpdateMapArray_Click(object sender, EventArgs e)
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
                    btnUpdate_MapArray.Enabled = false;
                    int colCnt = 0;
                    if (checkedItems.Count > 0)
                    {
                        var firstRow = checkedItems[0].Tag as DataRow;
                        if (firstRow != null && firstRow.Table.Columns.Contains("colCnt")
                            && firstRow["colCnt"] != DBNull.Value)
                            colCnt = Convert.ToInt32(firstRow["colCnt"]);
                    }
                    await UpdateMapArrayDataAsync(checkedItems, newMapArray, newBinCode, colCnt);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"수정 중 오류 발생: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnUpdate_MapArray.Enabled = true;
            }
        }

        /// <summary>
        /// MapArray 수정 — SP 'U' (usp_StripMap_Process)
        /// SP의 COALESCE(@mapArray, mapArray) 처리: 빈 값이면 NULL 전달 → 기존 값 유지
        /// </summary>
        private async Task UpdateMapArrayDataAsync(List<ListViewItem> checkedItems, string newMapArray, string newBinCode, int colCnt)
        {
            string workerIp = GetLocalIPAddress();
            string searchStripNo = textBox_PCB_MapArray.Text.Trim();

            try
            {
                var result = await Task.Run(() =>
                {
                    int successCount = 0;
                    int failCount = 0;
                    StringBuilder errorLog = new StringBuilder();

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

                            // 변경 좌표 계산
                            string origMapArray = row["mapArray"]?.ToString() ?? "";
                            (List<int> xList, List<int> yList) changedCoords = (new List<int>(), new List<int>());
                            if (!string.IsNullOrEmpty(newMapArray) && colCnt > 0)
                                changedCoords = CalcChangedCoords(origMapArray, newMapArray, colCnt);

                            string xposList = changedCoords.xList.Count > 0
                                ? string.Join(",", changedCoords.xList) : null;
                            string yposList = changedCoords.yList.Count > 0
                                ? string.Join(",", changedCoords.yList) : null;

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
                                new SqlParameter("@workerIp",      workerIp),
                                new SqlParameter("@changedXpos",   (object)xposList ?? DBNull.Value),
                                new SqlParameter("@changedYpos",   (object)yposList ?? DBNull.Value)
                            });

                            AppLogger.Info($"[{ActionTypes.STRIP_UPDATE}] user={currentUserId} | stripNo={stripNo} | mapArray={newMapArray} binCode={newBinCode}");
                            for (int i = 0; i < changedCoords.xList.Count; i++)
                                SendMesRvMessage(stripNo, "U", ActionTypes.STRIP_UPDATE,
                                                 changedCoords.xList[i], changedCoords.yList[i]);
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

                AppLogger.Info($"[{ActionTypes.STRIP_UPDATE}_RESULT] user={currentUserId} | 성공={result.successCount} 실패={result.failCount}");
                string resultMessage = $"성공: {result.successCount}건\n실패: {result.failCount}건";
                if (!string.IsNullOrEmpty(result.errorLog))
                    resultMessage += $"\n\n오류 내역:\n{result.errorLog}";

                MessageBox.Show(resultMessage, "수정 결과",
                    MessageBoxButtons.OK,
                    result.failCount > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);

                if (result.successCount > 0)
                    await LoadMapArrayDataAsync(searchStripNo);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"수정 처리 중 오류: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 구 mapArray와 신 mapArray를 비교하여 변경된 셀의 Gold Gate 좌표 목록을 반환한다.
        /// Gold Gate X = colCnt - ((pos-1) % colCnt)  [16→1, 좌→우]
        /// Gold Gate Y = ((pos-1) / colCnt) + 1         [1→rowCnt, 상→하]
        /// Flip 미적용 — 원본 문자열 위치 기준.
        /// </summary>
        private (List<int> xList, List<int> yList) CalcChangedCoords(
            string oldMap, string newMap, int colCnt)
        {
            var xList = new List<int>();
            var yList = new List<int>();

            if (string.IsNullOrEmpty(oldMap) || string.IsNullOrEmpty(newMap) || colCnt <= 0)
                return (xList, yList);

            int len = Math.Min(oldMap.Length, newMap.Length);
            for (int pos = 1; pos <= len; pos++)
            {
                if (oldMap[pos - 1] != newMap[pos - 1])
                {
                    xList.Add(colCnt - ((pos - 1) % colCnt));
                    yList.Add(((pos - 1) / colCnt) + 1);
                }
            }

            return (xList, yList);
        }

        #endregion

        #region PCB 2D ID 원복 탭

        /// <summary>
        /// offset에 따른 조회 기간(시작일, 종료일)을 반환한다.
        /// offset=0: 오늘 기준 한 달, offset=1: 1개월 전 한 달, ...
        /// </summary>
        private (DateTime start, DateTime end) GetPeriodRange(int offset)
        {
            DateTime end   = DateTime.Today.AddMonths(-offset);
            DateTime start = DateTime.Today.AddMonths(-offset - 1).AddDays(1);
            return (start, end);
        }

        /// <summary>
        /// 조회 버튼 클릭 (PCB 원복 탭) - tblStripMapHistory 조회
        /// </summary>
        private async void BtnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                btnSearch_PCB.Enabled = false;
                string lotNo = textBox_LOT.Text.Trim();
                string stripNo = textBox_PCB.Text.Trim();
                string mgzRf = textBox_MGZ.Text.Trim();

                // 검색 조건 체크 제거 - 빈 값이어도 전체 조회
                _periodOffset = 0;
                var (start, end) = GetPeriodRange(_periodOffset);
                await LoadHistoryDataAsync(lotNo, stripNo, mgzRf, start, end);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"조회 중 오류 발생: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnSearch_PCB.Enabled = true;
            }
        }

        private async Task LoadHistoryDataAsync(string lotNo, string stripNo, string mgzRf,
                                     DateTime startDate, DateTime endDate)
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

                // 기간 필터 (timekey 앞 8자리 = yyyyMMdd)
                queryBuilder.Append(" AND LEFT(timekey, 8) >= @StartDate");
                queryBuilder.Append(" AND LEFT(timekey, 8) <= @EndDate");
                parameters.Add(new SqlParameter("@StartDate", startDate.ToString("yyyyMMdd")));
                parameters.Add(new SqlParameter("@EndDate",   endDate.ToString("yyyyMMdd")));

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

                string query = queryBuilder.ToString();
                SqlParameter[] paramArray = parameters.ToArray();
                DataTable dt = await Task.Run(() => DatabaseHelper.ExecuteQuery(query, paramArray));
                originalData = dt.Copy();
                DisplayHistoryData(dt);

                labelResultTitle.Text = $"조회 결과 ({dt.Rows.Count}건)";
                // 기간 라벨 갱신
                labelPeriod.Text = $"{startDate:yyyy-MM-dd} ~ {endDate:yyyy-MM-dd}";
                // 다음 버튼: offset=0이면 현재 기간이므로 비활성
                btnNextPeriod.Enabled = (_periodOffset > 0);

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

        private async void BtnRestore_Click(object sender, EventArgs e)
        {
            // ① 권한 체크
            if (!HasPermission(UserPermissions.STRIP_ROLLBACK))
            {
                MessageBox.Show("원복 권한이 없습니다.", "권한 없음",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var checkedItems = GetCheckedPcbItems();

                if (checkedItems.Count == 0)
                {
                    MessageBox.Show("원복할 항목을 선택해주세요.", "알림",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (!ValidateNoPurgeItems(checkedItems)) return;
                if (!ValidateNoDuplicateStripNos(checkedItems)) return;
                if (!await CheckPostChangesWarningAsync(checkedItems)) return;

                DialogResult result2 = MessageBox.Show(
                    $"{checkedItems.Count}건의 데이터를 원복하시겠습니까?\n\n선택한 버전으로 데이터를 되돌립니다.",
                    "원복 확인",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result2 == DialogResult.Yes)
                {
                    btnRestore_PCB.Enabled = false;
                    await RestoreFromHistoryAsync(checkedItems);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"원복 중 오류 발생: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnRestore_PCB.Enabled = true;
            }
        }

        /// <summary>
        /// listViewResult_PCB에서 체크된 항목을 반환한다.
        /// BtnRestore_Click 및 BtnPurgeRollback_Click에서 공통 사용.
        /// </summary>
        private List<ListViewItem> GetCheckedPcbItems()
        {
            var items = new List<ListViewItem>();
            foreach (ListViewItem item in listViewResult_PCB.Items)
            {
                if (item.Checked)
                    items.Add(item);
            }
            return items;
        }

        /// <summary>
        /// 체크된 항목 중 STRIP_PURGE 이력 혼재 여부를 검증한다.
        /// </summary>
        /// <returns>검증 통과 시 true, 실패(MessageBox 표시) 시 false</returns>
        private bool ValidateNoPurgeItems(List<ListViewItem> checkedItems)
        {
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
                return false;
            }
            return true;
        }

        /// <summary>
        /// 체크된 항목 중 동일 stripNo 중복 선택 여부를 검증한다.
        /// </summary>
        /// <returns>검증 통과 시 true, 실패(MessageBox 표시) 시 false</returns>
        private bool ValidateNoDuplicateStripNos(List<ListViewItem> checkedItems)
        {
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
                return false;
            }
            return true;
        }

        /// <summary>
        /// 체크된 항목에 대해 선택 이력 이후 수정 이력이 존재하는지 비동기로 확인하고 경고한다.
        /// </summary>
        /// <returns>계속 진행해도 되면 true, 취소하면 false</returns>
        private async Task<bool> CheckPostChangesWarningAsync(List<ListViewItem> checkedItems)
        {
            var checkItems = checkedItems.Select(item =>
            {
                DataRow warnRow = item.Tag as DataRow;
                if (warnRow == null) return (sNo: "", tk: "");
                return (sNo: warnRow["stripNo"]?.ToString() ?? "", tk: warnRow["timekey"]?.ToString() ?? "");
            }).Where(x => !string.IsNullOrEmpty(x.sNo) && !string.IsNullOrEmpty(x.tk)).ToList();

            var stripsWithPostChanges = await Task.Run(() =>
            {
                var result = new List<string>();
                foreach (var (sNo, tk) in checkItems)
                {
                    DataTable dtChk = DatabaseHelper.ExecuteQuery(
                        "SELECT TOP 1 1 AS chk FROM dbo.tblStripMapHistory WHERE stripNo = @sn AND timekey > @tk",
                        new SqlParameter[] {
                            new SqlParameter("@sn", sNo),
                            new SqlParameter("@tk", tk)
                        });
                    if (dtChk.Rows.Count > 0)
                        result.Add(sNo);
                }
                return result;
            });

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
                if (warnResult != DialogResult.Yes) return false;
            }
            return true;
        }

        /// <summary>
        /// 체크된 항목이 모두 STRIP_PURGE 이력인지 검증한다.
        /// PURGE 이력이 아닌 항목이 하나라도 있으면 오류 메시지를 표시하고 false를 반환한다.
        /// </summary>
        /// <returns>검증 통과(PURGE 이력만 선택됨) 시 true, 실패(MessageBox) 시 false</returns>
        private bool ValidateOnlyPurgeItems(List<ListViewItem> checkedItems)
        {
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
                return false;
            }
            return true;
        }

        /// <summary>
        /// 체크된 항목 중 동일 stripNo에서 active=1 이력이 여러 건 선택되었는지 검증한다.
        /// active=0은 복수 선택 허용, active=1은 Strip 당 1건만 허용.
        /// </summary>
        /// <returns>검증 통과 시 true, 중복 선택 시 false</returns>
        private bool ValidateNoDuplicateActivePurgeStripNos(List<ListViewItem> checkedItems)
        {
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
                return false;
            }
            return true;
        }

        /// <summary>
        /// 체크된 PURGE 이력 이후 수정 이력이 존재하는지 비동기로 확인하고 경고한다.
        /// </summary>
        /// <returns>계속 진행 시 true, 취소 시 false</returns>
        private async Task<bool> CheckPurgePostChangesWarningAsync(List<ListViewItem> checkedItems)
        {
            var checkItems = checkedItems.Select(item =>
            {
                DataRow warnRow = item.Tag as DataRow;
                if (warnRow == null) return (sNo: "", tk: "");
                return (sNo: warnRow["stripNo"]?.ToString() ?? "", tk: warnRow["timekey"]?.ToString() ?? "");
            }).Where(x => !string.IsNullOrEmpty(x.sNo) && !string.IsNullOrEmpty(x.tk)).ToList();

            var stripsWithPostChanges = await Task.Run(() =>
            {
                var resultList = new List<string>();
                foreach (var (sNo, tk) in checkItems)
                {
                    DataTable dtChk = DatabaseHelper.ExecuteQuery(
                        "SELECT TOP 1 1 AS chk FROM dbo.tblStripMapHistory WHERE stripNo = @sn AND timekey > @tk AND actionType <> 'STRIP_PURGE'",
                        new SqlParameter[] {
                            new SqlParameter("@sn", sNo),
                            new SqlParameter("@tk", tk)
                        });
                    if (dtChk.Rows.Count > 0)
                        resultList.Add(sNo);
                }
                return resultList;
            });

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
                if (warnResult != DialogResult.Yes) return false;
            }
            return true;
        }

        /// <summary>
        /// Purge 복원 버튼 클릭 — PURGE 이력 레코드를 tblStripMap으로 복원 (SP 'Q')
        /// 대상: actionType = 'PURGE' 인 이력 레코드만 선택 가능
        /// 권한: STRIP_PURGE_ROLLBACK (Admin 이상)
        /// </summary>
        private async void BtnPurgeRollback_Click(object sender, EventArgs e)
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
                var checkedItems = GetCheckedPcbItems();

                if (checkedItems.Count == 0)
                {
                    MessageBox.Show("복원할 이력을 선택해주세요.", "알림",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (!ValidateOnlyPurgeItems(checkedItems)) return;
                if (!ValidateNoDuplicateActivePurgeStripNos(checkedItems)) return;
                if (!await CheckPurgePostChangesWarningAsync(checkedItems)) return;

                DialogResult result = MessageBox.Show(
                    $"⚠️ Purge 복원 확인 ⚠️\n\n" +
                    $"{checkedItems.Count}건의 PURGE 이력을 복원하시겠습니까?\n\n" +
                    "복원 시 해당 데이터가 tblStripMap에 되살아납니다.",
                    "Purge 복원 확인",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    btnPurgeRollback_PCB.Enabled = false;
                    await PurgeRollbackDataAsync(checkedItems);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Purge 복원 중 오류 발생: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnPurgeRollback_PCB.Enabled = true;
            }
        }

        /// <summary>
        /// PURGE 이력 복원 — SP 'Q' (usp_StripMap_Process)
        /// </summary>
        private async Task PurgeRollbackDataAsync(List<ListViewItem> checkedItems)
        {
            string workerIp = GetLocalIPAddress();
            string searchLotNo = textBox_LOT.Text.Trim();
            string searchStripNo = textBox_PCB.Text.Trim();
            string searchMgzRf = textBox_MGZ.Text.Trim();

            try
            {
                var result = await Task.Run(() =>
                {
                    int successCount = 0;
                    int failCount = 0;
                    StringBuilder errorLog = new StringBuilder();

                    foreach (ListViewItem item in checkedItems)
                    {
                        DataRow row = item.Tag as DataRow;
                        if (row == null) { failCount++; continue; }

                        string stripNo = string.Empty;
                        try
                        {
                            stripNo  = row["stripNo"]?.ToString();
                            string process       = row["process"]?.ToString();
                            string targetTimekey = row["timekey"]?.ToString();

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

                AppLogger.Info($"[{ActionTypes.STRIP_PURGE_ROLLBACK}_RESULT] user={currentUserId} | 성공={result.successCount} 실패={result.failCount}");
                string resultMessage = $"성공: {result.successCount}건\n실패: {result.failCount}건";
                if (!string.IsNullOrEmpty(result.errorLog))
                    resultMessage += $"\n\n오류 내역:\n{result.errorLog}";

                MessageBox.Show(resultMessage, "Purge 복원 결과",
                    MessageBoxButtons.OK,
                    result.failCount > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);

                if (result.successCount > 0)
                {
                    var (start, end) = GetPeriodRange(_periodOffset);
                    await LoadHistoryDataAsync(searchLotNo, searchStripNo, searchMgzRf, start, end);
                }
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
        private async Task RestoreFromHistoryAsync(List<ListViewItem> checkedItems)
        {
            string workerIp = GetLocalIPAddress();
            bool hasRollbackPerm = HasPermission(UserPermissions.STRIP_ROLLBACK);
            string searchLotNo = textBox_LOT.Text.Trim();
            string searchStripNo = textBox_PCB.Text.Trim();
            string searchMgzRf = textBox_MGZ.Text.Trim();

            try
            {
                var result = await Task.Run(() =>
                {
                    int successCount = 0;
                    int failCount = 0;
                    StringBuilder errorLog = new StringBuilder();

                    foreach (ListViewItem item in checkedItems)
                    {
                        string stripNo = string.Empty;
                        DataRow row = item.Tag as DataRow;
                        if (row == null) { failCount++; continue; }

                        try
                        {
                            stripNo = row["stripNo"]?.ToString();
                            string process = row["process"]?.ToString();
                            string targetTimekey = row["timekey"]?.ToString();
                            string actionType = row["actionType"] != DBNull.Value ? row["actionType"].ToString() : string.Empty;

                            if (actionType == ActionTypes.STRIP_PURGE)
                            {
                                failCount++;
                                errorLog.AppendLine($"stripNo: {stripNo} - STRIP_PURGE 이력은 'Purge복원' 버튼을 사용하세요.");
                                continue;
                            }

                            if (!hasRollbackPerm)
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
                            SendMesRvMessage(stripNo, "R", ActionTypes.STRIP_ROLLBACK);
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

                AppLogger.Info($"[{ActionTypes.STRIP_ROLLBACK}_RESULT] user={currentUserId} | 성공={result.successCount} 실패={result.failCount}");
                string resultMessage = $"성공: {result.successCount}건\n실패: {result.failCount}건";
                if (!string.IsNullOrEmpty(result.errorLog))
                    resultMessage += $"\n\n오류 내역:\n{result.errorLog}";

                MessageBox.Show(resultMessage, "원복 결과",
                    MessageBoxButtons.OK,
                    result.failCount > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);

                if (result.successCount > 0)
                {
                    var (start, end) = GetPeriodRange(_periodOffset);
                    await LoadHistoryDataAsync(searchLotNo, searchStripNo, searchMgzRf, start, end);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"원복 처리 중 오류: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnPrevPeriod_Click(object sender, EventArgs e)
        {
            _periodOffset++;
            var (start, end) = GetPeriodRange(_periodOffset);
            string lotNo   = textBox_LOT.Text.Trim();
            string stripNo = textBox_PCB.Text.Trim();
            string mgzRf   = textBox_MGZ.Text.Trim();
            await LoadHistoryDataAsync(lotNo, stripNo, mgzRf, start, end);
        }

        private async void BtnNextPeriod_Click(object sender, EventArgs e)
        {
            if (_periodOffset <= 0) return;
            _periodOffset--;
            var (start, end) = GetPeriodRange(_periodOffset);
            string lotNo   = textBox_LOT.Text.Trim();
            string stripNo = textBox_PCB.Text.Trim();
            string mgzRf   = textBox_MGZ.Text.Trim();
            await LoadHistoryDataAsync(lotNo, stripNo, mgzRf, start, end);
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

        // ─────────────────────────────────────────────
        // MES RV 메시지 송신
        // ─────────────────────────────────────────────

        /// <summary>
        /// MES 전송용 XML 메시지를 생성합니다.
        /// </summary>
        private string BuildMesRvXml(string frameId, string actionType, string functionId, int xpos = 0, int ypos = 0)
        {
            return
                "<message>" +
                  "<header>" +
                    $"<messagename>{functionId}</messagename>" +
                  "</header>" +
                  "<body>" +
                    $"<FRAME_ID>{frameId}</FRAME_ID>" +
                    $"<ACTIONTYPE>{actionType}</ACTIONTYPE>" +
                    $"<FRAME_LOC_XPOS>{xpos}</FRAME_LOC_XPOS>" +
                    $"<FRAME_LOC_YPOS>{ypos}</FRAME_LOC_YPOS>" +
                  "</body>" +
                "</message>";
        }

        /// <summary>
        /// MES RV 메시지를 전송합니다. RV 미연결 시 로그만 남기고 무시합니다.
        /// </summary>
        private void SendMesRvMessage(string frameId, string actionType, string functionId, int xpos = 0, int ypos = 0)
        {
            if (Rv == null || !Rv.IsConnected) return;
            try
            {
                Rv.RvSend(Rv.Subject, BuildMesRvXml(frameId, actionType, functionId, xpos, ypos));
            }
            catch (Exception ex)
            {
                AppLogger.Info($"[RV_SEND_FAIL] frameId={frameId} actionType={actionType} xpos={xpos} ypos={ypos} | {ex.Message}");
            }
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