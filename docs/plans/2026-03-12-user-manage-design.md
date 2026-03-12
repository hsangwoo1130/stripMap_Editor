# 사용자 관리 탭 설계 문서

**작성일**: 2026-03-12
**작성자**: Claude (브레인스토밍 세션)
**상태**: 설계 확정, 구현 대기

---

## 개요

SUPER 권한 전용 사용자 관리 팝업(`UserManageForm`)을 추가한다.
MainForm 탭에 `⚙ 사용자 관리` 탭을 추가하고, 클릭 시 팝업 형식으로 띄운다.
기존 `AdminForm` 팝업 방식과 동일한 구조.

---

## UI 레이아웃

### UserManageForm 팝업

```
┌─────────────────────────────────────────────┐
│  [사용자 등록]                               │
│  ID: [______] 이름: [______]                │
│  PW: [______] 권한: [USER▼]     [등록]      │
├─────────────────────────────────────────────┤
│  [사용자 목록]                               │
│   ID      이름    권한   활성  생성일         │
│ ▶ kim    김철수  ADMIN   1   2026-03-12      │
│   hong   홍길동  USER    0   2026-03-01      │
├─────────────────────────────────────────────┤
│  선택된 사용자: kim (김철수)                 │
│  권한: [ADMIN▼]   활성: [1▼]    [수정]      │
└─────────────────────────────────────────────┘
```

- **상단 등록 패널**: userId / userName / password / 권한 ComboBox(USER·ADMIN·SUPER) / [등록] 버튼
- **중단 ListView**: ID, 이름, 권한, 활성, 생성일 (5컬럼), 파란 헤더 (AdminForm 스타일)
- **하단 편집 패널**: 행 클릭 시 현재 값 자동 채워짐, 권한 ComboBox + 활성 ComboBox(1·0) + [수정] 버튼
  - 미선택 상태에서는 하단 패널 비활성화

---

## DB 작업

### 1. tblMenu INSERT

```sql
INSERT INTO tblMenu (menuId, menuName, parentId, menuUrl, sortOrder, isActive)
VALUES ('USER_MANAGE', '사용자 관리', NULL, 'usermanage', 11, 1);
```

### 2. tblRoleMenu INSERT — SUPER 전용

```sql
-- canEdit 컬럼 존재 시:
INSERT INTO tblRoleMenu (roleId, menuId, canView, canEdit)
VALUES ('SUPER', 'USER_MANAGE', 1, 1);

-- canEdit 컬럼 없는 경우:
-- INSERT INTO tblRoleMenu (roleId, menuId, canView)
-- VALUES ('SUPER', 'USER_MANAGE', 1);
```

> **확인 필요**: DB에 `tblRoleMenu.canEdit` 컬럼 존재 여부 사전 확인 후 적용

### 3. 사용자 등록 (C# 직접 쿼리)

```sql
-- tblUser INSERT
INSERT INTO tblUser (userId, userName, passwordHash, isActive, createdTime, createdBy)
VALUES (@userId, @userName, @passwordHash, 1, GETDATE(), @createdBy);

-- tblUserRole INSERT
INSERT INTO tblUserRole (userId, roleId)
VALUES (@userId, @roleId);
```

- `passwordHash`: `LoginForm.CreatePasswordHash(password)` 재사용 (Argon2id)
- `createdBy`: 현재 로그인한 사용자의 `userName`

### 4. 사용자 수정 (C# 직접 쿼리)

```sql
-- tblUser UPDATE (isActive, updatedTime, updatedBy)
UPDATE tblUser
SET isActive    = @isActive,
    updatedTime = GETDATE(),
    updatedBy   = @updatedBy
WHERE userId = @userId;

-- tblUserRole UPDATE (roleId 변경)
UPDATE tblUserRole
SET roleId = @roleId
WHERE userId = @userId;
```

- `updatedBy`: 현재 로그인한 사용자의 `userName`

### 5. isActive=0 → 탭 비노출

기존 `LoadRoleMenus`가 `AND m.isActive = 1` 조건으로 필터링 중.
`tblMenu.isActive = 0`으로 변경하면 추가 코드 없이 자동으로 탭 비노출.

---

## 코드 변경 목록

| 파일 | 변경 내용 |
|------|-----------|
| `UserRole.cs` | `MenuIds.USER_MANAGE = "USER_MANAGE"` 상수 추가 |
| `Forms/MainForm.cs` | `SetupUserManageTab()` 추가, `ApplyUserPermissions()`에 USER_MANAGE 탭 등록, `TabControl_SelectedIndexChanged`에서 클릭 시 `UserManageForm` 팝업 |
| `Forms/UserManageForm.cs` | 신규 — 등록/수정 로직 |
| `Forms/UserManageForm.Designer.cs` | 신규 — UI 레이아웃 |

---

## 권한 체계

- 접근 권한: `SUPER` 전용
- `_userMenus.Contains(MenuIds.USER_MANAGE)` 조건으로 탭 표시 여부 결정
- `LoggedInUserRole == "SUPER"` 이중 체크 (팝업 열 때 한 번 더 검증)

---

## Audit 로그

| 태그 | 시점 |
|------|------|
| `[USER_CREATE]` | 사용자 등록 성공 시 |
| `[USER_UPDATE]` | 권한 또는 활성 변경 성공 시 |

---

## 참고 — isActive 로그인 체크 현황

`LoginForm.ValidateLogin()`의 쿼리에 `AND u.isActive = 1` 조건 이미 포함.
비활성 계정은 로그인 불가 — 추가 변경 불필요.

---

## 구현 순서 (다음 세션에서 진행)

1. DB 스크립트 적용 (tblMenu, tblRoleMenu INSERT)
2. `UserRole.cs` — `MenuIds.USER_MANAGE` 상수 추가
3. `UserManageForm.Designer.cs` — UI 레이아웃 구성
4. `UserManageForm.cs` — 등록/수정 로직 구현
5. `MainForm.cs` — 탭 추가 및 팝업 연결
6. 빌드 및 동작 확인
