namespace HandlingDrawing
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
            this.components = new System.ComponentModel.Container();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.LogSelMgrBtn = new System.Windows.Forms.Button();
            this.animationTimer = new System.Windows.Forms.Timer(this.components);
            this.largePanel = new HandlingDrawing.Canvas();
            this.zoomPanel = new HandlingDrawing.Canvas();
            this.dgModel = new System.Windows.Forms.DataGridView();
            this.PhyEngBtn = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgModel)).BeginInit();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox1.Location = new System.Drawing.Point(12, 448);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox1.Size = new System.Drawing.Size(806, 52);
            this.textBox1.TabIndex = 2;
            // 
            // LogSelMgrBtn
            // 
            this.LogSelMgrBtn.Location = new System.Drawing.Point(588, 422);
            this.LogSelMgrBtn.Name = "LogSelMgrBtn";
            this.LogSelMgrBtn.Size = new System.Drawing.Size(80, 20);
            this.LogSelMgrBtn.TabIndex = 3;
            this.LogSelMgrBtn.Text = "See Sel Mgr";
            this.LogSelMgrBtn.UseVisualStyleBackColor = true;
            this.LogSelMgrBtn.Click += new System.EventHandler(this.button1_Click);
            // 
            // animationTimer
            // 
            this.animationTimer.Enabled = true;
            this.animationTimer.Interval = 10;
            this.animationTimer.Tick += new System.EventHandler(this.animationTimer_Tick);
            // 
            // largePanel
            // 
            this.largePanel.BackColor = System.Drawing.Color.White;
            this.largePanel.Location = new System.Drawing.Point(439, 12);
            this.largePanel.Name = "largePanel";
            this.largePanel.Size = new System.Drawing.Size(400, 200);
            this.largePanel.TabIndex = 1;
            this.largePanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.canvas_MouseDown);
            this.largePanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.canvas_MouseMove);
            this.largePanel.MouseUp += new System.Windows.Forms.MouseEventHandler(this.canvas_MouseUp);
            // 
            // zoomPanel
            // 
            this.zoomPanel.BackColor = System.Drawing.Color.White;
            this.zoomPanel.Location = new System.Drawing.Point(12, 12);
            this.zoomPanel.Name = "zoomPanel";
            this.zoomPanel.Size = new System.Drawing.Size(400, 400);
            this.zoomPanel.TabIndex = 0;
            this.zoomPanel.KeyDown += new System.Windows.Forms.KeyEventHandler(this.zoomPanel_KeyDown);
            this.zoomPanel.KeyUp += new System.Windows.Forms.KeyEventHandler(this.zoomPanel_KeyUp);
            this.zoomPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.canvas_MouseDown);
            this.zoomPanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.canvas_MouseMove);
            this.zoomPanel.MouseUp += new System.Windows.Forms.MouseEventHandler(this.canvas_MouseUp);
            // 
            // dgModel
            // 
            this.dgModel.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgModel.Location = new System.Drawing.Point(439, 237);
            this.dgModel.Name = "dgModel";
            this.dgModel.Size = new System.Drawing.Size(399, 175);
            this.dgModel.TabIndex = 4;
            // 
            // PhyEngBtn
            // 
            this.PhyEngBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.PhyEngBtn.Location = new System.Drawing.Point(493, 422);
            this.PhyEngBtn.Margin = new System.Windows.Forms.Padding(0);
            this.PhyEngBtn.Name = "PhyEngBtn";
            this.PhyEngBtn.Size = new System.Drawing.Size(89, 20);
            this.PhyEngBtn.TabIndex = 5;
            this.PhyEngBtn.Text = "Physics Engine";
            this.PhyEngBtn.UseVisualStyleBackColor = false;
            this.PhyEngBtn.Click += new System.EventHandler(this.PhyEngBtn_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(436, 426);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(39, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Debug";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(898, 510);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.PhyEngBtn);
            this.Controls.Add(this.dgModel);
            this.Controls.Add(this.LogSelMgrBtn);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.largePanel);
            this.Controls.Add(this.zoomPanel);
            this.Name = "Form1";
            this.Text = "Display at startup";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyUp);
            ((System.ComponentModel.ISupportInitialize)(this.dgModel)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Canvas zoomPanel;
        private Canvas largePanel;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button LogSelMgrBtn;
        private System.Windows.Forms.Timer animationTimer;
        private System.Windows.Forms.DataGridView dgModel;
        private System.Windows.Forms.Button PhyEngBtn;
        private System.Windows.Forms.Label label1;

    }
}

