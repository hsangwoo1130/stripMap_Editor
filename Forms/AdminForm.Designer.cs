using System;
using System.Drawing;
using System.Windows.Forms;

namespace stripMap_Editor.Forms
{
    partial class AdminForm
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
            this.panelSearch = new System.Windows.Forms.Panel();
            this.labelTitleSearch = new System.Windows.Forms.Label();
            this.labelStripNoLabel = new System.Windows.Forms.Label();
            this.textBoxStripNo = new System.Windows.Forms.TextBox();
            this.btnSearch = new System.Windows.Forms.Button();
            this.labelResultTitle = new System.Windows.Forms.Label();
            this.checkBoxAll = new System.Windows.Forms.CheckBox();
            this.panelResult = new System.Windows.Forms.Panel();
            this.listViewPurge = new System.Windows.Forms.ListView();
            this.colVersion = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colStripNo = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colLotNo = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colMgzRf = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colActive = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btnPurge = new System.Windows.Forms.Button();
            this.panelSearch.SuspendLayout();
            this.panelResult.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelSearch
            // 
            this.panelSearch.BackColor = System.Drawing.Color.WhiteSmoke;
            this.panelSearch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelSearch.Controls.Add(this.labelTitleSearch);
            this.panelSearch.Controls.Add(this.labelStripNoLabel);
            this.panelSearch.Controls.Add(this.textBoxStripNo);
            this.panelSearch.Controls.Add(this.btnSearch);
            this.panelSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelSearch.Location = new System.Drawing.Point(0, 0);
            this.panelSearch.Name = "panelSearch";
            this.panelSearch.Size = new System.Drawing.Size(720, 80);
            this.panelSearch.TabIndex = 0;
            // 
            // labelTitleSearch
            // 
            this.labelTitleSearch.AutoSize = true;
            this.labelTitleSearch.Font = new System.Drawing.Font("맑은 고딕", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.labelTitleSearch.ForeColor = System.Drawing.Color.Firebrick;
            this.labelTitleSearch.Location = new System.Drawing.Point(15, 10);
            this.labelTitleSearch.Name = "labelTitleSearch";
            this.labelTitleSearch.Size = new System.Drawing.Size(220, 20);
            this.labelTitleSearch.TabIndex = 0;
            this.labelTitleSearch.Text = "Strip 물리 삭제 (Admin Purge)";
            // 
            // labelStripNoLabel
            // 
            this.labelStripNoLabel.AutoSize = true;
            this.labelStripNoLabel.Font = new System.Drawing.Font("맑은 고딕", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.labelStripNoLabel.Location = new System.Drawing.Point(15, 44);
            this.labelStripNoLabel.Name = "labelStripNoLabel";
            this.labelStripNoLabel.Size = new System.Drawing.Size(89, 20);
            this.labelStripNoLabel.TabIndex = 1;
            this.labelStripNoLabel.Text = "PCB 2D ID :";
            // 
            // textBoxStripNo
            // 
            this.textBoxStripNo.Font = new System.Drawing.Font("맑은 고딕", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.textBoxStripNo.Location = new System.Drawing.Point(120, 41);
            this.textBoxStripNo.Name = "textBoxStripNo";
            this.textBoxStripNo.Size = new System.Drawing.Size(400, 27);
            this.textBoxStripNo.TabIndex = 1;
            this.textBoxStripNo.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBoxStripNo_KeyDown);
            // 
            // btnSearch
            // 
            this.btnSearch.BackColor = System.Drawing.Color.CornflowerBlue;
            this.btnSearch.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSearch.FlatAppearance.BorderSize = 0;
            this.btnSearch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSearch.Font = new System.Drawing.Font("맑은 고딕", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnSearch.ForeColor = System.Drawing.Color.White;
            this.btnSearch.Location = new System.Drawing.Point(530, 40);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(90, 28);
            this.btnSearch.TabIndex = 2;
            this.btnSearch.Text = "조회";
            this.btnSearch.UseVisualStyleBackColor = false;
            this.btnSearch.Click += new System.EventHandler(this.BtnSearch_Click);
            // 
            // labelResultTitle
            // 
            this.labelResultTitle.AutoSize = true;
            this.labelResultTitle.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.labelResultTitle.Location = new System.Drawing.Point(15, 92);
            this.labelResultTitle.Name = "labelResultTitle";
            this.labelResultTitle.Size = new System.Drawing.Size(65, 17);
            this.labelResultTitle.TabIndex = 1;
            this.labelResultTitle.Text = "조회 결과";
            // 
            // checkBoxAll
            // 
            this.checkBoxAll.AutoSize = true;
            this.checkBoxAll.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.checkBoxAll.Location = new System.Drawing.Point(18, 115);
            this.checkBoxAll.Name = "checkBoxAll";
            this.checkBoxAll.Size = new System.Drawing.Size(84, 21);
            this.checkBoxAll.TabIndex = 3;
            this.checkBoxAll.Text = "전체 선택";
            this.checkBoxAll.CheckedChanged += new System.EventHandler(this.CheckBoxAll_CheckedChanged);
            // 
            // panelResult
            // 
            this.panelResult.BackColor = System.Drawing.Color.Silver;
            this.panelResult.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panelResult.Controls.Add(this.listViewPurge);
            this.panelResult.Location = new System.Drawing.Point(15, 140);
            this.panelResult.Name = "panelResult";
            this.panelResult.Size = new System.Drawing.Size(690, 390);
            this.panelResult.TabIndex = 4;
            // 
            // listViewPurge
            // 
            this.listViewPurge.BackColor = System.Drawing.Color.White;
            this.listViewPurge.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listViewPurge.CheckBoxes = true;
            this.listViewPurge.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colVersion,
            this.colLotNo,
            this.colStripNo,
            this.colMgzRf,
            this.colActive});
            this.listViewPurge.FullRowSelect = true;
            this.listViewPurge.GridLines = true;
            this.listViewPurge.HideSelection = false;
            this.listViewPurge.Location = new System.Drawing.Point(3, 3);
            this.listViewPurge.Name = "listViewPurge";
            this.listViewPurge.OwnerDraw = true;
            this.listViewPurge.Size = new System.Drawing.Size(679, 381);
            this.listViewPurge.TabIndex = 0;
            this.listViewPurge.UseCompatibleStateImageBehavior = false;
            this.listViewPurge.View = System.Windows.Forms.View.Details;
            this.listViewPurge.ColumnWidthChanging += new System.Windows.Forms.ColumnWidthChangingEventHandler(this.ListViewPurge_ColumnWidthChanging);
            this.listViewPurge.DrawColumnHeader += new System.Windows.Forms.DrawListViewColumnHeaderEventHandler(this.ListViewPurge_DrawColumnHeader);
            this.listViewPurge.DrawItem += new System.Windows.Forms.DrawListViewItemEventHandler(this.ListViewPurge_DrawItem);
            this.listViewPurge.DrawSubItem += new System.Windows.Forms.DrawListViewSubItemEventHandler(this.ListViewPurge_DrawSubItem);
            this.listViewPurge.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.ListViewPurge_ItemChecked);
            // 
            // colVersion
            //
            this.colVersion.Text = "버전";
            this.colVersion.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.colVersion.Width = 70;
            //
            // colLotNo
            //
            this.colLotNo.Text = "LOT ID";
            this.colLotNo.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.colLotNo.Width = 150;
            //
            // colStripNo
            //
            this.colStripNo.Text = "PCB 2D ID";
            this.colStripNo.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.colStripNo.Width = 220;
            //
            // colMgzRf
            //
            this.colMgzRf.Text = "MGZ ID";
            this.colMgzRf.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.colMgzRf.Width = 120;
            //
            // colActive
            //
            this.colActive.Text = "Active";
            this.colActive.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.colActive.Width = 116;
            // 
            // btnPurge
            // 
            this.btnPurge.BackColor = System.Drawing.Color.Firebrick;
            this.btnPurge.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnPurge.FlatAppearance.BorderSize = 0;
            this.btnPurge.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPurge.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnPurge.ForeColor = System.Drawing.Color.White;
            this.btnPurge.Location = new System.Drawing.Point(559, 548);
            this.btnPurge.Name = "btnPurge";
            this.btnPurge.Size = new System.Drawing.Size(140, 50);
            this.btnPurge.TabIndex = 5;
            this.btnPurge.Text = "Purge 실행";
            this.btnPurge.UseVisualStyleBackColor = false;
            this.btnPurge.Click += new System.EventHandler(this.BtnPurge_Click);
            // 
            // AdminForm
            // 
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.ClientSize = new System.Drawing.Size(720, 610);
            this.Controls.Add(this.panelSearch);
            this.Controls.Add(this.labelResultTitle);
            this.Controls.Add(this.checkBoxAll);
            this.Controls.Add(this.panelResult);
            this.Controls.Add(this.btnPurge);
            this.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AdminForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "관리자 화면 — Strip Purge";
            this.panelSearch.ResumeLayout(false);
            this.panelSearch.PerformLayout();
            this.panelResult.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel     panelSearch;
        private System.Windows.Forms.Label     labelTitleSearch;
        private System.Windows.Forms.Label     labelStripNoLabel;
        private System.Windows.Forms.TextBox   textBoxStripNo;
        private System.Windows.Forms.Button    btnSearch;
        private System.Windows.Forms.Label     labelResultTitle;
        private System.Windows.Forms.CheckBox  checkBoxAll;
        private System.Windows.Forms.Panel     panelResult;
        private System.Windows.Forms.ListView  listViewPurge;
        private System.Windows.Forms.ColumnHeader colVersion;
        private System.Windows.Forms.ColumnHeader colStripNo;
        private System.Windows.Forms.ColumnHeader colLotNo;
        private System.Windows.Forms.ColumnHeader colMgzRf;
        private System.Windows.Forms.ColumnHeader colActive;
        private System.Windows.Forms.Button    btnPurge;
    }
}
