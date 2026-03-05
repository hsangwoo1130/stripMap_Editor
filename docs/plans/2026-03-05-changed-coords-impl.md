# MapArray 변경 좌표 저장 및 RV 전송 구현 계획

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** MapArray 'U' 수정 시 변경된 셀의 Gold Gate 좌표를 DB(changedXpos/changedYpos)에 저장하고, 변경 좌표별로 RV 메시지를 개별 전송한다.

**Architecture:** DB 컬럼 rename(spare3→changedXpos, spare4→changedYpos) → SP 파라미터 추가 → C# 좌표 계산 메서드 추가 → UpdateMapArrayData에 좌표 전달 및 RV 루프 전송. 기존 L/D/R 액션의 RV 전송은 기본값(xpos=0, ypos=0)으로 그대로 유지.

**Tech Stack:** SQL Server (sqlcmd), C# WinForms (.NET Framework 4.7.2), System.Data.SqlClient

**참고 설계 문서:** `docs/plans/2026-03-05-changed-coords-design.md`

---

### Task 1: DB 컬럼 rename

**Files:**
- Create: `Database/rename_spare_columns.sql`

**Step 1: SQL 파일 작성**

`Database/rename_spare_columns.sql` 파일을 생성한다:

```sql
-- tblStripMap spare3 → changedXpos, spare4 → changedYpos
EXEC sp_rename 'dbo.tblStripMap.spare3', 'changedXpos', 'COLUMN';
EXEC sp_rename 'dbo.tblStripMap.spare4', 'changedYpos', 'COLUMN';

-- tblStripMapHistory spare3 → changedXpos, spare4 → changedYpos
EXEC sp_rename 'dbo.tblStripMapHistory.spare3', 'changedXpos', 'COLUMN';
EXEC sp_rename 'dbo.tblStripMapHistory.spare4', 'changedYpos', 'COLUMN';

-- 확인
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'tblStripMap' AND COLUMN_NAME IN ('changedXpos','changedYpos','spare3','spare4');

SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'tblStripMapHistory' AND COLUMN_NAME IN ('changedXpos','changedYpos','spare3','spare4');
```

**Step 2: DB에 실행**

```bash
sqlcmd -S 192.168.10.79 -d SFA_TEST_DB -U sfa_test_login -P sfa_test_login \
  -i Database/rename_spare_columns.sql -f 65001
```

예상 출력: `COLUMN_NAME` 결과에 `changedXpos`, `changedYpos`만 표시 (spare3/spare4 없음).

**Step 3: Commit**

```bash
git add Database/rename_spare_columns.sql
git commit -m "feat: tblStripMap/History spare3→changedXpos, spare4→changedYpos 컬럼 rename"
```

---

### Task 2: SP 파라미터 및 INSERT 수정

**Files:**
- Modify: `Database/usp_StripMap_Process.sql` (SP 전체 재생성)

**배경:** SP는 `sys.sql_modules`에 저장. 현재 spare3/spare4를 INSERT에서 History 복사본으로 사용 중. 이를 @changedXpos/@changedYpos 파라미터로 교체한다.

**Step 1: SP 파라미터 추가**

`usp_StripMap_Process.sql`의 파라미터 블록에 추가 (기존 `@workerIp` 아래):

```sql
@changedXpos     VARCHAR(MAX)  = NULL,
@changedYpos     VARCHAR(MAX)  = NULL,
```

**Step 2: 'U' 액션 — tblStripMapHistory INSERT 수정**

'U' 액션 내 `INSERT dbo.tblStripMapHistory` 컬럼/값 목록에서:
- `spare3` → `changedXpos`
- `spare4` → `changedYpos`
- SELECT 절의 `spare3` → `@changedXpos`
- SELECT 절의 `spare4` → `@changedYpos`

변경 전:
```sql
INSERT dbo.tblStripMapHistory
(..., spare3, spare4, spare5, ...)
SELECT ..., spare3, spare4, spare5, ...
FROM dbo.tblStripMap
WHERE ...
```

변경 후:
```sql
INSERT dbo.tblStripMapHistory
(..., changedXpos, changedYpos, spare5, ...)
SELECT ..., @changedXpos, @changedYpos, spare5, ...
FROM dbo.tblStripMap
WHERE ...
```

**Step 3: 'U' 액션 — tblStripMap INSERT 수정**

'U' 액션 내 `INSERT dbo.tblStripMap` (History에서 복사하는 부분):
- `spare3` → `changedXpos`
- `spare4` → `changedYpos`
- SELECT 절의 `spare3` → `@changedXpos`
- SELECT 절의 `spare4` → `@changedYpos`

변경 후:
```sql
INSERT dbo.tblStripMap
(..., changedXpos, changedYpos, spare5, ...)
SELECT ..., @changedXpos, @changedYpos, spare5, ...
FROM dbo.tblStripMapHistory
WHERE timekey = @timekey;
```

**Step 4: DB에 SP 재생성**

```bash
sqlcmd -S 192.168.10.79 -d SFA_TEST_DB -U sfa_test_login -P sfa_test_login \
  -i Database/usp_StripMap_Process.sql -f 65001
```

예상 출력: `(1 rows affected)` — 오류 없으면 통과.

**Step 5: SP 동작 확인**

```bash
sqlcmd -S 192.168.10.79 -d SFA_TEST_DB -U sfa_test_login -P sfa_test_login -f 65001 -Q "
EXEC dbo.usp_StripMap_Process
  @actionType='L', @stripNo='TEST_DUMMY_NOEXIST',
  @process='TEST', @lotNo='NEWLOT',
  @changedXpos='16,1', @changedYpos='1,2',
  @workerId='admin', @comment='param test', @workerIp='127.0.0.1'
"
```

예상: `Msg 50002` (No function permission) → SP가 @changedXpos/Y 파라미터를 받고 정상 실행됨.

**Step 6: Commit**

```bash
git add Database/usp_StripMap_Process.sql
git commit -m "feat: usp_StripMap_Process에 @changedXpos/@changedYpos 파라미터 추가"
```

---

### Task 3: C# — CalcChangedCoords 메서드 추가

**Files:**
- Modify: `Forms/MainForm.cs` (MapArray region, ~line 1383 `#endregion` 앞)

**Step 1: 메서드 추가**

`UpdateMapArrayData` 메서드 바로 아래에 추가:

```csharp
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
```

**Step 2: 빌드 확인 (Ctrl+Shift+B)** — 오류 없으면 통과.

**Step 3: Commit**

```bash
git add Forms/MainForm.cs
git commit -m "feat: Gold Gate 좌표 계산 메서드 CalcChangedCoords 추가"
```

---

### Task 4: C# — UpdateMapArrayData에 좌표 계산 및 SP 파라미터 추가

**Files:**
- Modify: `Forms/MainForm.cs` (`UpdateMapArrayData` 메서드, ~line 1311)

**Step 1: colCnt 확보**

`UpdateMapArrayData` 시그니처 변경 — colCnt를 받도록 추가:

```csharp
// 변경 전
private void UpdateMapArrayData(List<ListViewItem> checkedItems, string newMapArray, string newBinCode)

// 변경 후
private void UpdateMapArrayData(List<ListViewItem> checkedItems, string newMapArray, string newBinCode, int colCnt)
```

**Step 2: 호출부 수정**

`BtnUpdateMapArray_Click`에서 호출하는 부분 (~line 1293):

```csharp
// 변경 전
UpdateMapArrayData(checkedItems, newMapArray, newBinCode);

// 변경 후
// checkedItems[0]에서 colCnt 추출 (모든 체크 항목은 같은 colCnt)
int colCnt = 0;
if (checkedItems.Count > 0)
{
    var firstRow = checkedItems[0].Tag as DataRow;
    if (firstRow != null && firstRow.Table.Columns.Contains("colCnt")
        && firstRow["colCnt"] != DBNull.Value)
        colCnt = Convert.ToInt32(firstRow["colCnt"]);
}
UpdateMapArrayData(checkedItems, newMapArray, newBinCode, colCnt);
```

**Step 3: UpdateMapArrayData 내부 — 좌표 계산 및 SP 파라미터 추가**

`foreach` 루프 내 `row["mapArray"]` 추출 후, SP 호출 전에 삽입:

```csharp
// 기존 코드 (유지)
string origMapArray = row["mapArray"]?.ToString() ?? "";

// 추가: 변경 좌표 계산
(List<int> xList, List<int> yList) changedCoords = (new List<int>(), new List<int>());
if (!string.IsNullOrEmpty(newMapArray) && colCnt > 0)
    changedCoords = CalcChangedCoords(origMapArray, newMapArray, colCnt);

string xposList = changedCoords.xList.Count > 0
    ? string.Join(",", changedCoords.xList) : null;
string yposList = changedCoords.yList.Count > 0
    ? string.Join(",", changedCoords.yList) : null;
```

SP 호출 파라미터에 추가 (`@workerIp` 아래):

```csharp
new SqlParameter("@changedXpos", (object)xposList ?? DBNull.Value),
new SqlParameter("@changedYpos", (object)yposList ?? DBNull.Value),
```

**Step 4: 빌드 확인 (Ctrl+Shift+B)**

**Step 5: Commit**

```bash
git add Forms/MainForm.cs
git commit -m "feat: UpdateMapArrayData에 Gold Gate 좌표 계산 및 SP 파라미터 전달"
```

---

### Task 5: C# — RV 메시지에 XPOS/YPOS 추가 및 'U' 액션 루프 전송

**Files:**
- Modify: `Forms/MainForm.cs` (`BuildMesRvXml`, `SendMesRvMessage`, `UpdateMapArrayData`, ~line 2058)

**Step 1: BuildMesRvXml 시그니처 및 본문 수정**

```csharp
// 변경 전
private string BuildMesRvXml(string frameId, string actionType, string functionId)
{
    return
        "<message>" +
          "<header>" +
            $"<messagename>{functionId}</messagename>" +
          "</header>" +
          "<body>" +
            $"<FRAME_ID>{frameId}</FRAME_ID>" +
            $"<ACTIONTYPE>{actionType}</ACTIONTYPE>" +
            "<FRAME_LOC_XPOS></FRAME_LOC_XPOS>" +
            "<FRAME_LOC_YPOS></FRAME_LOC_YPOS>" +
          "</body>" +
        "</message>";
}

// 변경 후
private string BuildMesRvXml(string frameId, string actionType,
                              string functionId, int xpos = 0, int ypos = 0)
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
```

**Step 2: SendMesRvMessage 시그니처 수정**

```csharp
// 변경 전
private void SendMesRvMessage(string frameId, string actionType, string functionId)
{
    if (Rv == null || !Rv.IsConnected) return;
    try
    {
        Rv.RvSend(Rv.Subject, BuildMesRvXml(frameId, actionType, functionId));
    }
    catch (Exception ex)
    {
        AppLogger.Info($"[RV_SEND_FAIL] frameId={frameId} actionType={actionType} | {ex.Message}");
    }
}

// 변경 후
private void SendMesRvMessage(string frameId, string actionType,
                               string functionId, int xpos = 0, int ypos = 0)
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
```

**Step 3: UpdateMapArrayData — SP 성공 후 RV 루프 전송 추가**

`successCount++;` 바로 위에 삽입:

```csharp
// 변경된 좌표별 RV 메시지 개별 전송
for (int i = 0; i < changedCoords.xList.Count; i++)
    SendMesRvMessage(stripNo, "U", ActionTypes.STRIP_UPDATE,
                     changedCoords.xList[i], changedCoords.yList[i]);
```

**Step 4: 빌드 확인 (Ctrl+Shift+B)** — 기존 L/D/R 호출부는 기본값(0,0) 사용으로 변경 없음.

**Step 5: Commit**

```bash
git add Forms/MainForm.cs
git commit -m "feat: RV 메시지에 XPOS/YPOS 추가 및 MapArray 'U' 액션 좌표별 개별 전송"
```

---

### Task 6: 수동 테스트 및 Push

**Step 1: 시뮬레이션 모드로 RV 전송 확인**

`config.ini`에서 `Simulation=true`로 설정 후 앱 실행.
MapArray 탭에서 조회 → 행 체크 → mapArray 일부 수정 → 수정 버튼 클릭.

로그 파일(`logs/stripmap_YYYYMMDD.txt`) 확인:
- `[RV_SIM] 전송 시뮬레이션 TO:... / DATA=<message>...<FRAME_LOC_XPOS>16</FRAME_LOC_XPOS>...` 형태로 변경된 셀 수만큼 출력되는지 확인.

**Step 2: DB 저장 확인**

```sql
SELECT TOP 1 stripNo, changedXpos, changedYpos
FROM dbo.tblStripMapHistory
WHERE actionType = 'UPDATE'
ORDER BY timekey DESC;
```

예상: changedXpos = "16,3" 형태로 저장됨.

**Step 3: BinCode만 수정 시 RV 미전송 확인**

mapArray를 비워두고 BinCode만 수정 → 로그에 RV 전송 없음 확인.

**Step 4: Push**

```bash
git push origin master
```
