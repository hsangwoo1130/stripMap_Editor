# MapArray Grid 미리보기 토글 Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** MapArray 수정 탭에서 새 값 입력 후 수정 버튼 클릭 전에 2D 그리드로 미리 확인할 수 있는 토글 기능 추가

**Architecture:** panelMapGrid 상단에 [기존]/[미리보기] 토글 버튼 2개와 [새로고침 ↺] 버튼을 추가하고, `_isPreviewMode` 플래그로 그리드 렌더 대상(기존 mapArray vs textBoxMapArray 입력값)을 전환한다. 기존 `DrawGrid()` 메서드를 그대로 재사용하며 UI 구조 변경은 최소화한다.

**Tech Stack:** C# / .NET Framework 4.7.2 / WinForms

---

## 파일 구조

- Modify: `Forms/MainForm.Designer.cs` — 컨트롤 3개 추가, checkBox/richTextBox y 위치 조정
- Modify: `Forms/MainForm.cs` — 상태 변수, 이벤트 등록/해제, 핸들러 구현

## 현재 panelMapGrid 레이아웃 (변경 전)

```
panelMapGrid: Location=(545,21), Size=(511,175)
  checkBoxVFlip:    Location=(10,8)
  checkBoxHFlip:    Location=(130,8)
  richTextBoxGrid:  Location=(5,38),  Size=(497,128)
```

## 변경 후 레이아웃

```
panelMapGrid: Location=(545,21), Size=(511,175)  ← 크기 그대로
  btnGridOriginal:  Location=(10,8),  Size=(50,22)   ← 신규
  btnGridPreview:   Location=(63,8),  Size=(65,22)   ← 신규
  btnRefreshGrid:   Location=(398,8), Size=(95,22)   ← 신규
  checkBoxVFlip:    Location=(10,36)                 ← y: 8→36
  checkBoxHFlip:    Location=(130,36)                ← y: 8→36
  richTextBoxGrid:  Location=(5,62),  Size=(497,104) ← y: 38→62, h: 128→104
```

---

### Task 1: Designer.cs — 컨트롤 추가 및 위치 조정

**Files:**
- Modify: `Forms/MainForm.Designer.cs`

**Step 1: `new` 선언 3개 추가**

`InitializeComponent` 상단 `new` 선언 블록에서 `this.panelMapGrid = new ...;` 바로 아래에 추가:

```csharp
this.btnGridOriginal = new System.Windows.Forms.Button();
this.btnGridPreview  = new System.Windows.Forms.Button();
this.btnRefreshGrid  = new System.Windows.Forms.Button();
```

**Step 2: panelMapGrid.Controls.Add 3개 추가**

`this.panelMapGrid.Controls.Add(this.checkBoxVFlip);` 위에 추가:

```csharp
this.panelMapGrid.Controls.Add(this.btnGridOriginal);
this.panelMapGrid.Controls.Add(this.btnGridPreview);
this.panelMapGrid.Controls.Add(this.btnRefreshGrid);
```

**Step 3: checkBoxVFlip y 위치 수정**

```
변경 전: this.checkBoxVFlip.Location = new System.Drawing.Point(10, 8);
변경 후: this.checkBoxVFlip.Location = new System.Drawing.Point(10, 36);
```

**Step 4: checkBoxHFlip y 위치 수정**

```
변경 전: this.checkBoxHFlip.Location = new System.Drawing.Point(130, 8);
변경 후: this.checkBoxHFlip.Location = new System.Drawing.Point(130, 36);
```

**Step 5: richTextBoxGrid 위치/크기 수정**

```
변경 전:
  this.richTextBoxGrid.Location = new System.Drawing.Point(5, 38);
  this.richTextBoxGrid.Size     = new System.Drawing.Size(497, 128);

변경 후:
  this.richTextBoxGrid.Location = new System.Drawing.Point(5, 62);
  this.richTextBoxGrid.Size     = new System.Drawing.Size(497, 104);
```

**Step 6: 새 컨트롤 속성 블록 추가**

`richTextBoxGrid` 속성 블록 바로 아래에 추가:

```csharp
// btnGridOriginal
this.btnGridOriginal.BackColor = System.Drawing.Color.CornflowerBlue;
this.btnGridOriginal.FlatAppearance.BorderSize = 0;
this.btnGridOriginal.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
this.btnGridOriginal.Font = new System.Drawing.Font("맑은 고딕", 9F);
this.btnGridOriginal.ForeColor = System.Drawing.Color.White;
this.btnGridOriginal.Location = new System.Drawing.Point(10, 8);
this.btnGridOriginal.Name = "btnGridOriginal";
this.btnGridOriginal.Size = new System.Drawing.Size(50, 22);
this.btnGridOriginal.TabIndex = 4;
this.btnGridOriginal.Text = "기존";
this.btnGridOriginal.UseVisualStyleBackColor = false;

// btnGridPreview
this.btnGridPreview.FlatAppearance.BorderSize = 1;
this.btnGridPreview.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
this.btnGridPreview.Font = new System.Drawing.Font("맑은 고딕", 9F);
this.btnGridPreview.Location = new System.Drawing.Point(63, 8);
this.btnGridPreview.Name = "btnGridPreview";
this.btnGridPreview.Size = new System.Drawing.Size(65, 22);
this.btnGridPreview.TabIndex = 5;
this.btnGridPreview.Text = "미리보기";
this.btnGridPreview.UseVisualStyleBackColor = true;

// btnRefreshGrid
this.btnRefreshGrid.Enabled = false;
this.btnRefreshGrid.Font = new System.Drawing.Font("맑은 고딕", 9F);
this.btnRefreshGrid.Location = new System.Drawing.Point(398, 8);
this.btnRefreshGrid.Name = "btnRefreshGrid";
this.btnRefreshGrid.Size = new System.Drawing.Size(95, 22);
this.btnRefreshGrid.TabIndex = 6;
this.btnRefreshGrid.Text = "새로고침 ↺";
this.btnRefreshGrid.UseVisualStyleBackColor = true;
```

**Step 7: 필드 선언 추가**

Designer.cs 하단 `private` 필드 선언 블록에 추가 (예: `private System.Windows.Forms.RichTextBox richTextBoxGrid;` 아래):

```csharp
private System.Windows.Forms.Button btnGridOriginal;
private System.Windows.Forms.Button btnGridPreview;
private System.Windows.Forms.Button btnRefreshGrid;
```

**Step 8: 빌드 확인**

```bash
powershell -Command "& 'C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe' 'd:\Workspace\SFA\StripMap\stripMap_Editor\stripMap_Editor.sln' /p:Configuration=Debug /t:Build /v:minimal 2>&1 | tail -5"
```

Expected: `Build succeeded.`

**Step 9: 커밋**

```bash
cd d:/Workspace/SFA/StripMap/stripMap_Editor
git add Forms/MainForm.Designer.cs
git commit -m "feat: MapArray grid 패널에 기존/미리보기/새로고침 컨트롤 추가"
```

---

### Task 2: MainForm.cs — 상태 변수 및 이벤트 등록/해제

**Files:**
- Modify: `Forms/MainForm.cs`

**Step 1: `_isPreviewMode` 필드 추가**

`private string _currentMapArray = "";` 바로 아래에 추가:

```csharp
private bool   _isPreviewMode    = false;
```

**Step 2: 이벤트 등록 추가**

`MainForm.cs` 초기화 코드에서 `checkBoxVFlip.CheckedChanged += CheckBoxFlip_CheckedChanged;` 아래에 추가:

```csharp
btnGridOriginal.Click      += BtnGridOriginal_Click;
btnGridPreview.Click       += BtnGridPreview_Click;
btnRefreshGrid.Click       += BtnRefreshGrid_Click;
textBoxMapArray.TextChanged += TextBoxMapArray_TextChanged;
```

**Step 3: 이벤트 해제 추가**

FormClosing 핸들러(또는 이벤트 해제 블록)에서 `checkBoxVFlip.CheckedChanged -= CheckBoxFlip_CheckedChanged;` 아래에 추가:

```csharp
btnGridOriginal.Click      -= BtnGridOriginal_Click;
btnGridPreview.Click       -= BtnGridPreview_Click;
btnRefreshGrid.Click       -= BtnRefreshGrid_Click;
textBoxMapArray.TextChanged -= TextBoxMapArray_TextChanged;
```

**Step 4: 빌드 확인**

```bash
powershell -Command "& 'C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe' 'd:\Workspace\SFA\StripMap\stripMap_Editor\stripMap_Editor.sln' /p:Configuration=Debug /t:Build /v:minimal 2>&1 | tail -5"
```

Expected: `Build succeeded.` (핸들러 미구현이라 경고 발생할 수 있으나 빌드 성공이면 OK)

---

### Task 3: MainForm.cs — 핸들러 구현

**Files:**
- Modify: `Forms/MainForm.cs`

**Step 1: `SetGridMode` 헬퍼 추가**

`CheckBoxFlip_CheckedChanged` 메서드 바로 위에 추가:

```csharp
private void SetGridMode(bool previewMode)
{
    _isPreviewMode = previewMode;
    btnGridOriginal.BackColor = previewMode ? SystemColors.Control       : Color.CornflowerBlue;
    btnGridOriginal.ForeColor = previewMode ? SystemColors.ControlText   : Color.White;
    btnGridPreview.BackColor  = previewMode ? Color.CornflowerBlue       : SystemColors.Control;
    btnGridPreview.ForeColor  = previewMode ? Color.White                : SystemColors.ControlText;
}
```

**Step 2: `RefreshPreviewGrid` 헬퍼 추가**

`SetGridMode` 바로 아래에 추가:

```csharp
private void RefreshPreviewGrid()
{
    string previewMapArray = textBoxMapArray.Text.Trim();
    SetGridMode(true);
    DrawGrid(previewMapArray, _currentColCnt, _currentRowCnt,
             checkBoxVFlip.Checked, checkBoxHFlip.Checked);
}
```

**Step 3: 이벤트 핸들러 3개 추가**

`RefreshPreviewGrid` 바로 아래에 추가:

```csharp
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
    if (!hasValue && _isPreviewMode)
    {
        SetGridMode(false);
        DrawGrid(_currentMapArray, _currentColCnt, _currentRowCnt,
                 checkBoxVFlip.Checked, checkBoxHFlip.Checked);
    }
}
```

**Step 4: `CheckBoxFlip_CheckedChanged` 수정**

```
변경 전:
private void CheckBoxFlip_CheckedChanged(object sender, EventArgs e)
{
    DrawGrid(_currentMapArray, _currentColCnt, _currentRowCnt,
             checkBoxVFlip.Checked, checkBoxHFlip.Checked);
}

변경 후:
private void CheckBoxFlip_CheckedChanged(object sender, EventArgs e)
{
    string mapArray = _isPreviewMode ? textBoxMapArray.Text.Trim() : _currentMapArray;
    DrawGrid(mapArray, _currentColCnt, _currentRowCnt,
             checkBoxVFlip.Checked, checkBoxHFlip.Checked);
}
```

**Step 5: `ListViewResultMapArray_SelectedIndexChanged`에 SetGridMode 추가**

해당 핸들러 내에서 `_currentMapArray = row["mapArray"]?.ToString() ?? "";` 바로 아래에 추가:

```csharp
SetGridMode(false);
```

> 위치 참고: `Forms/MainForm.cs:1084` 부근, `_currentMapArray` 대입 직후

**Step 6: 빌드 확인**

```bash
powershell -Command "& 'C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe' 'd:\Workspace\SFA\StripMap\stripMap_Editor\stripMap_Editor.sln' /p:Configuration=Debug /t:Build /v:minimal 2>&1 | tail -5"
```

Expected: `Build succeeded.`

**Step 7: 커밋**

```bash
cd d:/Workspace/SFA/StripMap/stripMap_Editor
git add Forms/MainForm.cs
git commit -m "feat: MapArray grid 미리보기 토글 기능 구현"
```
