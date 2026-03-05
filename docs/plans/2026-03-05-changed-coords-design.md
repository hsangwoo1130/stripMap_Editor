# MapArray 변경 좌표 저장 및 RV 전송 설계

## 개요

MapArray 'U' 수정 시, 변경된 셀의 Gold Gate 좌표(XPOS/YPOS)를 계산하여
DB spare 컬럼에 저장하고, 변경된 셀 수만큼 RV 메시지를 개별 전송한다.

---

## Gold Gate 좌표 체계

PPT(PCB MapArray 배열 순서.pptx) 기준. Flip 미적용 원본 좌표.

```
mapArray pos → Gold Gate X = colCnt - ((pos-1) % colCnt)   [16→1, 좌→우]
             → Gold Gate Y = ((pos-1) / colCnt) + 1          [1→rowCnt, 상→하]
```

예: pos=1 → X=16, Y=1 (Gold Gate: "16-1")

---

## 변경 사항

### 1. DB 컬럼 rename

```sql
-- tblStripMap
EXEC sp_rename 'dbo.tblStripMap.spare3', 'changedXpos', 'COLUMN';
EXEC sp_rename 'dbo.tblStripMap.spare4', 'changedYpos', 'COLUMN';

-- tblStripMapHistory
EXEC sp_rename 'dbo.tblStripMapHistory.spare3', 'changedXpos', 'COLUMN';
EXEC sp_rename 'dbo.tblStripMapHistory.spare4', 'changedYpos', 'COLUMN';
```

저장 형식: 쉼표 구분 문자열
- changedXpos = "16,3,1"
- changedYpos = "1,2,2"

### 2. SP 변경 (usp_StripMap_Process)

파라미터 추가:
```sql
@changedXpos VARCHAR(MAX) = NULL,
@changedYpos VARCHAR(MAX) = NULL,
```

'U' 액션 INSERT 두 곳(tblStripMapHistory, tblStripMap) 모두 spare3/spare4 → @changedXpos/@changedYpos로 변경.

### 3. C# — 좌표 계산

```csharp
// 구 vs 신 mapArray 비교 → Gold Gate 좌표 산출
private (List<int> xList, List<int> yList) CalcChangedCoords(
    string oldMap, string newMap, int colCnt)
{
    var xList = new List<int>();
    var yList = new List<int>();
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

### 4. C# — SP 호출 파라미터 추가

```csharp
var (xList, yList) = CalcChangedCoords(origMapArray, newMapArray, colCnt);
string xposList = string.Join(",", xList);
string yposList = string.Join(",", yList);

new SqlParameter("@changedXpos", xList.Count > 0 ? xposList : (object)DBNull.Value),
new SqlParameter("@changedYpos", yList.Count > 0 ? yposList : (object)DBNull.Value),
```

### 5. C# — RV 메시지 (XPOS/YPOS 추가 + 'U' 액션 전송 추가)

BuildMesRvXml에 xpos/ypos 파라미터 추가:
```csharp
private string BuildMesRvXml(string frameId, string actionType,
                              string functionId, int xpos = 0, int ypos = 0)
```

SendMesRvMessage에 xpos/ypos 파라미터 추가:
```csharp
private void SendMesRvMessage(string frameId, string actionType,
                               string functionId, int xpos = 0, int ypos = 0)
```

SP 성공 후 변경 좌표별 루프 전송:
```csharp
for (int i = 0; i < xList.Count; i++)
    SendMesRvMessage(stripNo, "U", ActionTypes.STRIP_UPDATE, xList[i], yList[i]);
```

기존 L/D/R 액션의 SendMesRvMessage 호출은 xpos/ypos 기본값(0) 사용 → 기존 동작 유지.

---

## 데이터 흐름

```
사용자 체크 + 새 mapArray 입력 → 수정 버튼 클릭
  ↓
CalcChangedCoords(old, new, colCnt) → xList, yList
  ↓
SP 'U' 호출 (@changedXpos, @changedYpos 포함)
  → tblStripMapHistory INSERT (changedXpos, changedYpos 저장)
  → tblStripMap DELETE + INSERT (changedXpos, changedYpos 저장)
  ↓
for each (x, y) in changed coords:
    SendMesRvMessage(stripNo, "U", STRIP_UPDATE, x, y)
    → XML: <FRAME_LOC_XPOS>x</FRAME_LOC_XPOS><FRAME_LOC_YPOS>y</FRAME_LOC_YPOS>
```

## 예외 처리

- newMapArray가 비어있으면(BinCode만 수정) xList가 빈 리스트 → RV 전송 없음, changedXpos/changedYpos = NULL
- colCnt = 0이면 CalcChangedCoords 건너뜀
