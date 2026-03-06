using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace stripMap_Editor
{
    public enum UserRole
    {
        USER,
        ADMIN,
        SUPER
    }
    public static class UserPermissions
    {
        public const string STRIP_DELETE         = "STRIP_DELETE";
        public const string STRIP_ROLLBACK       = "STRIP_ROLLBACK";
        public const string STRIP_PURGE_ROLLBACK = "STRIP_PURGE_ROLLBACK";
    }

    // 메뉴 ID 상수 (tblMenu.menuId 와 1:1 일치)
    public static class MenuIds
    {
        public const string STRIP_EDIT = "STRIP_EDIT";  // Lot ID 수정 탭  (menuUrl=lotidedit)
        public const string MAP_EDIT   = "MAP_EDIT";    // Map Array 수정 탭 (menuUrl=mapedit)
        public const string STRIP_HIST = "STRIP_HIST";  // PCB 원복 탭      (menuUrl=striphistory)
        public const string PURGE      = "PURGE";       // 관리자 Purge 탭  (menuUrl=purge)
    }

    // ActionType 상수 (dbo.tblActionFunction.functionId 와 1:1 일치)
    public static class ActionTypes
    {
        public const string LOT_UPDATE           = "LOT_UPDATE";           // Lot 수정        (actionType=L)
        public const string STRIP_UPDATE         = "STRIP_UPDATE";         // Strip 수정       (actionType=U)
        public const string STRIP_DELETE         = "STRIP_DELETE";         // 논리 삭제        (actionType=D)
        public const string STRIP_PURGE          = "STRIP_PURGE";          // 물리 삭제        (actionType=P)
        public const string STRIP_PURGE_ROLLBACK = "STRIP_PURGE_ROLLBACK"; // Purge 복원       (actionType=Q)
        public const string STRIP_ROLLBACK       = "STRIP_ROLLBACK";       // Rollback(원복)   (actionType=R)
    }

    /// <summary>
    /// SP THROW 에러 코드 → 사용자 메시지 (중앙화)
    /// usp_StripMap_Process의 THROW 코드와 1:1 대응
    /// </summary>
    public static class SpErrorCodes
    {
        public static string GetMessage(System.Data.SqlClient.SqlException sqlex)
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
}
