# MapArray 2D 그리드 시각화 구현 계획

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** MapArray 탭 우측 공간에 Flip 체크박스와 2D 그리드를 추가하여, ListView 행 선택 시 mapArray를 시각적으로 확인할 수 있게 한다.

**Architecture:** Designer.cs에 패널/컨트롤을 추가하고, MainForm.cs에 DrawGrid 메서드와 이벤트 핸들러를 연결한다. DB 쿼리에 colCnt/rowCnt를 추가해 가변 그리드를 지원한다.

**Tech Stack:** WinForms (.NET Framework 4.7.2), C#, System.Data.SqlClient

---

### Task 1: DB 쿼리에 colCnt, rowCnt 추가

**Files:**
- Modify: `Forms/MainForm.cs` (LoadMapArrayData 메서드, ~line 774)

**Step 1: SELECT에 colCnt, rowCnt 추가**

`MainForm.cs`의 `LoadMapArrayData` 메서드 내 쿼리를 수정한다.

```csharp
// 기존
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

// 변경
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
```

**Step 2: 빌드 확인**

Visual Studio에서 빌드(Ctrl+Shift+B). 오류 없으면 통과.

**Step 3: Commit**

```bash
git add Forms/MainForm.cs
git commit -m "feat: MapArray 쿼리에 colCnt/rowCnt 추가"
```

---

### Task 2: 멤버 변수 추가

**Files:**
- Modify: `Forms/MainForm.cs` (~line 729, MapArray region 상단)

**Step 1: 그리드 상태 캐싱 변수 추가**

`mapArrayData` 선언부 아래에 추가:

```csharp
// 그리드 시각화용 현재 선택 데이터
private string _currentMapArray = "";
private int    _currentColCnt   = 0;
private int    _currentRowCnt   = 0;
```

**Step 2: Commit**

```bash
git add Forms/MainForm.cs
git commit -m "feat: MapArray 그리드 캐싱 멤버 변수 추가"
```

---

### Task 3: DrawGrid 메서드 추가

**Files:**
- Modify: `Forms/MainForm.cs` (MapArray region 내부, ~line 848 아래)

**Step 1: DrawGrid 메서드 추가**

`DisplayMapArrayData` 메서드 아래에 추가:

```csharp
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
```

**Step 2: 빌드 확인** — richTextBoxGrid는 아직 없으므로 오류 발생 예상. Task 4 완료 후 재확인.

---

### Task 4: Designer.cs에 컨트롤 추가

**Files:**
- Modify: `Forms/MainForm.Designer.cs`

**Step 1: 컨트롤 필드 선언 추가**

Designer.cs 하단 필드 선언부 (`// MapArray 변경 탭 컨트롤들` 블록, ~line 1050)에 추가:

```csharp
// MapArray 그리드 시각화 컨트롤
private System.Windows.Forms.Panel panelMapGrid;
private System.Windows.Forms.CheckBox checkBoxVFlip;
private System.Windows.Forms.CheckBox checkBoxHFlip;
private System.Windows.Forms.Button btnRefreshGrid;
private System.Windows.Forms.RichTextBox richTextBoxGrid;
```

**Step 2: InitializeComponent에 인스턴스 생성 추가**

`InitializeComponent` 메서드 상단의 `new` 선언부에 추가 (다른 MapArray 컨트롤 선언 근처):

```csharp
this.panelMapGrid    = new System.Windows.Forms.Panel();
this.checkBoxVFlip   = new System.Windows.Forms.CheckBox();
this.checkBoxHFlip   = new System.Windows.Forms.CheckBox();
this.btnRefreshGrid  = new System.Windows.Forms.Button();
this.richTextBoxGrid = new System.Windows.Forms.RichTextBox();
```

**Step 3: SuspendLayout 추가**

`this.panelInputMapArray.SuspendLayout();` 근처에 추가:

```csharp
this.panelMapGrid.SuspendLayout();
```

**Step 4: tabPageMapArray.Controls.Add 추가**

`tabPageMapArray.Controls.Add(this.panelSearchMapArray);` 줄 아래에 추가:

```csharp
this.tabPageMapArray.Controls.Add(this.panelMapGrid);
```

**Step 5: 각 컨트롤 속성 설정 추가**

`// panelSearchMapArray` 속성 블록 아래에 다음 블록 추가:

```csharp
//
// panelMapGrid
//
this.panelMapGrid.BackColor    = System.Drawing.Color.White;
this.panelMapGrid.BorderStyle  = System.Windows.Forms.BorderStyle.Fixed3D;
this.panelMapGrid.Location     = new System.Drawing.Point(545, 21);
this.panelMapGrid.Name         = "panelMapGrid";
this.panelMapGrid.Size         = new System.Drawing.Size(511, 175);
this.panelMapGrid.TabIndex     = 10;
this.panelMapGrid.Controls.Add(this.checkBoxVFlip);
this.panelMapGrid.Controls.Add(this.checkBoxHFlip);
this.panelMapGrid.Controls.Add(this.btnRefreshGrid);
this.panelMapGrid.Controls.Add(this.richTextBoxGrid);
//
// checkBoxVFlip
//
this.checkBoxVFlip.AutoSize  = true;
this.checkBoxVFlip.Checked   = true;
this.checkBoxVFlip.CheckState = System.Windows.Forms.CheckState.Checked;
this.checkBoxVFlip.Font      = new System.Drawing.Font("맑은 고딕", 9.75F);
this.checkBoxVFlip.Location  = new System.Drawing.Point(10, 8);
this.checkBoxVFlip.Name      = "checkBoxVFlip";
this.checkBoxVFlip.Text      = "Vertical Flip";
this.checkBoxVFlip.TabIndex  = 0;
//
// checkBoxHFlip
//
this.checkBoxHFlip.AutoSize  = true;
this.checkBoxHFlip.Checked   = true;
this.checkBoxHFlip.CheckState = System.Windows.Forms.CheckState.Checked;
this.checkBoxHFlip.Font      = new System.Drawing.Font("맑은 고딕", 9.75F);
this.checkBoxHFlip.Location  = new System.Drawing.Point(130, 8);
this.checkBoxHFlip.Name      = "checkBoxHFlip";
this.checkBoxHFlip.Text      = "Horizontal Flip";
this.checkBoxHFlip.TabIndex  = 1;
//
// btnRefreshGrid
//
this.btnRefreshGrid.Font     = new System.Drawing.Font("맑은 고딕", 9.75F);
this.btnRefreshGrid.Location = new System.Drawing.Point(390, 5);
this.btnRefreshGrid.Name     = "btnRefreshGrid";
this.btnRefreshGrid.Size     = new System.Drawing.Size(100, 28);
this.btnRefreshGrid.TabIndex = 2;
this.btnRefreshGrid.Text     = "새로고침";
//
// richTextBoxGrid
//
this.richTextBoxGrid.BackColor    = System.Drawing.Color.White;
this.richTextBoxGrid.BorderStyle  = System.Windows.Forms.BorderStyle.None;
this.richTextBoxGrid.Font         = new System.Drawing.Font("Courier New", 8.25F);
this.richTextBoxGrid.Location     = new System.Drawing.Point(5, 38);
this.richTextBoxGrid.Name         = "richTextBoxGrid";
this.richTextBoxGrid.ReadOnly     = true;
this.richTextBoxGrid.ScrollBars   = System.Windows.Forms.RichTextBoxScrollBars.Both;
this.richTextBoxGrid.Size         = new System.Drawing.Size(497, 128);
this.richTextBoxGrid.TabIndex     = 3;
this.richTextBoxGrid.Text         = "";
this.richTextBoxGrid.WordWrap     = false;
```

**Step 6: ResumeLayout 추가**

`this.panelSearchMapArray.ResumeLayout(false);` 근처에 추가:

```csharp
this.panelMapGrid.ResumeLayout(false);
this.panelMapGrid.PerformLayout();
```

**Step 7: 빌드 확인 (Ctrl+Shift+B)** — 오류 없으면 통과.

**Step 8: Commit**

```bash
git add Forms/MainForm.Designer.cs Forms/MainForm.cs
git commit -m "feat: MapArray 그리드 패널 컨트롤 추가 (Designer)"
```

---

### Task 5: 이벤트 핸들러 연결

**Files:**
- Modify: `Forms/MainForm.cs` (이벤트 등록부 ~line 224, SelectedIndexChanged �핸들러 ~line 923)

**Step 1: InitializeComponent 이후 이벤트 등록 추가**

`listViewResult_MapArray.SelectedIndexChanged += ...` 줄 근처에 추가:

```csharp
this.checkBoxVFlip.CheckedChanged  += CheckBoxFlip_CheckedChanged;
this.checkBoxHFlip.CheckedChanged  += CheckBoxFlip_CheckedChanged;
this.btnRefreshGrid.Click          += BtnRefreshGrid_Click;
```

**Step 2: ListViewResultMapArray_SelectedIndexChanged에 DrawGrid 호출 추가**

기존 핸들러 (line ~923):

```csharp
private void ListViewResultMapArray_SelectedIndexChanged(object sender, EventArgs e)
{
    if (_syncingSelection) return;
    _syncingSelection = true;
    try { SyncListViewSelection(listViewResult_MapArray, listViewResult_MapArray_BinCode); }
    finally { _syncingSelection = false; }
}
```

아래와 같이 DrawGrid 호출 추가:

```csharp
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
    _currentColCnt   = row.Table.Columns.Contains("colCnt") && row["colCnt"] != DBNull.Value
                       ? Convert.ToInt32(row["colCnt"]) : 0;
    _currentRowCnt   = row.Table.Columns.Contains("rowCnt") && row["rowCnt"] != DBNull.Value
                       ? Convert.ToInt32(row["rowCnt"]) : 0;

    DrawGrid(_currentMapArray, _currentColCnt, _currentRowCnt,
             checkBoxVFlip.Checked, checkBoxHFlip.Checked);
}
```

**Step 3: Flip/새로고침 핸들러 추가**

MapArray region 내부에 추가:

```csharp
private void CheckBoxFlip_CheckedChanged(object sender, EventArgs e)
{
    DrawGrid(_currentMapArray, _currentColCnt, _currentRowCnt,
             checkBoxVFlip.Checked, checkBoxHFlip.Checked);
}

private void BtnRefreshGrid_Click(object sender, EventArgs e)
{
    DrawGrid(_currentMapArray, _currentColCnt, _currentRowCnt,
             checkBoxVFlip.Checked, checkBoxHFlip.Checked);
}
```

**Step 4: 빌드 확인 (Ctrl+Shift+B)**

**Step 5: Commit**

```bash
git add Forms/MainForm.cs
git commit -m "feat: MapArray 그리드 이벤트 핸들러 연결"
```

---

### Task 6: 수동 테스트 및 최종 커밋

**Step 1: 앱 실행 후 MapArray 탭 확인**
- 로그인 후 MapArray 탭 진입
- 우측에 `panelMapGrid`가 표시되는지 확인
- V-Flip, H-Flip 체크박스가 기본 체크(ON) 상태인지 확인

**Step 2: 조회 후 그리드 동작 확인**
- PCB 2D ID 입력 후 조회
- ListView에서 행 클릭 → 그리드 렌더링 확인
- `■`(='2'), `·`(='0') 심볼 표시 확인
- 헤더 `C01 C02 ... CXX`, 행 `R01 R02 ...` 표시 확인

**Step 3: Flip 동작 확인**
- V-Flip 체크 해제 → 그리드 즉시 갱신 확인
- H-Flip 체크 해제 → 그리드 즉시 갱신 확인
- 새로고침 버튼 클릭 → 그리드 재렌더링 확인

**Step 4: 가변 크기 확인**
- colCnt/rowCnt가 다른 여러 데이터로 조회
- ScrollBars=Both로 스크롤 가능한지 확인

**Step 5: 최종 Push**

```bash
git push origin master
```
