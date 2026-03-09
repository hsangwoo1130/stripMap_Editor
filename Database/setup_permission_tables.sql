-- =====================================================
-- StripMap Editor - 권한/메뉴 테이블 생성 및 초기 데이터
-- 적용 대상: 기존 SFA DB에 없는 테이블만 생성
-- 실행 전제: tblRole에 'USER', 'ADMIN', 'SUPER' roleId 존재
-- =====================================================

USE [SFA_TEST_DB]  -- ← 운영 DB명으로 변경 후 실행
GO

-- =====================================================
-- 1. tblFunction — 앱 기능(권한) 목록
-- =====================================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblFunction')
BEGIN
    CREATE TABLE dbo.tblFunction (
        functionId  VARCHAR(50)  NOT NULL,
        isActive    BIT          NOT NULL DEFAULT 1,
        CONSTRAINT PK_tblFunction PRIMARY KEY (functionId)
    );
    PRINT 'tblFunction 생성 완료';
END
ELSE PRINT 'tblFunction 이미 존재 — 건너뜀';
GO

-- =====================================================
-- 2. tblMenu — 앱 메뉴(탭) 목록
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

-- =====================================================
-- 3. tblRoleFunction — 역할별 기능 권한 매핑
-- =====================================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblRoleFunction')
BEGIN
    CREATE TABLE dbo.tblRoleFunction (
        roleId      VARCHAR(50) NOT NULL,
        functionId  VARCHAR(50) NOT NULL,
        CONSTRAINT PK_tblRoleFunction PRIMARY KEY (roleId, functionId)
    );
    PRINT 'tblRoleFunction 생성 완료';
END
ELSE PRINT 'tblRoleFunction 이미 존재 — 건너뜀';
GO

-- =====================================================
-- 4. tblRoleMenu — 역할별 메뉴 접근 권한 매핑
-- =====================================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblRoleMenu')
BEGIN
    CREATE TABLE dbo.tblRoleMenu (
        roleId   VARCHAR(50) NOT NULL,
        menuId   VARCHAR(50) NOT NULL,
        canView  BIT         NOT NULL DEFAULT 1,
        CONSTRAINT PK_tblRoleMenu PRIMARY KEY (roleId, menuId)
    );
    PRINT 'tblRoleMenu 생성 완료';
END
ELSE PRINT 'tblRoleMenu 이미 존재 — 건너뜀';
GO

-- =====================================================
-- 5. 초기 데이터 — tblFunction
--    STRIP_DELETE         : MapArray 논리 삭제 권한
--    STRIP_ROLLBACK       : PCB 원복 권한
--    STRIP_PURGE_ROLLBACK : Purge 복원 권한
-- =====================================================
MERGE dbo.tblFunction AS T
USING (VALUES
    ('STRIP_DELETE',         1),
    ('STRIP_ROLLBACK',       1),
    ('STRIP_PURGE_ROLLBACK', 1)
) AS S (functionId, isActive)
ON T.functionId = S.functionId
WHEN NOT MATCHED THEN INSERT (functionId, isActive) VALUES (S.functionId, S.isActive);
PRINT 'tblFunction 초기 데이터 적용 완료';
GO

-- =====================================================
-- 6. 초기 데이터 — tblMenu
--    STRIP_EDIT : LOT ID 수정 탭
--    MAP_EDIT   : MapArray 수정 탭
--    STRIP_HIST : PCB 원복 탭
--    PURGE      : 관리자(Purge) 탭
-- =====================================================
MERGE dbo.tblMenu AS T
USING (VALUES
    ('STRIP_EDIT', N'LOT ID 수정',  'lotidedit',    1),
    ('MAP_EDIT',   N'MapArray 수정', 'mapedit',      1),
    ('STRIP_HIST', N'PCB 원복',      'striphistory', 1),
    ('PURGE',      N'관리자',        'purge',        1)
) AS S (menuId, menuName, menuUrl, isActive)
ON T.menuId = S.menuId
WHEN NOT MATCHED THEN INSERT (menuId, menuName, menuUrl, isActive) VALUES (S.menuId, S.menuName, S.menuUrl, S.isActive)
WHEN MATCHED THEN UPDATE SET menuName = S.menuName, menuUrl = S.menuUrl, isActive = S.isActive;
PRINT 'tblMenu 초기 데이터 적용 완료';
GO

-- =====================================================
-- 7. 초기 데이터 — tblRoleFunction (역할별 기능 권한)
--
--    USER  : PCB 원복만 가능
--    ADMIN : 논리 삭제 + PCB 원복 + Purge 복원
--    SUPER : ADMIN 권한 상속 (코드에서 자동 처리, DB 별도 불필요)
--            단, 명시적으로 동일하게 등록
-- =====================================================
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
-- 8. 초기 데이터 — tblRoleMenu (역할별 메뉴 접근)
--
--    USER  : LOT ID 수정, MapArray 수정, PCB 원복
--    ADMIN : USER 탭 전체 + 관리자(Purge) 탭
--    SUPER : ADMIN과 동일
-- =====================================================
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
WHEN MATCHED THEN UPDATE SET canView = S.canView;
PRINT 'tblRoleMenu 초기 데이터 적용 완료';
GO

-- =====================================================
-- 확인 쿼리
-- =====================================================
SELECT 'tblFunction' AS tbl, functionId, CAST(isActive AS INT) AS isActive FROM dbo.tblFunction
UNION ALL
SELECT 'tblMenu', menuId, CAST(isActive AS INT) FROM dbo.tblMenu
ORDER BY tbl, functionId;

SELECT roleId, functionId FROM dbo.tblRoleFunction ORDER BY roleId, functionId;
SELECT roleId, menuId, CAST(canView AS INT) AS canView FROM dbo.tblRoleMenu ORDER BY roleId, menuId;
