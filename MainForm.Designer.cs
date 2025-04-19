namespace NGramm
{
    partial class MainForm
    {
        // <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.файлToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.порахуватиToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.еуіеToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.LiteralCountSpaces = new System.Windows.Forms.CheckBox();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.consequtiveSpaces = new System.Windows.Forms.CheckBox();
            this.ShowNPS = new System.Windows.Forms.CheckBox();
            this.SymbolsCountSpaces = new System.Windows.Forms.CheckBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.label16 = new System.Windows.Forms.Label();
            this.EndSignsList = new System.Windows.Forms.TextBox();
            this.SpecialSymbolsCount = new System.Windows.Forms.CheckBox();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.groupBox10 = new System.Windows.Forms.GroupBox();
            this.ShowMultStatButton = new System.Windows.Forms.Button();
            this.SaveMultStatsButton = new System.Windows.Forms.Button();
            this.label17 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.MultStatsEndN = new System.Windows.Forms.TextBox();
            this.MultStatsStartN = new System.Windows.Forms.TextBox();
            this.BuildMultStats = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.SingleStatN = new System.Windows.Forms.TextBox();
            this.SaveSingleStatButton = new System.Windows.Forms.Button();
            this.ShowSingleStatButton = new System.Windows.Forms.Button();
            this.ShowHeapsButton = new System.Windows.Forms.Button();
            this.listView1 = new System.Windows.Forms.ListView();
            this.Порядок = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Кількість = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.IgnoreRegisterChecbox = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.N = new System.Windows.Forms.TextBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.CommonRankBox = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.HeapsN = new System.Windows.Forms.TextBox();
            this.ShowPlotsButton = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.TimeLabel = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.numericUpDown2 = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.OperationNameLabel = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.debugTextBox = new System.Windows.Forms.TextBox();
            this.menuStrip1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.groupBox10.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.файлToolStripMenuItem,
            this.порахуватиToolStripMenuItem,
            this.еуіеToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(4, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(732, 27);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // файлToolStripMenuItem
            // 
            this.файлToolStripMenuItem.Name = "файлToolStripMenuItem";
            this.файлToolStripMenuItem.Size = new System.Drawing.Size(98, 23);
            this.файлToolStripMenuItem.Text = "Відкрити текст";
            this.файлToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // порахуватиToolStripMenuItem
            // 
            this.порахуватиToolStripMenuItem.Enabled = false;
            this.порахуватиToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.порахуватиToolStripMenuItem.Name = "порахуватиToolStripMenuItem";
            this.порахуватиToolStripMenuItem.Size = new System.Drawing.Size(105, 23);
            this.порахуватиToolStripMenuItem.Text = "Порахувати";
            this.порахуватиToolStripMenuItem.Click += new System.EventHandler(this.порахуватиToolStripMenuItem_Click);
            // 
            // еуіеToolStripMenuItem
            // 
            this.еуіеToolStripMenuItem.Name = "еуіеToolStripMenuItem";
            this.еуіеToolStripMenuItem.Size = new System.Drawing.Size(33, 23);
            this.еуіеToolStripMenuItem.Text = "L=";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Location = new System.Drawing.Point(6, 59);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(213, 101);
            this.tabControl1.TabIndex = 2;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.LiteralCountSpaces);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(205, 75);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Буквені";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // LiteralCountSpaces
            // 
            this.LiteralCountSpaces.AutoSize = true;
            this.LiteralCountSpaces.Location = new System.Drawing.Point(6, 6);
            this.LiteralCountSpaces.Name = "LiteralCountSpaces";
            this.LiteralCountSpaces.Size = new System.Drawing.Size(131, 17);
            this.LiteralCountSpaces.TabIndex = 7;
            this.LiteralCountSpaces.Text = "Враховувати пробіли";
            this.LiteralCountSpaces.UseVisualStyleBackColor = true;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.consequtiveSpaces);
            this.tabPage3.Controls.Add(this.ShowNPS);
            this.tabPage3.Controls.Add(this.SymbolsCountSpaces);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(205, 75);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Символьні";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // consequtiveSpaces
            // 
            this.consequtiveSpaces.AutoSize = true;
            this.consequtiveSpaces.Location = new System.Drawing.Point(105, 5);
            this.consequtiveSpaces.Margin = new System.Windows.Forms.Padding(2);
            this.consequtiveSpaces.Name = "consequtiveSpaces";
            this.consequtiveSpaces.Size = new System.Drawing.Size(40, 17);
            this.consequtiveSpaces.TabIndex = 2;
            this.consequtiveSpaces.Text = "CS";
            this.consequtiveSpaces.UseVisualStyleBackColor = true;
            // 
            // ShowNPS
            // 
            this.ShowNPS.AutoSize = true;
            this.ShowNPS.Location = new System.Drawing.Point(3, 24);
            this.ShowNPS.Margin = new System.Windows.Forms.Padding(2);
            this.ShowNPS.Name = "ShowNPS";
            this.ShowNPS.Size = new System.Drawing.Size(100, 17);
            this.ShowNPS.TabIndex = 1;
            this.ShowNPS.Text = "Показати NPS";
            this.ShowNPS.UseVisualStyleBackColor = true;
            this.ShowNPS.CheckedChanged += new System.EventHandler(this.ShowNPS_CheckedChanged);
            // 
            // SymbolsCountSpaces
            // 
            this.SymbolsCountSpaces.AutoSize = true;
            this.SymbolsCountSpaces.Location = new System.Drawing.Point(3, 5);
            this.SymbolsCountSpaces.Margin = new System.Windows.Forms.Padding(2);
            this.SymbolsCountSpaces.Name = "SymbolsCountSpaces";
            this.SymbolsCountSpaces.Size = new System.Drawing.Size(66, 17);
            this.SymbolsCountSpaces.TabIndex = 0;
            this.SymbolsCountSpaces.Text = "Пробіли";
            this.SymbolsCountSpaces.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.label16);
            this.tabPage2.Controls.Add(this.EndSignsList);
            this.tabPage2.Controls.Add(this.SpecialSymbolsCount);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(205, 75);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Словесні";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(3, 3);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(121, 13);
            this.label16.TabIndex = 13;
            this.label16.Text = "Додати стоп-символи:";
            // 
            // EndSignsList
            // 
            this.EndSignsList.Location = new System.Drawing.Point(6, 19);
            this.EndSignsList.Name = "EndSignsList";
            this.EndSignsList.Size = new System.Drawing.Size(169, 20);
            this.EndSignsList.TabIndex = 10;
            this.EndSignsList.TextChanged += new System.EventHandler(this.EndSignsChanged);
            // 
            // SpecialSymbolsCount
            // 
            this.SpecialSymbolsCount.AutoSize = true;
            this.SpecialSymbolsCount.Location = new System.Drawing.Point(6, 46);
            this.SpecialSymbolsCount.Name = "SpecialSymbolsCount";
            this.SpecialSymbolsCount.Size = new System.Drawing.Size(172, 17);
            this.SpecialSymbolsCount.TabIndex = 6;
            this.SpecialSymbolsCount.Text = "Враховувати розділові знаки";
            this.SpecialSymbolsCount.UseVisualStyleBackColor = true;
            this.SpecialSymbolsCount.CheckedChanged += new System.EventHandler(this.SpecialSymbolsCount_CheckedChanged);
            // 
            // tabPage4
            // 
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(205, 75);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Код";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // groupBox10
            // 
            this.groupBox10.AutoSize = true;
            this.groupBox10.Controls.Add(this.ShowMultStatButton);
            this.groupBox10.Controls.Add(this.SaveMultStatsButton);
            this.groupBox10.Controls.Add(this.label17);
            this.groupBox10.Controls.Add(this.label13);
            this.groupBox10.Controls.Add(this.label7);
            this.groupBox10.Controls.Add(this.MultStatsEndN);
            this.groupBox10.Controls.Add(this.MultStatsStartN);
            this.groupBox10.Controls.Add(this.BuildMultStats);
            this.groupBox10.Enabled = false;
            this.groupBox10.Location = new System.Drawing.Point(254, 32);
            this.groupBox10.Name = "groupBox10";
            this.groupBox10.Size = new System.Drawing.Size(183, 157);
            this.groupBox10.TabIndex = 3;
            this.groupBox10.TabStop = false;
            this.groupBox10.Text = "Спільна статистика";
            // 
            // ShowMultStatButton
            // 
            this.ShowMultStatButton.Enabled = false;
            this.ShowMultStatButton.Location = new System.Drawing.Point(7, 86);
            this.ShowMultStatButton.Name = "ShowMultStatButton";
            this.ShowMultStatButton.Size = new System.Drawing.Size(169, 20);
            this.ShowMultStatButton.TabIndex = 8;
            this.ShowMultStatButton.Text = "Показати n-грами";
            this.ShowMultStatButton.UseVisualStyleBackColor = true;
            this.ShowMultStatButton.Click += new System.EventHandler(this.ShowMultStatButton_Click);
            // 
            // SaveMultStatsButton
            // 
            this.SaveMultStatsButton.Enabled = false;
            this.SaveMultStatsButton.Location = new System.Drawing.Point(7, 115);
            this.SaveMultStatsButton.Name = "SaveMultStatsButton";
            this.SaveMultStatsButton.Size = new System.Drawing.Size(170, 23);
            this.SaveMultStatsButton.TabIndex = 7;
            this.SaveMultStatsButton.Text = "Зберегти статистику";
            this.SaveMultStatsButton.UseVisualStyleBackColor = true;
            this.SaveMultStatsButton.Click += new System.EventHandler(this.button5_Click);
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(6, 35);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(21, 13);
            this.label17.TabIndex = 4;
            this.label17.Text = "від";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(96, 35);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(19, 13);
            this.label13.TabIndex = 4;
            this.label13.Text = "до";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 16);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(123, 13);
            this.label7.TabIndex = 4;
            this.label7.Text = "Статистика для n-грам";
            // 
            // MultStatsEndN
            // 
            this.MultStatsEndN.Location = new System.Drawing.Point(118, 32);
            this.MultStatsEndN.Name = "MultStatsEndN";
            this.MultStatsEndN.Size = new System.Drawing.Size(58, 20);
            this.MultStatsEndN.TabIndex = 5;
            this.MultStatsEndN.Text = "1";
            this.MultStatsEndN.TextChanged += new System.EventHandler(this.textBox10_TextChanged);
            // 
            // MultStatsStartN
            // 
            this.MultStatsStartN.Location = new System.Drawing.Point(33, 32);
            this.MultStatsStartN.Name = "MultStatsStartN";
            this.MultStatsStartN.Size = new System.Drawing.Size(58, 20);
            this.MultStatsStartN.TabIndex = 5;
            this.MultStatsStartN.Text = "1";
            // 
            // BuildMultStats
            // 
            this.BuildMultStats.Location = new System.Drawing.Point(6, 57);
            this.BuildMultStats.Name = "BuildMultStats";
            this.BuildMultStats.Size = new System.Drawing.Size(170, 23);
            this.BuildMultStats.TabIndex = 3;
            this.BuildMultStats.Text = "Побудувати";
            this.BuildMultStats.UseVisualStyleBackColor = true;
            this.BuildMultStats.Click += new System.EventHandler(this.button16_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.AutoSize = true;
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.SingleStatN);
            this.groupBox2.Controls.Add(this.SaveSingleStatButton);
            this.groupBox2.Controls.Add(this.ShowSingleStatButton);
            this.groupBox2.Enabled = false;
            this.groupBox2.Location = new System.Drawing.Point(254, 195);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(183, 117);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Статистика";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(0, 23);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(106, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "для n-грам порядку";
            // 
            // SingleStatN
            // 
            this.SingleStatN.Location = new System.Drawing.Point(118, 19);
            this.SingleStatN.Name = "SingleStatN";
            this.SingleStatN.Size = new System.Drawing.Size(58, 20);
            this.SingleStatN.TabIndex = 9;
            this.SingleStatN.Text = "1";
            this.SingleStatN.TextChanged += new System.EventHandler(this.textBox2_TextChanged);
            // 
            // SaveSingleStatButton
            // 
            this.SaveSingleStatButton.Location = new System.Drawing.Point(6, 75);
            this.SaveSingleStatButton.Name = "SaveSingleStatButton";
            this.SaveSingleStatButton.Size = new System.Drawing.Size(170, 23);
            this.SaveSingleStatButton.TabIndex = 7;
            this.SaveSingleStatButton.Text = "Зберегти статистику";
            this.SaveSingleStatButton.UseVisualStyleBackColor = true;
            this.SaveSingleStatButton.Click += new System.EventHandler(this.button6_Click_1);
            // 
            // ShowSingleStatButton
            // 
            this.ShowSingleStatButton.Location = new System.Drawing.Point(6, 45);
            this.ShowSingleStatButton.Name = "ShowSingleStatButton";
            this.ShowSingleStatButton.Size = new System.Drawing.Size(170, 23);
            this.ShowSingleStatButton.TabIndex = 3;
            this.ShowSingleStatButton.Text = "Показати n-грами";
            this.ShowSingleStatButton.UseVisualStyleBackColor = true;
            this.ShowSingleStatButton.Click += new System.EventHandler(this.button2_Click);
            // 
            // ShowHeapsButton
            // 
            this.ShowHeapsButton.Location = new System.Drawing.Point(6, 45);
            this.ShowHeapsButton.Name = "ShowHeapsButton";
            this.ShowHeapsButton.Size = new System.Drawing.Size(170, 23);
            this.ShowHeapsButton.TabIndex = 7;
            this.ShowHeapsButton.Text = "Гіпс";
            this.ShowHeapsButton.UseVisualStyleBackColor = true;
            this.ShowHeapsButton.Click += new System.EventHandler(this.button23_Click);
            // 
            // listView1
            // 
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.Порядок,
            this.Кількість});
            this.listView1.GridLines = true;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(17, 229);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(225, 184);
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            // 
            // Порядок
            // 
            this.Порядок.Text = "Порядок";
            this.Порядок.Width = 93;
            // 
            // Кількість
            // 
            this.Кількість.Text = "Словник";
            this.Кількість.Width = 127;
            // 
            // IgnoreRegisterChecbox
            // 
            this.IgnoreRegisterChecbox.AutoSize = true;
            this.IgnoreRegisterChecbox.Checked = true;
            this.IgnoreRegisterChecbox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.IgnoreRegisterChecbox.Location = new System.Drawing.Point(6, 19);
            this.IgnoreRegisterChecbox.Name = "IgnoreRegisterChecbox";
            this.IgnoreRegisterChecbox.Size = new System.Drawing.Size(119, 17);
            this.IgnoreRegisterChecbox.TabIndex = 7;
            this.IgnoreRegisterChecbox.Text = "Ігнорувати регістр";
            this.IgnoreRegisterChecbox.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 39);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(91, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Довжина n-грам";
            // 
            // N
            // 
            this.N.Location = new System.Drawing.Point(99, 36);
            this.N.Name = "N";
            this.N.Size = new System.Drawing.Size(120, 20);
            this.N.TabIndex = 5;
            this.N.Text = "1";
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 557);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(2, 0, 14, 0);
            this.statusStrip1.Size = new System.Drawing.Size(732, 22);
            this.statusStrip1.TabIndex = 3;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(102, 17);
            this.toolStripStatusLabel1.Text = "Текст не вибрано";
            // 
            // groupBox4
            // 
            this.groupBox4.AutoSize = true;
            this.groupBox4.Controls.Add(this.tabControl1);
            this.groupBox4.Controls.Add(this.CommonRankBox);
            this.groupBox4.Controls.Add(this.IgnoreRegisterChecbox);
            this.groupBox4.Controls.Add(this.N);
            this.groupBox4.Controls.Add(this.label1);
            this.groupBox4.Enabled = false;
            this.groupBox4.Location = new System.Drawing.Point(12, 29);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(230, 198);
            this.groupBox4.TabIndex = 8;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Параметри";
            // 
            // CommonRankBox
            // 
            this.CommonRankBox.AutoSize = true;
            this.CommonRankBox.Location = new System.Drawing.Point(6, 162);
            this.CommonRankBox.Margin = new System.Windows.Forms.Padding(2);
            this.CommonRankBox.Name = "CommonRankBox";
            this.CommonRankBox.Size = new System.Drawing.Size(96, 17);
            this.CommonRankBox.TabIndex = 8;
            this.CommonRankBox.Text = "Common Rank";
            this.CommonRankBox.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.HeapsN);
            this.groupBox1.Controls.Add(this.ShowHeapsButton);
            this.groupBox1.Enabled = false;
            this.groupBox1.Location = new System.Drawing.Point(255, 338);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(182, 75);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Heaps";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(0, 23);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(106, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "для n-грам порядку";
            // 
            // HeapsN
            // 
            this.HeapsN.Location = new System.Drawing.Point(118, 19);
            this.HeapsN.Name = "HeapsN";
            this.HeapsN.Size = new System.Drawing.Size(58, 20);
            this.HeapsN.TabIndex = 11;
            this.HeapsN.Text = "1";
            this.HeapsN.TextChanged += new System.EventHandler(this.textBox3_TextChanged);
            // 
            // ShowPlotsButton
            // 
            this.ShowPlotsButton.Location = new System.Drawing.Point(259, 298);
            this.ShowPlotsButton.Margin = new System.Windows.Forms.Padding(2);
            this.ShowPlotsButton.Name = "ShowPlotsButton";
            this.ShowPlotsButton.Size = new System.Drawing.Size(172, 24);
            this.ShowPlotsButton.TabIndex = 10;
            this.ShowPlotsButton.Text = "Графіки";
            this.ShowPlotsButton.UseVisualStyleBackColor = true;
            this.ShowPlotsButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(14, 467);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(412, 20);
            this.progressBar1.TabIndex = 11;
            // 
            // TimeLabel
            // 
            this.TimeLabel.AutoSize = true;
            this.TimeLabel.Location = new System.Drawing.Point(371, 489);
            this.TimeLabel.Name = "TimeLabel";
            this.TimeLabel.Size = new System.Drawing.Size(64, 13);
            this.TimeLabel.TabIndex = 12;
            this.TimeLabel.Text = "00.00.00:00";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.numericUpDown2);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Controls.Add(this.numericUpDown1);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Location = new System.Drawing.Point(14, 419);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(412, 42);
            this.groupBox3.TabIndex = 13;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Швидкодія";
            this.groupBox3.Enter += new System.EventHandler(this.groupBox3_Enter);
            // 
            // numericUpDown2
            // 
            this.numericUpDown2.Location = new System.Drawing.Point(242, 15);
            this.numericUpDown2.Name = "numericUpDown2";
            this.numericUpDown2.Size = new System.Drawing.Size(68, 20);
            this.numericUpDown2.TabIndex = 3;
            this.numericUpDown2.ValueChanged += new System.EventHandler(this.numericUpDown2_ValueChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(150, 16);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(94, 13);
            this.label6.TabIndex = 2;
            this.label6.Text = "Поріг статистики";
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(56, 15);
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(68, 20);
            this.numericUpDown1.TabIndex = 1;
            this.numericUpDown1.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(9, 16);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(44, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "Потоки";
            // 
            // OperationNameLabel
            // 
            this.OperationNameLabel.AutoSize = true;
            this.OperationNameLabel.Location = new System.Drawing.Point(14, 477);
            this.OperationNameLabel.Name = "OperationNameLabel";
            this.OperationNameLabel.Size = new System.Drawing.Size(0, 13);
            this.OperationNameLabel.TabIndex = 14;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(7, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(39, 13);
            this.label4.TabIndex = 15;
            this.label4.Text = "Debug";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.debugTextBox);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Location = new System.Drawing.Point(443, 32);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(277, 153);
            this.panel1.TabIndex = 16;
            // 
            // debugTextBox
            // 
            this.debugTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.debugTextBox.Location = new System.Drawing.Point(10, 28);
            this.debugTextBox.Multiline = true;
            this.debugTextBox.Name = "debugTextBox";
            this.debugTextBox.Size = new System.Drawing.Size(255, 120);
            this.debugTextBox.TabIndex = 16;
            this.debugTextBox.TextChanged += new System.EventHandler(this.debugTextBox_TextChanged);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(732, 579);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.OperationNameLabel);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.TimeLabel);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.ShowPlotsButton);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.groupBox10);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.MinimumSize = new System.Drawing.Size(454, 560);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "NGramm v3";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.groupBox10.ResumeLayout(false);
            this.groupBox10.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem файлToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox N;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button ShowSingleStatButton;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader Порядок;
        private System.Windows.Forms.ColumnHeader Кількість;
        private System.Windows.Forms.CheckBox SpecialSymbolsCount;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Button SaveSingleStatButton;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.GroupBox groupBox10;
        private System.Windows.Forms.Button SaveMultStatsButton;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox MultStatsEndN;
        private System.Windows.Forms.TextBox MultStatsStartN;
        private System.Windows.Forms.Button BuildMultStats;
        private System.Windows.Forms.CheckBox LiteralCountSpaces;
        private System.Windows.Forms.CheckBox IgnoreRegisterChecbox;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.TextBox EndSignsList;
        private System.Windows.Forms.Button ShowHeapsButton;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.ToolStripMenuItem порахуватиToolStripMenuItem;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox SingleStatN;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox HeapsN;
        private System.Windows.Forms.ToolStripMenuItem еуіеToolStripMenuItem;
        private System.Windows.Forms.CheckBox CommonRankBox;
        private System.Windows.Forms.Button ShowPlotsButton;
        private System.Windows.Forms.CheckBox SymbolsCountSpaces;
        private System.Windows.Forms.Button ShowMultStatButton;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label TimeLabel;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown numericUpDown2;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.Label OperationNameLabel;
        private System.Windows.Forms.CheckBox ShowNPS;
        private System.Windows.Forms.CheckBox consequtiveSpaces;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox debugTextBox;
    }
}

