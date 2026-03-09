# 프로젝트 품질 개선 구현 계획

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Critical~Low 우선순위 항목들을 순차적으로 수정하여 프로젝트 안정성과 코드 품질을 개선한다.

**Architecture:** Program.cs에 전역 예외 핸들러를 추가하고, Serilog를 안정 버전으로 교체하며, SP 에러코드 처리를 중앙화하고, 미사용 패키지/폴더를 정리한다. 이 프로젝트에는 자동화 테스트가 없으므로, 각 변경 후 빌드 성공 여부로 검증한다.

**Tech Stack:** C# / .NET Framework 4.7.2 / WinForms / NuGet / MSBuild

---

## Task 1: 전역 예외 핸들러 추가 (Critical)

**Files:**
- Modify: `Program.cs:14-42`

**Step 1: Program.cs에 전역 예외 핸들러 추가**

`Main()` 메서드 시작 부분에 `Application.ThreadException`과 `AppDomain.CurrentDomain.UnhandledException` 핸들러를 등록한다. 기존 `AppLogger.Initialize()` 호출 직후에 배치한다.

```csharp
[STAThread]
static void Main()
{
    AppLogger.Initialize();

    // 전역 예외 핸들러 등록
    Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
    Application.ThreadException += Application_ThreadException;
    AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

    Application.EnableVisualStyles();
    Application.SetCompatibleTextRenderingDefault(false);

    // ... 기존 코드 그대로 ...
}

/// <summary>
/// UI 스레드 미처리 예외 핸들러
/// </summary>
private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
{
    AppLogger.Info($"[UNHANDLED_EXCEPTION] UI Thread: {e.Exception}");
    MessageBox.Show(
        $"예기치 않은 오류가 발생했습니다.\n\n{e.Exception.Message}",
        "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
}

/// <summary>
/// 비-UI 스레드 미처리 예외 핸들러
/// </summary>
private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
{
    var ex = e.ExceptionObject as Exception;
    AppLogger.Info($"[UNHANDLED_EXCEPTION] Domain: {ex?.ToString() ?? e.ExceptionObject.ToString()}");
    AppLogger.Close();

    MessageBox.Show(
        $"치명적인 오류가 발생하여 프로그램을 종료합니다.\n\n{ex?.Message ?? "알 수 없는 오류"}",
        "치명적 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
}
```

**Step 2: 빌드 확인**

Run: `powershell -Command "& 'C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe' 'd:\Workspace\SFA\StripMap\stripMap_Editor\stripMap_Editor.sln' /p:Configuration=Debug /t:Build /v:minimal"`
Expected: Build succeeded

**Step 3: 커밋**

```bash
git add Program.cs
git commit -m "feat: 전역 예외 핸들러 추가 (ThreadException + UnhandledException)"
```

---

## Task 2: Serilog 안정 버전으로 교체 (Critical)

**Files:**
- Modify: `packages.config:9` — Serilog 버전 변경
- Modify: `stripMap_Editor.csproj:78-79, 198, 203` — HintPath 및 build targets 경로 변경

현재 `Serilog 4.3.1-dev-02406` (프리릴리스)를 안정 버전 `4.2.0`으로 교체한다.
Serilog 4.x 안정 릴리스 중 .NET Framework 4.7.2 호환 최신은 `4.2.0`이다.

**Step 1: 기존 Serilog 패키지 제거 후 안정 버전 설치**

Run: `cd "d:/Workspace/SFA/StripMap/stripMap_Editor" && nuget install Serilog -Version 4.2.0 -OutputDirectory packages`

**Step 2: packages.config 수정**

```xml
<!-- 변경 전 -->
<package id="Serilog" version="4.3.1-dev-02406" targetFramework="net472" />
<!-- 변경 후 -->
<package id="Serilog" version="4.2.0" targetFramework="net472" />
```

**Step 3: stripMap_Editor.csproj 수정**

3개 위치를 변경:

```xml
<!-- Line 78-79: Reference HintPath -->
<!-- 변경 전 -->
<Reference Include="Serilog, Version=4.3.0.0, ...">
  <HintPath>packages\Serilog.4.3.1-dev-02406\lib\net471\Serilog.dll</HintPath>
</Reference>
<!-- 변경 후 -->
<Reference Include="Serilog, Version=4.2.0.0, Culture=neutral, PublicKeyToken=24c2f752a8e58a10, processorArchitecture=MSIL">
  <HintPath>packages\Serilog.4.2.0\lib\net462\Serilog.dll</HintPath>
</Reference>

<!-- Line 198: Import -->
<!-- 변경 전 -->
<Import Project="packages\Serilog.4.3.1-dev-02406\build\Serilog.targets" ... />
<!-- 변경 후 -->
<Import Project="packages\Serilog.4.2.0\build\Serilog.targets" ... />

<!-- Line 203: Error Condition -->
<!-- 변경 전 -->
<Error Condition="!Exists('packages\Serilog.4.3.1-dev-02406\build\Serilog.targets')" ... />
<!-- 변경 후 -->
<Error Condition="!Exists('packages\Serilog.4.2.0\build\Serilog.targets')" ... />
```

**Step 4: 빌드 확인**

Run: `powershell -Command "& 'C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe' 'd:\Workspace\SFA\StripMap\stripMap_Editor\stripMap_Editor.sln' /p:Configuration=Debug /t:Build /v:minimal"`
Expected: Build succeeded

**Step 5: 커밋**

```bash
git add packages.config stripMap_Editor.csproj
git commit -m "fix: Serilog 프리릴리스(4.3.1-dev) → 안정 버전(4.2.0)으로 교체"
```

---

## Task 3: DB 작업 시 UI 스레드 차단 해소 — 기반 헬퍼 구축 (High)

현재 모든 DB 호출이 UI 스레드에서 동기 실행되어 앱이 먹통이 됨.
`Task.Run` + `Invoke` 패턴으로 주요 검색/저장 작업을 비동기화한다.

**Files:**
- Modify: `Forms/MainForm.cs` — 주요 검색/저장 메서드들

**접근 방식:**
.NET Framework 4.7.2에서는 `async/await`을 사용할 수 있다. 핵심 패턴:
1. 버튼 클릭 시 UI 비활성화 + 커서 변경
2. `Task.Run()`으로 DB 작업 실행
3. 결과를 UI 스레드에서 바인딩
4. finally에서 UI 복원

**Step 1: 검색 버튼 핸들러 비동기화 — LOT ID 탭 (BtnSearch2_Click)**

이벤트 핸들러를 `async void`로 변경하고, DB 호출을 `Task.Run`으로 래핑:

```csharp
private async void BtnSearch2_Click(object sender, EventArgs e)
{
    // 기존 검색 전 초기화/유효성 코드는 그대로 유지

    btnSearch_LotId.Enabled = false;
    Cursor = Cursors.WaitCursor;
    try
    {
        // DB 조회를 백그라운드에서 실행
        DataTable result = await Task.Run(() =>
        {
            // 기존 DatabaseHelper.ExecuteQuery() 호출 코드
            return DatabaseHelper.ExecuteQuery(query, parameters.ToArray());
        });

        // UI 업데이트는 메인 스레드에서 (await 후 자동으로 메인 스레드)
        lotIdData = result;
        // ... ListView 바인딩 코드 ...
    }
    catch (Exception ex)
    {
        MessageBox.Show($"검색 중 오류: {ex.Message}", "오류",
            MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
    finally
    {
        btnSearch_LotId.Enabled = true;
        Cursor = Cursors.Default;
    }
}
```

동일 패턴을 아래 메서드들에도 적용:
- `BtnSearchMapArray_Click` (MapArray 검색)
- `BtnSearch_Click` (PCB 검색)
- `BtnSave_Click` (LOT ID 저장)
- `BtnUpdateMapArray_Click` (MapArray 수정)
- `BtnDeleteMapArray_Click` (MapArray 삭제)
- `BtnRestore_Click` (PCB 원복)
- `BtnPurgeRollback_Click` (Purge 원복)

**Step 2: AdminForm도 동일 패턴 적용**

- `LoadPurgeData()` — 검색
- `ExecutePurge()` — 삭제 실행

**Step 3: 빌드 확인**

Run: MSBuild
Expected: Build succeeded

**Step 4: 커밋**

```bash
git add Forms/MainForm.cs Forms/AdminForm.cs
git commit -m "feat: DB 작업 비동기화 (UI 스레드 차단 해소)"
```

---

## Task 4: SP 에러코드 처리 통합 (Medium)

**Files:**
- Modify: `UserRole.cs` — `SpErrorCodes` 정적 클래스 추가
- Modify: `Forms/MainForm.cs:636-652` — `GetSpErrorMessage` 제거, 공통 클래스 사용
- Modify: `Forms/AdminForm.cs:362-373` — `GetSpErrorMessage` 제거, 공통 클래스 사용

**현재 문제:**
- MainForm은 50001, 50002, 50010, 50011, 50012, 50020, 50021, 50030, 50040, 50041 처리
- AdminForm은 50001, 50002, 50012, 50040, 50041만 처리
- SP에서 실제 THROW하는 코드: 50001, 50002, 50011, 50012, 50021, 50030, 50040, 50041
- 50010, 50020은 SP에 없으나 MainForm에서 처리 중 (불필요)

**Step 1: UserRole.cs에 SpErrorCodes 클래스 추가**

```csharp
/// <summary>
/// SP THROW 에러 코드 → 사용자 메시지 매핑 (중앙화)
/// usp_StripMap_Process.sql에 정의된 에러 코드와 1:1 대응
/// </summary>
public static class SpErrorCodes
{
    public static string GetMessage(SqlException sqlex)
    {
        switch (sqlex.Number)
        {
            case 50001: return "지원하지 않는 작업 유형입니다.";
            case 50002: return "이 작업에 대한 권한이 없습니다.";
            case 50011: return "대상 TimeKey가 지정되지 않았습니다.";
            case 50012: return "히스토리에서 대상 레코드를 찾을 수 없습니다.";
            case 50021: return "Purge 원복 대상 TimeKey가 지정되지 않았습니다.";
            case 50030: return "지원하지 않는 작업 유형입니다.";
            case 50040: return "targetVersion이 지정되지 않았습니다.";
            case 50041: return "삭제 대상 행을 찾을 수 없습니다.";
            default:    return sqlex.Message;
        }
    }
}
```

상단에 `using System.Data.SqlClient;` 추가 필요.

**Step 2: MainForm.cs에서 GetSpErrorMessage 메서드를 SpErrorCodes.GetMessage로 교체**

- `GetSpErrorMessage(sqlex)` 호출부를 `SpErrorCodes.GetMessage(sqlex)`로 변경
- `private string GetSpErrorMessage(SqlException sqlex)` 메서드 삭제

**Step 3: AdminForm.cs에서 동일하게 교체**

- `GetSpErrorMessage(sqlex)` 호출부를 `SpErrorCodes.GetMessage(sqlex)`로 변경
- `private string GetSpErrorMessage(SqlException sqlex)` 메서드 삭제

**Step 4: 빌드 확인**

Run: MSBuild
Expected: Build succeeded

**Step 5: 커밋**

```bash
git add UserRole.cs Forms/MainForm.cs Forms/AdminForm.cs
git commit -m "refactor: SP 에러코드 처리를 SpErrorCodes 클래스로 중앙화"
```

---

## Task 5: 이벤트 핸들러 해제 추가 (Medium)

**Files:**
- Modify: `Forms/MainForm.cs` — `FormClosing` 이벤트에서 이벤트 핸들러 해제

**Step 1: MainForm 생성자 또는 InitializeForm에 FormClosing 이벤트 등록**

```csharp
// InitializeForm() 메서드 끝에 추가
this.FormClosing += MainForm_FormClosing;
```

**Step 2: MainForm_FormClosing 메서드 추가**

`InitializeForm()`에서 등록한 이벤트와 정확히 대응하는 해제 코드:

```csharp
private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
{
    // Lot ID 탭
    btnSearch_LotId.Click -= BtnSearch2_Click;
    btnModify_LotId.Click -= BtnModify_Click;
    btnUpdate_LotId.Click -= BtnSave_Click;
    listViewResult_LotId.ItemChecked -= ListViewResult2_ItemChecked;
    listViewResult_LotId.ColumnWidthChanging -= ListView_ColumnWidthChanging;

    // MapArray 탭
    btnSearch_MapArray.Click -= BtnSearchMapArray_Click;
    btnDelete_MapArray.Click -= BtnDeleteMapArray_Click;
    btnUpdate_MapArray.Click -= BtnUpdateMapArray_Click;
    listViewResult_MapArray.ItemChecked -= ListViewResultMapArray_ItemChecked;
    listViewResult_MapArray.ColumnWidthChanging -= ListView_ColumnWidthChanging;
    listViewResult_MapArray_BinCode.MouseDoubleClick -= ListViewResultMapArrayBinCode_MouseDoubleClick;
    listViewResult_MapArray.SelectedIndexChanged -= ListViewResultMapArray_SelectedIndexChanged;
    listViewResult_MapArray_BinCode.SelectedIndexChanged -= ListViewResultMapArrayBinCode_SelectedIndexChanged;
    checkBoxVFlip.CheckedChanged -= CheckBoxFlip_CheckedChanged;
    checkBoxHFlip.CheckedChanged -= CheckBoxFlip_CheckedChanged;

    // PCB 원복 탭
    btnSearch_PCB.Click -= BtnSearch_Click;
    btnRestore_PCB.Click -= BtnRestore_Click;
    btnPurgeRollback_PCB.Click -= BtnPurgeRollback_Click;
    btnPrevPeriod.Click -= BtnPrevPeriod_Click;
    btnNextPeriod.Click -= BtnNextPeriod_Click;
    listViewResult_PCB.ItemChecked -= ListViewResult_ItemChecked;
    listViewResult_PCB.ColumnWidthChanging -= ListView_ColumnWidthChanging;

    // 엔터 키 조회
    textBox_LOT2.KeyDown -= SearchTextBox_LotId_KeyDown;
    textBox_PCB2.KeyDown -= SearchTextBox_LotId_KeyDown;
    textBox_MGZ2.KeyDown -= SearchTextBox_LotId_KeyDown;
    textBox_PCB_MapArray.KeyDown -= SearchTextBox_MapArray_KeyDown;
    textBox_LOT.KeyDown -= SearchTextBox_PCB_KeyDown;
    textBox_PCB.KeyDown -= SearchTextBox_PCB_KeyDown;
    textBox_MGZ.KeyDown -= SearchTextBox_PCB_KeyDown;

    // ListView 복사/드래그
    listViewResult_LotId.KeyDown -= ListView_CopyKeyDown;
    listViewResult_LotId.MouseDown -= ListView_MouseDown;
    listViewResult_LotId.ItemDrag -= ListView_ItemDrag;
    listViewResult_MapArray.KeyDown -= ListView_CopyKeyDown;
    listViewResult_MapArray.MouseDown -= ListView_MouseDown;
    listViewResult_MapArray.ItemDrag -= ListView_ItemDrag;
    listViewResult_MapArray_BinCode.KeyDown -= ListView_CopyKeyDown;
    listViewResult_MapArray_BinCode.MouseDown -= ListView_MouseDown;
    listViewResult_MapArray_BinCode.ItemDrag -= ListView_ItemDrag;
    listViewResult_PCB.KeyDown -= ListView_CopyKeyDown;
    listViewResult_PCB.MouseDown -= ListView_MouseDown;
    listViewResult_PCB.ItemDrag -= ListView_ItemDrag;

    // 탭 컨트롤
    tabControl_Strip.SelectedIndexChanged -= TabControl_SelectedIndexChanged;
}
```

**Step 3: 빌드 확인**

Run: MSBuild
Expected: Build succeeded

**Step 4: 커밋**

```bash
git add Forms/MainForm.cs
git commit -m "fix: FormClosing에서 이벤트 핸들러 해제 추가 (메모리 누수 방지)"
```

---

## Task 6: MetroFramework 패키지 삭제 (Medium)

**Files:**
- Modify: `packages.config:5-8` — MetroFramework 4개 패키지 제거
- Modify: `stripMap_Editor.csproj:68-77` — MetroFramework Reference 3개 제거

**현황 확인 완료:**
- `.cs` 파일 어디에서도 `MetroFramework` using/참조 없음
- `.Designer.cs` 파일에서도 사용 없음
- 완전히 미사용 상태

**Step 1: packages.config에서 MetroFramework 제거**

아래 4줄 삭제:
```xml
<package id="MetroFramework" version="1.2.0.3" targetFramework="net472" />
<package id="MetroFramework.Design" version="1.2.0.3" targetFramework="net472" />
<package id="MetroFramework.Fonts" version="1.2.0.3" targetFramework="net472" />
<package id="MetroFramework.RunTime" version="1.2.0.3" targetFramework="net472" />
```

**Step 2: stripMap_Editor.csproj에서 MetroFramework Reference 제거**

아래 Reference 블록 3개 삭제 (lines 68-77):
```xml
<Reference Include="MetroFramework, ...">
  <HintPath>packages\MetroFramework.RunTime.1.2.0.3\...</HintPath>
</Reference>
<Reference Include="MetroFramework.Design, ...">
  <HintPath>packages\MetroFramework.Design.1.2.0.3\...</HintPath>
  <Private>False</Private>
</Reference>
<Reference Include="MetroFramework.Fonts, ...">
  <HintPath>packages\MetroFramework.Fonts.1.2.0.3\...</HintPath>
</Reference>
```

**Step 3: 빌드 확인**

Run: MSBuild
Expected: Build succeeded

**Step 4: (선택) packages 폴더에서 MetroFramework 삭제**

```bash
rm -rf packages/MetroFramework.1.2.0.3 packages/MetroFramework.Design.1.2.0.3 packages/MetroFramework.Fonts.1.2.0.3 packages/MetroFramework.RunTime.1.2.0.3
```

**Step 5: 커밋**

```bash
git add packages.config stripMap_Editor.csproj
git commit -m "chore: 미사용 MetroFramework 패키지 제거"
```

---

## Task 7: 빈 catch 블록 수정 (Medium)

**Files:**
- Modify: `Forms/MainForm.cs:671-674` — `GetLocalIPAddress()`의 빈 catch
- Modify: `Forms/AdminForm.cs:385` — `GetLocalIPAddress()`의 빈 catch

**문제:** `catch { return "127.0.0.1"; }` — 예외가 무시되어 네트워크 문제 감지 불가

**Step 1: MainForm.cs의 GetLocalIPAddress 수정**

```csharp
catch (Exception ex)
{
    AppLogger.Info($"[WARN] 로컬 IP 주소 조회 실패: {ex.Message}");
    return "127.0.0.1";
}
```

**Step 2: AdminForm.cs의 GetLocalIPAddress 수정**

```csharp
catch (Exception ex)
{
    AppLogger.Info($"[WARN] 로컬 IP 주소 조회 실패: {ex.Message}");
    return "127.0.0.1";
}
```

AdminForm.cs 상단에 `using StripMapEditor.Utils;` 추가 필요 (없는 경우).

**Step 3: 빌드 확인**

Run: MSBuild
Expected: Build succeeded

**Step 4: 커밋**

```bash
git add Forms/MainForm.cs Forms/AdminForm.cs
git commit -m "fix: GetLocalIPAddress 빈 catch 블록에 로깅 추가"
```

---

## Task 8: 미사용 폴더 및 불필요 파일 정리 (Low)

**Files:**
- Modify: `stripMap_Editor.csproj:191-193` — 빈 폴더 참조 제거
- Delete: `App.config` 내 미사용 connectionString (PmsDb) 제거 또는 App.config 정리
- Delete: `sample.txt` (프로젝트 루트, untracked)
- Delete: `Database/sp_current.txt` (untracked)
- Delete: `PCB MapArray 배열 순서.pptx` (untracked, 소스에 불필요)

**Step 1: .csproj에서 빈 폴더 참조 제거**

아래 블록 삭제 (lines 190-193):
```xml
<ItemGroup>
  <Folder Include="Business\" />
  <Folder Include="Models\" />
</ItemGroup>
```

**Step 2: App.config에서 미사용 connectionString 제거**

현재 App.config의 `PmsDb` 연결 문자열은 코드 어디에서도 참조되지 않음 (확인 완료).
connectionStrings 섹션 전체를 제거:

```xml
<!-- 삭제 대상 -->
<connectionStrings>
  <add name="PmsDb" connectionString="Server=192.168.10.79;..." />
</connectionStrings>
```

변경 후 App.config:
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <runtime>
    <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
      <dependentAssembly>
        <assemblyIdentity name="System.Runtime.CompilerServices.Unsafe" publicKeyToken="b03f5f7f11d50a3a" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-6.0.0.0" newVersion="6.0.0.0" />
      </dependentAssembly>
    </assemblyBinding>
  </runtime>
</configuration>
```

**Step 3: 불필요 파일 정리 확인**

아래 파일들은 git untracked 상태이므로 사용자 판단에 따라 삭제:
- `sample.txt` — 테스트용 샘플 파일
- `Database/sp_current.txt` — SP 텍스트 백업 (usp_StripMap_Process.sql이 이미 존재)
- `PCB MapArray 배열 순서.pptx` — 문서 파일 (별도 위치로 이동 권장)

**Step 4: 빌드 확인**

Run: MSBuild
Expected: Build succeeded

**Step 5: 커밋**

```bash
git add stripMap_Editor.csproj App.config
git commit -m "chore: 미사용 폴더 참조/connectionString 제거 및 정리"
```

---

## 실행 순서 요약

| 순서 | Task | 우선순위 | 예상 변경 파일 |
|------|------|----------|----------------|
| 1 | 전역 예외 핸들러 | Critical | Program.cs |
| 2 | Serilog 안정 버전 | Critical | packages.config, .csproj |
| 3 | DB 비동기화 | High | MainForm.cs, AdminForm.cs |
| 4 | SP 에러코드 통합 | Medium | UserRole.cs, MainForm.cs, AdminForm.cs |
| 5 | 이벤트 핸들러 해제 | Medium | MainForm.cs |
| 6 | MetroFramework 삭제 | Medium | packages.config, .csproj |
| 7 | 빈 catch 블록 수정 | Medium | MainForm.cs, AdminForm.cs |
| 8 | 미사용 폴더/파일 정리 | Low | .csproj, App.config |

**주의:** Task 3 (DB 비동기화)은 가장 변경 범위가 넓으므로 신중하게 진행. 각 메서드를 하나씩 변경하고 빌드 확인 후 다음으로 넘어간다.