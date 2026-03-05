-- tblStripMap spare3 → changedXpos, spare4 → changedYpos
EXEC sp_rename 'dbo.tblStripMap.spare3', 'changedXpos', 'COLUMN';
EXEC sp_rename 'dbo.tblStripMap.spare4', 'changedYpos', 'COLUMN';

-- tblStripMapHistory spare3 → changedXpos, spare4 → changedYpos
EXEC sp_rename 'dbo.tblStripMapHistory.spare3', 'changedXpos', 'COLUMN';
EXEC sp_rename 'dbo.tblStripMapHistory.spare4', 'changedYpos', 'COLUMN';

-- 확인
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'tblStripMap' AND COLUMN_NAME IN ('changedXpos','changedYpos','spare3','spare4');

SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'tblStripMapHistory' AND COLUMN_NAME IN ('changedXpos','changedYpos','spare3','spare4');