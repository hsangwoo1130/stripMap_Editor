using System;
using System.Drawing;
using System.Windows.Forms;

namespace stripMap_Editor.Forms
{
    partial class UserManageForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            // ── 컨트롤 선언 ──────────────────────────────────────────
            this.panelRegister      = new System.Windows.Forms.Panel();
            this.labelTitleRegister = new System.Windows.Forms.Label();
            this.labelUserId        = new System.Windows.Forms.Label();
            this.textBoxUserId      = new System.Windows.Forms.TextBox();
            this.labelUserName      = new System.Windows.Forms.Label();
            this.textBoxUserName    = new System.Windows.Forms.TextBox();
            this.labelPassword      = new System.Windows.Forms.Label();
            this.textBoxPassword    = new System.Windows.Forms.TextBox();
            this.labelRoleReg       = new System.Windows.Forms.Label();
            this.comboBoxRoleReg    = new System.Windows.Forms.ComboBox();
            this.btnRegister        = new System.Windows.Forms.Button();

            this.labelListTitle     = new System.Windows.Forms.Label();
            this.panelList          = new System.Windows.Forms.Panel();
            this.listViewUsers      = new System.Windows.Forms.ListView();
            this.colUserId          = new System.Windows.Forms.ColumnHeader();
            this.colUserName        = new System.Windows.Forms.ColumnHeader();
            this.colRole            = new System.Windows.Forms.ColumnHeader();
            this.colIsActive        = new System.Windows.Forms.ColumnHeader();
            this.colCreatedTime     = new System.Windows.Forms.ColumnHeader();

            this.panelEdit          = new System.Windows.Forms.Panel();
            this.labelTitleEdit     = new System.Windows.Forms.Label();
            this.labelSelectedUser  = new System.Windows.Forms.Label();
            this.labelRoleEdit      = new System.Windows.Forms.Label();
            this.comboBoxRoleEdit   = new System.Windows.Forms.ComboBox();
            this.labelIsActive      = new System.Windows.Forms.Label();
            this.comboBoxIsActive   = new System.Windows.Forms.ComboBox();
            this.btnUpdate          = new System.Windows.Forms.Button();

            // ── SuspendLayout ────────────────────────────────────────
            this.panelRegister.SuspendLayout();
            this.panelList.SuspendLayout();
            this.panelEdit.SuspendLayout();
            this.SuspendLayout();

            // ── panelRegister (상단 등록 패널) ───────────────────────
            this.panelRegister.BackColor    = System.Drawing.Color.WhiteSmoke;
            this.panelRegister.BorderStyle  = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelRegister.Controls.Add(this.labelTitleRegister);
            this.panelRegister.Controls.Add(this.labelUserId);
            this.panelRegister.Controls.Add(this.textBoxUserId);
            this.panelRegister.Controls.Add(this.labelUserName);
            this.panelRegister.Controls.Add(this.textBoxUserName);
            this.panelRegister.Controls.Add(this.labelPassword);
            this.panelRegister.Controls.Add(this.textBoxPassword);
            this.panelRegister.Controls.Add(this.labelRoleReg);
            this.panelRegister.Controls.Add(this.comboBoxRoleReg);
            this.panelRegister.Controls.Add(this.btnRegister);
            this.panelRegister.Dock     = System.Windows.Forms.DockStyle.Top;
            this.panelRegister.Location = new System.Drawing.Point(0, 0);
            this.panelRegister.Name     = "panelRegister";
            this.panelRegister.Size     = new System.Drawing.Size(720, 110);
            this.panelRegister.TabIndex = 0;

            // labelTitleRegister
            this.labelTitleRegister.AutoSize  = true;
            this.labelTitleRegister.Font      = new System.Drawing.Font("맑은 고딕", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.labelTitleRegister.ForeColor = System.Drawing.Color.SteelBlue;
            this.labelTitleRegister.Location  = new System.Drawing.Point(15, 10);
            this.labelTitleRegister.Name      = "labelTitleRegister";
            this.labelTitleRegister.TabIndex  = 0;
            this.labelTitleRegister.Text      = "사용자 등록";

            // labelUserId
            this.labelUserId.AutoSize = true;
            this.labelUserId.Font     = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.labelUserId.Location = new System.Drawing.Point(15, 45);
            this.labelUserId.Name     = "labelUserId";
            this.labelUserId.TabIndex = 1;
            this.labelUserId.Text     = "ID :";

            // textBoxUserId
            this.textBoxUserId.Font     = new System.Drawing.Font("맑은 고딕", 10F);
            this.textBoxUserId.Location = new System.Drawing.Point(48, 42);
            this.textBoxUserId.Name     = "textBoxUserId";
            this.textBoxUserId.Size     = new System.Drawing.Size(110, 26);
            this.textBoxUserId.TabIndex = 1;

            // labelUserName
            this.labelUserName.AutoSize = true;
            this.labelUserName.Font     = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.labelUserName.Location = new System.Drawing.Point(170, 45);
            this.labelUserName.Name     = "labelUserName";
            this.labelUserName.TabIndex = 2;
            this.labelUserName.Text     = "이름 :";

            // textBoxUserName
            this.textBoxUserName.Font     = new System.Drawing.Font("맑은 고딕", 10F);
            this.textBoxUserName.Location = new System.Drawing.Point(218, 42);
            this.textBoxUserName.Name     = "textBoxUserName";
            this.textBoxUserName.Size     = new System.Drawing.Size(110, 26);
            this.textBoxUserName.TabIndex = 2;

            // labelPassword
            this.labelPassword.AutoSize = true;
            this.labelPassword.Font     = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.labelPassword.Location = new System.Drawing.Point(340, 45);
            this.labelPassword.Name     = "labelPassword";
            this.labelPassword.TabIndex = 3;
            this.labelPassword.Text     = "PW :";

            // textBoxPassword
            this.textBoxPassword.Font         = new System.Drawing.Font("맑은 고딕", 10F);
            this.textBoxPassword.Location     = new System.Drawing.Point(380, 42);
            this.textBoxPassword.Name         = "textBoxPassword";
            this.textBoxPassword.PasswordChar = '●';
            this.textBoxPassword.Size         = new System.Drawing.Size(110, 26);
            this.textBoxPassword.TabIndex     = 3;

            // labelRoleReg
            this.labelRoleReg.AutoSize = true;
            this.labelRoleReg.Font     = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.labelRoleReg.Location = new System.Drawing.Point(15, 76);
            this.labelRoleReg.Name     = "labelRoleReg";
            this.labelRoleReg.TabIndex = 4;
            this.labelRoleReg.Text     = "권한 :";

            // comboBoxRoleReg
            this.comboBoxRoleReg.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxRoleReg.Font          = new System.Drawing.Font("맑은 고딕", 10F);
            this.comboBoxRoleReg.Items.AddRange(new object[] { "USER", "ADMIN", "SUPER" });
            this.comboBoxRoleReg.Location      = new System.Drawing.Point(60, 73);
            this.comboBoxRoleReg.Name          = "comboBoxRoleReg";
            this.comboBoxRoleReg.Size          = new System.Drawing.Size(100, 26);
            this.comboBoxRoleReg.TabIndex      = 4;

            // btnRegister
            this.btnRegister.BackColor              = System.Drawing.Color.CornflowerBlue;
            this.btnRegister.Cursor                 = System.Windows.Forms.Cursors.Hand;
            this.btnRegister.FlatAppearance.BorderSize = 0;
            this.btnRegister.FlatStyle             = System.Windows.Forms.FlatStyle.Flat;
            this.btnRegister.Font                  = new System.Drawing.Font("맑은 고딕", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnRegister.ForeColor             = System.Drawing.Color.White;
            this.btnRegister.Location              = new System.Drawing.Point(580, 42);
            this.btnRegister.Name                  = "btnRegister";
            this.btnRegister.Size                  = new System.Drawing.Size(90, 54);
            this.btnRegister.TabIndex              = 5;
            this.btnRegister.Text                  = "등록";
            this.btnRegister.UseVisualStyleBackColor = false;
            this.btnRegister.Click                 += new System.EventHandler(this.BtnRegister_Click);

            // ── labelListTitle ────────────────────────────────────────
            this.labelListTitle.AutoSize  = true;
            this.labelListTitle.Font      = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.labelListTitle.Location  = new System.Drawing.Point(15, 118);
            this.labelListTitle.Name      = "labelListTitle";
            this.labelListTitle.TabIndex  = 1;
            this.labelListTitle.Text      = "사용자 목록";

            // ── panelList (중단 ListView 패널) ────────────────────────
            this.panelList.BackColor   = System.Drawing.Color.Silver;
            this.panelList.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panelList.Controls.Add(this.listViewUsers);
            this.panelList.Location    = new System.Drawing.Point(15, 140);
            this.panelList.Name        = "panelList";
            this.panelList.Size        = new System.Drawing.Size(690, 240);
            this.panelList.TabIndex    = 2;

            // listViewUsers
            this.listViewUsers.BackColor   = System.Drawing.Color.White;
            this.listViewUsers.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listViewUsers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                this.colUserId,
                this.colUserName,
                this.colRole,
                this.colIsActive,
                this.colCreatedTime
            });
            this.listViewUsers.FullRowSelect = true;
            this.listViewUsers.GridLines     = true;
            this.listViewUsers.HideSelection = false;
            this.listViewUsers.Location      = new System.Drawing.Point(3, 3);
            this.listViewUsers.Name          = "listViewUsers";
            this.listViewUsers.OwnerDraw     = true;
            this.listViewUsers.Size          = new System.Drawing.Size(680, 230);
            this.listViewUsers.TabIndex      = 0;
            this.listViewUsers.UseCompatibleStateImageBehavior = false;
            this.listViewUsers.View          = System.Windows.Forms.View.Details;
            this.listViewUsers.DrawColumnHeader     += new System.Windows.Forms.DrawListViewColumnHeaderEventHandler(this.ListViewUsers_DrawColumnHeader);
            this.listViewUsers.DrawItem             += new System.Windows.Forms.DrawListViewItemEventHandler(this.ListViewUsers_DrawItem);
            this.listViewUsers.DrawSubItem          += new System.Windows.Forms.DrawListViewSubItemEventHandler(this.ListViewUsers_DrawSubItem);
            this.listViewUsers.SelectedIndexChanged += new System.EventHandler(this.ListViewUsers_SelectedIndexChanged);
            this.listViewUsers.ColumnWidthChanging  += new System.Windows.Forms.ColumnWidthChangingEventHandler(this.ListViewUsers_ColumnWidthChanging);

            // 컬럼 헤더
            this.colUserId.Text      = "ID";
            this.colUserId.Width     = 120;
            this.colUserName.Text    = "이름";
            this.colUserName.Width   = 120;
            this.colRole.Text        = "권한";
            this.colRole.Width       = 80;
            this.colIsActive.Text    = "활성";
            this.colIsActive.Width   = 60;
            this.colCreatedTime.Text = "생성일";
            this.colCreatedTime.Width = 160;

            // ── panelEdit (하단 편집 패널) ────────────────────────────
            this.panelEdit.BackColor   = System.Drawing.Color.WhiteSmoke;
            this.panelEdit.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelEdit.Controls.Add(this.labelTitleEdit);
            this.panelEdit.Controls.Add(this.labelSelectedUser);
            this.panelEdit.Controls.Add(this.labelRoleEdit);
            this.panelEdit.Controls.Add(this.comboBoxRoleEdit);
            this.panelEdit.Controls.Add(this.labelIsActive);
            this.panelEdit.Controls.Add(this.comboBoxIsActive);
            this.panelEdit.Controls.Add(this.btnUpdate);
            this.panelEdit.Location    = new System.Drawing.Point(15, 390);
            this.panelEdit.Name        = "panelEdit";
            this.panelEdit.Size        = new System.Drawing.Size(690, 80);
            this.panelEdit.TabIndex    = 3;

            // labelTitleEdit
            this.labelTitleEdit.AutoSize  = true;
            this.labelTitleEdit.Font      = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.labelTitleEdit.ForeColor = System.Drawing.Color.SteelBlue;
            this.labelTitleEdit.Location  = new System.Drawing.Point(10, 8);
            this.labelTitleEdit.Name      = "labelTitleEdit";
            this.labelTitleEdit.TabIndex  = 0;
            this.labelTitleEdit.Text      = "선택된 사용자:";

            // labelSelectedUser
            this.labelSelectedUser.AutoSize  = true;
            this.labelSelectedUser.Font      = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.labelSelectedUser.ForeColor = System.Drawing.Color.DimGray;
            this.labelSelectedUser.Location  = new System.Drawing.Point(110, 8);
            this.labelSelectedUser.Name      = "labelSelectedUser";
            this.labelSelectedUser.TabIndex  = 1;
            this.labelSelectedUser.Text      = "(없음)";

            // labelRoleEdit
            this.labelRoleEdit.AutoSize = true;
            this.labelRoleEdit.Font     = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.labelRoleEdit.Location = new System.Drawing.Point(10, 42);
            this.labelRoleEdit.Name     = "labelRoleEdit";
            this.labelRoleEdit.TabIndex = 2;
            this.labelRoleEdit.Text     = "권한 :";

            // comboBoxRoleEdit
            this.comboBoxRoleEdit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxRoleEdit.Font          = new System.Drawing.Font("맑은 고딕", 10F);
            this.comboBoxRoleEdit.Items.AddRange(new object[] { "USER", "ADMIN", "SUPER" });
            this.comboBoxRoleEdit.Location      = new System.Drawing.Point(58, 39);
            this.comboBoxRoleEdit.Name          = "comboBoxRoleEdit";
            this.comboBoxRoleEdit.Size          = new System.Drawing.Size(100, 26);
            this.comboBoxRoleEdit.TabIndex      = 3;

            // labelIsActive
            this.labelIsActive.AutoSize = true;
            this.labelIsActive.Font     = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.labelIsActive.Location = new System.Drawing.Point(175, 42);
            this.labelIsActive.Name     = "labelIsActive";
            this.labelIsActive.TabIndex = 4;
            this.labelIsActive.Text     = "활성 :";

            // comboBoxIsActive
            this.comboBoxIsActive.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxIsActive.Font          = new System.Drawing.Font("맑은 고딕", 10F);
            this.comboBoxIsActive.Items.AddRange(new object[] { "1", "0" });
            this.comboBoxIsActive.Location      = new System.Drawing.Point(221, 39);
            this.comboBoxIsActive.Name          = "comboBoxIsActive";
            this.comboBoxIsActive.Size          = new System.Drawing.Size(70, 26);
            this.comboBoxIsActive.TabIndex      = 4;

            // btnUpdate
            this.btnUpdate.BackColor              = System.Drawing.Color.SeaGreen;
            this.btnUpdate.Cursor                 = System.Windows.Forms.Cursors.Hand;
            this.btnUpdate.FlatAppearance.BorderSize = 0;
            this.btnUpdate.FlatStyle             = System.Windows.Forms.FlatStyle.Flat;
            this.btnUpdate.Font                  = new System.Drawing.Font("맑은 고딕", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnUpdate.ForeColor             = System.Drawing.Color.White;
            this.btnUpdate.Location              = new System.Drawing.Point(580, 30);
            this.btnUpdate.Name                  = "btnUpdate";
            this.btnUpdate.Size                  = new System.Drawing.Size(90, 40);
            this.btnUpdate.TabIndex              = 5;
            this.btnUpdate.Text                  = "수정";
            this.btnUpdate.UseVisualStyleBackColor = false;
            this.btnUpdate.Click                 += new System.EventHandler(this.BtnUpdate_Click);

            // ── UserManageForm ────────────────────────────────────────
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize          = new System.Drawing.Size(720, 490);
            this.Controls.Add(this.labelListTitle);
            this.Controls.Add(this.panelList);
            this.Controls.Add(this.panelEdit);
            this.Controls.Add(this.panelRegister);
            this.Font            = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.Name            = "UserManageForm";
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text            = "사용자 관리";

            // ── ResumeLayout ─────────────────────────────────────────
            this.panelRegister.ResumeLayout(false);
            this.panelRegister.PerformLayout();
            this.panelList.ResumeLayout(false);
            this.panelEdit.ResumeLayout(false);
            this.panelEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        // ── 필드 선언 ─────────────────────────────────────────────
        private System.Windows.Forms.Panel      panelRegister;
        private System.Windows.Forms.Label      labelTitleRegister;
        private System.Windows.Forms.Label      labelUserId;
        private System.Windows.Forms.TextBox    textBoxUserId;
        private System.Windows.Forms.Label      labelUserName;
        private System.Windows.Forms.TextBox    textBoxUserName;
        private System.Windows.Forms.Label      labelPassword;
        private System.Windows.Forms.TextBox    textBoxPassword;
        private System.Windows.Forms.Label      labelRoleReg;
        private System.Windows.Forms.ComboBox   comboBoxRoleReg;
        private System.Windows.Forms.Button     btnRegister;

        private System.Windows.Forms.Label      labelListTitle;
        private System.Windows.Forms.Panel      panelList;
        private System.Windows.Forms.ListView   listViewUsers;
        private System.Windows.Forms.ColumnHeader colUserId;
        private System.Windows.Forms.ColumnHeader colUserName;
        private System.Windows.Forms.ColumnHeader colRole;
        private System.Windows.Forms.ColumnHeader colIsActive;
        private System.Windows.Forms.ColumnHeader colCreatedTime;

        private System.Windows.Forms.Panel      panelEdit;
        private System.Windows.Forms.Label      labelTitleEdit;
        private System.Windows.Forms.Label      labelSelectedUser;
        private System.Windows.Forms.Label      labelRoleEdit;
        private System.Windows.Forms.ComboBox   comboBoxRoleEdit;
        private System.Windows.Forms.Label      labelIsActive;
        private System.Windows.Forms.ComboBox   comboBoxIsActive;
        private System.Windows.Forms.Button     btnUpdate;
    }
}
