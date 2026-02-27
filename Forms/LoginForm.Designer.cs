using System.Drawing;
using System.Windows.Forms;

namespace stripMap_Editor.Forms
{
    partial class LoginForm
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
            this.panelMain = new System.Windows.Forms.Panel();
            this.btnLogin = new System.Windows.Forms.Button();
            this.textBoxPassword = new System.Windows.Forms.TextBox();
            this.labelPassword = new System.Windows.Forms.Label();
            this.textBoxUserId = new System.Windows.Forms.TextBox();
            this.labelUserId = new System.Windows.Forms.Label();
            this.labelTitle_Login = new System.Windows.Forms.Label();
            this.pictureBox_Login_Logo = new System.Windows.Forms.PictureBox();
            this.panelMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_Login_Logo)).BeginInit();
            this.SuspendLayout();
            // 
            // panelMain
            // 
            this.panelMain.BackColor = System.Drawing.Color.White;
            this.panelMain.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelMain.Controls.Add(this.btnLogin);
            this.panelMain.Controls.Add(this.textBoxPassword);
            this.panelMain.Controls.Add(this.labelPassword);
            this.panelMain.Controls.Add(this.textBoxUserId);
            this.panelMain.Controls.Add(this.labelUserId);
            this.panelMain.Controls.Add(this.labelTitle_Login);
            this.panelMain.Controls.Add(this.pictureBox_Login_Logo);
            this.panelMain.Location = new System.Drawing.Point(12, 12);
            this.panelMain.Name = "panelMain";
            this.panelMain.Size = new System.Drawing.Size(376, 326);
            this.panelMain.TabIndex = 0;
            // 
            // btnLogin
            // 
            this.btnLogin.BackColor = System.Drawing.Color.CornflowerBlue;
            this.btnLogin.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLogin.FlatAppearance.BorderSize = 0;
            this.btnLogin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLogin.Font = new System.Drawing.Font("맑은 고딕", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnLogin.ForeColor = System.Drawing.Color.White;
            this.btnLogin.Location = new System.Drawing.Point(138, 240);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(100, 45);
            this.btnLogin.TabIndex = 2;
            this.btnLogin.Text = "로그인";
            this.btnLogin.UseVisualStyleBackColor = false;
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
            // 
            // textBoxPassword
            // 
            this.textBoxPassword.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.textBoxPassword.Location = new System.Drawing.Point(120, 182);
            this.textBoxPassword.Name = "textBoxPassword";
            this.textBoxPassword.PasswordChar = '●';
            this.textBoxPassword.Size = new System.Drawing.Size(200, 25);
            this.textBoxPassword.TabIndex = 1;
            this.textBoxPassword.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxPassword_KeyDown);
            // 
            // labelPassword
            // 
            this.labelPassword.AutoSize = true;
            this.labelPassword.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.labelPassword.Location = new System.Drawing.Point(35, 185);
            this.labelPassword.Name = "labelPassword";
            this.labelPassword.Size = new System.Drawing.Size(73, 19);
            this.labelPassword.TabIndex = 4;
            this.labelPassword.Text = "비밀번호 :";
            // 
            // textBoxUserId
            // 
            this.textBoxUserId.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.textBoxUserId.Location = new System.Drawing.Point(120, 142);
            this.textBoxUserId.Name = "textBoxUserId";
            this.textBoxUserId.Size = new System.Drawing.Size(200, 25);
            this.textBoxUserId.TabIndex = 0;
            this.textBoxUserId.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxUserId_KeyDown);
            // 
            // labelUserId
            // 
            this.labelUserId.AutoSize = true;
            this.labelUserId.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.labelUserId.Location = new System.Drawing.Point(50, 145);
            this.labelUserId.Name = "labelUserId";
            this.labelUserId.Size = new System.Drawing.Size(59, 19);
            this.labelUserId.TabIndex = 2;
            this.labelUserId.Text = "아이디 :";
            // 
            // labelTitle_Login
            // 
            this.labelTitle_Login.AutoSize = true;
            this.labelTitle_Login.Font = new System.Drawing.Font("맑은 고딕", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.labelTitle_Login.Location = new System.Drawing.Point(115, 95);
            this.labelTitle_Login.Name = "labelTitle_Login";
            this.labelTitle_Login.Size = new System.Drawing.Size(154, 25);
            this.labelTitle_Login.TabIndex = 1;
            this.labelTitle_Login.Text = "StripMap Editor";
            // 
            // pictureBox_Login_Logo
            // 
            this.pictureBox_Login_Logo.BackColor = System.Drawing.Color.White;
            this.pictureBox_Login_Logo.Image = global::stripMap_Editor.Properties.Resources.SFA_logo;
            this.pictureBox_Login_Logo.Location = new System.Drawing.Point(88, 20);
            this.pictureBox_Login_Logo.Name = "pictureBox_Login_Logo";
            this.pictureBox_Login_Logo.Size = new System.Drawing.Size(200, 60);
            this.pictureBox_Login_Logo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox_Login_Logo.TabIndex = 0;
            this.pictureBox_Login_Logo.TabStop = false;
            // 
            // LoginForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.ClientSize = new System.Drawing.Size(400, 350);
            this.Controls.Add(this.panelMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LoginForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "StripMap Editor - 로그인";
            this.panelMain.ResumeLayout(false);
            this.panelMain.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_Login_Logo)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Panel panelMain;
        private PictureBox pictureBox_Login_Logo;
        private Label labelTitle_Login;
        private Label labelUserId;
        private TextBox textBoxUserId;
        private Label labelPassword;
        private TextBox textBoxPassword;
        private Button btnLogin;
    }
}