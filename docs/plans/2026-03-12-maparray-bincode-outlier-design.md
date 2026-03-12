# MapArray BinCode 아웃라이어 검토 설계 문서

**작성일**: 2026-03-12
**작성자**: Claude (브레인스토밍 세션)
**상태**: 설계 확정, 구현 대기
**관련 브랜치**: feature/bincode-autosync (revert된 작업 재설계)

---

## 배경

`feature/bincode-autosync` 브랜치에서 MapArray→BinCode 자동 계산 기능을 구현했으나,
mapArray/binCode 대응 규칙이 일관적이지 않아 `ad7923b`에서 revert됨.

이번 설계는 "자동 변환은 명확한 규칙(→2=D)만 적용하고,
나머지는 수정 시 작업자가 검토·수정할 수 있도록 하는" 방향으로 재설계.

---

## 전체 흐름

```
[MapArray 텍스트박스 입력]
    ↓ 실시간
[자동 변환: 새 MapArray에서 값=2인 자릿수 → BinCode 해당 자릿수를 D로 교체]
    ↓ [확인] 버튼 클릭 (기존 [수정] 버튼 rename)
[아웃라이어 탐지]
  - 탐지 조건: BinCode='D' 이지만 새 MapArray 해당 자릿수 ≠ 2
    ↓ 아웃라이어 있으면
[아웃라이어 다이얼로그 표시 — 전체 선택 행을 한 창에]
    ↓ [수정] 클릭 (기존 [확인] 버튼 rename) 또는 [취소]
[DB 저장] 또는 [전체 중단]
    ↓ 아웃라이어 없으면
[바로 DB 저장]
```

---

## 자동 변환 규칙 (실시간, 고정)

| MapArray 자릿수 변화 | BinCode 처리 |
|---|---|
| **any → 2** | 해당 자릿수 BinCode를 `D`로 자동 교체 |
| 2 → 2 외 값 | 자동 변환 없음 (아웃라이어 탐지 대상) |
| 그 외 → 그 외 | 변환 없음 |

> **포인트**: 0→2뿐만 아니라 어떤 값에서든 2로 바뀌면 BinCode=D 자동 적용.

---

## 아웃라이어 탐지 조건

수정 버튼(확인) 클릭 시, 선택된 모든 행에 대해 자릿수 단위로 검사:

```
새 MapArray[i] ≠ 2  AND  현재 BinCode[i] = 'D'
→ 아웃라이어
```

즉, MapArray가 2가 아닌데 BinCode에 D가 남아있는 자릿수를 아웃라이어로 판단.

---

## 아웃라이어 다이얼로그 UI

```
┌────────────────────────────────────────────────────┐
│  BinCode 검토 필요 항목                             │
│  MapArray=2가 아닌 자릿수에 D가 남아있습니다.       │
├────────────────────────────────────────────────────┤
│  ABC123                                            │
│  4번째  MapArray: 0  /  BinCode: D  → [  1  ]     │
│                                                    │
│  DEF456                                            │
│  2번째  MapArray: 0  /  BinCode: D  → [  1  ]     │
│  5번째  MapArray: 0  /  BinCode: D  → [     ]     │
├────────────────────────────────────────────────────┤
│  * 텍스트박스를 비워두면 D 그대로 저장됩니다.       │
│                   [수정]   [취소]                   │
└────────────────────────────────────────────────────┘
```

- 선택된 모든 행의 아웃라이어를 한 창에 표시
- 각 항목: stripNo 헤더 + 자릿수 인덱스(1-based) + MapArray값 + BinCode값 + 편집 텍스트박스
- 텍스트박스 비워두면 → 해당 자릿수 BinCode 그대로 D 유지
- 값 입력 시 → 해당 자릿수 BinCode를 입력값으로 교체
- [수정] 클릭 → 최종 BinCode로 DB 저장
- [취소] 클릭 → 수정 전체 중단 (DB 저장 안 함)

### 아웃라이어 없을 경우
다이얼로그 생략, 즉시 DB 저장 진행.

---

## 버튼 명칭 변경

| 위치 | 기존 | 변경 후 |
|---|---|---|
| MapArray 탭 수정 버튼 | `수정` | `확인` |
| 아웃라이어 다이얼로그 적용 버튼 | `확인` | `수정` |

---

## 코드 변경 목록

| 파일 | 변경 내용 |
|---|---|
| `Forms/MainForm.cs` | `textBoxMapArray_TextChanged`: any→2 자릿수 감지 후 textBoxBinCode 해당 자릿수 D로 교체 (실시간) |
| `Forms/MainForm.cs` | `BtnUpdateMapArray_Click` → 버튼 명 `확인`으로 변경, 아웃라이어 탐지 로직 추가 |
| `Forms/MainForm.cs` | `UpdateMapArrayDataAsync`: 아웃라이어 다이얼로그 결과 반영 후 DB 저장 |
| `Forms/MainForm.Designer.cs` | `btnUpdate_MapArray` Text: `수정` → `확인` |
| `Forms/OutlierReviewDialog.cs` | 신규 — 아웃라이어 다이얼로그 Form |
| `Forms/OutlierReviewDialog.Designer.cs` | 신규 — 다이얼로그 UI 레이아웃 |

---

## 구현 시 주의사항

- 실시간 자동 변환은 `textBoxMapArray`와 `textBoxBinCode` 길이가 같을 때만 동작
- 자릿수 인덱스는 1-based로 표시 (사용자 친화적)
- `feature/bincode-autosync`의 `ComputeBinCode` 로직은 이번 설계에서 사용하지 않음
  (규칙 불일치 문제로 revert된 로직이므로)

---

## 구현 순서 (다음 세션에서 진행)

1. `MainForm.Designer.cs` — `btnUpdate_MapArray` Text `확인`으로 변경
2. `MainForm.cs` — `textBoxMapArray_TextChanged` 실시간 자동변환 로직
3. `Forms/OutlierReviewDialog.cs` + `Designer.cs` — 다이얼로그 신규 생성
4. `MainForm.cs` — `BtnUpdateMapArray_Click` 아웃라이어 탐지 및 다이얼로그 연결
5. `MainForm.cs` — `UpdateMapArrayDataAsync` 다이얼로그 결과 BinCode 반영
6. 빌드 및 동작 확인
