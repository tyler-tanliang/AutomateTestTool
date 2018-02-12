namespace GoTesting
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tsStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsProgress = new System.Windows.Forms.ToolStripProgressBar();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.SendEmail = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnExecuteTestPlan = new System.Windows.Forms.Button();
            this.cbTestPlan = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cbDelay = new System.Windows.Forms.ComboBox();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.TestPlan = new System.Windows.Forms.TabControl();
            this.statusStrip1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.TestPlan.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsStatus,
            this.tsProgress});
            this.statusStrip1.Location = new System.Drawing.Point(0, 781);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(19, 0, 1, 0);
            this.statusStrip1.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.statusStrip1.Size = new System.Drawing.Size(1363, 25);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // tsStatus
            // 
            this.tsStatus.Name = "tsStatus";
            this.tsStatus.Size = new System.Drawing.Size(50, 20);
            this.tsStatus.Text = "Ready";
            // 
            // tsProgress
            // 
            this.tsProgress.Name = "tsProgress";
            this.tsProgress.Size = new System.Drawing.Size(267, 19);
            this.tsProgress.Visible = false;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.SendEmail);
            this.tabPage1.Controls.Add(this.groupBox1);
            this.tabPage1.Controls.Add(this.dataGridView1);
            this.tabPage1.Location = new System.Drawing.Point(4, 25);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(4);
            this.tabPage1.Size = new System.Drawing.Size(1353, 746);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Test Plan Testing";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // SendEmail
            // 
            this.SendEmail.Location = new System.Drawing.Point(976, 19);
            this.SendEmail.Name = "SendEmail";
            this.SendEmail.Size = new System.Drawing.Size(247, 22);
            this.SendEmail.TabIndex = 0;
            this.SendEmail.Text = "SendResultViaEmail";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnExecuteTestPlan);
            this.groupBox1.Controls.Add(this.cbTestPlan);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.cbDelay);
            this.groupBox1.Location = new System.Drawing.Point(8, 0);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox1.Size = new System.Drawing.Size(909, 69);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            // 
            // btnExecuteTestPlan
            // 
            this.btnExecuteTestPlan.Location = new System.Drawing.Point(563, 20);
            this.btnExecuteTestPlan.Margin = new System.Windows.Forms.Padding(4);
            this.btnExecuteTestPlan.Name = "btnExecuteTestPlan";
            this.btnExecuteTestPlan.Size = new System.Drawing.Size(96, 29);
            this.btnExecuteTestPlan.TabIndex = 9;
            this.btnExecuteTestPlan.Text = "Execute";
            this.btnExecuteTestPlan.UseVisualStyleBackColor = true;
            this.btnExecuteTestPlan.Click += new System.EventHandler(this.btnExecuteTestPlan_Click);
            // 
            // cbTestPlan
            // 
            this.cbTestPlan.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbTestPlan.FormattingEnabled = true;
            this.cbTestPlan.Location = new System.Drawing.Point(103, 23);
            this.cbTestPlan.Margin = new System.Windows.Forms.Padding(4);
            this.cbTestPlan.Name = "cbTestPlan";
            this.cbTestPlan.Size = new System.Drawing.Size(451, 23);
            this.cbTestPlan.TabIndex = 8;
            this.cbTestPlan.SelectedIndexChanged += new System.EventHandler(this.cbTestPlan_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 26);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(87, 15);
            this.label3.TabIndex = 7;
            this.label3.Text = "Test Plan:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(667, 26);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(87, 15);
            this.label2.TabIndex = 6;
            this.label2.Text = "Delay(ms):";
            // 
            // cbDelay
            // 
            this.cbDelay.FormattingEnabled = true;
            this.cbDelay.Items.AddRange(new object[] {
            "500",
            "1000",
            "2000",
            "3000",
            "4000",
            "5000",
            "6000",
            "8000",
            "10000"});
            this.cbDelay.Location = new System.Drawing.Point(761, 23);
            this.cbDelay.Margin = new System.Windows.Forms.Padding(4);
            this.cbDelay.Name = "cbDelay";
            this.cbDelay.Size = new System.Drawing.Size(128, 23);
            this.cbDelay.TabIndex = 5;
            this.cbDelay.Text = "1000";
            // 
            // dataGridView1
            // 
            this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView1.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(8, 76);
            this.dataGridView1.Margin = new System.Windows.Forms.Padding(4);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowTemplate.Height = 23;
            this.dataGridView1.Size = new System.Drawing.Size(1343, 665);
            this.dataGridView1.TabIndex = 1;
            // 
            // TestPlan
            // 
            this.TestPlan.Controls.Add(this.tabPage1);
            this.TestPlan.Location = new System.Drawing.Point(0, 2);
            this.TestPlan.Margin = new System.Windows.Forms.Padding(4);
            this.TestPlan.Name = "TestPlan";
            this.TestPlan.SelectedIndex = 0;
            this.TestPlan.Size = new System.Drawing.Size(1361, 775);
            this.TestPlan.TabIndex = 3;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1363, 806);
            this.Controls.Add(this.TestPlan);
            this.Controls.Add(this.statusStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "GoTesting";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tabPage1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.TestPlan.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel tsStatus;
        private System.Windows.Forms.ToolStripProgressBar tsProgress;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnExecuteTestPlan;
        private System.Windows.Forms.ComboBox cbTestPlan;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cbDelay;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.TabControl TestPlan;
        private System.Windows.Forms.CheckBox SendEmail;
    }
}

