-- =====================================================
-- StripMap Editor - 전체 DB 셋업 스크립트
-- 대상: 계약사 신규 환경 구성용
-- 실행 순서: 1) 이 파일 실행 → 2) usp_StripMap_Process.sql 실행
-- 주의: USE 구문의 DB명을 운영 환경에 맞게 변경 후 실행
-- =====================================================

USE [SFA_TEST_DB]  -- ← 운영 DB명으로 변경 후 실행
GO

-- =====================================================
-- 1. tblRole — 역할 정의 (USER < ADMIN < SUPER)
-- =====================================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblRole')
BEGIN
    CREATE TABLE dbo.tblRole (
        roleId   VARCHAR(20) NOT NULL,
        roleName VARCHAR(50) NULL,
        CONSTRAINT PK_tblRole PRIMARY KEY (roleId)
    );
    PRINT 'tblRole 생성 완료';
END
ELSE PRINT 'tblRole 이미 존재 — 건너뜀';
GO

MERGE dbo.tblRole AS T
USING (VALUES
    ('USER',  N'일반 사용자'),
    ('ADMIN', N'관리자'),
    ('SUPER', N'슈퍼 관리자')
) AS S (roleId, roleName)
ON T.roleId = S.roleId
WHEN NOT MATCHED THEN INSERT (roleId, roleName) VALUES (S.roleId, S.roleName);
PRINT 'tblRole 초기 데이터 적용 완료';
GO

-- =====================================================
-- 2. tblUser — 사용자 계정
--    passwordHash 형식: {salt_base64}:{hash_base64}
--    (Argon2id: parallelism=2, memory=64MB, iterations=3, 256-bit hash)
-- =====================================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblUser')
BEGIN
    CREATE TABLE dbo.tblUser (
        userId        VARCHAR(20)   NOT NULL,
        userName      NVARCHAR(50)  NOT NULL,
        passwordHash  VARCHAR(200)  NOT NULL,
        isActive      BIT           NULL DEFAULT 1,
        createdTime   DATETIME      NULL DEFAULT GETDATE(),
        createdBy     VARCHAR(20)   NULL,
        updatedTime   DATETIME      NULL,
        updatedBy     VARCHAR(20)   NULL,
        CONSTRAINT PK_tblUser PRIMARY KEY (userId)
    );
    PRINT 'tblUser 생성 완료';
END
ELSE PRINT 'tblUser 이미 존재 — 건너뜀';
GO

-- 테스트용 초기 계정 (비밀번호: 계정명과 동일 — admin01 / super01 / user01)
MERGE dbo.tblUser AS T
USING (VALUES
    ('admin01', N'관리자',     '5rkzuMPFKhgsg2FZ74SiKQ==:dbbo5Y9lgLgIjtWorJXjERjhQU0fVgKhtutuomxJo+Q=', 1),
    ('super01', N'슈퍼관리자', 'MbHuEI69lNTvTMr12nqARA==:D5nMISTfu0iivGJqeFRvhosltwfXKiKPHhb8JSXoIEo=', 1),
    ('user01',  N'일반사용자', '8RBeZv5oqFeTG9hQDWL8Ng==:LIC68ktT9t9Wa83rX5BOorbiJCiGGy+mfK6XsGxEt9Q=', 1)
) AS S (userId, userName, passwordHash, isActive)
ON T.userId = S.userId
WHEN NOT MATCHED THEN INSERT (userId, userName, passwordHash, isActive)
    VALUES (S.userId, S.userName, S.passwordHash, S.isActive);
PRINT '초기 사용자 계정 적용 완료 (admin01 / super01 / user01)';
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

MERGE dbo.tblUserRole AS T
USING (VALUES
    ('admin01', 'ADMIN'),
    ('super01', 'SUPER'),
    ('user01',  'USER')
) AS S (userId, roleId)
ON T.userId = S.userId AND T.roleId = S.roleId
WHEN NOT MATCHED THEN INSERT (userId, roleId) VALUES (S.userId, S.roleId);
PRINT 'tblUserRole 초기 데이터 적용 완료';
GO

-- =====================================================
-- 4. tblFunction — 앱 기능(버튼 권한) 목록
-- =====================================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblFunction')
BEGIN
    CREATE TABLE dbo.tblFunction (
        functionId    VARCHAR(30)  NOT NULL,
        functionName  VARCHAR(100) NULL,
        description   VARCHAR(200) NULL,
        isActive      BIT          NULL DEFAULT 1,
        CONSTRAINT PK_tblFunction PRIMARY KEY (functionId)
    );
    PRINT 'tblFunction 생성 완료';
END
ELSE PRINT 'tblFunction 이미 존재 — 건너뜀';
GO

MERGE dbo.tblFunction AS T
USING (VALUES
    ('STRIP_UPDATE',         N'Strip 수정',   N'MapArray/BinCode 수정', 1),
    ('LOT_UPDATE',           N'LOT ID 수정',  N'LOT ID 수정',           1),
    ('STRIP_DELETE',         N'논리 삭제',    N'MapArray 논리 삭제',    1),
    ('STRIP_PURGE',          N'물리 삭제',    N'Strip 물리 삭제',       1),
    ('STRIP_ROLLBACK',       N'PCB 원복',     N'PCB 원복(복원)',        1),
    ('STRIP_PURGE_ROLLBACK', N'Purge 복원',   N'물리 삭제 복원',        1)
) AS S (functionId, functionName, description, isActive)
ON T.functionId = S.functionId
WHEN NOT MATCHED THEN INSERT (functionId, functionName, description, isActive)
    VALUES (S.functionId, S.functionName, S.description, S.isActive)
WHEN MATCHED    THEN UPDATE SET functionName = S.functionName, description = S.description, isActive = S.isActive;
PRINT 'tblFunction 초기 데이터 적용 완료';
GO

-- =====================================================
-- 5. tblMenu — 앱 탭(메뉴) 목록
-- =====================================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblMenu')
BEGIN
    CREATE TABLE dbo.tblMenu (
        menuId       VARCHAR(30)  NOT NULL,
        menuName     VARCHAR(100) NULL,
        parentMenuId VARCHAR(30)  NULL,
        menuUrl      VARCHAR(200) NULL,
        menuOrder    INT          NULL DEFAULT 0,
        isActive     BIT          NULL DEFAULT 1,
        CONSTRAINT PK_tblMenu PRIMARY KEY (menuId)
    );
    PRINT 'tblMenu 생성 완료';
END
ELSE PRINT 'tblMenu 이미 존재 — 건너뜀';
GO

MERGE dbo.tblMenu AS T
USING (VALUES
    ('STRIP_EDIT', N'LOT ID 수정',   NULL, 'lotidedit',    1, 1),
    ('MAP_EDIT',   N'MapArray 수정', NULL, 'mapedit',      2, 1),
    ('STRIP_HIST', N'PCB 원복',      NULL, 'striphistory', 3, 1),
    ('PURGE',      N'관리자',        NULL, 'purge',        4, 1)
) AS S (menuId, menuName, parentMenuId, menuUrl, menuOrder, isActive)
ON T.menuId = S.menuId
WHEN NOT MATCHED THEN INSERT (menuId, menuName, parentMenuId, menuUrl, menuOrder, isActive)
    VALUES (S.menuId, S.menuName, S.parentMenuId, S.menuUrl, S.menuOrder, S.isActive)
WHEN MATCHED    THEN UPDATE SET menuName = S.menuName, menuUrl = S.menuUrl, menuOrder = S.menuOrder, isActive = S.isActive;
PRINT 'tblMenu 초기 데이터 적용 완료';
GO

-- =====================================================
-- 6. tblActionFunction — actionType → functionId 매핑
--    SP 권한 체크: actionType → functionId → tblRoleFunction
-- =====================================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblActionFunction')
BEGIN
    CREATE TABLE dbo.tblActionFunction (
        actionType  VARCHAR(10)  NOT NULL,
        functionId  VARCHAR(30)  NOT NULL,
        description VARCHAR(200) NULL,
        CONSTRAINT PK_tblActionFunction PRIMARY KEY (actionType)
    );
    PRINT 'tblActionFunction 생성 완료';
END
ELSE PRINT 'tblActionFunction 이미 존재 — 건너뜀';
GO

MERGE dbo.tblActionFunction AS T
USING (VALUES
    ('U', 'STRIP_UPDATE',         N'MapArray/BinCode 수정'),
    ('L', 'LOT_UPDATE',           N'LOT ID 수정'),
    ('D', 'STRIP_DELETE',         N'논리 삭제'),
    ('P', 'STRIP_PURGE',          N'물리 삭제'),
    ('R', 'STRIP_ROLLBACK',       N'PCB 원복'),
    ('Q', 'STRIP_PURGE_ROLLBACK', N'Purge 복원')
) AS S (actionType, functionId, description)
ON T.actionType = S.actionType
WHEN NOT MATCHED THEN INSERT (actionType, functionId, description) VALUES (S.actionType, S.functionId, S.description)
WHEN MATCHED    THEN UPDATE SET functionId = S.functionId, description = S.description;
PRINT 'tblActionFunction 초기 데이터 적용 완료';
GO

-- =====================================================
-- 7. tblRoleFunction — 역할별 기능 권한 매핑
--    USER  : Strip 수정 / LOT ID 수정 / PCB 원복
--    ADMIN : USER 전체 + 논리 삭제 / 물리 삭제 / Purge 복원
--    SUPER : ADMIN과 동일
-- =====================================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblRoleFunction')
BEGIN
    CREATE TABLE dbo.tblRoleFunction (
        roleId      VARCHAR(20) NOT NULL,
        functionId  VARCHAR(30) NOT NULL,
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
    ('USER',  'STRIP_UPDATE'),
    ('USER',  'LOT_UPDATE'),
    ('USER',  'STRIP_ROLLBACK'),
    ('ADMIN', 'STRIP_UPDATE'),
    ('ADMIN', 'LOT_UPDATE'),
    ('ADMIN', 'STRIP_DELETE'),
    ('ADMIN', 'STRIP_PURGE'),
    ('ADMIN', 'STRIP_ROLLBACK'),
    ('ADMIN', 'STRIP_PURGE_ROLLBACK'),
    ('SUPER', 'STRIP_UPDATE'),
    ('SUPER', 'LOT_UPDATE'),
    ('SUPER', 'STRIP_DELETE'),
    ('SUPER', 'STRIP_PURGE'),
    ('SUPER', 'STRIP_ROLLBACK'),
    ('SUPER', 'STRIP_PURGE_ROLLBACK')
) AS S (roleId, functionId)
ON T.roleId = S.roleId AND T.functionId = S.functionId
WHEN NOT MATCHED THEN INSERT (roleId, functionId) VALUES (S.roleId, S.functionId);
PRINT 'tblRoleFunction 초기 데이터 적용 완료';
GO

-- =====================================================
-- 8. tblRoleMenu — 역할별 메뉴 접근 권한
--    USER  : LOT ID 수정 / MapArray 수정 / PCB 원복
--    ADMIN : USER 전체 + 관리자(Purge) 탭
--    SUPER : ADMIN과 동일
-- =====================================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblRoleMenu')
BEGIN
    CREATE TABLE dbo.tblRoleMenu (
        roleId    VARCHAR(20) NOT NULL,
        menuId    VARCHAR(30) NOT NULL,
        canView   BIT         NULL DEFAULT 1,
        canEdit   BIT         NULL DEFAULT 0,
        canDelete BIT         NULL DEFAULT 0,
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
-- 9. tblStripMap — 핵심 데이터 테이블
--    주의: 계약사에서 제공한 기존 데이터가 있는 경우 건너뜀
-- =====================================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblStripMap')
BEGIN
    CREATE TABLE dbo.tblStripMap (
        active         BIT          NOT NULL,
        [lock]         BIT          NOT NULL,
        stripNo        VARCHAR(80)  NOT NULL,
        [version]      INT          NOT NULL,
        process        VARCHAR(10)  NOT NULL,
        machineId      VARCHAR(20)  NOT NULL,
        lotNo          VARCHAR(80)  NOT NULL,
        blockNo        SMALLINT     NOT NULL,
        originLocation TINYINT      NOT NULL,
        rowCnt         SMALLINT     NOT NULL,
        colCnt         SMALLINT     NOT NULL,
        mapArray       VARCHAR(MAX) NOT NULL,
        createdTime    DATETIME     NOT NULL,
        userId         VARCHAR(20)  NOT NULL,
        createdPgm     VARCHAR(20)  NOT NULL,
        mgzRf          VARCHAR(MAX) NOT NULL,
        pcbLotNo       VARCHAR(80)  NOT NULL,
        erpCode        VARCHAR(80)  NOT NULL,
        quantity       INT          NULL,
        mfgDate        DATE         NULL,
        expDate        DATE         NULL,
        bincode        VARCHAR(MAX) NULL,
        unitIdList     VARCHAR(MAX) NULL,
        changedXpos    VARCHAR(20)  NULL,
        changedYpos    VARCHAR(20)  NULL,
        spare5         VARCHAR(20)  NULL,
        CONSTRAINT tblStripMap_2 PRIMARY KEY CLUSTERED (stripNo ASC, [version] DESC)
    );
    PRINT 'tblStripMap 생성 완료';
END
ELSE PRINT 'tblStripMap 이미 존재 — 건너뜀';
GO

-- =====================================================
-- 10. tblStripMapHistory — 변경 이력 테이블
--     주의: 계약사에서 제공한 기존 데이터가 있는 경우 건너뜀
-- =====================================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblStripMapHistory')
BEGIN
    CREATE TABLE dbo.tblStripMapHistory (
        timekey        VARCHAR(20)  NOT NULL,
        [version]      INT          NOT NULL,
        active         BIT          NOT NULL,
        [lock]         BIT          NOT NULL,
        stripNo        VARCHAR(80)  NOT NULL,
        process        VARCHAR(10)  NOT NULL,
        machineId      VARCHAR(40)  NULL,
        lotNo          VARCHAR(80)  NULL,
        blockNo        VARCHAR(40)  NULL,
        originLocation VARCHAR(40)  NULL,
        rowCnt         INT          NULL,
        colCnt         INT          NULL,
        mapArray       VARCHAR(MAX) NULL,
        createdTime    DATETIME2(7) NULL,
        userId         VARCHAR(40)  NULL,
        createdPgm     VARCHAR(40)  NULL,
        mgzRf          VARCHAR(40)  NULL,
        pcbLotNo       VARCHAR(80)  NULL,
        erpCode        VARCHAR(40)  NULL,
        quantity       INT          NULL,
        mfgDate        DATE         NULL,
        expDate        DATE         NULL,
        bincode        VARCHAR(MAX) NULL,
        unitIdList     VARCHAR(MAX) NULL,
        changedXpos    VARCHAR(200) NULL,
        changedYpos    VARCHAR(200) NULL,
        spare5         VARCHAR(200) NULL,
        workerId       VARCHAR(20)  NOT NULL,
        actionType     VARCHAR(20)  NOT NULL,
        comment        VARCHAR(500) NULL,
        workerIp       VARCHAR(45)  NULL,
        CONSTRAINT PK_tblStripMapHistory PRIMARY KEY (timekey ASC, stripNo ASC, [version] ASC)
    );
    PRINT 'tblStripMapHistory 생성 완료';
END
ELSE PRINT 'tblStripMapHistory 이미 존재 — 건너뜀';
GO

-- =====================================================
-- 확인 쿼리
-- =====================================================
PRINT '=== 생성된 테이블 목록 ===';
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME IN (
    'tblRole','tblUser','tblUserRole','tblFunction','tblMenu',
    'tblActionFunction','tblRoleFunction','tblRoleMenu',
    'tblStripMap','tblStripMapHistory'
)
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

PRINT '=== actionType → functionId 매핑 ===';
SELECT actionType, functionId, description FROM dbo.tblActionFunction ORDER BY actionType;
