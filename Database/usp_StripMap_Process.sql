IF OBJECT_ID('dbo.usp_StripMap_Process', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_StripMap_Process;
GO

CREATE PROCEDURE [dbo].[usp_StripMap_Process]
(
    @actionType      CHAR(1),
    @stripNo         VARCHAR(80),
    @process         VARCHAR(10),

    @mapArray        VARCHAR(MAX)  = NULL,
    @bincode         VARCHAR(MAX)  = NULL,
    @lotNo           VARCHAR(80)   = NULL,

    @targetTimekey   VARCHAR(20)   = NULL,   -- 'R'/'Q' 전용: 입력 레코드 식별자
    @targetVersion   INT           = NULL,   -- 'P' 전용: tblStripMap.[version]

    @workerId        VARCHAR(20),
    @comment         VARCHAR(500),
    @workerIp        VARCHAR(45),

    @changedXpos     VARCHAR(MAX)  = NULL,
    @changedYpos     VARCHAR(MAX)  = NULL
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    -- timekey: yyyyMMddHHmmssffffff (20자리) → 입력 레코드의 고유 식별자
    DECLARE @timekey         VARCHAR(20) = FORMAT(SYSDATETIME(), 'yyyyMMddHHmmssffffff');
    DECLARE @functionId      VARCHAR(40);
    DECLARE @newStripVersion INT;   -- 신규 업데이트 version (U/L/R/Q 공통)
    DECLARE @origActive      BIT;   -- 원본 active 값 (Q 전용)

    /* 기능 권한 체크 시작 */
    SELECT @functionId = functionId
    FROM dbo.tblActionFunction
    WHERE actionType = @actionType;

    IF @functionId IS NULL
        THROW 50001, 'Undefined ActionType', 1;

    IF NOT EXISTS (
        SELECT 1
        FROM dbo.tblUserRole UR
        JOIN dbo.tblRoleFunction RF ON UR.roleId = RF.roleId
        WHERE UR.userId   = @workerId
          AND RF.functionId = @functionId
    )
        THROW 50002, 'No function permission', 1;

    BEGIN TRY
        BEGIN TRAN;

        /* =================================================================
           U → MapArray / BinCode 수정
           변경 active=0 보존 없이 History 기록 후 DELETE + INSERT
        ================================================================= */
        IF @actionType = 'U'
        BEGIN
            SELECT @newStripVersion = ISNULL(MAX([version]), 0) + 1
            FROM dbo.tblStripMap
            WHERE stripNo = @stripNo AND process = @process;

            -- ① History 기록 (Before-image)
            INSERT dbo.tblStripMapHistory
            (timekey, [version], active, [lock], stripNo, process, machineId, lotNo, blockNo, originLocation, rowCnt, colCnt, mapArray, createdTime, userId, createdPgm, mgzRf, pcbLotNo, erpCode, quantity, mfgDate, expDate, bincode, unitIdList, changedXpos, changedYpos, spare5, workerId, actionType, comment, workerIp)
            SELECT @timekey, [version], active, [lock], stripNo, process, machineId, lotNo, blockNo, originLocation, rowCnt, colCnt, mapArray, createdTime, userId, createdPgm, mgzRf, pcbLotNo, erpCode, quantity, mfgDate, expDate, bincode, unitIdList, @changedXpos, @changedYpos, spare5, @workerId, 'UPDATE', @comment, @workerIp
            FROM dbo.tblStripMap
            WHERE stripNo = @stripNo AND process = @process AND active = 1;

            -- ② 기존 active=1 행 삭제 (active=0 보존 없음)
            DELETE dbo.tblStripMap
            WHERE stripNo = @stripNo AND process = @process AND active = 1;

            -- ③ 신규행 재입력 (History에서 불변 필드 참조, 변경필드 COALESCE 사용)
            INSERT dbo.tblStripMap
            (active, [lock], stripNo, [version], process, machineId, lotNo, blockNo, originLocation, rowCnt, colCnt, mapArray, createdTime, userId, createdPgm, mgzRf, pcbLotNo, erpCode, quantity, mfgDate, expDate, bincode, unitIdList, changedXpos, changedYpos, spare5)
            SELECT 1, [lock], @stripNo, @newStripVersion, process, machineId, lotNo, blockNo, originLocation, rowCnt, colCnt,
                   COALESCE(@mapArray, mapArray),
                   createdTime, userId, createdPgm, mgzRf, pcbLotNo, erpCode, quantity, mfgDate, expDate,
                   COALESCE(@bincode, bincode),
                   unitIdList, @changedXpos, @changedYpos, spare5
            FROM dbo.tblStripMapHistory
            WHERE timekey = @timekey;
        END

        /* =================================================================
           L → LOT ID 수정
           변경 active=0 보존 없이 History 기록 후 DELETE + INSERT
        ================================================================= */
        ELSE IF @actionType = 'L'
        BEGIN
            SELECT @newStripVersion = ISNULL(MAX([version]), 0) + 1
            FROM dbo.tblStripMap
            WHERE stripNo = @stripNo AND process = @process;

            -- ① History 기록 (Before-image)
            INSERT dbo.tblStripMapHistory
            (timekey, [version], active, [lock], stripNo, process, machineId, lotNo, blockNo, originLocation, rowCnt, colCnt, mapArray, createdTime, userId, createdPgm, mgzRf, pcbLotNo, erpCode, quantity, mfgDate, expDate, bincode, unitIdList, changedXpos, changedYpos, spare5, workerId, actionType, comment, workerIp)
            SELECT @timekey, [version], active, [lock], stripNo, process, machineId, lotNo, blockNo, originLocation, rowCnt, colCnt, mapArray, createdTime, userId, createdPgm, mgzRf, pcbLotNo, erpCode, quantity, mfgDate, expDate, bincode, unitIdList, changedXpos, changedYpos, spare5, @workerId, 'LOT_UPDATE', @comment, @workerIp
            FROM dbo.tblStripMap
            WHERE stripNo = @stripNo AND process = @process AND active = 1;

            -- ② 기존 active=1 행 삭제
            DELETE dbo.tblStripMap
            WHERE stripNo = @stripNo AND process = @process AND active = 1;

            -- ③ 신규행 재입력 (History에서 불변 필드 참조, lotNo만 새값 사용)
            INSERT dbo.tblStripMap
            (active, [lock], stripNo, [version], process, machineId, lotNo, blockNo, originLocation, rowCnt, colCnt, mapArray, createdTime, userId, createdPgm, mgzRf, pcbLotNo, erpCode, quantity, mfgDate, expDate, bincode, unitIdList, changedXpos, changedYpos, spare5)
            SELECT 1, [lock], @stripNo, @newStripVersion, process, machineId, @lotNo, blockNo, originLocation, rowCnt, colCnt, mapArray,
                   createdTime, userId, createdPgm, mgzRf, pcbLotNo, erpCode, quantity, mfgDate, expDate, bincode,
                   unitIdList, changedXpos, changedYpos, spare5
            FROM dbo.tblStripMapHistory
            WHERE timekey = @timekey;
        END

        /* =================================================================
           D → 논리 삭제
           동일하게 active=0 행을 tblStripMap에 기록하는 케이스 (변경없음)
        ================================================================= */
        ELSE IF @actionType = 'D'
        BEGIN
            -- History 기록
            INSERT dbo.tblStripMapHistory
            (timekey, [version], active, [lock], stripNo, process, machineId, lotNo, blockNo, originLocation, rowCnt, colCnt, mapArray, createdTime, userId, createdPgm, mgzRf, pcbLotNo, erpCode, quantity, mfgDate, expDate, bincode, unitIdList, changedXpos, changedYpos, spare5, workerId, actionType, comment, workerIp)
            SELECT @timekey, [version], active, [lock], stripNo, process, machineId, lotNo, blockNo, originLocation, rowCnt, colCnt, mapArray, createdTime, userId, createdPgm, mgzRf, pcbLotNo, erpCode, quantity, mfgDate, expDate, bincode, unitIdList, changedXpos, changedYpos, spare5, @workerId, 'DELETE', @comment, @workerIp
            FROM dbo.tblStripMap
            WHERE stripNo = @stripNo AND process = @process AND active = 1;

            -- 논리 삭제: active=0으로 변경 (tblStripMap에 동일하게 보존)
            UPDATE dbo.tblStripMap
            SET active = 0
            WHERE stripNo = @stripNo AND process = @process AND active = 1;
        END

        /* =================================================================
           P → Admin 물리 삭제 (ADMIN/SUPER 전용)
           지정 (stripNo, process, @targetVersion) 행을 tblStripMap에서 물리 삭제
           History에 'STRIP_PURGE' 감사 입력 기록 후 'Q' (Purge복원)으로 복원 가능
        ================================================================= */
        ELSE IF @actionType = 'P'
        BEGIN
            IF @targetVersion IS NULL
                THROW 50040, 'targetVersion is required for purge', 1;

            IF NOT EXISTS (
                SELECT 1 FROM dbo.tblStripMap
                WHERE stripNo = @stripNo AND process = @process AND [version] = @targetVersion
            )
                THROW 50041, 'Target row not found in tblStripMap', 1;

            -- ① 삭제 전 History 감사 입력 기록 (actionType='STRIP_PURGE' → 'Q'로 복원 가능)
            INSERT dbo.tblStripMapHistory
            (timekey, [version], active, [lock], stripNo, process, machineId, lotNo, blockNo, originLocation, rowCnt, colCnt, mapArray, createdTime, userId, createdPgm, mgzRf, pcbLotNo, erpCode, quantity, mfgDate, expDate, bincode, unitIdList, changedXpos, changedYpos, spare5, workerId, actionType, comment, workerIp)
            SELECT @timekey, [version], active, [lock], stripNo, process, machineId, lotNo, blockNo, originLocation, rowCnt, colCnt, mapArray, createdTime, userId, createdPgm, mgzRf, pcbLotNo, erpCode, quantity, mfgDate, expDate, bincode, unitIdList, changedXpos, changedYpos, spare5, @workerId, 'STRIP_PURGE', @comment, @workerIp
            FROM dbo.tblStripMap
            WHERE stripNo = @stripNo AND process = @process AND [version] = @targetVersion;

            -- ② 물리 삭제
            DELETE dbo.tblStripMap
            WHERE stripNo = @stripNo AND process = @process AND [version] = @targetVersion;
        END

        /* =================================================================
           R → 복원 (History → tblStripMap 복원)
           변경 flag-swap 없이 현재 행 전체 DELETE + History에서 신규 active=1 INSERT
           논리 삭제(active=0) 상태도 동일하게 처리 (삭제 후 복원)
        ================================================================= */
        ELSE IF @actionType = 'R'
        BEGIN
            IF @targetTimekey IS NULL
                THROW 50011, 'targetTimekey is required for rollback', 1;

            IF NOT EXISTS (
                SELECT 1 FROM dbo.tblStripMapHistory
                WHERE stripNo = @stripNo AND process = @process AND timekey = @targetTimekey
            )
                THROW 50012, 'Rollback target not found in history', 1;

            -- ① 복원할 버전: History에 기록된 원본 [version] 그대로 사용
            SELECT @newStripVersion = [version]
            FROM dbo.tblStripMapHistory
            WHERE timekey = @targetTimekey;

            -- ② 현재 tblStripMap 행 전체 삭제 (active=1 및 논리 삭제 active=0 모두)
            DELETE dbo.tblStripMap
            WHERE stripNo = @stripNo AND process = @process;

            -- ③ History 데이터로 신규 active=1 재입력 (원본 버전 그대로 복원)
            INSERT dbo.tblStripMap
            (active, [lock], stripNo, [version], process, machineId, lotNo, blockNo, originLocation, rowCnt, colCnt, mapArray, createdTime, userId, createdPgm, mgzRf, pcbLotNo, erpCode, quantity, mfgDate, expDate, bincode, unitIdList, changedXpos, changedYpos, spare5)
            SELECT 1, [lock], @stripNo, @newStripVersion, process, machineId, lotNo, blockNo, originLocation, rowCnt, colCnt, mapArray,
                   createdTime, userId, createdPgm, mgzRf, pcbLotNo, erpCode, quantity, mfgDate, expDate, bincode,
                   unitIdList, changedXpos, changedYpos, spare5
            FROM dbo.tblStripMapHistory
            WHERE stripNo = @stripNo AND process = @process AND timekey = @targetTimekey;

            -- ④ ROLLBACK 감사 입력 기록 (복원 후 상태)
            INSERT dbo.tblStripMapHistory
            (timekey, [version], active, [lock], stripNo, process, machineId, lotNo, blockNo, originLocation, rowCnt, colCnt, mapArray, createdTime, userId, createdPgm, mgzRf, pcbLotNo, erpCode, quantity, mfgDate, expDate, bincode, unitIdList, changedXpos, changedYpos, spare5, workerId, actionType, comment, workerIp)
            SELECT @timekey, [version], active, [lock], stripNo, process, machineId, lotNo, blockNo, originLocation, rowCnt, colCnt, mapArray, createdTime, userId, createdPgm, mgzRf, pcbLotNo, erpCode, quantity, mfgDate, expDate, bincode, unitIdList, changedXpos, changedYpos, spare5, @workerId, 'ROLLBACK', @comment, @workerIp
            FROM dbo.tblStripMap
            WHERE stripNo = @stripNo AND process = @process AND active = 1;
        END

        /* =================================================================
           Q → Purge 복원 (STRIP_PURGE 입력 후 tblStripMap 복원)
           - active=1 복원: 기존 active=1 행만 삭제 후 원본 active=1로 재입력
           - active=0 복원: 기존 삭제 않고, 원본 active=0으로 재입력 (동일 stripNo 여러 행 허용)
           호출 경로: PCB 복원 후 Purge복원 버튼 사용 (RestoreFromHistory 경유 불가)
        ================================================================= */
        ELSE IF @actionType = 'Q'
        BEGIN
            IF @targetTimekey IS NULL
                THROW 50021, 'targetTimekey is required for purge rollback', 1;

            IF NOT EXISTS (
                SELECT 1 FROM dbo.tblStripMapHistory
                WHERE stripNo = @stripNo AND process = @process AND timekey = @targetTimekey AND actionType = 'STRIP_PURGE'
            )
                THROW 50012, 'Purge rollback target not found in history', 1;

            -- ① 복원할 버전·원본 active 확인
            SELECT @newStripVersion = [version], @origActive = active
            FROM dbo.tblStripMapHistory
            WHERE timekey = @targetTimekey AND actionType = 'STRIP_PURGE';

            -- ② active=1 복원 경우에만 기존 active=1 삭제 (active=0 복원은 기존 삭제 안 함)
            IF @origActive = 1
            BEGIN
                DELETE dbo.tblStripMap
                WHERE stripNo = @stripNo AND process = @process AND active = 1;
            END

            -- ③ STRIP_PURGE 입력의 원본 active 값으로 복원 (hardcoded 1 대신 원본값)
            INSERT dbo.tblStripMap
            (active, [lock], stripNo, [version], process, machineId, lotNo, blockNo, originLocation, rowCnt, colCnt, mapArray, createdTime, userId, createdPgm, mgzRf, pcbLotNo, erpCode, quantity, mfgDate, expDate, bincode, unitIdList, changedXpos, changedYpos, spare5)
            SELECT active, [lock], @stripNo, @newStripVersion, process, machineId, lotNo, blockNo, originLocation, rowCnt, colCnt, mapArray,
                   createdTime, userId, createdPgm, mgzRf, pcbLotNo, erpCode, quantity, mfgDate, expDate, bincode,
                   unitIdList, changedXpos, changedYpos, spare5
            FROM dbo.tblStripMapHistory
            WHERE stripNo = @stripNo AND process = @process AND timekey = @targetTimekey AND actionType = 'STRIP_PURGE';

            -- ④ PURGE_ROLLBACK 감사 입력 기록 (복원한 version 기준)
            INSERT dbo.tblStripMapHistory
            (timekey, [version], active, [lock], stripNo, process, machineId, lotNo, blockNo, originLocation, rowCnt, colCnt, mapArray, createdTime, userId, createdPgm, mgzRf, pcbLotNo, erpCode, quantity, mfgDate, expDate, bincode, unitIdList, changedXpos, changedYpos, spare5, workerId, actionType, comment, workerIp)
            SELECT @timekey, [version], active, [lock], stripNo, process, machineId, lotNo, blockNo, originLocation, rowCnt, colCnt, mapArray, createdTime, userId, createdPgm, mgzRf, pcbLotNo, erpCode, quantity, mfgDate, expDate, bincode, unitIdList, changedXpos, changedYpos, spare5, @workerId, 'PURGE_ROLLBACK', @comment, @workerIp
            FROM dbo.tblStripMap
            WHERE stripNo = @stripNo AND process = @process AND [version] = @newStripVersion;
        END
        ELSE
        BEGIN
            THROW 50030, 'Unsupported actionType', 1;
        END

        COMMIT;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        THROW;
    END CATCH
END
GO