# MapArray BinCode 자동 동기화 Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** textBoxMapArray 입력 시 binCode를 실시간 자동 계산하여 textBoxBinCode에 반영하고, 저장 시에도 재계산하여 DB에 정확한 값을 저장한다.

**Architecture:** `ComputeBinCode` 헬퍼가 mapArray 문자열과 원본 binCode를 받아 '0'→'1', '2'→'D' 규칙으로 새 binCode를 반환한다. `TextBoxMapArray_TextChanged`에서 실시간으로 textBoxBinCode를 갱신하고, `BtnUpdateMapArray_Click`에서 저장 직전 재계산하여 DB에 정확한 값을 저장한다. 원본 binCode는 `_currentBinCode` 필드에 보관한다.

**Tech Stack:** C# / .NET Framework 4.7.2 / WinForms

---

## 현재 관련 코드 위치 (MainForm.cs)

- `_currentMapArray` 필드: line ~813
- `ListViewResultMapArray_SelectedIndexChanged`: line ~1081 — `_currentMapArray` 저장 위치
- `TextBoxMapArray_TextChanged`: line ~1152 — 실시간 핸들러
- `BtnUpdateMapArray_Click`: line ~1362 — 저장 버튼 핸들러
  - `newMapArray`, `newBinCode` 읽는 위치: line ~1380
  - 자릿수 검증 끝 주석: line ~1430 (`// ── 자릿수 검증 끝 ──`)
  - `UpdateMapArrayDataAsync` 호출: line ~1449

---

### Task 1: `ComputeBinCode` 헬퍼 + `_currentBinCode` 필드 + 실시간 연동

**Files:**
- Modify: `Forms/MainForm.cs`

**Step 1: `_currentBinCode` 필드 추가**

`private string _currentMapArray = "";` 바로 아래에 추가:

```csharp
private string _currentBinCode   = "";
```

**Step 2: `ListViewResultMapArray_SelectedIndexChanged`에서 `_currentBinCode` 저장**

해당 핸들러에서 `SetGridMode(false);` 바로 아래에 추가:

```csharp
_currentBinCode  = row["bincode"]?.ToString() ?? "";
```

> 위치 참고: `Forms/MainForm.cs` ~1094행, `_currentMapArray` 대입 직후 `SetGridMode(false)` 다음 줄

**Step 3: `ComputeBinCode` 헬퍼 추가**

`SetGridMode` 메서드 바로 위에 추가:

```csharp
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
```

> `StringBuilder`는 이미 `using System.Text;`가 선언되어 있으므로 추가 import 불필요

**Step 4: `TextBoxMapArray_TextChanged`에 실시간 binCode 갱신 추가**

현재 코드:
```csharp
private void TextBoxMapArray_TextChanged(object sender, EventArgs e)
{
    bool hasValue = !string.IsNullOrEmpty(textBoxMapArray.Text.Trim());
    btnRefreshGrid.Enabled = hasValue;
    btnGridPreview.Enabled = hasValue;
    if (!hasValue && _isPreviewMode)
    {
        SetGridMode(false);
        DrawGrid(_currentMapArray, _currentColCnt, _currentRowCnt,
                 checkBoxVFlip.Checked, checkBoxHFlip.Checked);
    }
}
```

변경 후:
```csharp
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
```

**Step 5: 빌드 확인**

```bash
powershell -Command "& 'C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe' 'd:\Workspace\SFA\StripMap\stripMap_Editor\stripMap_Editor.sln' /p:Configuration=Debug /t:Build /v:minimal 2>&1 | tail -5"
```

Expected: `Build succeeded.`

**Step 6: 커밋**

```bash
cd d:/Workspace/SFA/StripMap/stripMap_Editor
git add Forms/MainForm.cs
git commit -m "feat: MapArray 입력 시 BinCode 실시간 자동 계산 연동"
```

---

### Task 2: 저장 시 binCode 재계산 (`BtnUpdateMapArray_Click` 수정)

**Files:**
- Modify: `Forms/MainForm.cs`

**Step 1: 자릿수 검증 끝 직후에 binCode 재계산 블록 삽입**

`BtnUpdateMapArray_Click` 내에서 아래 주석 바로 아래에 삽입:

```
// ── 자릿수 검증 끝 ──
```

삽입할 코드:

```csharp
// ── mapArray 입력 시 binCode 자동 계산 ──
if (!string.IsNullOrEmpty(newMapArray) && checkedItems.Count > 0)
{
    var firstRow = checkedItems[0].Tag as DataRow;
    if (firstRow != null)
    {
        string origBinCode = firstRow["bincode"]?.ToString() ?? "";
        newBinCode = ComputeBinCode(newMapArray, origBinCode);
    }
}
// ── binCode 자동 계산 끝 ──
```

> 위치: `// ── 자릿수 검증 끝 ──` (line ~1430) 바로 다음 줄
> `newBinCode`는 이미 `string newBinCode = textBoxBinCode.Text.Trim();`로 선언되어 있으므로 override만 함

**Step 2: 빌드 확인**

```bash
powershell -Command "& 'C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe' 'd:\Workspace\SFA\StripMap\stripMap_Editor\stripMap_Editor.sln' /p:Configuration=Debug /t:Build /v:minimal 2>&1 | tail -5"
```

Expected: `Build succeeded.`

**Step 3: 커밋**

```bash
cd d:/Workspace/SFA/StripMap/stripMap_Editor
git add Forms/MainForm.cs
git commit -m "feat: 저장 시 mapArray 기반 binCode 자동 재계산 적용"
```
