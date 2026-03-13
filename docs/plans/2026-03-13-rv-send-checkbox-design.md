# LoginForm RV 메시지 전송 체크박스 설계 문서

**작성일**: 2026-03-13
**상태**: 설계 확정, 구현 대기

---

## 배경

현재 RV 시뮬레이션 모드는 `#if DEBUG` 조건부 컴파일 + `config.ini [RV] Simulation=true`로만
활성화 가능하여 Release 빌드에서 사용 불가. 라이선스 만료나 TIBCO 미설치 환경에서도
UI/DB 기능을 테스트할 수 있도록 LoginForm에 체크박스로 제어하는 방식으로 변경.

---

## 동작 설계

| 체크 상태 | 동작 |
|---|---|
| ☑ 체크 (기본값) | 현재와 동일 — Service/Daemon/Subject 필수 검증, 실제 TIBCO 연결 및 메시지 전송 |
| ☐ 미체크 | RV 전체 건너뜀 — 설정 검증 없음, 연결 없음, 전송 시 로그만 기록 (`[RV_SIM]`) |

---

## UI

LoginForm에 체크박스 추가:

```
┌─────────────────────────────┐
│  ID: [___________]          │
│  PW: [___________]          │
│                             │
│  ☑ RV 메시지 전송           │  ← 기본 Checked=true
│                             │
│         [로그인]            │
└─────────────────────────────┘
```

- 컨트롤명: `checkBox_RvSend`
- 기본값: `Checked = true`
- 위치: 비밀번호 입력란 아래, 로그인 버튼 위

---

## 코드 변경 목록

### 1. `Utils/RvManager.cs`
- `SimulationMode` 속성에서 `#if DEBUG` 래퍼 제거
- `RvInit`, `RvConnect`, `RvSend`, `RvTerminate` 내부 `#if DEBUG` + `SimulationMode` 분기 → 단순 `if (SimulationMode)` 로 변경
- Release 빌드에서도 시뮬레이션 모드 동작

### 2. `Forms/LoginForm.cs` + `LoginForm.Designer.cs`
- `checkBox_RvSend` 추가 (`Checked = true`, Text = "RV 메시지 전송")
- `DialogResult.OK` 반환 시 체크 상태를 외부에서 읽을 수 있도록 프로퍼티 노출:
  ```csharp
  public bool RvSendEnabled => checkBox_RvSend.Checked;
  ```

### 3. `Program.cs`
- `LoginForm` 로그인 성공 후 `loginForm.RvSendEnabled` 읽기
- `RvSendEnabled == false` → `rv.SimulationMode = true` 설정 후 RV 설정 검증 블록 스킵
- `RvSendEnabled == true` → 기존 로직 그대로 (Service/Daemon/Subject 검증 → RvInit → RvConnect)
- `#if DEBUG` SimulationMode 설정 블록 및 config.ini `Simulation` 읽기 제거

### 4. `Database/DatabaseHelper.cs`
- `#if DEBUG` `_iniFile.Write("RV", "Simulation", "false")` 제거

---

## 제거 대상

| 항목 | 위치 | 이유 |
|---|---|---|
| `#if DEBUG` SimulationMode 분기 전체 | `RvManager.cs` | 체크박스로 통합 |
| config.ini `[RV] Simulation` 키 읽기/쓰기 | `Program.cs`, `DatabaseHelper.cs` | 체크박스로 대체 |

---

## 구현 순서

1. `RvManager.cs` — `#if DEBUG` 제거, `SimulationMode` 단순 프로퍼티로 변경
2. `LoginForm.Designer.cs` — `checkBox_RvSend` 컨트롤 추가
3. `LoginForm.cs` — `RvSendEnabled` 프로퍼티 추가
4. `Program.cs` — 로그인 후 체크 상태 읽어 SimulationMode 설정, DEBUG 분기 제거
5. `DatabaseHelper.cs` — `#if DEBUG` Simulation 쓰기 제거
6. 빌드 및 동작 확인 (체크/미체크 양쪽)
