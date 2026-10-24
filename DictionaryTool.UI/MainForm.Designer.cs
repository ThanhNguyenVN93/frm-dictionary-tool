namespace DictionaryTool.UI
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        private Guna.UI2.WinForms.Guna2TextBox txtSearch;
        private Guna.UI2.WinForms.Guna2Button btnSearch;
        private Guna.UI2.WinForms.Guna2Panel pnlLanguageChip;
        private System.Windows.Forms.Label lblLanguageChip;
        private Guna.UI2.WinForms.Guna2CircleProgressBar circleLoading;
        private System.Windows.Forms.Label lblImportStatus;
        private Guna.UI2.WinForms.Guna2Button btnClipboardToggle;
        private Guna.UI2.WinForms.Guna2Button btnMiniMode;
        private Guna.UI2.WinForms.Guna2Button btnBookmarkToggle;
        private Guna.UI2.WinForms.Guna2Button btnHistory;
        private Guna.UI2.WinForms.Guna2Button btnHotkeyConfig;
        private Guna.UI2.WinForms.Guna2Button btnInfo;
        private System.Windows.Forms.ContextMenuStrip cmsInfo;
        private System.Windows.Forms.ToolStripMenuItem tsmiAbout;
        private System.Windows.Forms.ToolStripMenuItem tsmiGopY;
        private System.Windows.Forms.ToolStripMenuItem tsmiDongGop;
        private System.Windows.Forms.ToolTip toolTipToolbar;
        private System.Windows.Forms.ToolStripMenuItem tsmiLangToggle;
        private System.Windows.Forms.ToolStripSeparator tsmiSepLang;

        // Cột EN
        private DictionaryTool.UI.FlickerFreePanel pnlColumnEn;
        private DictionaryTool.UI.FlickerFreeFlowLayoutPanel pnlColumnEnContent;
        private System.Windows.Forms.Label lblEnColumnHeader;
        private System.Windows.Forms.Label lblWord;
        private DictionaryTool.UI.FlickerFreeFlowLayoutPanel pnlPhonetics;
        private Guna.UI2.WinForms.Guna2Separator sepAfterPhonetics;
        private System.Windows.Forms.Label lblDefinitionsHeader;
        private DictionaryTool.UI.FlickerFreeFlowLayoutPanel pnlDefinitions;
        private Guna.UI2.WinForms.Guna2Separator sepAfterDefinitions;
        private System.Windows.Forms.Label lblLearnerHeader;
        private DictionaryTool.UI.FlickerFreeFlowLayoutPanel pnlLearnerDefs;

        // Cột VI
        private DictionaryTool.UI.FlickerFreePanel pnlColumnVi;
        private DictionaryTool.UI.FlickerFreeFlowLayoutPanel pnlColumnViContent;
        private System.Windows.Forms.Label lblViColumnHeader;
        private System.Windows.Forms.Label lblViWord;
        private DictionaryTool.UI.FlickerFreeFlowLayoutPanel pnlViDefinitions;

        // Cột ZH
        private DictionaryTool.UI.FlickerFreePanel pnlColumnZh;
        private DictionaryTool.UI.FlickerFreeFlowLayoutPanel pnlColumnZhContent;
        private System.Windows.Forms.Label lblZhColumnHeader;
        private System.Windows.Forms.Label lblZhWord;
        private System.Windows.Forms.Label lblZhPinyin;
        private DictionaryTool.UI.FlickerFreeFlowLayoutPanel pnlZhDefinitions;

        // Câu ví dụ (dùng chung cho cả 3 cột, nằm dưới cùng)
        private DictionaryTool.UI.FlickerFreePanel pnlExamples;
        private DictionaryTool.UI.FlickerFreeFlowLayoutPanel pnlExamplesContent;
        private System.Windows.Forms.Label lblExamplesHeader;

        private DictionaryTool.UI.FlickerFreePanel pnlError;
        private System.Windows.Forms.Label lblErrorMessage;
        private Guna.UI2.WinForms.Guna2Button btnRetry;

        private System.Windows.Forms.Label lblUsage;

        private System.Windows.Forms.Timer debounceTimer;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.txtSearch = new Guna.UI2.WinForms.Guna2TextBox();
            this.btnSearch = new Guna.UI2.WinForms.Guna2Button();
            this.pnlLanguageChip = new Guna.UI2.WinForms.Guna2Panel();
            this.lblLanguageChip = new System.Windows.Forms.Label();
            this.circleLoading = new Guna.UI2.WinForms.Guna2CircleProgressBar();
            this.lblImportStatus = new System.Windows.Forms.Label();
            this.btnClipboardToggle = new Guna.UI2.WinForms.Guna2Button();
            this.btnMiniMode = new Guna.UI2.WinForms.Guna2Button();
            this.btnBookmarkToggle = new Guna.UI2.WinForms.Guna2Button();
            this.btnHistory = new Guna.UI2.WinForms.Guna2Button();
            this.btnHotkeyConfig = new Guna.UI2.WinForms.Guna2Button();
            this.btnInfo = new Guna.UI2.WinForms.Guna2Button();
            this.cmsInfo = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiGopY = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiDongGop = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiSepLang = new System.Windows.Forms.ToolStripSeparator();
            this.tsmiLangToggle = new System.Windows.Forms.ToolStripMenuItem();
            this.toolTipToolbar = new System.Windows.Forms.ToolTip(this.components);
            this.btnRetry = new Guna.UI2.WinForms.Guna2Button();
            this.pnlColumnEn = new DictionaryTool.UI.FlickerFreePanel();
            this.pnlColumnEnContent = new DictionaryTool.UI.FlickerFreeFlowLayoutPanel();
            this.lblEnColumnHeader = new System.Windows.Forms.Label();
            this.lblWord = new System.Windows.Forms.Label();
            this.pnlPhonetics = new DictionaryTool.UI.FlickerFreeFlowLayoutPanel();
            this.sepAfterPhonetics = new Guna.UI2.WinForms.Guna2Separator();
            this.lblDefinitionsHeader = new System.Windows.Forms.Label();
            this.pnlDefinitions = new DictionaryTool.UI.FlickerFreeFlowLayoutPanel();
            this.sepAfterDefinitions = new Guna.UI2.WinForms.Guna2Separator();
            this.lblLearnerHeader = new System.Windows.Forms.Label();
            this.pnlLearnerDefs = new DictionaryTool.UI.FlickerFreeFlowLayoutPanel();
            this.pnlColumnVi = new DictionaryTool.UI.FlickerFreePanel();
            this.pnlColumnViContent = new DictionaryTool.UI.FlickerFreeFlowLayoutPanel();
            this.lblViColumnHeader = new System.Windows.Forms.Label();
            this.lblViWord = new System.Windows.Forms.Label();
            this.pnlViDefinitions = new DictionaryTool.UI.FlickerFreeFlowLayoutPanel();
            this.pnlColumnZh = new DictionaryTool.UI.FlickerFreePanel();
            this.pnlColumnZhContent = new DictionaryTool.UI.FlickerFreeFlowLayoutPanel();
            this.lblZhColumnHeader = new System.Windows.Forms.Label();
            this.lblZhWord = new System.Windows.Forms.Label();
            this.lblZhPinyin = new System.Windows.Forms.Label();
            this.pnlZhDefinitions = new DictionaryTool.UI.FlickerFreeFlowLayoutPanel();
            this.pnlExamples = new DictionaryTool.UI.FlickerFreePanel();
            this.pnlExamplesContent = new DictionaryTool.UI.FlickerFreeFlowLayoutPanel();
            this.lblExamplesHeader = new System.Windows.Forms.Label();
            this.pnlError = new DictionaryTool.UI.FlickerFreePanel();
            this.lblErrorMessage = new System.Windows.Forms.Label();
            this.lblUsage = new System.Windows.Forms.Label();
            this.debounceTimer = new System.Windows.Forms.Timer(this.components);
            this.pnlLanguageChip.SuspendLayout();
            this.cmsInfo.SuspendLayout();
            this.pnlColumnEn.SuspendLayout();
            this.pnlColumnEnContent.SuspendLayout();
            this.pnlColumnVi.SuspendLayout();
            this.pnlColumnViContent.SuspendLayout();
            this.pnlColumnZh.SuspendLayout();
            this.pnlColumnZhContent.SuspendLayout();
            this.pnlExamples.SuspendLayout();
            this.pnlExamplesContent.SuspendLayout();
            this.pnlError.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtSearch
            // 
            this.txtSearch.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.txtSearch.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.txtSearch.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(52)))), ((int)(((byte)(63)))));
            this.txtSearch.BorderRadius = 14;
            this.txtSearch.BorderThickness = 2;
            this.txtSearch.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtSearch.DefaultText = "";
            this.txtSearch.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(33)))), ((int)(((byte)(42)))));
            this.txtSearch.FocusedState.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(124)))), ((int)(((byte)(140)))), ((int)(((byte)(255)))));
            this.txtSearch.Font = new System.Drawing.Font("Segoe UI", 11.5F);
            this.txtSearch.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(238)))), ((int)(((byte)(242)))));
            this.txtSearch.HoverState.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(190)))), ((int)(((byte)(197)))), ((int)(((byte)(219)))));
            this.txtSearch.Location = new System.Drawing.Point(24, 24);
            this.txtSearch.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.PlaceholderForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(163)))), ((int)(((byte)(175)))));
            this.txtSearch.PlaceholderText = "🔍  Nhập từ EN / VI / ZH cần tra...";
            this.txtSearch.SelectedText = "";
            this.txtSearch.Size = new System.Drawing.Size(360, 46);
            this.txtSearch.TabIndex = 13;
            this.txtSearch.TextOffset = new System.Drawing.Point(4, 0);
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
            // 
            // btnSearch
            // 
            this.btnSearch.BorderRadius = 14;
            this.btnSearch.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSearch.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(124)))), ((int)(((byte)(140)))), ((int)(((byte)(255)))));
            this.btnSearch.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
            this.btnSearch.ForeColor = System.Drawing.Color.White;
            this.btnSearch.HoverState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(145)))), ((int)(((byte)(159)))), ((int)(((byte)(255)))));
            this.btnSearch.Location = new System.Drawing.Point(456, 24);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.PressedColor = System.Drawing.Color.FromArgb(((int)(((byte)(102)))), ((int)(((byte)(118)))), ((int)(((byte)(230)))));
            this.btnSearch.Size = new System.Drawing.Size(100, 46);
            this.btnSearch.TabIndex = 15;
            this.btnSearch.Text = "Tra từ";
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // pnlLanguageChip
            // 
            this.pnlLanguageChip.BorderRadius = 12;
            this.pnlLanguageChip.Controls.Add(this.lblLanguageChip);
            this.pnlLanguageChip.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pnlLanguageChip.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(56)))), ((int)(((byte)(84)))));
            this.pnlLanguageChip.Location = new System.Drawing.Point(394, 36);
            this.pnlLanguageChip.Name = "pnlLanguageChip";
            this.pnlLanguageChip.Size = new System.Drawing.Size(52, 24);
            this.pnlLanguageChip.TabIndex = 12;
            this.pnlLanguageChip.Click += new System.EventHandler(this.pnlLanguageChip_Click);
            // 
            // lblLanguageChip
            // 
            this.lblLanguageChip.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblLanguageChip.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblLanguageChip.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblLanguageChip.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(124)))), ((int)(((byte)(140)))), ((int)(((byte)(255)))));
            this.lblLanguageChip.Location = new System.Drawing.Point(0, 0);
            this.lblLanguageChip.Name = "lblLanguageChip";
            this.lblLanguageChip.Size = new System.Drawing.Size(52, 24);
            this.lblLanguageChip.TabIndex = 0;
            this.lblLanguageChip.Text = "EN";
            this.lblLanguageChip.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblLanguageChip.Click += new System.EventHandler(this.pnlLanguageChip_Click);
            // 
            // circleLoading
            // 
            this.circleLoading.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(20)))), ((int)(((byte)(26)))));
            this.circleLoading.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(213)))), ((int)(((byte)(218)))), ((int)(((byte)(223)))));
            this.circleLoading.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.circleLoading.ForeColor = System.Drawing.Color.White;
            this.circleLoading.Location = new System.Drawing.Point(483, 24);
            this.circleLoading.Minimum = 0;
            this.circleLoading.Name = "circleLoading";
            this.circleLoading.ProgressColor = System.Drawing.Color.FromArgb(((int)(((byte)(124)))), ((int)(((byte)(140)))), ((int)(((byte)(255)))));
            this.circleLoading.ProgressColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(145)))), ((int)(((byte)(159)))), ((int)(((byte)(255)))));
            this.circleLoading.ProgressThickness = 5;
            this.circleLoading.Size = new System.Drawing.Size(46, 46);
            this.circleLoading.TabIndex = 16;
            this.circleLoading.Value = 70;
            this.circleLoading.Visible = false;
            // 
            // lblImportStatus
            // 
            this.lblImportStatus.AutoSize = true;
            this.lblImportStatus.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic);
            this.lblImportStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(163)))), ((int)(((byte)(175)))));
            this.lblImportStatus.Location = new System.Drawing.Point(24, 72);
            this.lblImportStatus.Name = "lblImportStatus";
            this.lblImportStatus.Size = new System.Drawing.Size(0, 15);
            this.lblImportStatus.TabIndex = 5;
            this.lblImportStatus.Visible = false;
            // 
            // btnClipboardToggle
            // 
            this.btnClipboardToggle.BorderRadius = 14;
            this.btnClipboardToggle.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClipboardToggle.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(50)))), ((int)(((byte)(61)))));
            this.btnClipboardToggle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnClipboardToggle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(163)))), ((int)(((byte)(175)))));
            this.btnClipboardToggle.HoverState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(70)))), ((int)(((byte)(104)))));
            this.btnClipboardToggle.Location = new System.Drawing.Point(564, 24);
            this.btnClipboardToggle.Name = "btnClipboardToggle";
            this.btnClipboardToggle.Size = new System.Drawing.Size(88, 46);
            this.btnClipboardToggle.TabIndex = 10;
            this.btnClipboardToggle.Text = "📋 Tắt";
            this.btnClipboardToggle.Click += new System.EventHandler(this.btnClipboardToggle_Click);
            // 
            // btnMiniMode
            // 
            this.btnMiniMode.BorderRadius = 14;
            this.btnMiniMode.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnMiniMode.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(50)))), ((int)(((byte)(61)))));
            this.btnMiniMode.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnMiniMode.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(163)))), ((int)(((byte)(175)))));
            this.btnMiniMode.HoverState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(70)))), ((int)(((byte)(104)))));
            this.btnMiniMode.Location = new System.Drawing.Point(714, 24);
            this.btnMiniMode.Name = "btnMiniMode";
            this.btnMiniMode.Size = new System.Drawing.Size(80, 46);
            this.btnMiniMode.TabIndex = 9;
            this.btnMiniMode.Text = "⤢ Mini";
            this.btnMiniMode.Click += new System.EventHandler(this.btnMiniMode_Click);
            // 
            // btnBookmarkToggle
            // 
            this.btnBookmarkToggle.BorderRadius = 14;
            this.btnBookmarkToggle.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnBookmarkToggle.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(50)))), ((int)(((byte)(61)))));
            this.btnBookmarkToggle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnBookmarkToggle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(163)))), ((int)(((byte)(175)))));
            this.btnBookmarkToggle.HoverState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(70)))), ((int)(((byte)(104)))));
            this.btnBookmarkToggle.Location = new System.Drawing.Point(802, 24);
            this.btnBookmarkToggle.Name = "btnBookmarkToggle";
            this.btnBookmarkToggle.Size = new System.Drawing.Size(56, 46);
            this.btnBookmarkToggle.TabIndex = 7;
            this.btnBookmarkToggle.Text = "☆";
            this.btnBookmarkToggle.Click += new System.EventHandler(this.btnBookmarkToggle_Click);
            // 
            // btnHistory
            // 
            this.btnHistory.BorderRadius = 14;
            this.btnHistory.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnHistory.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(50)))), ((int)(((byte)(61)))));
            this.btnHistory.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.btnHistory.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(163)))), ((int)(((byte)(175)))));
            this.btnHistory.HoverState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(70)))), ((int)(((byte)(104)))));
            this.btnHistory.Location = new System.Drawing.Point(866, 24);
            this.btnHistory.Name = "btnHistory";
            this.btnHistory.Size = new System.Drawing.Size(56, 46);
            this.btnHistory.TabIndex = 6;
            this.btnHistory.Text = "🕘";
            this.btnHistory.Click += new System.EventHandler(this.btnHistory_Click);
            // 
            // btnHotkeyConfig
            // 
            this.btnHotkeyConfig.BorderRadius = 14;
            this.btnHotkeyConfig.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnHotkeyConfig.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(50)))), ((int)(((byte)(61)))));
            this.btnHotkeyConfig.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.btnHotkeyConfig.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(163)))), ((int)(((byte)(175)))));
            this.btnHotkeyConfig.HoverState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(70)))), ((int)(((byte)(104)))));
            this.btnHotkeyConfig.Location = new System.Drawing.Point(660, 24);
            this.btnHotkeyConfig.Name = "btnHotkeyConfig";
            this.btnHotkeyConfig.Size = new System.Drawing.Size(46, 46);
            this.btnHotkeyConfig.TabIndex = 8;
            this.btnHotkeyConfig.Text = "⚙";
            this.btnHotkeyConfig.Click += new System.EventHandler(this.btnHotkeyConfig_Click);
            // 
            // btnInfo
            // 
            this.btnInfo.BorderRadius = 14;
            this.btnInfo.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnInfo.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(50)))), ((int)(((byte)(61)))));
            this.btnInfo.Font = new System.Drawing.Font("Segoe UI", 13F);
            this.btnInfo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(163)))), ((int)(((byte)(175)))));
            this.btnInfo.HoverState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(70)))), ((int)(((byte)(104)))));
            this.btnInfo.Location = new System.Drawing.Point(930, 24);
            this.btnInfo.Name = "btnInfo";
            this.btnInfo.Size = new System.Drawing.Size(46, 46);
            this.btnInfo.TabIndex = 11;
            this.btnInfo.Text = "ℹ";
            this.btnInfo.Click += new System.EventHandler(this.btnInfo_Click);
            // 
            // cmsInfo
            // 
            this.cmsInfo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(33)))), ((int)(((byte)(42)))));
            this.cmsInfo.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiAbout,
            this.tsmiGopY,
            this.tsmiDongGop,
            this.tsmiSepLang,
            this.tsmiLangToggle});
            this.cmsInfo.Name = "cmsInfo";
            this.cmsInfo.ShowImageMargin = false;
            this.cmsInfo.Size = new System.Drawing.Size(158, 98);
            // 
            // tsmiAbout
            // 
            this.tsmiAbout.Name = "tsmiAbout";
            this.tsmiAbout.Size = new System.Drawing.Size(157, 22);
            this.tsmiAbout.Text = "ℹ️  Giới thiệu";
            this.tsmiAbout.Click += new System.EventHandler(this.tsmiAbout_Click);
            // 
            // tsmiGopY
            // 
            this.tsmiGopY.Name = "tsmiGopY";
            this.tsmiGopY.Size = new System.Drawing.Size(157, 22);
            this.tsmiGopY.Text = "💬  Góp ý";
            this.tsmiGopY.Click += new System.EventHandler(this.tsmiGopY_Click);
            // 
            // tsmiDongGop
            // 
            this.tsmiDongGop.Name = "tsmiDongGop";
            this.tsmiDongGop.Size = new System.Drawing.Size(157, 22);
            this.tsmiDongGop.Text = "🤝  Đóng góp";
            this.tsmiDongGop.Click += new System.EventHandler(this.tsmiDongGop_Click);
            // 
            // tsmiSepLang
            // 
            this.tsmiSepLang.Name = "tsmiSepLang";
            this.tsmiSepLang.Size = new System.Drawing.Size(154, 6);
            // 
            // tsmiLangToggle
            // 
            this.tsmiLangToggle.Name = "tsmiLangToggle";
            this.tsmiLangToggle.Size = new System.Drawing.Size(157, 22);
            this.tsmiLangToggle.Text = "🌐  Switch to English";
            this.tsmiLangToggle.Click += new System.EventHandler(this.tsmiLangToggle_Click);
            // 
            // btnRetry
            // 
            this.btnRetry.BorderRadius = 14;
            this.btnRetry.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRetry.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(124)))), ((int)(((byte)(140)))), ((int)(((byte)(255)))));
            this.btnRetry.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnRetry.ForeColor = System.Drawing.Color.White;
            this.btnRetry.HoverState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(145)))), ((int)(((byte)(159)))), ((int)(((byte)(255)))));
            this.btnRetry.Location = new System.Drawing.Point(456, 24);
            this.btnRetry.Name = "btnRetry";
            this.btnRetry.PressedColor = System.Drawing.Color.FromArgb(((int)(((byte)(102)))), ((int)(((byte)(118)))), ((int)(((byte)(230)))));
            this.btnRetry.Size = new System.Drawing.Size(100, 46);
            this.btnRetry.TabIndex = 17;
            this.btnRetry.Text = "Thử lại";
            this.toolTipToolbar.SetToolTip(this.btnRetry, "Tra lại từ sau khi gặp lỗi");
            this.btnRetry.Visible = false;
            this.btnRetry.Click += new System.EventHandler(this.btnRetry_Click);
            // 
            // pnlColumnEn
            // 
            this.pnlColumnEn.AutoScroll = true;
            this.pnlColumnEn.BackColor = System.Drawing.Color.Transparent;
            this.pnlColumnEn.BorderRadius = 18;
            this.pnlColumnEn.Controls.Add(this.pnlColumnEnContent);
            this.pnlColumnEn.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(33)))), ((int)(((byte)(42)))));
            this.pnlColumnEn.Location = new System.Drawing.Point(20, 90);
            this.pnlColumnEn.Name = "pnlColumnEn";
            this.pnlColumnEn.Padding = new System.Windows.Forms.Padding(20, 18, 20, 18);
            this.pnlColumnEn.ShadowDecoration.Color = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(30)))), ((int)(((byte)(40)))), ((int)(((byte)(60)))));
            this.pnlColumnEn.ShadowDecoration.Depth = 25;
            this.pnlColumnEn.ShadowDecoration.Enabled = true;
            this.pnlColumnEn.Size = new System.Drawing.Size(300, 380);
            this.pnlColumnEn.TabIndex = 4;
            // 
            // pnlColumnEnContent
            // 
            this.pnlColumnEnContent.AutoSize = true;
            this.pnlColumnEnContent.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlColumnEnContent.Controls.Add(this.lblEnColumnHeader);
            this.pnlColumnEnContent.Controls.Add(this.lblWord);
            this.pnlColumnEnContent.Controls.Add(this.pnlPhonetics);
            this.pnlColumnEnContent.Controls.Add(this.sepAfterPhonetics);
            this.pnlColumnEnContent.Controls.Add(this.lblDefinitionsHeader);
            this.pnlColumnEnContent.Controls.Add(this.pnlDefinitions);
            this.pnlColumnEnContent.Controls.Add(this.sepAfterDefinitions);
            this.pnlColumnEnContent.Controls.Add(this.lblLearnerHeader);
            this.pnlColumnEnContent.Controls.Add(this.pnlLearnerDefs);
            this.pnlColumnEnContent.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlColumnEnContent.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.pnlColumnEnContent.Location = new System.Drawing.Point(20, 18);
            this.pnlColumnEnContent.Name = "pnlColumnEnContent";
            this.pnlColumnEnContent.Size = new System.Drawing.Size(260, 191);
            this.pnlColumnEnContent.TabIndex = 0;
            this.pnlColumnEnContent.WrapContents = false;
            // 
            // lblEnColumnHeader
            // 
            this.lblEnColumnHeader.AutoSize = true;
            this.lblEnColumnHeader.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblEnColumnHeader.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(163)))), ((int)(((byte)(175)))));
            this.lblEnColumnHeader.Location = new System.Drawing.Point(0, 0);
            this.lblEnColumnHeader.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.lblEnColumnHeader.Name = "lblEnColumnHeader";
            this.lblEnColumnHeader.Size = new System.Drawing.Size(81, 17);
            this.lblEnColumnHeader.TabIndex = 0;
            this.lblEnColumnHeader.Text = "🇬🇧 ENGLISH";
            // 
            // lblWord
            // 
            this.lblWord.AutoSize = true;
            this.lblWord.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
            this.lblWord.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(238)))), ((int)(((byte)(242)))));
            this.lblWord.Location = new System.Drawing.Point(0, 27);
            this.lblWord.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.lblWord.MaximumSize = new System.Drawing.Size(260, 0);
            this.lblWord.Name = "lblWord";
            this.lblWord.Size = new System.Drawing.Size(0, 28);
            this.lblWord.TabIndex = 1;
            // 
            // pnlPhonetics
            // 
            this.pnlPhonetics.AutoSize = true;
            this.pnlPhonetics.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlPhonetics.Location = new System.Drawing.Point(0, 65);
            this.pnlPhonetics.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
            this.pnlPhonetics.Name = "pnlPhonetics";
            this.pnlPhonetics.Size = new System.Drawing.Size(0, 0);
            this.pnlPhonetics.TabIndex = 2;
            // 
            // sepAfterPhonetics
            // 
            this.sepAfterPhonetics.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(52)))), ((int)(((byte)(63)))));
            this.sepAfterPhonetics.Location = new System.Drawing.Point(0, 77);
            this.sepAfterPhonetics.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
            this.sepAfterPhonetics.Name = "sepAfterPhonetics";
            this.sepAfterPhonetics.Size = new System.Drawing.Size(260, 10);
            this.sepAfterPhonetics.TabIndex = 3;
            // 
            // lblDefinitionsHeader
            // 
            this.lblDefinitionsHeader.AutoSize = true;
            this.lblDefinitionsHeader.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblDefinitionsHeader.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(124)))), ((int)(((byte)(140)))), ((int)(((byte)(255)))));
            this.lblDefinitionsHeader.Location = new System.Drawing.Point(0, 99);
            this.lblDefinitionsHeader.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.lblDefinitionsHeader.Name = "lblDefinitionsHeader";
            this.lblDefinitionsHeader.Size = new System.Drawing.Size(80, 15);
            this.lblDefinitionsHeader.TabIndex = 4;
            this.lblDefinitionsHeader.Text = "ĐỊNH NGHĨA";
            // 
            // pnlDefinitions
            // 
            this.pnlDefinitions.AutoSize = true;
            this.pnlDefinitions.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlDefinitions.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.pnlDefinitions.Location = new System.Drawing.Point(0, 122);
            this.pnlDefinitions.Margin = new System.Windows.Forms.Padding(0, 0, 0, 14);
            this.pnlDefinitions.Name = "pnlDefinitions";
            this.pnlDefinitions.Size = new System.Drawing.Size(0, 0);
            this.pnlDefinitions.TabIndex = 5;
            this.pnlDefinitions.WrapContents = false;
            // 
            // sepAfterDefinitions
            // 
            this.sepAfterDefinitions.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(52)))), ((int)(((byte)(63)))));
            this.sepAfterDefinitions.Location = new System.Drawing.Point(0, 136);
            this.sepAfterDefinitions.Margin = new System.Windows.Forms.Padding(0, 0, 0, 14);
            this.sepAfterDefinitions.Name = "sepAfterDefinitions";
            this.sepAfterDefinitions.Size = new System.Drawing.Size(260, 10);
            this.sepAfterDefinitions.TabIndex = 6;
            // 
            // lblLearnerHeader
            // 
            this.lblLearnerHeader.AutoSize = true;
            this.lblLearnerHeader.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblLearnerHeader.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(124)))), ((int)(((byte)(140)))), ((int)(((byte)(255)))));
            this.lblLearnerHeader.Location = new System.Drawing.Point(0, 160);
            this.lblLearnerHeader.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.lblLearnerHeader.Name = "lblLearnerHeader";
            this.lblLearnerHeader.Size = new System.Drawing.Size(122, 15);
            this.lblLearnerHeader.TabIndex = 7;
            this.lblLearnerHeader.Text = "MERRIAM-WEBSTER";
            // 
            // pnlLearnerDefs
            // 
            this.pnlLearnerDefs.AutoSize = true;
            this.pnlLearnerDefs.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlLearnerDefs.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.pnlLearnerDefs.Location = new System.Drawing.Point(0, 183);
            this.pnlLearnerDefs.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.pnlLearnerDefs.Name = "pnlLearnerDefs";
            this.pnlLearnerDefs.Size = new System.Drawing.Size(0, 0);
            this.pnlLearnerDefs.TabIndex = 8;
            this.pnlLearnerDefs.WrapContents = false;
            // 
            // pnlColumnVi
            // 
            this.pnlColumnVi.AutoScroll = true;
            this.pnlColumnVi.BackColor = System.Drawing.Color.Transparent;
            this.pnlColumnVi.BorderRadius = 18;
            this.pnlColumnVi.Controls.Add(this.pnlColumnViContent);
            this.pnlColumnVi.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(33)))), ((int)(((byte)(42)))));
            this.pnlColumnVi.Location = new System.Drawing.Point(336, 90);
            this.pnlColumnVi.Name = "pnlColumnVi";
            this.pnlColumnVi.Padding = new System.Windows.Forms.Padding(20, 18, 20, 18);
            this.pnlColumnVi.ShadowDecoration.Color = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(30)))), ((int)(((byte)(40)))), ((int)(((byte)(60)))));
            this.pnlColumnVi.ShadowDecoration.Depth = 25;
            this.pnlColumnVi.ShadowDecoration.Enabled = true;
            this.pnlColumnVi.Size = new System.Drawing.Size(300, 380);
            this.pnlColumnVi.TabIndex = 3;
            // 
            // pnlColumnViContent
            // 
            this.pnlColumnViContent.AutoSize = true;
            this.pnlColumnViContent.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlColumnViContent.Controls.Add(this.lblViColumnHeader);
            this.pnlColumnViContent.Controls.Add(this.lblViWord);
            this.pnlColumnViContent.Controls.Add(this.pnlViDefinitions);
            this.pnlColumnViContent.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlColumnViContent.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.pnlColumnViContent.Location = new System.Drawing.Point(20, 18);
            this.pnlColumnViContent.Name = "pnlColumnViContent";
            this.pnlColumnViContent.Size = new System.Drawing.Size(260, 75);
            this.pnlColumnViContent.TabIndex = 0;
            this.pnlColumnViContent.WrapContents = false;
            // 
            // lblViColumnHeader
            // 
            this.lblViColumnHeader.AutoSize = true;
            this.lblViColumnHeader.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblViColumnHeader.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(163)))), ((int)(((byte)(175)))));
            this.lblViColumnHeader.Location = new System.Drawing.Point(0, 0);
            this.lblViColumnHeader.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.lblViColumnHeader.Name = "lblViColumnHeader";
            this.lblViColumnHeader.Size = new System.Drawing.Size(98, 17);
            this.lblViColumnHeader.TabIndex = 0;
            this.lblViColumnHeader.Text = "🇻🇳 TIẾNG VIỆT";
            // 
            // lblViWord
            // 
            this.lblViWord.AutoSize = true;
            this.lblViWord.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
            this.lblViWord.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(238)))), ((int)(((byte)(242)))));
            this.lblViWord.Location = new System.Drawing.Point(0, 27);
            this.lblViWord.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
            this.lblViWord.MaximumSize = new System.Drawing.Size(260, 0);
            this.lblViWord.Name = "lblViWord";
            this.lblViWord.Size = new System.Drawing.Size(0, 28);
            this.lblViWord.TabIndex = 1;
            // 
            // pnlViDefinitions
            // 
            this.pnlViDefinitions.AutoSize = true;
            this.pnlViDefinitions.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlViDefinitions.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.pnlViDefinitions.Location = new System.Drawing.Point(0, 67);
            this.pnlViDefinitions.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.pnlViDefinitions.Name = "pnlViDefinitions";
            this.pnlViDefinitions.Size = new System.Drawing.Size(0, 0);
            this.pnlViDefinitions.TabIndex = 2;
            this.pnlViDefinitions.WrapContents = false;
            // 
            // pnlColumnZh
            // 
            this.pnlColumnZh.AutoScroll = true;
            this.pnlColumnZh.BackColor = System.Drawing.Color.Transparent;
            this.pnlColumnZh.BorderRadius = 18;
            this.pnlColumnZh.Controls.Add(this.pnlColumnZhContent);
            this.pnlColumnZh.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(33)))), ((int)(((byte)(42)))));
            this.pnlColumnZh.Location = new System.Drawing.Point(652, 90);
            this.pnlColumnZh.Name = "pnlColumnZh";
            this.pnlColumnZh.Padding = new System.Windows.Forms.Padding(20, 18, 20, 18);
            this.pnlColumnZh.ShadowDecoration.Color = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(30)))), ((int)(((byte)(40)))), ((int)(((byte)(60)))));
            this.pnlColumnZh.ShadowDecoration.Depth = 25;
            this.pnlColumnZh.ShadowDecoration.Enabled = true;
            this.pnlColumnZh.Size = new System.Drawing.Size(300, 380);
            this.pnlColumnZh.TabIndex = 2;
            // 
            // pnlColumnZhContent
            // 
            this.pnlColumnZhContent.AutoSize = true;
            this.pnlColumnZhContent.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlColumnZhContent.Controls.Add(this.lblZhColumnHeader);
            this.pnlColumnZhContent.Controls.Add(this.lblZhWord);
            this.pnlColumnZhContent.Controls.Add(this.lblZhPinyin);
            this.pnlColumnZhContent.Controls.Add(this.pnlZhDefinitions);
            this.pnlColumnZhContent.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlColumnZhContent.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.pnlColumnZhContent.Location = new System.Drawing.Point(20, 18);
            this.pnlColumnZhContent.Name = "pnlColumnZhContent";
            this.pnlColumnZhContent.Size = new System.Drawing.Size(260, 98);
            this.pnlColumnZhContent.TabIndex = 0;
            this.pnlColumnZhContent.WrapContents = false;
            // 
            // lblZhColumnHeader
            // 
            this.lblZhColumnHeader.AutoSize = true;
            this.lblZhColumnHeader.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblZhColumnHeader.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(163)))), ((int)(((byte)(175)))));
            this.lblZhColumnHeader.Location = new System.Drawing.Point(0, 0);
            this.lblZhColumnHeader.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.lblZhColumnHeader.Name = "lblZhColumnHeader";
            this.lblZhColumnHeader.Size = new System.Drawing.Size(60, 17);
            this.lblZhColumnHeader.TabIndex = 0;
            this.lblZhColumnHeader.Text = "🇨🇳 中文";
            // 
            // lblZhWord
            // 
            this.lblZhWord.AutoSize = true;
            this.lblZhWord.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
            this.lblZhWord.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(238)))), ((int)(((byte)(242)))));
            this.lblZhWord.Location = new System.Drawing.Point(0, 27);
            this.lblZhWord.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
            this.lblZhWord.MaximumSize = new System.Drawing.Size(260, 0);
            this.lblZhWord.Name = "lblZhWord";
            this.lblZhWord.Size = new System.Drawing.Size(0, 28);
            this.lblZhWord.TabIndex = 1;
            // 
            // lblZhPinyin
            // 
            this.lblZhPinyin.AutoSize = true;
            this.lblZhPinyin.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Italic);
            this.lblZhPinyin.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(124)))), ((int)(((byte)(140)))), ((int)(((byte)(255)))));
            this.lblZhPinyin.Location = new System.Drawing.Point(0, 59);
            this.lblZhPinyin.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
            this.lblZhPinyin.Name = "lblZhPinyin";
            this.lblZhPinyin.Size = new System.Drawing.Size(0, 19);
            this.lblZhPinyin.TabIndex = 2;
            // 
            // pnlZhDefinitions
            // 
            this.pnlZhDefinitions.AutoSize = true;
            this.pnlZhDefinitions.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlZhDefinitions.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.pnlZhDefinitions.Location = new System.Drawing.Point(0, 90);
            this.pnlZhDefinitions.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.pnlZhDefinitions.Name = "pnlZhDefinitions";
            this.pnlZhDefinitions.Size = new System.Drawing.Size(0, 0);
            this.pnlZhDefinitions.TabIndex = 3;
            this.pnlZhDefinitions.WrapContents = false;
            // 
            // pnlExamples
            // 
            this.pnlExamples.AutoScroll = true;
            this.pnlExamples.BackColor = System.Drawing.Color.Transparent;
            this.pnlExamples.BorderRadius = 18;
            this.pnlExamples.Controls.Add(this.pnlExamplesContent);
            this.pnlExamples.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(33)))), ((int)(((byte)(42)))));
            this.pnlExamples.Location = new System.Drawing.Point(20, 482);
            this.pnlExamples.Name = "pnlExamples";
            this.pnlExamples.Padding = new System.Windows.Forms.Padding(20, 16, 20, 16);
            this.pnlExamples.ShadowDecoration.Color = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(30)))), ((int)(((byte)(40)))), ((int)(((byte)(60)))));
            this.pnlExamples.ShadowDecoration.Depth = 25;
            this.pnlExamples.ShadowDecoration.Enabled = true;
            this.pnlExamples.Size = new System.Drawing.Size(932, 130);
            this.pnlExamples.TabIndex = 1;
            // 
            // pnlExamplesContent
            // 
            this.pnlExamplesContent.AutoSize = true;
            this.pnlExamplesContent.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlExamplesContent.Controls.Add(this.lblExamplesHeader);
            this.pnlExamplesContent.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlExamplesContent.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.pnlExamplesContent.Location = new System.Drawing.Point(20, 16);
            this.pnlExamplesContent.Name = "pnlExamplesContent";
            this.pnlExamplesContent.Size = new System.Drawing.Size(892, 25);
            this.pnlExamplesContent.TabIndex = 0;
            this.pnlExamplesContent.WrapContents = false;
            // 
            // lblExamplesHeader
            // 
            this.lblExamplesHeader.AutoSize = true;
            this.lblExamplesHeader.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblExamplesHeader.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(124)))), ((int)(((byte)(140)))), ((int)(((byte)(255)))));
            this.lblExamplesHeader.Location = new System.Drawing.Point(0, 0);
            this.lblExamplesHeader.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.lblExamplesHeader.Name = "lblExamplesHeader";
            this.lblExamplesHeader.Size = new System.Drawing.Size(127, 15);
            this.lblExamplesHeader.TabIndex = 0;
            this.lblExamplesHeader.Text = "CÂU VÍ DỤ · TATOEBA";
            // 
            // pnlError
            // 
            this.pnlError.BackColor = System.Drawing.Color.Transparent;
            this.pnlError.BorderRadius = 18;
            this.pnlError.Controls.Add(this.lblErrorMessage);
            this.pnlError.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(33)))), ((int)(((byte)(42)))));
            this.pnlError.Location = new System.Drawing.Point(20, 90);
            this.pnlError.Name = "pnlError";
            this.pnlError.ShadowDecoration.Color = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(30)))), ((int)(((byte)(40)))), ((int)(((byte)(60)))));
            this.pnlError.ShadowDecoration.Depth = 25;
            this.pnlError.ShadowDecoration.Enabled = true;
            this.pnlError.Size = new System.Drawing.Size(932, 522);
            this.pnlError.TabIndex = 0;
            this.pnlError.Visible = false;
            // 
            // lblErrorMessage
            // 
            this.lblErrorMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblErrorMessage.Font = new System.Drawing.Font("Segoe UI", 13F);
            this.lblErrorMessage.Location = new System.Drawing.Point(32, 32);
            this.lblErrorMessage.Name = "lblErrorMessage";
            this.lblErrorMessage.Size = new System.Drawing.Size(860, 80);
            this.lblErrorMessage.TabIndex = 0;
            // 
            // lblUsage
            // 
            this.lblUsage.AutoSize = true;
            this.lblUsage.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblUsage.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(163)))), ((int)(((byte)(175)))));
            this.lblUsage.Location = new System.Drawing.Point(20, 624);
            this.lblUsage.Name = "lblUsage";
            this.lblUsage.Size = new System.Drawing.Size(0, 13);
            this.lblUsage.TabIndex = 14;
            // 
            // debounceTimer
            // 
            this.debounceTimer.Interval = 450;
            this.debounceTimer.Tick += new System.EventHandler(this.debounceTimer_Tick);
            // 
            // MainForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(20)))), ((int)(((byte)(26)))));
            this.ClientSize = new System.Drawing.Size(996, 660);
            this.Controls.Add(this.pnlError);
            this.Controls.Add(this.pnlExamples);
            this.Controls.Add(this.pnlColumnZh);
            this.Controls.Add(this.pnlColumnVi);
            this.Controls.Add(this.pnlColumnEn);
            this.Controls.Add(this.lblImportStatus);
            this.Controls.Add(this.btnHistory);
            this.Controls.Add(this.btnBookmarkToggle);
            this.Controls.Add(this.btnHotkeyConfig);
            this.Controls.Add(this.btnMiniMode);
            this.Controls.Add(this.btnClipboardToggle);
            this.Controls.Add(this.btnInfo);
            this.Controls.Add(this.pnlLanguageChip);
            this.Controls.Add(this.txtSearch);
            this.Controls.Add(this.lblUsage);
            this.Controls.Add(this.btnSearch);
            this.Controls.Add(this.circleLoading);
            this.Controls.Add(this.btnRetry);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "Dictionary Tool — EN / VI / ZH";
            this.pnlLanguageChip.ResumeLayout(false);
            this.cmsInfo.ResumeLayout(false);
            this.pnlColumnEn.ResumeLayout(false);
            this.pnlColumnEn.PerformLayout();
            this.pnlColumnEnContent.ResumeLayout(false);
            this.pnlColumnEnContent.PerformLayout();
            this.pnlColumnVi.ResumeLayout(false);
            this.pnlColumnVi.PerformLayout();
            this.pnlColumnViContent.ResumeLayout(false);
            this.pnlColumnViContent.PerformLayout();
            this.pnlColumnZh.ResumeLayout(false);
            this.pnlColumnZh.PerformLayout();
            this.pnlColumnZhContent.ResumeLayout(false);
            this.pnlColumnZhContent.PerformLayout();
            this.pnlExamples.ResumeLayout(false);
            this.pnlExamples.PerformLayout();
            this.pnlExamplesContent.ResumeLayout(false);
            this.pnlExamplesContent.PerformLayout();
            this.pnlError.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}
