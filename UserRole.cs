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
}
