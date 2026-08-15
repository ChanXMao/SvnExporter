namespace SvnExporter
{
    partial class MainWnd
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            tb_VersionNum = new TextBox();
            label1 = new Label();
            btn_Export = new Button();
            label2 = new Label();
            tb_ProjectPath = new TextBox();
            btn_Browse = new Button();
            label3 = new Label();
            tb_ExportSubPath = new TextBox();
            tb_ExportFileType = new TextBox();
            label4 = new Label();
            lb_ExportAuthor = new ListBox();
            label5 = new Label();
            label6 = new Label();
            lb_AllAuthor = new ListBox();
            btn_AddToExport = new Button();
            btn_RemoveForExport = new Button();
            groupBox1 = new GroupBox();
            cb_ExportVersion = new CheckBox();
            groupBox2 = new GroupBox();
            cb_ExportModify = new CheckBox();
            btn_DebugMode = new CheckBox();
            btn_RemoveAuthor = new Button();
            btn_AddAuthor = new Button();
            tb_BeginVersionNum = new TextBox();
            label7 = new Label();
            rtb_Console = new RichTextBox();
            btn_SaveConfig = new Button();
            tb_ExportPath = new TextBox();
            label8 = new Label();
            groupBox3 = new GroupBox();
            btn_Browse2 = new Button();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox3.SuspendLayout();
            SuspendLayout();
            // 
            // tb_VersionNum
            // 
            tb_VersionNum.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            tb_VersionNum.Font = new Font("Microsoft YaHei UI", 10F);
            tb_VersionNum.Location = new Point(702, 339);
            tb_VersionNum.Margin = new Padding(6);
            tb_VersionNum.Name = "tb_VersionNum";
            tb_VersionNum.Size = new Size(236, 50);
            tb_VersionNum.TabIndex = 0;
            tb_VersionNum.TextAlign = HorizontalAlignment.Right;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            label1.AutoSize = true;
            label1.Font = new Font("Microsoft YaHei UI", 10F);
            label1.Location = new Point(26, 339);
            label1.Margin = new Padding(6, 0, 6, 0);
            label1.Name = "label1";
            label1.Size = new Size(640, 45);
            label1.TabIndex = 1;
            label1.Text = "工程导出版本号（为空则导出最新版本）:";
            // 
            // btn_Export
            // 
            btn_Export.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btn_Export.Font = new Font("Microsoft YaHei UI", 14F);
            btn_Export.Location = new Point(1291, 22);
            btn_Export.Margin = new Padding(6);
            btn_Export.Name = "btn_Export";
            btn_Export.Size = new Size(168, 76);
            btn_Export.TabIndex = 2;
            btn_Export.Text = "导出";
            btn_Export.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Microsoft YaHei UI", 10F);
            label2.Location = new Point(26, 63);
            label2.Margin = new Padding(6, 0, 6, 0);
            label2.Name = "label2";
            label2.Size = new Size(130, 45);
            label2.TabIndex = 3;
            label2.Text = "主目录:";
            // 
            // tb_ProjectPath
            // 
            tb_ProjectPath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tb_ProjectPath.BackColor = SystemColors.Control;
            tb_ProjectPath.Font = new Font("Microsoft YaHei UI", 10F);
            tb_ProjectPath.Location = new Point(168, 63);
            tb_ProjectPath.Margin = new Padding(6);
            tb_ProjectPath.Name = "tb_ProjectPath";
            tb_ProjectPath.ReadOnly = true;
            tb_ProjectPath.Size = new Size(1334, 50);
            tb_ProjectPath.TabIndex = 4;
            // 
            // btn_Browse
            // 
            btn_Browse.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btn_Browse.Font = new Font("Microsoft YaHei UI", 9F);
            btn_Browse.Location = new Point(1514, 63);
            btn_Browse.Margin = new Padding(6);
            btn_Browse.Name = "btn_Browse";
            btn_Browse.Size = new Size(144, 50);
            btn_Browse.TabIndex = 5;
            btn_Browse.Text = "浏览";
            btn_Browse.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Microsoft YaHei UI", 10F);
            label3.Location = new Point(26, 119);
            label3.Margin = new Padding(6, 0, 6, 0);
            label3.Name = "label3";
            label3.Size = new Size(492, 45);
            label3.TabIndex = 6;
            label3.Text = "导出子目录（以 , 或 ; 号间隔）:";
            // 
            // tb_ExportSubPath
            // 
            tb_ExportSubPath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tb_ExportSubPath.BackColor = SystemColors.Window;
            tb_ExportSubPath.Font = new Font("Microsoft YaHei UI", 10F);
            tb_ExportSubPath.Location = new Point(26, 170);
            tb_ExportSubPath.Margin = new Padding(6);
            tb_ExportSubPath.Name = "tb_ExportSubPath";
            tb_ExportSubPath.Size = new Size(1632, 50);
            tb_ExportSubPath.TabIndex = 7;
            // 
            // tb_ExportFileType
            // 
            tb_ExportFileType.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tb_ExportFileType.BackColor = SystemColors.Window;
            tb_ExportFileType.Font = new Font("Microsoft YaHei UI", 10F);
            tb_ExportFileType.Location = new Point(26, 277);
            tb_ExportFileType.Margin = new Padding(6);
            tb_ExportFileType.Name = "tb_ExportFileType";
            tb_ExportFileType.Size = new Size(1632, 50);
            tb_ExportFileType.TabIndex = 9;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Microsoft YaHei UI", 10F);
            label4.Location = new Point(26, 226);
            label4.Margin = new Padding(6, 0, 6, 0);
            label4.Name = "label4";
            label4.Size = new Size(601, 45);
            label4.TabIndex = 8;
            label4.Text = "导出文件类型(*.cs, *.lua, *.shader...)：";
            // 
            // lb_ExportAuthor
            // 
            lb_ExportAuthor.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            lb_ExportAuthor.FormattingEnabled = true;
            lb_ExportAuthor.ItemHeight = 40;
            lb_ExportAuthor.Location = new Point(65, 96);
            lb_ExportAuthor.Name = "lb_ExportAuthor";
            lb_ExportAuthor.SelectionMode = SelectionMode.MultiExtended;
            lb_ExportAuthor.Size = new Size(495, 284);
            lb_ExportAuthor.TabIndex = 10;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Microsoft YaHei UI", 10F);
            label5.Location = new Point(65, 48);
            label5.Margin = new Padding(6, 0, 6, 0);
            label5.Name = "label5";
            label5.Size = new Size(164, 45);
            label5.TabIndex = 11;
            label5.Text = "导出账号:";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Microsoft YaHei UI", 10F);
            label6.Location = new Point(755, 48);
            label6.Margin = new Padding(6, 0, 6, 0);
            label6.Name = "label6";
            label6.Size = new Size(164, 45);
            label6.TabIndex = 13;
            label6.Text = "所有账号:";
            // 
            // lb_AllAuthor
            // 
            lb_AllAuthor.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            lb_AllAuthor.FormattingEnabled = true;
            lb_AllAuthor.ItemHeight = 40;
            lb_AllAuthor.Location = new Point(755, 96);
            lb_AllAuthor.Name = "lb_AllAuthor";
            lb_AllAuthor.SelectionMode = SelectionMode.MultiExtended;
            lb_AllAuthor.Size = new Size(495, 284);
            lb_AllAuthor.TabIndex = 12;
            // 
            // btn_AddToExport
            // 
            btn_AddToExport.Font = new Font("Microsoft YaHei UI", 10F);
            btn_AddToExport.Location = new Point(586, 137);
            btn_AddToExport.Margin = new Padding(6);
            btn_AddToExport.Name = "btn_AddToExport";
            btn_AddToExport.Size = new Size(144, 58);
            btn_AddToExport.TabIndex = 15;
            btn_AddToExport.Text = "<<--";
            btn_AddToExport.UseVisualStyleBackColor = true;
            // 
            // btn_RemoveForExport
            // 
            btn_RemoveForExport.Font = new Font("Microsoft YaHei UI", 10F);
            btn_RemoveForExport.Location = new Point(586, 257);
            btn_RemoveForExport.Margin = new Padding(6);
            btn_RemoveForExport.Name = "btn_RemoveForExport";
            btn_RemoveForExport.Size = new Size(144, 58);
            btn_RemoveForExport.TabIndex = 16;
            btn_RemoveForExport.Text = "-->>";
            btn_RemoveForExport.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            groupBox1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBox1.BackColor = Color.RosyBrown;
            groupBox1.Controls.Add(cb_ExportVersion);
            groupBox1.Controls.Add(tb_ProjectPath);
            groupBox1.Controls.Add(btn_Browse);
            groupBox1.Controls.Add(label3);
            groupBox1.Controls.Add(label4);
            groupBox1.Controls.Add(tb_ExportSubPath);
            groupBox1.Controls.Add(tb_ExportFileType);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(tb_VersionNum);
            groupBox1.Controls.Add(label1);
            groupBox1.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
            groupBox1.Location = new Point(6, 5);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(1679, 422);
            groupBox1.TabIndex = 17;
            groupBox1.TabStop = false;
            groupBox1.Text = "版本导出";
            // 
            // cb_ExportVersion
            // 
            cb_ExportVersion.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            cb_ExportVersion.AutoSize = true;
            cb_ExportVersion.Checked = true;
            cb_ExportVersion.CheckState = CheckState.Checked;
            cb_ExportVersion.Font = new Font("Microsoft YaHei UI", 10F);
            cb_ExportVersion.Location = new Point(1215, 339);
            cb_ExportVersion.Name = "cb_ExportVersion";
            cb_ExportVersion.Size = new Size(194, 49);
            cb_ExportVersion.TabIndex = 10;
            cb_ExportVersion.Text = "是否导出";
            cb_ExportVersion.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            groupBox2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBox2.BackColor = Color.DarkKhaki;
            groupBox2.Controls.Add(cb_ExportModify);
            groupBox2.Controls.Add(btn_DebugMode);
            groupBox2.Controls.Add(btn_RemoveAuthor);
            groupBox2.Controls.Add(btn_AddAuthor);
            groupBox2.Controls.Add(tb_BeginVersionNum);
            groupBox2.Controls.Add(label7);
            groupBox2.Controls.Add(lb_AllAuthor);
            groupBox2.Controls.Add(lb_ExportAuthor);
            groupBox2.Controls.Add(btn_RemoveForExport);
            groupBox2.Controls.Add(label5);
            groupBox2.Controls.Add(btn_AddToExport);
            groupBox2.Controls.Add(label6);
            groupBox2.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
            groupBox2.Location = new Point(6, 427);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(1679, 488);
            groupBox2.TabIndex = 18;
            groupBox2.TabStop = false;
            groupBox2.Text = "账号修改导出";
            // 
            // cb_ExportModify
            // 
            cb_ExportModify.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            cb_ExportModify.AutoSize = true;
            cb_ExportModify.Font = new Font("Microsoft YaHei UI", 10F);
            cb_ExportModify.Location = new Point(1215, 416);
            cb_ExportModify.Name = "cb_ExportModify";
            cb_ExportModify.Size = new Size(194, 49);
            cb_ExportModify.TabIndex = 11;
            cb_ExportModify.Text = "是否导出";
            cb_ExportModify.UseVisualStyleBackColor = true;
            // 
            // btn_DebugMode
            // 
            btn_DebugMode.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btn_DebugMode.AutoSize = true;
            btn_DebugMode.Font = new Font("Microsoft YaHei UI", 9F);
            btn_DebugMode.Location = new Point(1464, 418);
            btn_DebugMode.Name = "btn_DebugMode";
            btn_DebugMode.Size = new Size(175, 43);
            btn_DebugMode.TabIndex = 21;
            btn_DebugMode.Text = "调试模式";
            btn_DebugMode.UseVisualStyleBackColor = true;
            // 
            // btn_RemoveAuthor
            // 
            btn_RemoveAuthor.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btn_RemoveAuthor.Font = new Font("Microsoft YaHei UI", 10F);
            btn_RemoveAuthor.Location = new Point(1326, 206);
            btn_RemoveAuthor.Margin = new Padding(6);
            btn_RemoveAuthor.Name = "btn_RemoveAuthor";
            btn_RemoveAuthor.Size = new Size(260, 82);
            btn_RemoveAuthor.TabIndex = 17;
            btn_RemoveAuthor.Text = "移除账号";
            btn_RemoveAuthor.UseVisualStyleBackColor = true;
            // 
            // btn_AddAuthor
            // 
            btn_AddAuthor.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btn_AddAuthor.Font = new Font("Microsoft YaHei UI", 10F);
            btn_AddAuthor.Location = new Point(1326, 96);
            btn_AddAuthor.Margin = new Padding(6);
            btn_AddAuthor.Name = "btn_AddAuthor";
            btn_AddAuthor.Size = new Size(260, 82);
            btn_AddAuthor.TabIndex = 10;
            btn_AddAuthor.Text = "添加账号";
            btn_AddAuthor.UseVisualStyleBackColor = true;
            // 
            // tb_BeginVersionNum
            // 
            tb_BeginVersionNum.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            tb_BeginVersionNum.Font = new Font("Microsoft YaHei UI", 10F);
            tb_BeginVersionNum.Location = new Point(702, 416);
            tb_BeginVersionNum.Margin = new Padding(6);
            tb_BeginVersionNum.Name = "tb_BeginVersionNum";
            tb_BeginVersionNum.Size = new Size(257, 50);
            tb_BeginVersionNum.TabIndex = 10;
            tb_BeginVersionNum.TextAlign = HorizontalAlignment.Right;
            // 
            // label7
            // 
            label7.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            label7.AutoSize = true;
            label7.Font = new Font("Microsoft YaHei UI", 10F);
            label7.Location = new Point(65, 416);
            label7.Margin = new Padding(6, 0, 6, 0);
            label7.Name = "label7";
            label7.Size = new Size(606, 45);
            label7.TabIndex = 11;
            label7.Text = "起始版本号（为空则导出今天的修改）:";
            // 
            // rtb_Console
            // 
            rtb_Console.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            rtb_Console.BackColor = Color.Black;
            rtb_Console.ForeColor = Color.LawnGreen;
            rtb_Console.Location = new Point(6, 1036);
            rtb_Console.Name = "rtb_Console";
            rtb_Console.ReadOnly = true;
            rtb_Console.Size = new Size(1679, 441);
            rtb_Console.TabIndex = 19;
            rtb_Console.Text = "";
            // 
            // btn_SaveConfig
            // 
            btn_SaveConfig.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btn_SaveConfig.Font = new Font("Microsoft YaHei UI", 10F);
            btn_SaveConfig.Location = new Point(1482, 22);
            btn_SaveConfig.Name = "btn_SaveConfig";
            btn_SaveConfig.Size = new Size(168, 76);
            btn_SaveConfig.TabIndex = 20;
            btn_SaveConfig.Text = "保存配置";
            btn_SaveConfig.UseVisualStyleBackColor = true;
            // 
            // tb_ExportPath
            // 
            tb_ExportPath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tb_ExportPath.BackColor = SystemColors.Control;
            tb_ExportPath.Font = new Font("Microsoft YaHei UI", 10F);
            tb_ExportPath.Location = new Point(189, 48);
            tb_ExportPath.Margin = new Padding(6);
            tb_ExportPath.Name = "tb_ExportPath";
            tb_ExportPath.ReadOnly = true;
            tb_ExportPath.Size = new Size(894, 50);
            tb_ExportPath.TabIndex = 12;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Font = new Font("Microsoft YaHei UI", 10F);
            label8.Location = new Point(26, 48);
            label8.Margin = new Padding(6, 0, 6, 0);
            label8.Name = "label8";
            label8.Size = new Size(164, 45);
            label8.TabIndex = 11;
            label8.Text = "输出目录:";
            // 
            // groupBox3
            // 
            groupBox3.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBox3.BackColor = Color.DarkSeaGreen;
            groupBox3.Controls.Add(btn_Browse2);
            groupBox3.Controls.Add(tb_ExportPath);
            groupBox3.Controls.Add(btn_SaveConfig);
            groupBox3.Controls.Add(label8);
            groupBox3.Controls.Add(btn_Export);
            groupBox3.Location = new Point(6, 915);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(1679, 121);
            groupBox3.TabIndex = 22;
            groupBox3.TabStop = false;
            groupBox3.Text = "输出";
            // 
            // btn_Browse2
            // 
            btn_Browse2.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btn_Browse2.Font = new Font("Microsoft YaHei UI", 9F);
            btn_Browse2.Location = new Point(1119, 48);
            btn_Browse2.Margin = new Padding(6);
            btn_Browse2.Name = "btn_Browse2";
            btn_Browse2.Size = new Size(144, 50);
            btn_Browse2.TabIndex = 11;
            btn_Browse2.Text = "浏览";
            btn_Browse2.UseVisualStyleBackColor = true;
            // 
            // MainWnd
            // 
            AutoScaleDimensions = new SizeF(18F, 39F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1691, 1482);
            Controls.Add(groupBox3);
            Controls.Add(rtb_Console);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Margin = new Padding(6);
            MinimumSize = new Size(1450, 1200);
            Name = "MainWnd";
            Text = "SvnExporter    by 陈小猫 izchemo@gmail.com";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TextBox tb_VersionNum;
        private Label label1;
        private Button btn_Export;
        private Label label2;
        private TextBox tb_ProjectPath;
        private Button btn_Browse;
        private Label label3;
        private TextBox tb_ExportSubPath;
        private TextBox tb_ExportFileType;
        private Label label4;
        private ListBox lb_ExportAuthor;
        private Label label5;
        private Label label6;
        private ListBox lb_AllAuthor;
        private Button btn_AddToExport;
        private Button btn_RemoveForExport;
        private GroupBox groupBox1;
        private GroupBox groupBox2;
        private TextBox tb_BeginVersionNum;
        private Label label7;
        private Button btn_AddAuthor;
        private CheckBox cb_ExportVersion;
        private Button btn_RemoveAuthor;
        private CheckBox cb_ExportModify;
        private RichTextBox rtb_Console;
        private Button btn_SaveConfig;
        private CheckBox btn_DebugMode;
        private TextBox tb_ExportPath;
        private Label label8;
        private GroupBox groupBox3;
        private Button btn_Browse2;
    }
}
