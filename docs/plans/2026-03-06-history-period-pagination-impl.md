# PCB 원복 탭 기간 페이지네이션 구현 계획

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** PCB 원복 탭(tblStripMapHistory 조회)에 이전/다음 버튼으로 한 달 단위 기간 탐색 기능을 추가한다.

**Architecture:** `_periodOffset`(int) 멤버 변수로 기간 위치를 관리하고, 이전/다음 버튼 클릭 시 offset 증감 후 DB를 재조회한다. 기간은 오늘 기준 day 단위로 정확히 한 달씩 나눈다.

**Tech Stack:** C# WinForms (.NET Framework 4.7.2), System.Data.SqlClient

**참고 설계 문서:** `docs/plans/2026-03-06-history-period-pagination-design.md`

---

### Task 1: Designer.cs — 이전/기간라벨/다음 컨트롤 추가

**Files:**
- Modify: `Forms/MainForm.Designer.cs`

**Step 1: 필드 선언 추가**

`// PCB 2D ID 원복 탭 컨트롤들` 블록 (~line 1137) 내 `btnPurgeRollback_PCB` 선언 아래에 추가:

```csharp
// 기간 페이지네이션 컨트롤
private System.Windows.Forms.Button btnPrevPeriod;
private System.Windows.Forms.Label  labelPeriod;
private System.Windows.Forms.Button btnNextPeriod;
```

**Step 2: InitializeComponent 상단 `new` 선언 추가**

`this.btnPurgeRollback_PCB = new ...;` 줄 (~line 96) 바로 아래에 추가:

```csharp
this.btnPrevPeriod = new System.Windows.Forms.Button();
this.labelPeriod   = new System.Windows.Forms.Label();
this.btnNextPeriod = new System.Windows.Forms.Button();
```

**Step 3: tabPagePcbRestore.Controls.Add 추가**

`this.tabPagePcbRestore.Controls.Add(this.labelResultTitle);` 줄 (~line 687) 바로 아래에 추가:

```csharp
this.tabPagePcbRestore.Controls.Add(this.btnPrevPeriod);
this.tabPagePcbRestore.Controls.Add(this.labelPeriod);
this.tabPagePcbRestore.Controls.Add(this.btnNextPeriod);
```

**Step 4: 컨트롤 속성 블록 추가**

`// labelResultTitle` 속성 블록 끝 (`this.labelResultTitle.Text = "조회 결과";` 줄, ~line 719) 바로 아래에 추가:

```csharp
//
// btnPrevPeriod
//
this.btnPrevPeriod.Font     = new System.Drawing.Font("맑은 고딕", 9F);
this.btnPrevPeriod.Location = new System.Drawing.Point(500, 212);
this.btnPrevPeriod.Name     = "btnPrevPeriod";
this.btnPrevPeriod.Size     = new System.Drawing.Size(65, 24);
this.btnPrevPeriod.TabIndex = 20;
this.btnPrevPeriod.Text     = "◀ 이전";
this.btnPrevPeriod.UseVisualStyleBackColor = true;
//
// labelPeriod
//
this.labelPeriod.AutoSize  = false;
this.labelPeriod.Font      = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Bold);
this.labelPeriod.Location  = new System.Drawing.Point(572, 216);
this.labelPeriod.Name      = "labelPeriod";
this.labelPeriod.Size      = new System.Drawing.Size(220, 17);
this.labelPeriod.TabIndex  = 21;
this.labelPeriod.Text      = "";
this.labelPeriod.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
//
// btnNextPeriod
//
this.btnNextPeriod.Enabled  = false;
this.btnNextPeriod.Font     = new System.Drawing.Font("맑은 고딕", 9F);
this.btnNextPeriod.Location = new System.Drawing.Point(800, 212);
this.btnNextPeriod.Name     = "btnNextPeriod";
this.btnNextPeriod.Size     = new System.Drawing.Size(65, 24);
this.btnNextPeriod.TabIndex = 22;
this.btnNextPeriod.Text     = "다음 ▶";
this.btnNextPeriod.UseVisualStyleBackColor = true;
```

**Step 5: 빌드 확인**

```
msbuild stripMap_Editor.sln /p:Configuration=Debug /t:Build /v:minimal
```

예상: 오류 없음. (이벤트 핸들러는 Task 3에서 연결하므로 빌드만 확인)

**Step 6: Commit**

```bash
git add Forms/MainForm.Designer.cs
git commit -m "feat: PCB 원복 탭에 기간 이전/다음 버튼 및 기간 라벨 컨트롤 추가"
```

---

### Task 2: MainForm.cs — 멤버 변수, 헬퍼, LoadHistoryData 수정

**Files:**
- Modify: `Forms/MainForm.cs` (~line 24, ~line 1460)

**Step 1: 멤버 변수 추가**

`private DataTable originalData;` 줄 (~line 24) 아래에 추가:

```csharp
private int _periodOffset = 0;  // 0=현재 기간, 1=1개월 전, ...
```

**Step 2: GetPeriodRange 헬퍼 추가**

`#region PCB 2D ID 원복 탭` (~line 1437) 바로 아래에 추가:

```csharp
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
```

**Step 3: LoadHistoryData 시그니처 수정 및 SQL 날짜 필터 추가**

현재 (~line 1460):
```csharp
private void LoadHistoryData(string lotNo, string stripNo, string mgzRf)
```

변경 후:
```csharp
private void LoadHistoryData(string lotNo, string stripNo, string mgzRf,
                             DateTime startDate, DateTime endDate)
```

SQL WHERE 절 (`WHERE 1=1` 아래, ~line 1481) 에 날짜 파라미터 추가:

```csharp
// 기간 필터 (timekey 앞 8자리 = yyyyMMdd)
queryBuilder.Append(" AND LEFT(timekey, 8) >= @StartDate");
queryBuilder.Append(" AND LEFT(timekey, 8) <= @EndDate");
parameters.Add(new SqlParameter("@StartDate", startDate.ToString("yyyyMMdd")));
parameters.Add(new SqlParameter("@EndDate",   endDate.ToString("yyyyMMdd")));
```

위치: `var parameters = new List<SqlParameter>();` 다음, 기존 lotNo/stripNo/mgzRf 필터보다 **앞**에 추가.

**Step 4: LoadHistoryData 완료 후 UI 갱신 로직 추가**

`labelResultTitle.Text = $"조회 결과 ({dt.Rows.Count}건)";` 줄 (~line 1509) 아래에 추가:

```csharp
// 기간 라벨 갱신
labelPeriod.Text = $"{startDate:yyyy-MM-dd} ~ {endDate:yyyy-MM-dd}";
// 다음 버튼: offset=0이면 현재 기간이므로 비활성
btnNextPeriod.Enabled = (_periodOffset > 0);
```

**Step 5: 빌드 확인**

```
msbuild stripMap_Editor.sln /p:Configuration=Debug /t:Build /v:minimal
```

예상: `BtnSearch_Click`에서 `LoadHistoryData` 호출부 시그니처 불일치 컴파일 오류 발생 — 정상 (Step 6에서 수정).

**Step 6: BtnSearch_Click 수정**

현재 (~line 1451):
```csharp
LoadHistoryData(lotNo, stripNo, mgzRf);
```

변경 후:
```csharp
_periodOffset = 0;
var (start, end) = GetPeriodRange(_periodOffset);
LoadHistoryData(lotNo, stripNo, mgzRf, start, end);
```

**Step 7: 빌드 확인**

```
msbuild stripMap_Editor.sln /p:Configuration=Debug /t:Build /v:minimal
```

예상: 오류 없음.

**Step 8: Commit**

```bash
git add Forms/MainForm.cs
git commit -m "feat: LoadHistoryData에 날짜 기간 파라미터 추가 및 기간 UI 갱신"
```

---

### Task 3: MainForm.cs — 이전/다음 버튼 이벤트 핸들러 연결

**Files:**
- Modify: `Forms/MainForm.cs` (~line 237, ~line 1523)

**Step 1: 이벤트 등록 추가**

`btnPurgeRollback_PCB.Click += BtnPurgeRollback_Click;` 줄 (~line 237) 아래에 추가:

```csharp
btnPrevPeriod.Click += BtnPrevPeriod_Click;
btnNextPeriod.Click += BtnNextPeriod_Click;
```

**Step 2: 핸들러 추가**

`LoadHistoryData` 메서드 끝 (`#endregion` 앞) 에 추가:

```csharp
private void BtnPrevPeriod_Click(object sender, EventArgs e)
{
    _periodOffset++;
    var (start, end) = GetPeriodRange(_periodOffset);
    string lotNo   = textBox_LOT.Text.Trim();
    string stripNo = textBox_PCB.Text.Trim();
    string mgzRf   = textBox_MGZ.Text.Trim();
    LoadHistoryData(lotNo, stripNo, mgzRf, start, end);
}

private void BtnNextPeriod_Click(object sender, EventArgs e)
{
    if (_periodOffset <= 0) return;
    _periodOffset--;
    var (start, end) = GetPeriodRange(_periodOffset);
    string lotNo   = textBox_LOT.Text.Trim();
    string stripNo = textBox_PCB.Text.Trim();
    string mgzRf   = textBox_MGZ.Text.Trim();
    LoadHistoryData(lotNo, stripNo, mgzRf, start, end);
}
```

**Step 3: 빌드 확인**

```
msbuild stripMap_Editor.sln /p:Configuration=Debug /t:Build /v:minimal
```

예상: 오류 없음.

**Step 4: Commit**

```bash
git add Forms/MainForm.cs
git commit -m "feat: PCB 원복 탭 이전/다음 기간 버튼 핸들러 추가"
```

---

### Task 4: 수동 테스트 및 Push

**Step 1: 앱 실행 후 PCB 원복 탭 확인**
- 로그인 후 PCB 원복 탭 진입
- 이전/다음 버튼과 기간 라벨이 표시되는지 확인
- 기간 라벨 초기값은 비어 있음 (조회 전)

**Step 2: 조회 버튼 클릭**
- 조회 버튼 클릭 시 기간 라벨에 오늘 기준 한 달 범위 표시 확인
  - 예: `2026-02-07 ~ 2026-03-06`
- `다음 ▶` 버튼이 비활성(회색) 상태인지 확인
- `◀ 이전` 버튼은 항상 활성

**Step 3: 이전 버튼 클릭**
- `◀ 이전` 클릭 → 기간 라벨이 1개월 이전으로 변경됨 확인
  - 예: `2026-01-07 ~ 2026-02-06`
- `다음 ▶` 버튼이 활성화되는지 확인
- ListView에 해당 기간 데이터 표시 확인

**Step 4: 다음 버튼 클릭**
- `다음 ▶` 클릭 → 기간이 원래 기간으로 복귀 확인
- `다음 ▶` 다시 비활성 확인

**Step 5: 검색 필터 + 기간 이동**
- LOT ID 텍스트 입력 후 조회 → offset=0으로 초기화되는지 확인
- 이전 버튼으로 과거 기간 이동 후 조회 버튼 클릭 → 최신 기간으로 초기화 확인

**Step 6: Push**

```bash
git push origin master
```
