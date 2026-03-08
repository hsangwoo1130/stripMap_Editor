# MapArray Grid 미리보기 토글 설계

**날짜:** 2026-03-08
**목표:** MapArray 수정 탭에서 새 값 입력 후 수정 버튼을 누르기 전에 2D 그리드로 미리 확인할 수 있도록 토글 기능 추가

---

## 현재 상태

- `panelMapGrid` (545, 21, 511×175): `richTextBoxGrid` 하나
- ListView 항목 선택 시 해당 행의 기존 `mapArray` 값을 2D 그리드로 표시 (`DrawGrid`)
- `textBoxMapArray`: 수정할 새 값을 입력하는 칸 (현재는 수정 전 미리보기 불가)

## 설계 결정

### UI 변경 (panelMapGrid 내부)

panelMapGrid 상단에 컨트롤 추가:

```
┌──────────────────────────────────────────────────────┐
│ [기존] [미리보기]                     [새로고침 ↺]   │  ← 추가 (y=8)
│ ──────────────────────────────────────────────────── │
│ □ 상하반전  □ 좌우반전                                │  ← 기존 (y 이동)
│                                                      │
│  (richTextBoxGrid)                                   │
└──────────────────────────────────────────────────────┘
```

**추가 컨트롤:**
- `btnGridOriginal` ("기존"): 기본 활성(강조) 상태
- `btnGridPreview` ("미리보기"): 기본 비활성 상태
- `btnRefreshGrid` ("새로고침 ↺"): textBoxMapArray 비어있으면 Disabled

**버튼 강조 표현:**
- 활성: `BackColor = CornflowerBlue`, `ForeColor = White`
- 비활성: `BackColor = SystemColors.Control`, `ForeColor = SystemColors.ControlText`

### 상태 변수

```csharp
private bool _isPreviewMode = false;
```

### 핵심 동작

| 이벤트 | 동작 |
|--------|------|
| ListView 항목 선택 | 기존 그리드 렌더, [기존] 강조 |
| textBoxMapArray 변경 | 값 있으면 `btnRefreshGrid.Enabled = true`, 비어있으면 false |
| [새로고침] 클릭 | textBoxMapArray 값으로 그리드 렌더, [미리보기] 강조 |
| [기존] 클릭 | 기존 mapArray로 그리드 재렌더, [기존] 강조 |
| [미리보기] 클릭 | textBoxMapArray가 비어있으면 무시, 아니면 새로고침과 동일 |
| 수정 후 조회 재로딩 | 기존 모드로 복귀 |

> 세부 동작은 구현 후 실물 확인하며 조정 예정

### 변경 파일

- `Forms/MainForm.Designer.cs`: 컨트롤 3개 추가, flip 체크박스 y 위치 조정
- `Forms/MainForm.cs`: 이벤트 핸들러 3개 추가, textBoxMapArray TextChanged 핸들러 추가
