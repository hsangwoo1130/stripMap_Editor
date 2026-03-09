-- =====================================================
-- StripMap Editor - 전체 DB 셋업 스크립트
-- 대상: 계약사 신규 환경 구성용
-- 실행 순서: 이 파일 → rename_spare_columns.sql → usp_StripMap_Process.sql
-- =====================================================

USE [SFA_TEST_DB]  -- ← 운영 DB명으로 변경 후 실행
GO

-- =====================================================
-- 1. tblRole — 역할 정의
--    USER < ADMIN < SUPER (SUPER는 코드에서 ADMIN 권한 상속)
-- =====================================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblRole')
BEGIN
    CREATE TABLE dbo.tblRole (
        roleId  VARCHAR(20) NOT NULL,
        CONSTRAINT PK_tblRole PRIMARY KEY (roleId)
    );
    PRINT 'tblRole 생성 완료';
END
ELSE PRINT 'tblRole 이미 존재 — 건너뜀';
GO

MERGE dbo.tblRole AS T
USING (VALUES ('USER'), ('ADMIN'), ('SUPER')) AS S (roleId)
ON T.roleId = S.roleId
WHEN NOT MATCHED THEN INSERT (roleId) VALUES (S.roleId);
PRINT 'tblRole 초기 데이터 적용 완료';
GO

-- =====================================================
-- 2. tblUser — 사용자 계정
--    passwordHash 형식: {salt_base64}:{hash_base64}
--    (Argon2id: parallelism=2, memory=64MB, iterations=3, 256-bit hash)
--    → 앱 로그인 후 비밀번호 변경 또는 별도 hash 생성 도구 사용
-- =====================================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblUser')
BEGIN
    CREATE TABLE dbo.tblUser (
        userId        VARCHAR(20)   NOT NULL,
        userName      NVARCHAR(50)  NOT NULL,
        passwordHash  VARCHAR(200)  NOT NULL,
        isActive      BIT           NOT NULL DEFAULT 1,
        CONSTRAINT PK_tblUser PRIMARY KEY (userId)
    );
    PRINT 'tblUser 생성 완료';
END
ELSE PRINT 'tblUser 이미 존재 — 건너뜀';
GO

-- ※ 초기 계정: admin / Admin1234!
--   아래 해시값은 위 비밀번호의 Argon2id 해시입니다.
--   운영 환경에서는 반드시 비밀번호를 변경하세요.
IF NOT EXISTS (SELECT 1 FROM dbo.tblUser WHERE userId = 'admin')
BEGIN
    INSERT INTO dbo.tblUser (userId, userName, passwordHash, isActive)
    VALUES (
        'admin',
        N'관리자',
        'bXlTYWx0U2FtcGxlMTY=:AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=',
        -- ↑ 위 해시는 플레이스홀더입니다.
        --   실제 사용을 위해서는 아래 방법으로 해시를 교체하세요:
        --   1. 앱을 Debug 빌드로 실행
        --   2. LoginForm.cs의 HashPassword("원하는비밀번호") 호출 결과값을 이 컬럼에 UPDATE
        1
    );
    PRINT '초기 admin 계정 삽입 — 비밀번호 해시 교체 필요';
END
GO

-- =====================================================
-- 3. tblUserRole — 사용자-역할 매핑
-- =====================================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblUserRole')
BEGIN
    CREATE TABLE dbo.tblUserRole (
        userId  VARCHAR(20) NOT NULL,
        roleId  VARCHAR(20) NOT NULL,
        CONSTRAINT PK_tblUserRole PRIMARY KEY (userId, roleId),
        CONSTRAINT FK_tblUserRole_User FOREIGN KEY (userId) REFERENCES dbo.tblUser(userId),
        CONSTRAINT FK_tblUserRole_Role FOREIGN KEY (roleId) REFERENCES dbo.tblRole(roleId)
    );
    PRINT 'tblUserRole 생성 완료';
END
ELSE PRINT 'tblUserRole 이미 존재 — 건너뜀';
GO

IF NOT EXISTS (SELECT 1 FROM dbo.tblUserRole WHERE userId = 'admin' AND roleId = 'SUPER')
    INSERT INTO dbo.tblUserRole (userId, roleId) VALUES ('admin', 'SUPER');
PRINT 'tblUserRole 초기 데이터 적용 완료';
GO

-- =====================================================
-- 4. tblFunction — 앱 기능(버튼 권한) 목록
-- =====================================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblFunction')
BEGIN
    CREATE TABLE dbo.tblFunction (
        functionId  VARCHAR(50) NOT NULL,
        isActive    BIT         NOT NULL DEFAULT 1,
        CONSTRAINT PK_tblFunction PRIMARY KEY (functionId)
    );
    PRINT 'tblFunction 생성 완료';
END
ELSE PRINT 'tblFunction 이미 존재 — 건너뜀';
GO

MERGE dbo.tblFunction AS T
USING (VALUES
    ('STRIP_DELETE',         1),  -- MapArray 논리 삭제
    ('STRIP_ROLLBACK',       1),  -- PCB 원복
    ('STRIP_PURGE_ROLLBACK', 1)   -- Purge 복원
) AS S (functionId, isActive)
ON T.functionId = S.functionId
WHEN NOT MATCHED THEN INSERT (functionId, isActive) VALUES (S.functionId, S.isActive)
WHEN MATCHED    THEN UPDATE SET isActive = S.isActive;
PRINT 'tblFunction 초기 데이터 적용 완료';
GO

-- =====================================================
-- 5. tblMenu — 앱 탭(메뉴) 목록
-- =====================================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblMenu')
BEGIN
    CREATE TABLE dbo.tblMenu (
        menuId    VARCHAR(50)   NOT NULL,
        menuName  NVARCHAR(100) NOT NULL,
        menuUrl   VARCHAR(100)  NULL,
        isActive  BIT           NOT NULL DEFAULT 1,
        CONSTRAINT PK_tblMenu PRIMARY KEY (menuId)
    );
    PRINT 'tblMenu 생성 완료';
END
ELSE PRINT 'tblMenu 이미 존재 — 건너뜀';
GO

MERGE dbo.tblMenu AS T
USING (VALUES
    ('STRIP_EDIT', N'LOT ID 수정',   'lotidedit',    1),
    ('MAP_EDIT',   N'MapArray 수정',  'mapedit',      1),
    ('STRIP_HIST', N'PCB 원복',       'striphistory', 1),
    ('PURGE',      N'관리자',         'purge',        1)
) AS S (menuId, menuName, menuUrl, isActive)
ON T.menuId = S.menuId
WHEN NOT MATCHED THEN INSERT (menuId, menuName, menuUrl, isActive) VALUES (S.menuId, S.menuName, S.menuUrl, S.isActive)
WHEN MATCHED    THEN UPDATE SET menuName = S.menuName, menuUrl = S.menuUrl, isActive = S.isActive;
PRINT 'tblMenu 초기 데이터 적용 완료';
GO

-- =====================================================
-- 6. tblRoleFunction — 역할별 기능 권한 매핑
--    USER  : PCB 원복
--    ADMIN : 논리 삭제 + PCB 원복 + Purge 복원
--    SUPER : ADMIN과 동일 (코드에서 SUPER→ADMIN 상속하지만 명시 등록)
-- =====================================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblRoleFunction')
BEGIN
    CREATE TABLE dbo.tblRoleFunction (
        roleId      VARCHAR(20) NOT NULL,
        functionId  VARCHAR(50) NOT NULL,
        CONSTRAINT PK_tblRoleFunction PRIMARY KEY (roleId, functionId),
        CONSTRAINT FK_tblRoleFunction_Role     FOREIGN KEY (roleId)     REFERENCES dbo.tblRole(roleId),
        CONSTRAINT FK_tblRoleFunction_Function FOREIGN KEY (functionId) REFERENCES dbo.tblFunction(functionId)
    );
    PRINT 'tblRoleFunction 생성 완료';
END
ELSE PRINT 'tblRoleFunction 이미 존재 — 건너뜀';
GO

MERGE dbo.tblRoleFunction AS T
USING (VALUES
    ('USER',  'STRIP_ROLLBACK'),
    ('ADMIN', 'STRIP_DELETE'),
    ('ADMIN', 'STRIP_ROLLBACK'),
    ('ADMIN', 'STRIP_PURGE_ROLLBACK'),
    ('SUPER', 'STRIP_DELETE'),
    ('SUPER', 'STRIP_ROLLBACK'),
    ('SUPER', 'STRIP_PURGE_ROLLBACK')
) AS S (roleId, functionId)
ON T.roleId = S.roleId AND T.functionId = S.functionId
WHEN NOT MATCHED THEN INSERT (roleId, functionId) VALUES (S.roleId, S.functionId);
PRINT 'tblRoleFunction 초기 데이터 적용 완료';
GO

-- =====================================================
-- 7. tblRoleMenu — 역할별 메뉴 접근 권한
--    USER  : LOT ID 수정, MapArray 수정, PCB 원복
--    ADMIN : USER 전체 + 관리자(Purge) 탭
--    SUPER : ADMIN과 동일
-- =====================================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblRoleMenu')
BEGIN
    CREATE TABLE dbo.tblRoleMenu (
        roleId   VARCHAR(20) NOT NULL,
        menuId   VARCHAR(50) NOT NULL,
        canView  BIT         NOT NULL DEFAULT 1,
        CONSTRAINT PK_tblRoleMenu PRIMARY KEY (roleId, menuId),
        CONSTRAINT FK_tblRoleMenu_Role FOREIGN KEY (roleId) REFERENCES dbo.tblRole(roleId),
        CONSTRAINT FK_tblRoleMenu_Menu FOREIGN KEY (menuId) REFERENCES dbo.tblMenu(menuId)
    );
    PRINT 'tblRoleMenu 생성 완료';
END
ELSE PRINT 'tblRoleMenu 이미 존재 — 건너뜀';
GO

MERGE dbo.tblRoleMenu AS T
USING (VALUES
    ('USER',  'STRIP_EDIT', 1),
    ('USER',  'MAP_EDIT',   1),
    ('USER',  'STRIP_HIST', 1),
    ('ADMIN', 'STRIP_EDIT', 1),
    ('ADMIN', 'MAP_EDIT',   1),
    ('ADMIN', 'STRIP_HIST', 1),
    ('ADMIN', 'PURGE',      1),
    ('SUPER', 'STRIP_EDIT', 1),
    ('SUPER', 'MAP_EDIT',   1),
    ('SUPER', 'STRIP_HIST', 1),
    ('SUPER', 'PURGE',      1)
) AS S (roleId, menuId, canView)
ON T.roleId = S.roleId AND T.menuId = S.menuId
WHEN NOT MATCHED THEN INSERT (roleId, menuId, canView) VALUES (S.roleId, S.menuId, S.canView)
WHEN MATCHED    THEN UPDATE SET canView = S.canView;
PRINT 'tblRoleMenu 초기 데이터 적용 완료';
GO

-- =====================================================
-- 확인 쿼리
-- =====================================================
PRINT '=== 생성된 테이블 목록 ===';
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME IN ('tblRole','tblUser','tblUserRole','tblFunction','tblMenu','tblRoleFunction','tblRoleMenu')
ORDER BY TABLE_NAME;

PRINT '=== 사용자 목록 ===';
SELECT u.userId, u.userName, r.roleId, u.isActive
FROM dbo.tblUser u
JOIN dbo.tblUserRole ur ON u.userId = ur.userId
JOIN dbo.tblRole r ON ur.roleId = r.roleId;

PRINT '=== 역할별 메뉴 권한 ===';
SELECT roleId, menuId, canView FROM dbo.tblRoleMenu ORDER BY roleId, menuId;

PRINT '=== 역할별 기능 권한 ===';
SELECT roleId, functionId FROM dbo.tblRoleFunction ORDER BY roleId, functionId;
