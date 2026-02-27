using System;
using System.Drawing;
using System.Windows.Forms;

namespace stripMap_Editor.Forms
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.tabControl_Strip = new System.Windows.Forms.TabControl();
            this.tabPageLotId = new System.Windows.Forms.TabPage();
            this.pictureBox_LotId_Logo = new System.Windows.Forms.PictureBox();
            this.btnUpdate_LotId = new stripMap_Editor.Controls.RoundedButton();
            this.labelResultTitle2 = new System.Windows.Forms.Label();
            this.panelResult_LotId = new System.Windows.Forms.Panel();
            this.listViewResult_LotId = new System.Windows.Forms.ListView();
            this.columnHeaderLot1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderLot2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderLot3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderLot4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.panelSearch2 = new System.Windows.Forms.Panel();
            this.btnModify_LotId = new stripMap_Editor.Controls.RoundedButton();
            this.btnSearch_LotId = new System.Windows.Forms.Button();
            this.textBox_MGZ2 = new System.Windows.Forms.TextBox();
            this.textBox_PCB2 = new System.Windows.Forms.TextBox();
            this.textBox_LOT2 = new System.Windows.Forms.TextBox();
            this.labelMGZId2 = new System.Windows.Forms.Label();
            this.labelPCBId2 = new System.Windows.Forms.Label();
            this.labelLOTId2 = new System.Windows.Forms.Label();
            this.tabPageMapArray = new System.Windows.Forms.TabPage();
            this.pictureBox_MapArray_Logo = new System.Windows.Forms.PictureBox();
            this.panel2_MapArray_BorderMask = new System.Windows.Forms.Panel();
            this.labelResultTitleMapArray = new System.Windows.Forms.Label();
            this.panelInputMapArray = new System.Windows.Forms.Panel();
            this.labelMapArrayInput = new System.Windows.Forms.Label();
            this.textBoxMapArray = new System.Windows.Forms.TextBox();
            this.labelBinCodeInput = new System.Windows.Forms.Label();
            this.textBoxBinCode = new System.Windows.Forms.TextBox();
            this.panelResult_MapArray = new System.Windows.Forms.Panel();
            this.panel_MapArray_BorderMask = new System.Windows.Forms.Panel();
            this.listViewResult_MapArray = new System.Windows.Forms.ListView();
            this.columnHeaderMapArray1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderMapArray2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.listViewResult_MapArray_BinCode = new System.Windows.Forms.ListView();
            this.columnHeaderMapArray_BinCode1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderMapArray_BinCode2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.panelSearchMapArray = new System.Windows.Forms.Panel();
            this.btnSearch_MapArray = new System.Windows.Forms.Button();
            this.textBox_PCB_MapArray = new System.Windows.Forms.TextBox();
            this.labelPCBId_MapArray = new System.Windows.Forms.Label();
            this.btnUpdate_MapArray = new stripMap_Editor.Controls.RoundedButton();
            this.btnDelete_MapArray = new stripMap_Editor.Controls.RoundedButton();
            this.tabPagePcbRestore = new System.Windows.Forms.TabPage();
            this.pictureBox_PCB_Logo = new System.Windows.Forms.PictureBox();
            this.labelResultTitle = new System.Windows.Forms.Label();
            this.panelResult_PCB = new System.Windows.Forms.Panel();
            this.listViewResult_PCB = new System.Windows.Forms.ListView();
            this.columnHeaderPCB1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderPCB2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderPCB3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderPCB6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderPCB4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderPCB5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.panelSearch = new System.Windows.Forms.Panel();
            this.btnSearch_PCB = new System.Windows.Forms.Button();
            this.textBox_MGZ = new System.Windows.Forms.TextBox();
            this.textBox_PCB = new System.Windows.Forms.TextBox();
            this.textBox_LOT = new System.Windows.Forms.TextBox();
            this.labelMGZId = new System.Windows.Forms.Label();
            this.labelPCBId = new System.Windows.Forms.Label();
            this.labelLOTId = new System.Windows.Forms.Label();
            this.btnPurgeRollback_PCB = new stripMap_Editor.Controls.RoundedButton();
            this.btnRestore_PCB = new stripMap_Editor.Controls.RoundedButton();
            this.columnHeaderLot5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabControl_Strip.SuspendLayout();
            this.tabPageLotId.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_LotId_Logo)).BeginInit();
            this.panelResult_LotId.SuspendLayout();
            this.panelSearch2.SuspendLayout();
            this.tabPageMapArray.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_MapArray_Logo)).BeginInit();
            this.panelInputMapArray.SuspendLayout();
            this.panelResult_MapArray.SuspendLayout();
            this.panelSearchMapArray.SuspendLayout();
            this.tabPagePcbRestore.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_PCB_Logo)).BeginInit();
            this.panelResult_PCB.SuspendLayout();
            this.panelSearch.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl_Strip
            // 
            this.tabControl_Strip.Appearance = System.Windows.Forms.TabAppearance.Buttons;
            this.tabControl_Strip.Controls.Add(this.tabPageLotId);
            this.tabControl_Strip.Controls.Add(this.tabPageMapArray);
            this.tabControl_Strip.Controls.Add(this.tabPagePcbRestore);
            this.tabControl_Strip.Cursor = System.Windows.Forms.Cursors.Hand;
            this.tabControl_Strip.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl_Strip.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.tabControl_Strip.ItemSize = new System.Drawing.Size(150, 30);
            this.tabControl_Strip.Location = new System.Drawing.Point(0, 0);
            this.tabControl_Strip.Name = "tabControl_Strip";
            this.tabControl_Strip.SelectedIndex = 0;
            this.tabControl_Strip.Size = new System.Drawing.Size(1089, 690);
            this.tabControl_Strip.TabIndex = 7;
            // 
            // tabPageLotId
            // 
            this.tabPageLotId.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.tabPageLotId.Controls.Add(this.pictureBox_LotId_Logo);
            this.tabPageLotId.Controls.Add(this.btnUpdate_LotId);
            this.tabPageLotId.Controls.Add(this.labelResultTitle2);
            this.tabPageLotId.Controls.Add(this.panelResult_LotId);
            this.tabPageLotId.Controls.Add(this.panelSearch2);
            this.tabPageLotId.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.tabPageLotId.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.tabPageLotId.Location = new System.Drawing.Point(4, 34);
            this.tabPageLotId.Name = "tabPageLotId";
            this.tabPageLotId.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageLotId.Size = new System.Drawing.Size(1081, 652);
            this.tabPageLotId.TabIndex = 0;
            this.tabPageLotId.Text = "Lot ID 변경";
            // 
            // pictureBox_LotId_Logo
            // 
            this.pictureBox_LotId_Logo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.pictureBox_LotId_Logo.Image = global::stripMap_Editor.Properties.Resources.SFA_logo;
            this.pictureBox_LotId_Logo.Location = new System.Drawing.Point(19, 574);
            this.pictureBox_LotId_Logo.Name = "pictureBox_LotId_Logo";
            this.pictureBox_LotId_Logo.Size = new System.Drawing.Size(216, 67);
            this.pictureBox_LotId_Logo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox_LotId_Logo.TabIndex = 5;
            this.pictureBox_LotId_Logo.TabStop = false;
            // 
            // btnUpdate_LotId
            // 
            this.btnUpdate_LotId.BackColor = System.Drawing.Color.CornflowerBlue;
            this.btnUpdate_LotId.BorderRadius = 25;
            this.btnUpdate_LotId.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnUpdate_LotId.FlatAppearance.BorderSize = 0;
            this.btnUpdate_LotId.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnUpdate_LotId.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnUpdate_LotId.ForeColor = System.Drawing.Color.White;
            this.btnUpdate_LotId.Location = new System.Drawing.Point(946, 574);
            this.btnUpdate_LotId.Name = "btnUpdate_LotId";
            this.btnUpdate_LotId.Size = new System.Drawing.Size(110, 55);
            this.btnUpdate_LotId.TabIndex = 2;
            this.btnUpdate_LotId.Text = " 저장 ✔️";
            this.btnUpdate_LotId.UseVisualStyleBackColor = false;
            // 
            // labelResultTitle2
            // 
            this.labelResultTitle2.AutoSize = true;
            this.labelResultTitle2.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.labelResultTitle2.Location = new System.Drawing.Point(21, 217);
            this.labelResultTitle2.Name = "labelResultTitle2";
            this.labelResultTitle2.Size = new System.Drawing.Size(65, 17);
            this.labelResultTitle2.TabIndex = 4;
            this.labelResultTitle2.Text = "조회 결과";
            // 
            // panelResult_LotId
            // 
            this.panelResult_LotId.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.panelResult_LotId.BackColor = System.Drawing.Color.Silver;
            this.panelResult_LotId.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panelResult_LotId.Controls.Add(this.listViewResult_LotId);
            this.panelResult_LotId.Location = new System.Drawing.Point(19, 237);
            this.panelResult_LotId.Name = "panelResult_LotId";
            this.panelResult_LotId.Size = new System.Drawing.Size(1037, 331);
            this.panelResult_LotId.TabIndex = 1;
            // 
            // listViewResult_LotId
            // 
            this.listViewResult_LotId.BackColor = System.Drawing.Color.White;
            this.listViewResult_LotId.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listViewResult_LotId.CheckBoxes = true;
            this.listViewResult_LotId.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderLot1,
            this.columnHeaderLot2,
            this.columnHeaderLot3,
            this.columnHeaderLot4,
            this.columnHeaderLot5});
            this.listViewResult_LotId.FullRowSelect = true;
            this.listViewResult_LotId.GridLines = true;
            this.listViewResult_LotId.HideSelection = false;
            this.listViewResult_LotId.Location = new System.Drawing.Point(3, 3);
            this.listViewResult_LotId.Name = "listViewResult_LotId";
            this.listViewResult_LotId.OwnerDraw = true;
            this.listViewResult_LotId.Size = new System.Drawing.Size(1025, 321);
            this.listViewResult_LotId.TabIndex = 0;
            this.listViewResult_LotId.UseCompatibleStateImageBehavior = false;
            this.listViewResult_LotId.View = System.Windows.Forms.View.Details;
            this.listViewResult_LotId.DrawColumnHeader += new System.Windows.Forms.DrawListViewColumnHeaderEventHandler(this.listViewResult2_DrawColumnHeader);
            this.listViewResult_LotId.DrawItem += new System.Windows.Forms.DrawListViewItemEventHandler(this.listViewResult2_DrawItem);
            this.listViewResult_LotId.DrawSubItem += new System.Windows.Forms.DrawListViewSubItemEventHandler(this.listViewResult2_DrawSubItem);
            // 
            // columnHeaderLot1
            //
            this.columnHeaderLot1.Text = "LOT ID";
            this.columnHeaderLot1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeaderLot1.Width = 200;
            //
            // columnHeaderLot2
            //
            this.columnHeaderLot2.Text = "수정 LOT ID";
            this.columnHeaderLot2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeaderLot2.Width = 200;
            //
            // columnHeaderLot3
            //
            this.columnHeaderLot3.Text = "PCB 2D ID";
            this.columnHeaderLot3.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeaderLot3.Width = 175;
            // 
            // columnHeaderLot4
            // 
            this.columnHeaderLot4.Text = "MGZ ID";
            this.columnHeaderLot4.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeaderLot4.Width = 200;
            // 
            // panelSearch2
            // 
            this.panelSearch2.BackColor = System.Drawing.Color.White;
            this.panelSearch2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panelSearch2.Controls.Add(this.btnModify_LotId);
            this.panelSearch2.Controls.Add(this.btnSearch_LotId);
            this.panelSearch2.Controls.Add(this.textBox_MGZ2);
            this.panelSearch2.Controls.Add(this.textBox_PCB2);
            this.panelSearch2.Controls.Add(this.textBox_LOT2);
            this.panelSearch2.Controls.Add(this.labelMGZId2);
            this.panelSearch2.Controls.Add(this.labelPCBId2);
            this.panelSearch2.Controls.Add(this.labelLOTId2);
            this.panelSearch2.Font = new System.Drawing.Font("맑은 고딕", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.panelSearch2.Location = new System.Drawing.Point(19, 21);
            this.panelSearch2.Name = "panelSearch2";
            this.panelSearch2.Size = new System.Drawing.Size(710, 175);
            this.panelSearch2.TabIndex = 0;
            // 
            // btnModify_LotId
            // 
            this.btnModify_LotId.BackColor = System.Drawing.Color.CornflowerBlue;
            this.btnModify_LotId.BorderRadius = 0;
            this.btnModify_LotId.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnModify_LotId.FlatAppearance.BorderSize = 0;
            this.btnModify_LotId.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnModify_LotId.Font = new System.Drawing.Font("맑은 고딕", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnModify_LotId.ForeColor = System.Drawing.Color.White;
            this.btnModify_LotId.Location = new System.Drawing.Point(512, 37);
            this.btnModify_LotId.Name = "btnModify_LotId";
            this.btnModify_LotId.Size = new System.Drawing.Size(154, 83);
            this.btnModify_LotId.TabIndex = 5;
            this.btnModify_LotId.Text = "  수 정  ⚙️";
            this.btnModify_LotId.UseVisualStyleBackColor = false;
            // 
            // btnSearch_LotId
            // 
            this.btnSearch_LotId.BackColor = System.Drawing.Color.CornflowerBlue;
            this.btnSearch_LotId.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSearch_LotId.FlatAppearance.BorderSize = 0;
            this.btnSearch_LotId.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSearch_LotId.Font = new System.Drawing.Font("맑은 고딕", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnSearch_LotId.ForeColor = System.Drawing.Color.White;
            this.btnSearch_LotId.Location = new System.Drawing.Point(334, 37);
            this.btnSearch_LotId.Name = "btnSearch_LotId";
            this.btnSearch_LotId.Size = new System.Drawing.Size(154, 83);
            this.btnSearch_LotId.TabIndex = 4;
            this.btnSearch_LotId.Text = "  조 회  🔍";
            this.btnSearch_LotId.UseVisualStyleBackColor = false;
            // 
            // textBox_MGZ2
            // 
            this.textBox_MGZ2.Location = new System.Drawing.Point(115, 107);
            this.textBox_MGZ2.Name = "textBox_MGZ2";
            this.textBox_MGZ2.Size = new System.Drawing.Size(186, 27);
            this.textBox_MGZ2.TabIndex = 5;
            // 
            // textBox_PCB2
            // 
            this.textBox_PCB2.Location = new System.Drawing.Point(115, 70);
            this.textBox_PCB2.Name = "textBox_PCB2";
            this.textBox_PCB2.Size = new System.Drawing.Size(186, 27);
            this.textBox_PCB2.TabIndex = 4;
            // 
            // textBox_LOT2
            // 
            this.textBox_LOT2.Location = new System.Drawing.Point(115, 30);
            this.textBox_LOT2.Name = "textBox_LOT2";
            this.textBox_LOT2.Size = new System.Drawing.Size(186, 27);
            this.textBox_LOT2.TabIndex = 3;
            // 
            // labelMGZId2
            // 
            this.labelMGZId2.AutoSize = true;
            this.labelMGZId2.Font = new System.Drawing.Font("맑은 고딕", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.labelMGZId2.Location = new System.Drawing.Point(20, 110);
            this.labelMGZId2.Name = "labelMGZId2";
            this.labelMGZId2.Size = new System.Drawing.Size(71, 20);
            this.labelMGZId2.TabIndex = 2;
            this.labelMGZId2.Text = "MGZ ID :";
            // 
            // labelPCBId2
            // 
            this.labelPCBId2.AutoSize = true;
            this.labelPCBId2.Font = new System.Drawing.Font("맑은 고딕", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.labelPCBId2.Location = new System.Drawing.Point(20, 70);
            this.labelPCBId2.Name = "labelPCBId2";
            this.labelPCBId2.Size = new System.Drawing.Size(89, 20);
            this.labelPCBId2.TabIndex = 1;
            this.labelPCBId2.Text = "PCB 2D ID :";
            // 
            // labelLOTId2
            // 
            this.labelLOTId2.AutoSize = true;
            this.labelLOTId2.Font = new System.Drawing.Font("맑은 고딕", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.labelLOTId2.Location = new System.Drawing.Point(20, 30);
            this.labelLOTId2.Name = "labelLOTId2";
            this.labelLOTId2.Size = new System.Drawing.Size(64, 20);
            this.labelLOTId2.TabIndex = 0;
            this.labelLOTId2.Text = "LOT ID :";
            // 
            // tabPageMapArray
            // 
            this.tabPageMapArray.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.tabPageMapArray.Controls.Add(this.pictureBox_MapArray_Logo);
            this.tabPageMapArray.Controls.Add(this.panel2_MapArray_BorderMask);
            this.tabPageMapArray.Controls.Add(this.labelResultTitleMapArray);
            this.tabPageMapArray.Controls.Add(this.panelInputMapArray);
            this.tabPageMapArray.Controls.Add(this.panelResult_MapArray);
            this.tabPageMapArray.Controls.Add(this.panelSearchMapArray);
            this.tabPageMapArray.Controls.Add(this.btnUpdate_MapArray);
            this.tabPageMapArray.Controls.Add(this.btnDelete_MapArray);
            this.tabPageMapArray.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.tabPageMapArray.Location = new System.Drawing.Point(4, 34);
            this.tabPageMapArray.Name = "tabPageMapArray";
            this.tabPageMapArray.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageMapArray.Size = new System.Drawing.Size(1081, 652);
            this.tabPageMapArray.TabIndex = 1;
            this.tabPageMapArray.Text = "MapArray 변경";
            // 
            // pictureBox_MapArray_Logo
            // 
            this.pictureBox_MapArray_Logo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.pictureBox_MapArray_Logo.Image = global::stripMap_Editor.Properties.Resources.SFA_logo;
            this.pictureBox_MapArray_Logo.Location = new System.Drawing.Point(19, 574);
            this.pictureBox_MapArray_Logo.Name = "pictureBox_MapArray_Logo";
            this.pictureBox_MapArray_Logo.Size = new System.Drawing.Size(216, 67);
            this.pictureBox_MapArray_Logo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox_MapArray_Logo.TabIndex = 9;
            this.pictureBox_MapArray_Logo.TabStop = false;
            // 
            // panel2_MapArray_BorderMask
            // 
            this.panel2_MapArray_BorderMask.BackColor = System.Drawing.Color.White;
            this.panel2_MapArray_BorderMask.Location = new System.Drawing.Point(25, 288);
            this.panel2_MapArray_BorderMask.Name = "panel2_MapArray_BorderMask";
            this.panel2_MapArray_BorderMask.Size = new System.Drawing.Size(201, 2);
            this.panel2_MapArray_BorderMask.TabIndex = 8;
            // 
            // labelResultTitleMapArray
            // 
            this.labelResultTitleMapArray.AutoSize = true;
            this.labelResultTitleMapArray.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Bold);
            this.labelResultTitleMapArray.Location = new System.Drawing.Point(24, 220);
            this.labelResultTitleMapArray.Name = "labelResultTitleMapArray";
            this.labelResultTitleMapArray.Size = new System.Drawing.Size(271, 17);
            this.labelResultTitleMapArray.TabIndex = 4;
            this.labelResultTitleMapArray.Text = "조회 결과 (BOX Check 시 수정 되도록 필요)";
            // 
            // panelInputMapArray
            // 
            this.panelInputMapArray.BackColor = System.Drawing.Color.White;
            this.panelInputMapArray.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panelInputMapArray.Controls.Add(this.labelMapArrayInput);
            this.panelInputMapArray.Controls.Add(this.textBoxMapArray);
            this.panelInputMapArray.Controls.Add(this.labelBinCodeInput);
            this.panelInputMapArray.Controls.Add(this.textBoxBinCode);
            this.panelInputMapArray.Location = new System.Drawing.Point(19, 390);
            this.panelInputMapArray.Name = "panelInputMapArray";
            this.panelInputMapArray.Size = new System.Drawing.Size(1037, 100);
            this.panelInputMapArray.TabIndex = 5;
            // 
            // labelMapArrayInput
            // 
            this.labelMapArrayInput.AutoSize = true;
            this.labelMapArrayInput.Font = new System.Drawing.Font("맑은 고딕", 11.25F, System.Drawing.FontStyle.Bold);
            this.labelMapArrayInput.Location = new System.Drawing.Point(14, 20);
            this.labelMapArrayInput.Name = "labelMapArrayInput";
            this.labelMapArrayInput.Size = new System.Drawing.Size(133, 20);
            this.labelMapArrayInput.TabIndex = 0;
            this.labelMapArrayInput.Text = "MapArray (수정) :";
            // 
            // textBoxMapArray
            // 
            this.textBoxMapArray.Font = new System.Drawing.Font("맑은 고딕", 11.25F);
            this.textBoxMapArray.Location = new System.Drawing.Point(150, 17);
            this.textBoxMapArray.Name = "textBoxMapArray";
            this.textBoxMapArray.Size = new System.Drawing.Size(850, 27);
            this.textBoxMapArray.TabIndex = 1;
            // 
            // labelBinCodeInput
            // 
            this.labelBinCodeInput.AutoSize = true;
            this.labelBinCodeInput.Font = new System.Drawing.Font("맑은 고딕", 11.25F, System.Drawing.FontStyle.Bold);
            this.labelBinCodeInput.Location = new System.Drawing.Point(20, 60);
            this.labelBinCodeInput.Name = "labelBinCodeInput";
            this.labelBinCodeInput.Size = new System.Drawing.Size(127, 20);
            this.labelBinCodeInput.TabIndex = 2;
            this.labelBinCodeInput.Text = "Bin Code (수정) :";
            // 
            // textBoxBinCode
            // 
            this.textBoxBinCode.Font = new System.Drawing.Font("맑은 고딕", 11.25F);
            this.textBoxBinCode.Location = new System.Drawing.Point(150, 57);
            this.textBoxBinCode.Name = "textBoxBinCode";
            this.textBoxBinCode.Size = new System.Drawing.Size(850, 27);
            this.textBoxBinCode.TabIndex = 3;
            // 
            // panelResult_MapArray
            // 
            this.panelResult_MapArray.BackColor = System.Drawing.Color.Silver;
            this.panelResult_MapArray.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panelResult_MapArray.Controls.Add(this.panel_MapArray_BorderMask);
            this.panelResult_MapArray.Controls.Add(this.listViewResult_MapArray);
            this.panelResult_MapArray.Controls.Add(this.listViewResult_MapArray_BinCode);
            this.panelResult_MapArray.Location = new System.Drawing.Point(19, 237);
            this.panelResult_MapArray.Name = "panelResult_MapArray";
            this.panelResult_MapArray.Size = new System.Drawing.Size(1037, 106);
            this.panelResult_MapArray.TabIndex = 1;
            // 
            // panel_MapArray_BorderMask
            // 
            this.panel_MapArray_BorderMask.BackColor = System.Drawing.Color.CornflowerBlue;
            this.panel_MapArray_BorderMask.Location = new System.Drawing.Point(203, 49);
            this.panel_MapArray_BorderMask.Name = "panel_MapArray_BorderMask";
            this.panel_MapArray_BorderMask.Size = new System.Drawing.Size(824, 2);
            this.panel_MapArray_BorderMask.TabIndex = 8;
            // 
            // listViewResult_MapArray
            // 
            this.listViewResult_MapArray.BackColor = System.Drawing.Color.White;
            this.listViewResult_MapArray.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listViewResult_MapArray.CheckBoxes = true;
            this.listViewResult_MapArray.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderMapArray1,
            this.columnHeaderMapArray2});
            this.listViewResult_MapArray.FullRowSelect = true;
            this.listViewResult_MapArray.GridLines = true;
            this.listViewResult_MapArray.HideSelection = false;
            this.listViewResult_MapArray.Location = new System.Drawing.Point(3, 3);
            this.listViewResult_MapArray.Name = "listViewResult_MapArray";
            this.listViewResult_MapArray.OwnerDraw = true;
            this.listViewResult_MapArray.Scrollable = false;
            this.listViewResult_MapArray.Size = new System.Drawing.Size(1025, 48);
            this.listViewResult_MapArray.TabIndex = 0;
            this.listViewResult_MapArray.UseCompatibleStateImageBehavior = false;
            this.listViewResult_MapArray.View = System.Windows.Forms.View.Details;
            this.listViewResult_MapArray.ColumnWidthChanging += new System.Windows.Forms.ColumnWidthChangingEventHandler(this.ListView_ColumnWidthChanging);
            this.listViewResult_MapArray.DrawColumnHeader += new System.Windows.Forms.DrawListViewColumnHeaderEventHandler(this.listViewResultMapArray_DrawColumnHeader);
            this.listViewResult_MapArray.DrawItem += new System.Windows.Forms.DrawListViewItemEventHandler(this.listViewResultMapArray_DrawItem);
            this.listViewResult_MapArray.DrawSubItem += new System.Windows.Forms.DrawListViewSubItemEventHandler(this.listViewResultMapArray_DrawSubItem);
            // 
            // columnHeaderMapArray1
            // 
            this.columnHeaderMapArray1.Text = "PCB 2D ID";
            this.columnHeaderMapArray1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeaderMapArray1.Width = 200;
            // 
            // columnHeaderMapArray2
            // 
            this.columnHeaderMapArray2.Text = "MapArray";
            this.columnHeaderMapArray2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeaderMapArray2.Width = 825;
            // 
            // listViewResult_MapArray_BinCode
            // 
            this.listViewResult_MapArray_BinCode.BackColor = System.Drawing.Color.White;
            this.listViewResult_MapArray_BinCode.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listViewResult_MapArray_BinCode.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderMapArray_BinCode1,
            this.columnHeaderMapArray_BinCode2});
            this.listViewResult_MapArray_BinCode.FullRowSelect = true;
            this.listViewResult_MapArray_BinCode.GridLines = true;
            this.listViewResult_MapArray_BinCode.HideSelection = false;
            this.listViewResult_MapArray_BinCode.Location = new System.Drawing.Point(3, 49);
            this.listViewResult_MapArray_BinCode.Name = "listViewResult_MapArray_BinCode";
            this.listViewResult_MapArray_BinCode.OwnerDraw = true;
            this.listViewResult_MapArray_BinCode.Scrollable = false;
            this.listViewResult_MapArray_BinCode.Size = new System.Drawing.Size(1025, 48);
            this.listViewResult_MapArray_BinCode.TabIndex = 1;
            this.listViewResult_MapArray_BinCode.UseCompatibleStateImageBehavior = false;
            this.listViewResult_MapArray_BinCode.View = System.Windows.Forms.View.Details;
            this.listViewResult_MapArray_BinCode.ColumnWidthChanging += new System.Windows.Forms.ColumnWidthChangingEventHandler(this.ListView_ColumnWidthChanging);
            this.listViewResult_MapArray_BinCode.DrawColumnHeader += new System.Windows.Forms.DrawListViewColumnHeaderEventHandler(this.listViewResultMapArrayBinCode_DrawColumnHeader);
            this.listViewResult_MapArray_BinCode.DrawItem += new System.Windows.Forms.DrawListViewItemEventHandler(this.listViewResultMapArrayBinCode_DrawItem);
            this.listViewResult_MapArray_BinCode.DrawSubItem += new System.Windows.Forms.DrawListViewSubItemEventHandler(this.listViewResultMapArrayBinCode_DrawSubItem);
            // 
            // columnHeaderMapArray_BinCode1
            // 
            this.columnHeaderMapArray_BinCode1.Text = "";
            this.columnHeaderMapArray_BinCode1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeaderMapArray_BinCode1.Width = 200;
            // 
            // columnHeaderMapArray_BinCode2
            // 
            this.columnHeaderMapArray_BinCode2.Text = "Bin Code";
            this.columnHeaderMapArray_BinCode2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeaderMapArray_BinCode2.Width = 825;
            // 
            // panelSearchMapArray
            // 
            this.panelSearchMapArray.BackColor = System.Drawing.Color.White;
            this.panelSearchMapArray.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panelSearchMapArray.Controls.Add(this.btnSearch_MapArray);
            this.panelSearchMapArray.Controls.Add(this.textBox_PCB_MapArray);
            this.panelSearchMapArray.Controls.Add(this.labelPCBId_MapArray);
            this.panelSearchMapArray.Font = new System.Drawing.Font("맑은 고딕", 11.25F);
            this.panelSearchMapArray.Location = new System.Drawing.Point(19, 21);
            this.panelSearchMapArray.Name = "panelSearchMapArray";
            this.panelSearchMapArray.Size = new System.Drawing.Size(518, 175);
            this.panelSearchMapArray.TabIndex = 0;
            // 
            // btnSearch_MapArray
            // 
            this.btnSearch_MapArray.BackColor = System.Drawing.Color.CornflowerBlue;
            this.btnSearch_MapArray.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSearch_MapArray.FlatAppearance.BorderSize = 0;
            this.btnSearch_MapArray.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSearch_MapArray.Font = new System.Drawing.Font("맑은 고딕", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnSearch_MapArray.ForeColor = System.Drawing.Color.White;
            this.btnSearch_MapArray.Location = new System.Drawing.Point(334, 37);
            this.btnSearch_MapArray.Name = "btnSearch_MapArray";
            this.btnSearch_MapArray.Size = new System.Drawing.Size(154, 83);
            this.btnSearch_MapArray.TabIndex = 4;
            this.btnSearch_MapArray.Text = "  조 회  🔍";
            this.btnSearch_MapArray.UseVisualStyleBackColor = false;
            // 
            // textBox_PCB_MapArray
            // 
            this.textBox_PCB_MapArray.Location = new System.Drawing.Point(115, 39);
            this.textBox_PCB_MapArray.Name = "textBox_PCB_MapArray";
            this.textBox_PCB_MapArray.Size = new System.Drawing.Size(186, 27);
            this.textBox_PCB_MapArray.TabIndex = 4;
            // 
            // labelPCBId_MapArray
            // 
            this.labelPCBId_MapArray.AutoSize = true;
            this.labelPCBId_MapArray.Font = new System.Drawing.Font("맑은 고딕", 11.25F);
            this.labelPCBId_MapArray.Location = new System.Drawing.Point(20, 42);
            this.labelPCBId_MapArray.Name = "labelPCBId_MapArray";
            this.labelPCBId_MapArray.Size = new System.Drawing.Size(89, 20);
            this.labelPCBId_MapArray.TabIndex = 1;
            this.labelPCBId_MapArray.Text = "PCB 2D ID :";
            // 
            // btnUpdate_MapArray
            // 
            this.btnUpdate_MapArray.BackColor = System.Drawing.Color.CornflowerBlue;
            this.btnUpdate_MapArray.BorderRadius = 25;
            this.btnUpdate_MapArray.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnUpdate_MapArray.FlatAppearance.BorderSize = 0;
            this.btnUpdate_MapArray.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnUpdate_MapArray.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold);
            this.btnUpdate_MapArray.ForeColor = System.Drawing.Color.White;
            this.btnUpdate_MapArray.Location = new System.Drawing.Point(946, 574);
            this.btnUpdate_MapArray.Name = "btnUpdate_MapArray";
            this.btnUpdate_MapArray.Size = new System.Drawing.Size(110, 55);
            this.btnUpdate_MapArray.TabIndex = 7;
            this.btnUpdate_MapArray.Text = " 수정 ⚙️";
            this.btnUpdate_MapArray.UseVisualStyleBackColor = false;
            // 
            // btnDelete_MapArray
            // 
            this.btnDelete_MapArray.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(53)))), ((int)(((byte)(69)))));
            this.btnDelete_MapArray.BorderRadius = 25;
            this.btnDelete_MapArray.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDelete_MapArray.FlatAppearance.BorderSize = 0;
            this.btnDelete_MapArray.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDelete_MapArray.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold);
            this.btnDelete_MapArray.ForeColor = System.Drawing.Color.White;
            this.btnDelete_MapArray.Location = new System.Drawing.Point(815, 574);
            this.btnDelete_MapArray.Name = "btnDelete_MapArray";
            this.btnDelete_MapArray.Size = new System.Drawing.Size(110, 55);
            this.btnDelete_MapArray.TabIndex = 6;
            this.btnDelete_MapArray.Text = " 삭제 🗑️";
            this.btnDelete_MapArray.UseVisualStyleBackColor = false;
            // 
            // tabPagePcbRestore
            // 
            this.tabPagePcbRestore.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.tabPagePcbRestore.Controls.Add(this.pictureBox_PCB_Logo);
            this.tabPagePcbRestore.Controls.Add(this.labelResultTitle);
            this.tabPagePcbRestore.Controls.Add(this.panelResult_PCB);
            this.tabPagePcbRestore.Controls.Add(this.panelSearch);
            this.tabPagePcbRestore.Controls.Add(this.btnPurgeRollback_PCB);
            this.tabPagePcbRestore.Controls.Add(this.btnRestore_PCB);
            this.tabPagePcbRestore.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.tabPagePcbRestore.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.tabPagePcbRestore.Location = new System.Drawing.Point(4, 34);
            this.tabPagePcbRestore.Name = "tabPagePcbRestore";
            this.tabPagePcbRestore.Size = new System.Drawing.Size(1081, 652);
            this.tabPagePcbRestore.TabIndex = 2;
            this.tabPagePcbRestore.Text = "PCB 2D ID 원복";
            // 
            // pictureBox_PCB_Logo
            // 
            this.pictureBox_PCB_Logo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.pictureBox_PCB_Logo.Image = global::stripMap_Editor.Properties.Resources.SFA_logo;
            this.pictureBox_PCB_Logo.Location = new System.Drawing.Point(19, 574);
            this.pictureBox_PCB_Logo.Name = "pictureBox_PCB_Logo";
            this.pictureBox_PCB_Logo.Size = new System.Drawing.Size(216, 67);
            this.pictureBox_PCB_Logo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox_PCB_Logo.TabIndex = 10;
            this.pictureBox_PCB_Logo.TabStop = false;
            // 
            // labelResultTitle
            // 
            this.labelResultTitle.AutoSize = true;
            this.labelResultTitle.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.labelResultTitle.Location = new System.Drawing.Point(21, 217);
            this.labelResultTitle.Name = "labelResultTitle";
            this.labelResultTitle.Size = new System.Drawing.Size(65, 17);
            this.labelResultTitle.TabIndex = 4;
            this.labelResultTitle.Text = "조회 결과";
            // 
            // panelResult_PCB
            // 
            this.panelResult_PCB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.panelResult_PCB.BackColor = System.Drawing.Color.Silver;
            this.panelResult_PCB.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panelResult_PCB.Controls.Add(this.listViewResult_PCB);
            this.panelResult_PCB.Location = new System.Drawing.Point(19, 237);
            this.panelResult_PCB.Name = "panelResult_PCB";
            this.panelResult_PCB.Size = new System.Drawing.Size(1037, 331);
            this.panelResult_PCB.TabIndex = 1;
            // 
            // listViewResult_PCB
            // 
            this.listViewResult_PCB.BackColor = System.Drawing.Color.White;
            this.listViewResult_PCB.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listViewResult_PCB.CheckBoxes = true;
            this.listViewResult_PCB.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderPCB1,
            this.columnHeaderPCB2,
            this.columnHeaderPCB3,
            this.columnHeaderPCB6,
            this.columnHeaderPCB4,
            this.columnHeaderPCB5});
            this.listViewResult_PCB.FullRowSelect = true;
            this.listViewResult_PCB.GridLines = true;
            this.listViewResult_PCB.HideSelection = false;
            this.listViewResult_PCB.Location = new System.Drawing.Point(3, 3);
            this.listViewResult_PCB.Name = "listViewResult_PCB";
            this.listViewResult_PCB.OwnerDraw = true;
            this.listViewResult_PCB.Size = new System.Drawing.Size(1025, 321);
            this.listViewResult_PCB.TabIndex = 0;
            this.listViewResult_PCB.UseCompatibleStateImageBehavior = false;
            this.listViewResult_PCB.View = System.Windows.Forms.View.Details;
            this.listViewResult_PCB.DrawColumnHeader += new System.Windows.Forms.DrawListViewColumnHeaderEventHandler(this.listViewResult_DrawColumnHeader);
            this.listViewResult_PCB.DrawItem += new System.Windows.Forms.DrawListViewItemEventHandler(this.listViewResult_DrawItem);
            this.listViewResult_PCB.DrawSubItem += new System.Windows.Forms.DrawListViewSubItemEventHandler(this.listViewResult_DrawSubItem);
            // 
            // columnHeaderPCB1
            // 
            this.columnHeaderPCB1.Text = "버전";
            this.columnHeaderPCB1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // columnHeaderPCB2
            //
            this.columnHeaderPCB2.Text = "LOT ID";
            this.columnHeaderPCB2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeaderPCB2.Width = 100;
            //
            // columnHeaderPCB3
            //
            this.columnHeaderPCB3.Text = "PCB 2D ID";
            this.columnHeaderPCB3.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeaderPCB3.Width = 160;
            // 
            // columnHeaderPCB6
            // 
            this.columnHeaderPCB6.Text = "MGZ ID";
            this.columnHeaderPCB6.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeaderPCB6.Width = 100;
            // 
            // columnHeaderPCB4
            // 
            this.columnHeaderPCB4.Text = "사 유";
            this.columnHeaderPCB4.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeaderPCB4.Width = 370;
            // 
            // columnHeaderPCB5
            // 
            this.columnHeaderPCB5.Text = "생성 시각";
            this.columnHeaderPCB5.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeaderPCB5.Width = 235;
            // 
            // panelSearch
            // 
            this.panelSearch.BackColor = System.Drawing.Color.White;
            this.panelSearch.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panelSearch.Controls.Add(this.btnSearch_PCB);
            this.panelSearch.Controls.Add(this.textBox_MGZ);
            this.panelSearch.Controls.Add(this.textBox_PCB);
            this.panelSearch.Controls.Add(this.textBox_LOT);
            this.panelSearch.Controls.Add(this.labelMGZId);
            this.panelSearch.Controls.Add(this.labelPCBId);
            this.panelSearch.Controls.Add(this.labelLOTId);
            this.panelSearch.Font = new System.Drawing.Font("맑은 고딕", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.panelSearch.Location = new System.Drawing.Point(19, 21);
            this.panelSearch.Name = "panelSearch";
            this.panelSearch.Size = new System.Drawing.Size(518, 175);
            this.panelSearch.TabIndex = 0;
            // 
            // btnSearch_PCB
            // 
            this.btnSearch_PCB.BackColor = System.Drawing.Color.CornflowerBlue;
            this.btnSearch_PCB.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSearch_PCB.FlatAppearance.BorderSize = 0;
            this.btnSearch_PCB.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSearch_PCB.Font = new System.Drawing.Font("맑은 고딕", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnSearch_PCB.ForeColor = System.Drawing.Color.White;
            this.btnSearch_PCB.Location = new System.Drawing.Point(334, 37);
            this.btnSearch_PCB.Name = "btnSearch_PCB";
            this.btnSearch_PCB.Size = new System.Drawing.Size(154, 83);
            this.btnSearch_PCB.TabIndex = 4;
            this.btnSearch_PCB.Text = "  조 회  🔍";
            this.btnSearch_PCB.UseVisualStyleBackColor = false;
            // 
            // textBox_MGZ
            // 
            this.textBox_MGZ.Location = new System.Drawing.Point(115, 107);
            this.textBox_MGZ.Name = "textBox_MGZ";
            this.textBox_MGZ.Size = new System.Drawing.Size(186, 27);
            this.textBox_MGZ.TabIndex = 5;
            // 
            // textBox_PCB
            // 
            this.textBox_PCB.Location = new System.Drawing.Point(115, 70);
            this.textBox_PCB.Name = "textBox_PCB";
            this.textBox_PCB.Size = new System.Drawing.Size(186, 27);
            this.textBox_PCB.TabIndex = 4;
            // 
            // textBox_LOT
            // 
            this.textBox_LOT.Location = new System.Drawing.Point(115, 30);
            this.textBox_LOT.Name = "textBox_LOT";
            this.textBox_LOT.Size = new System.Drawing.Size(186, 27);
            this.textBox_LOT.TabIndex = 3;
            // 
            // labelMGZId
            // 
            this.labelMGZId.AutoSize = true;
            this.labelMGZId.Font = new System.Drawing.Font("맑은 고딕", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.labelMGZId.Location = new System.Drawing.Point(20, 110);
            this.labelMGZId.Name = "labelMGZId";
            this.labelMGZId.Size = new System.Drawing.Size(71, 20);
            this.labelMGZId.TabIndex = 2;
            this.labelMGZId.Text = "MGZ ID :";
            // 
            // labelPCBId
            // 
            this.labelPCBId.AutoSize = true;
            this.labelPCBId.Font = new System.Drawing.Font("맑은 고딕", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.labelPCBId.Location = new System.Drawing.Point(20, 70);
            this.labelPCBId.Name = "labelPCBId";
            this.labelPCBId.Size = new System.Drawing.Size(89, 20);
            this.labelPCBId.TabIndex = 1;
            this.labelPCBId.Text = "PCB 2D ID :";
            // 
            // labelLOTId
            // 
            this.labelLOTId.AutoSize = true;
            this.labelLOTId.Font = new System.Drawing.Font("맑은 고딕", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.labelLOTId.Location = new System.Drawing.Point(20, 30);
            this.labelLOTId.Name = "labelLOTId";
            this.labelLOTId.Size = new System.Drawing.Size(64, 20);
            this.labelLOTId.TabIndex = 0;
            this.labelLOTId.Text = "LOT ID :";
            // 
            // btnPurgeRollback_PCB
            // 
            this.btnPurgeRollback_PCB.BackColor = System.Drawing.Color.DarkOrange;
            this.btnPurgeRollback_PCB.BorderRadius = 25;
            this.btnPurgeRollback_PCB.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnPurgeRollback_PCB.FlatAppearance.BorderSize = 0;
            this.btnPurgeRollback_PCB.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPurgeRollback_PCB.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnPurgeRollback_PCB.ForeColor = System.Drawing.Color.White;
            this.btnPurgeRollback_PCB.Location = new System.Drawing.Point(815, 574);
            this.btnPurgeRollback_PCB.Name = "btnPurgeRollback_PCB";
            this.btnPurgeRollback_PCB.Size = new System.Drawing.Size(120, 55);
            this.btnPurgeRollback_PCB.TabIndex = 3;
            this.btnPurgeRollback_PCB.Text = "Purge 복원 ↩";
            this.btnPurgeRollback_PCB.UseVisualStyleBackColor = false;
            // 
            // btnRestore_PCB
            // 
            this.btnRestore_PCB.BackColor = System.Drawing.Color.CornflowerBlue;
            this.btnRestore_PCB.BorderRadius = 25;
            this.btnRestore_PCB.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRestore_PCB.FlatAppearance.BorderSize = 0;
            this.btnRestore_PCB.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRestore_PCB.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnRestore_PCB.ForeColor = System.Drawing.Color.White;
            this.btnRestore_PCB.Location = new System.Drawing.Point(946, 574);
            this.btnRestore_PCB.Name = "btnRestore_PCB";
            this.btnRestore_PCB.Size = new System.Drawing.Size(110, 55);
            this.btnRestore_PCB.TabIndex = 2;
            this.btnRestore_PCB.Text = "  원복 ↩️";
            this.btnRestore_PCB.UseVisualStyleBackColor = false;
            // 
            // columnHeaderLot5
            // 
            this.columnHeaderLot5.Text = "";
            this.columnHeaderLot5.Width = 250;
            // 
            // MainForm
            // 
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.ClientSize = new System.Drawing.Size(1089, 690);
            this.Controls.Add(this.tabControl_Strip);
            this.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "7";
            this.tabControl_Strip.ResumeLayout(false);
            this.tabPageLotId.ResumeLayout(false);
            this.tabPageLotId.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_LotId_Logo)).EndInit();
            this.panelResult_LotId.ResumeLayout(false);
            this.panelSearch2.ResumeLayout(false);
            this.panelSearch2.PerformLayout();
            this.tabPageMapArray.ResumeLayout(false);
            this.tabPageMapArray.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_MapArray_Logo)).EndInit();
            this.panelInputMapArray.ResumeLayout(false);
            this.panelInputMapArray.PerformLayout();
            this.panelResult_MapArray.ResumeLayout(false);
            this.panelSearchMapArray.ResumeLayout(false);
            this.panelSearchMapArray.PerformLayout();
            this.tabPagePcbRestore.ResumeLayout(false);
            this.tabPagePcbRestore.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_PCB_Logo)).EndInit();
            this.panelResult_PCB.ResumeLayout(false);
            this.panelSearch.ResumeLayout(false);
            this.panelSearch.PerformLayout();
            this.ResumeLayout(false);

        }

        // Lot ID 변경 탭 - ListView2 Draw 이벤트
        private void listViewResult2_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            using (SolidBrush backBrush = new SolidBrush(Color.CornflowerBlue))
            {
                e.Graphics.FillRectangle(backBrush, e.Bounds);
            }
            e.Graphics.DrawRectangle(Pens.White, e.Bounds);
            TextRenderer.DrawText(
                e.Graphics,
                e.Header.Text,
                e.Font,
                e.Bounds,
                Color.White,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
            );
        }

        private void listViewResult2_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            e.DrawDefault = true;
        }

        private void listViewResult2_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            DrawSubItemDefault(e, sender as ListView);
        }

        // MapArray 변경 탭 - ListView Draw 이벤트
        private void listViewResultMapArray_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            using (SolidBrush backBrush = new SolidBrush(Color.CornflowerBlue))
            {
                e.Graphics.FillRectangle(backBrush, e.Bounds);
            }
            e.Graphics.DrawRectangle(Pens.White, e.Bounds);
            TextRenderer.DrawText(
                e.Graphics,
                e.Header.Text,
                e.Font,
                e.Bounds,
                Color.White,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
            );
        }

        private void listViewResultMapArray_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            e.DrawDefault = true;
        }

        private void listViewResultMapArray_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            DrawSubItemDefault(e, sender as ListView);
        }

        private void listViewResultMapArrayBinCode_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            Color backColor;
            Color textColor = Color.White;

            // 첫 번째 컬럼만 흰색
            if (e.ColumnIndex == 0)
            {
                backColor = Color.White;
                //textColor = Color.Black;
            }
            else
            {
                backColor = Color.CornflowerBlue;
            }

            using (SolidBrush backBrush = new SolidBrush(backColor))
            {
                e.Graphics.FillRectangle(backBrush, e.Bounds);
            }

            e.Graphics.DrawRectangle(Pens.White, e.Bounds);

            TextRenderer.DrawText(
                e.Graphics,
                e.Header.Text,
                e.Font,
                e.Bounds,
                textColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
            );
        }

        private void listViewResultMapArrayBinCode_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            e.DrawDefault = true;
        }

        private void listViewResultMapArrayBinCode_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            DrawSubItemDefault(e, sender as ListView);
        }

        // PCB 2D ID 원복 탭 - ListView Draw 이벤트
        private void listViewResult_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            using (SolidBrush backBrush = new SolidBrush(Color.CornflowerBlue))
            {
                e.Graphics.FillRectangle(backBrush, e.Bounds);
            }
            e.Graphics.DrawRectangle(Pens.White, e.Bounds);
            TextRenderer.DrawText(
                e.Graphics,
                e.Header.Text,
                e.Font,
                e.Bounds,
                Color.White,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
            );
        }

        private void listViewResult_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            e.DrawDefault = true;
        }

        private void listViewResult_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            DrawSubItemDefault(e, sender as ListView);
        }

        #endregion

        // Lot ID 변경 탭 컨트롤들
        private System.Windows.Forms.Panel panelSearch2;
        private System.Windows.Forms.Panel panelResult_LotId;
        private System.Windows.Forms.Button btnSearch_LotId;
        private System.Windows.Forms.TextBox textBox_LOT2;
        private System.Windows.Forms.Label labelMGZId2;
        private System.Windows.Forms.Label labelPCBId2;
        private System.Windows.Forms.Label labelLOTId2;
        private System.Windows.Forms.TextBox textBox_MGZ2;
        private System.Windows.Forms.TextBox textBox_PCB2;
        private System.Windows.Forms.ListView listViewResult_LotId;
        private System.Windows.Forms.Label labelResultTitle2;
        private System.Windows.Forms.ColumnHeader columnHeaderLot1;  // PCB 2D ID
        private System.Windows.Forms.ColumnHeader columnHeaderLot2;  // LOT ID
        private System.Windows.Forms.ColumnHeader columnHeaderLot4;  // MGZ ID
        private Controls.RoundedButton btnUpdate_LotId;

        // MapArray 변경 탭 컨트롤들
        private System.Windows.Forms.Panel panelSearchMapArray;
        private System.Windows.Forms.Panel panelResult_MapArray;
        private System.Windows.Forms.Panel panelInputMapArray;
        private System.Windows.Forms.Button btnSearch_MapArray;
        private System.Windows.Forms.TextBox textBox_PCB_MapArray;
        private System.Windows.Forms.Label labelPCBId_MapArray;
        private System.Windows.Forms.ListView listViewResult_MapArray;
        private System.Windows.Forms.ListView listViewResult_MapArray_BinCode;
        private System.Windows.Forms.Label labelResultTitleMapArray;
        private System.Windows.Forms.ColumnHeader columnHeaderMapArray1;
        private System.Windows.Forms.ColumnHeader columnHeaderMapArray2;
        private System.Windows.Forms.ColumnHeader columnHeaderMapArray_BinCode1;
        private System.Windows.Forms.ColumnHeader columnHeaderMapArray_BinCode2;
        private System.Windows.Forms.Label labelMapArrayInput;
        private System.Windows.Forms.Label labelBinCodeInput;
        private System.Windows.Forms.TextBox textBoxMapArray;
        private System.Windows.Forms.TextBox textBoxBinCode;
        private Controls.RoundedButton btnDelete_MapArray;
        private Controls.RoundedButton btnUpdate_MapArray;

        // PCB 2D ID 원복 탭 컨트롤들
        private System.Windows.Forms.Panel panelSearch;
        private System.Windows.Forms.Panel panelResult_PCB;
        private System.Windows.Forms.Button btnSearch_PCB;
        private System.Windows.Forms.TextBox textBox_LOT;
        private System.Windows.Forms.Label labelMGZId;
        private System.Windows.Forms.Label labelPCBId;
        private System.Windows.Forms.Label labelLOTId;
        private System.Windows.Forms.TextBox textBox_MGZ;
        private System.Windows.Forms.TextBox textBox_PCB;
        private System.Windows.Forms.ListView listViewResult_PCB;
        private System.Windows.Forms.Label labelResultTitle;
        private System.Windows.Forms.ColumnHeader columnHeaderPCB1;
        private System.Windows.Forms.ColumnHeader columnHeaderPCB2;
        private System.Windows.Forms.ColumnHeader columnHeaderPCB3;
        private System.Windows.Forms.ColumnHeader columnHeaderPCB4;
        private System.Windows.Forms.ColumnHeader columnHeaderPCB5;
        private System.Windows.Forms.ColumnHeader columnHeaderPCB6;  // MGZ ID
        private Controls.RoundedButton btnRestore_PCB;
        private Controls.RoundedButton btnPurgeRollback_PCB;

        // Tab
        private System.Windows.Forms.TabControl tabControl_Strip;
        private System.Windows.Forms.TabPage tabPageLotId;
        private System.Windows.Forms.TabPage tabPageMapArray;
        private System.Windows.Forms.TabPage tabPagePcbRestore;
        private ColumnHeader columnHeaderLot3;
        private Controls.RoundedButton btnModify_LotId;
        private Panel panel_MapArray_BorderMask;
        private Panel panel2_MapArray_BorderMask;
        private PictureBox pictureBox_LotId_Logo;
        private PictureBox pictureBox_MapArray_Logo;
        private PictureBox pictureBox_PCB_Logo;
        private ColumnHeader columnHeaderLot5;
    }
}