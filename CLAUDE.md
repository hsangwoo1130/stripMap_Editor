# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

StripMap Editor is a WinForms desktop application for managing Strip Map data in a semiconductor factory automation (SFA) system. Core operations: LOT ID 변경, MapArray 수정/논리 삭제, PCB 2D ID 원복/물리 삭제.

## Build & Run

**IDE**: Visual Studio 2019 (target: .NET Framework 4.7.2, WinExe)

```bash
# Restore NuGet packages (run before first build)
nuget restore stripMap_Editor.sln

# Build via MSBuild (Debug)
msbuild stripMap_Editor.sln /p:Configuration=Debug

# Build (Release)
msbuild stripMap_Editor.sln /p:Configuration=Release
# Output: bin\Release\stripMap_Editor.exe
```

No automated test project exists in this repository.

## Configuration

`config.ini` (next to the executable) holds all DB connection settings and is loaded at startup by `DatabaseHelper` (static constructor). Edit before running:

```ini
[Database]
Server=192.168.10.79
Database=SFA_TEST_DB
UserId=sfa_test_login
Password=sfa_test_login
Timeout=5
Encrypt=false
```

If `config.ini` is missing, `DatabaseHelper` auto-creates it with the defaults above.

## Architecture

### Application Flow

```
Program.Main()
  └─ AppLogger.Initialize()          // Serilog to logs/stripmap_YYYYMMDD.txt
  └─ LoginForm (modal dialog)        // Argon2id password verification against DB
       └─ DialogResult.OK
            └─ MainForm              // Main tabbed UI
                 └─ AdminForm        // Opened as embedded tab (ADMIN/SUPER role)
```

### Layer Structure

| Layer | Location | Responsibility |
|-------|----------|----------------|
| Forms | `Forms/` | UI logic — `LoginForm`, `MainForm`, `AdminForm` |
| Database | `Database/DatabaseHelper.cs` | All SQL/SP execution; static class |
| Utils | `Utils/` | `AppLogger` (Serilog wrapper), `IniFileHelper` (INI R/W) |
| Domain | `UserRole.cs` | `UserRole` enum, permission/menu/action constants |

### Permission System

Roles: `USER` < `ADMIN` < `SUPER` (SUPER inherits ADMIN permissions).

Permissions and menus are loaded from the DB at login into two `HashSet<string>` fields on `MainForm`:
- `_userPermissions` — from `tblRoleFunction` → guards buttons/actions
- `_userMenus` — from `tblRoleMenu` → controls which `TabPage` instances are added to `tabControl_Strip`

Constants are defined in `UserRole.cs`:
- `UserPermissions.*` — function IDs matching `tblRoleFunction`
- `MenuIds.*` — menu IDs matching `tblMenu` (STRIP_EDIT, MAP_EDIT, STRIP_HIST, ADMIN)
- `ActionTypes.*` — audit log action type strings

### DatabaseHelper

Static class; connection string is built once in the static constructor from `config.ini`. Key methods:

- `ExecuteQuery` — SELECT → `DataTable`
- `ExecuteNonQuery` — INSERT/UPDATE/DELETE → rows affected
- `ExecuteStoredProcedure` — SP → `DataTable`
- `ExecuteStoredProcedureNonQuery` — SP without result set; re-throws `SqlException` raw so callers can branch on `ex.Number` (SP THROW codes 50001–50030)
- `ExecuteTransaction` — wraps an `Action<SqlConnection, SqlTransaction>` in a transaction

All DB operations go directly through `DatabaseHelper`; the `Business/` and `Models/` folders are currently empty.

### Logging

`AppLogger` is a thin static wrapper around Serilog. Log files: `<exe dir>\logs\stripmap_YYYYMMDD.txt`, daily rolling, 90-day retention. All audit events use `AppLogger.Info()` with structured tags like `[LOGIN_SUCCESS]`, `[APP_EXIT]`, `[APP_START]`.

### Key NuGet Packages

- **MetroFramework 1.2.0.3** — Metro-style WinForms controls used throughout the UI
- **Konscious.Security.Cryptography.Argon2 1.3.1** — Argon2id password hashing (params: parallelism=2, memory=64MB, iterations=3, hash=256-bit); stored as `{salt_base64}:{hash_base64}`
- **Serilog + Serilog.Sinks.File** — structured logging
- **System.Data.SqlClient 4.9.0** — SQL Server connectivity

## SP 적용 방법

SP 파일 위치: `Database/usp_StripMap_Process.sql` (인코딩: **UTF-8 with BOM**)

```bash
# sqlcmd으로 SP 적용 (BOM이 있으면 한글 주석 정상 처리됨)
sqlcmd -S 192.168.10.79 -d SFA_TEST_DB -U sfa_test_login -P sfa_test_login -i Database\usp_StripMap_Process.sql
```

> **주의**: SQL 파일을 편집 후 저장할 때 반드시 **UTF-8 with BOM**으로 저장해야 합니다.
> UTF-8 without BOM으로 저장하면 sqlcmd가 CP949로 읽어 한글 주석이 깨져 DB에 저장됩니다.
> - Visual Studio Code: 우하단 인코딩 클릭 → "Save with Encoding" → `UTF-8 with BOM` 선택
> - Visual Studio: 파일 → 다른 이름으로 저장 → 저장 버튼 드롭다운 → "인코딩하여 저장" → `Unicode (UTF-8 서명 있음)`

## DB Schema Notes (inferred from queries)

- `tblUser` — userId, userName, passwordHash, isActive
- `tblUserRole` — userId, roleId
- `tblRole` — roleId
- `tblRoleFunction` — roleId, functionId
- `tblRoleMenu` — roleId, menuId, canView
- `tblFunction` — functionId, isActive
- `tblMenu` — menuId, isActive
- `tblStripMap` — stripNo, version, process, lotNo, mgzRf, active
