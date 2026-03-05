# MapArray 2D 그리드 시각화 설계

## 개요

MapArray 탭에서 조회한 데이터를 2D 그리드로 시각화하여 설비 방향 검증을 지원한다.
Vertical/Horizontal Flip 체크박스로 방향을 전환하며, ListView 행 선택 시 즉시 그리드가 갱신된다.

---

## 레이아웃

```
x=19                     x=545
┌──────────────────────┐ ┌──────────────────────────────────┐
│ panelSearchMapArray  │ │ panelMapGrid (신규)               │
│ PCB 2D ID: [      ]  │ │ [V Flip] [H Flip]    [새로고침]  │  y=21
│ [조회]               │ │                                  │  h=175
│                      │ │ C01 C02 C03 ... CXX              │
│                      │ │ R01  ■   ·   ·  ...  ■           │
│                      │ │ R0N  ·   ·   ·  ...  ·           │
└──────────────────────┘ └──────────────────────────────────┘
```

- `panelSearchMapArray`: Location=(19,21), Size=(518,175) — 변경 없음
- `panelMapGrid` (신규): Location=(545,21), Size=(511,175)

---

## 신규 컨트롤

| 컨트롤명 | 타입 | 속성 | 설명 |
|---------|------|------|------|
| `panelMapGrid` | Panel | Location=(545,21), Size=(511,175), BackColor=White, BorderStyle=Fixed3D | 그리드 전체 컨테이너 |
| `checkBoxVFlip` | CheckBox | Checked=true, Text="Vertical Flip" | 상하 반전 (기본 ON) |
| `checkBoxHFlip` | CheckBox | Checked=true, Text="Horizontal Flip" | 좌우 반전 (기본 ON) |
| `btnRefreshGrid` | Button | Text="새로고침" | 강제 그리드 재렌더링 |
| `richTextBoxGrid` | RichTextBox | Font=Courier New 9pt, ReadOnly=true, ScrollBars=Both, BackColor=White, BorderStyle=None | 그리드 텍스트 출력 |

---

## DB 쿼리 변경

**파일**: `MainForm.cs` MapArray 검색 쿼리 (line ~774)

```sql
-- 기존
SELECT stripNo, process, mapArray, bincode
FROM dbo.[tblStripMap]
WHERE active = 1 AND stripNo LIKE @StripNo

-- 변경
SELECT stripNo, process, mapArray, bincode, colCnt, rowCnt
FROM dbo.[tblStripMap]
WHERE active = 1 AND stripNo LIKE @StripNo
```

---

## 좌표 변환 로직 (자리수 기준)

```csharp
// mapArray 문자열 위치 → 2D 그리드 좌표
// 규칙: 오른쪽에서 왼쪽으로 채움 (pos=1 → Col=colCnt, pos=colCnt → Col=1)
int r = ((pos - 1) / colCnt) + 1;
int c = colCnt - ((pos - 1) % colCnt);

// Flip 적용
if (flipV) r = rowCnt - r + 1;
if (flipH) c = colCnt - c + 1;

// 심볼
// '2' → "■", '0' → "·", 그 외 → "?"
```

---

## 그리드 렌더링 메서드

```csharp
// DrawGrid(string mapArray, int colCnt, int rowCnt, bool flipV, bool flipH)
// - char[,] grid 배열 구성 후 StringBuilder로 텍스트 생성
// - 헤더: "       C01 C02 ... CXX"
// - 행:   "R01    ■   ·  ...  ■"
// - richTextBoxGrid.Text에 출력
// - colCnt/rowCnt 가변 → ScrollBars=Both로 스크롤 지원
```

---

## 트리거

| 이벤트 | 동작 |
|--------|------|
| `listViewResult_MapArray.SelectedIndexChanged` | 선택 행의 mapArray/colCnt/rowCnt로 DrawGrid 호출 |
| `checkBoxVFlip.CheckedChanged` | DrawGrid 재호출 |
| `checkBoxHFlip.CheckedChanged` | DrawGrid 재호출 |
| `btnRefreshGrid.Click` | DrawGrid 강제 재호출 |

---

## 데이터 저장

현재 선택된 행의 colCnt, rowCnt를 멤버 변수로 캐싱:

```csharp
private int _currentColCnt = 0;
private int _currentRowCnt = 0;
private string _currentMapArray = "";
```

ListView 선택 시 갱신 → Flip 변경/새로고침 시 재사용.

---

## 예외 처리

- mapArray가 null/empty → richTextBoxGrid에 "데이터 없음" 표시
- colCnt == 0 → 렌더링 건너뜀
- mapArray.Length % colCnt != 0 → rowCnt = mapArray.Length / colCnt (정수 나눗셈)
