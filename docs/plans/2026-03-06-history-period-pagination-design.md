# PCB 원복 탭 기간 페이지네이션 설계

## 개요

PCB 원복 탭(tblStripMapHistory 조회)에서 이력이 누적될수록 전체 조회 성능이 저하되는 문제를 방지하기 위해,
조회 기간을 한 달 단위로 나누어 이전/다음 버튼으로 탐색할 수 있도록 한다.

---

## 기간 계산 규칙

오늘 날짜의 day를 기준으로 정확히 한 달 단위로 기간을 구분한다 (calendar month 아님).

```
offset=0 (현재):
  endDate   = DateTime.Today
  startDate = DateTime.Today.AddMonths(-1).AddDays(1)

  예: 오늘=2026-03-06 → 2026-02-07 ~ 2026-03-06

offset=n (n개월 전):
  endDate   = DateTime.Today.AddMonths(-n)
  startDate = DateTime.Today.AddMonths(-n-1).AddDays(1)

  예: n=1 → 2026-01-07 ~ 2026-02-06
      n=2 → 2025-12-07 ~ 2026-01-06
```

---

## DB 조회 방식

기간별 DB 재조회 방식 사용. 이전/다음 버튼 클릭 시 해당 기간의 데이터를 DB에서 새로 SELECT한다.

timekey 컬럼은 yyyyMMddHHmmssffffff(20자리 문자열)이므로 날짜 8자리를 문자열 비교로 필터링한다:

```sql
AND LEFT(timekey, 8) >= @StartDate   -- 예: '20260207'
AND LEFT(timekey, 8) <= @EndDate     -- 예: '20260306'
```

파라미터 타입: `SqlParameter("@StartDate", startDate.ToString("yyyyMMdd"))`

---

## C# 상태 관리

```csharp
private int _periodOffset = 0;  // 0=현재 기간, 1=1개월 전, 2=2개월 전, ...
```

### 기간 계산 헬퍼

```csharp
private (DateTime start, DateTime end) GetPeriodRange(int offset)
{
    DateTime end   = DateTime.Today.AddMonths(-offset);
    DateTime start = DateTime.Today.AddMonths(-offset - 1).AddDays(1);
    return (start, end);
}
```

---

## UI 변경

### 신규 컨트롤 (Designer.cs)

`tabPagePcbRestore` 내 `labelResultTitle` 영역을 재배치:

```
[◀ 이전]  2026-02-07 ~ 2026-03-06  [다음 ▶]   조회 결과 (142건)
```

| 컨트롤명 | 타입 | 속성 |
|---------|------|------|
| `btnPrevPeriod` | Button | Text="◀ 이전", 항상 활성 |
| `labelPeriod` | Label | 현재 기간 텍스트, 중앙 정렬 |
| `btnNextPeriod` | Button | Text="다음 ▶", offset=0이면 Enabled=false |

배치: `labelResultTitle` 위쪽 또는 같은 행에 좌측부터 배치.

### 버튼 동작

| 액션 | 동작 |
|------|------|
| 조회 버튼 클릭 | `_periodOffset = 0` 초기화 → `LoadHistoryData()` |
| ◀ 이전 클릭 | `_periodOffset++` → `LoadHistoryData()` |
| 다음 ▶ 클릭 | `_periodOffset--` → `LoadHistoryData()` |

`btnNextPeriod.Enabled = (_periodOffset > 0)` — LoadHistoryData 완료 후 갱신.

### labelPeriod 텍스트

```csharp
labelPeriod.Text = $"{start:yyyy-MM-dd} ~ {end:yyyy-MM-dd}";
```

### labelResultTitle 갱신

```csharp
labelResultTitle.Text = $"조회 결과 ({dt.Rows.Count}건)";
```

---

## 데이터 흐름

```
조회 버튼 클릭 / 이전,다음 버튼 클릭
  ↓
_periodOffset 갱신
  ↓
GetPeriodRange(offset) → (startDate, endDate)
  ↓
LoadHistoryData(lotNo, stripNo, mgzRf, startDate, endDate)
  ↓
SQL: WHERE ... AND LEFT(timekey,8) >= @StartDate AND LEFT(timekey,8) <= @EndDate
  ↓
DisplayHistoryData(dt)
labelPeriod.Text 갱신
labelResultTitle.Text 갱신
btnNextPeriod.Enabled 갱신
```

---

## 예외 처리

- 조회 결과 0건: 기존처럼 MessageBox로 안내, 기간 네비게이션은 유지
- offset=0일 때 다음 버튼 비활성 → 미래 기간으로 이동 불가
- 이전 버튼은 제한 없이 항상 활성
