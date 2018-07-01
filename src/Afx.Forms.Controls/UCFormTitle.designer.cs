namespace Afx.Forms.Controls
{
    partial class UCFormTitle
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.picClose = new System.Windows.Forms.PictureBox();
            this.picMax = new System.Windows.Forms.PictureBox();
            this.picMin = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.picClose)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picMax)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picMin)).BeginInit();
            this.SuspendLayout();
            // 
            // picClose
            // 
            this.picClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.picClose.BackColor = System.Drawing.Color.Transparent;
            this.picClose.Image = global::Afx.Forms.Controls.Properties.Resources.frmTitle_close_normal;
            this.picClose.Location = new System.Drawing.Point(377, -1);
            this.picClose.Margin = new System.Windows.Forms.Padding(0);
            this.picClose.Name = "picClose";
            this.picClose.Size = new System.Drawing.Size(39, 20);
            this.picClose.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.picClose.TabIndex = 11;
            this.picClose.TabStop = false;
            this.picClose.Click += new System.EventHandler(this.picClose_Click);
            this.picClose.MouseEnter += new System.EventHandler(this.picClose_MouseEnter);
            this.picClose.MouseLeave += new System.EventHandler(this.picClose_MouseLeave);
            // 
            // picMax
            // 
            this.picMax.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.picMax.BackColor = System.Drawing.Color.Transparent;
            this.picMax.Image = global::Afx.Forms.Controls.Properties.Resources.frmTitle_max_normal;
            this.picMax.Location = new System.Drawing.Point(349, -1);
            this.picMax.Margin = new System.Windows.Forms.Padding(0);
            this.picMax.Name = "picMax";
            this.picMax.Size = new System.Drawing.Size(28, 20);
            this.picMax.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.picMax.TabIndex = 10;
            this.picMax.TabStop = false;
            this.picMax.Click += new System.EventHandler(this.picMax_Click);
            this.picMax.MouseEnter += new System.EventHandler(this.picMax_MouseEnter);
            this.picMax.MouseLeave += new System.EventHandler(this.picMax_MouseLeave);
            // 
            // picMin
            // 
            this.picMin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.picMin.BackColor = System.Drawing.Color.Transparent;
            this.picMin.Image = global::Afx.Forms.Controls.Properties.Resources.frmTitle_mini_normal;
            this.picMin.Location = new System.Drawing.Point(321, -1);
            this.picMin.Margin = new System.Windows.Forms.Padding(0);
            this.picMin.Name = "picMin";
            this.picMin.Size = new System.Drawing.Size(28, 20);
            this.picMin.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.picMin.TabIndex = 9;
            this.picMin.TabStop = false;
            this.picMin.Click += new System.EventHandler(this.picMin_Click);
            this.picMin.MouseEnter += new System.EventHandler(this.picMin_MouseEnter);
            this.picMin.MouseLeave += new System.EventHandler(this.picMin_MouseLeave);
            // 
            // UCFormTitle
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.CornflowerBlue;
            this.Controls.Add(this.picClose);
            this.Controls.Add(this.picMax);
            this.Controls.Add(this.picMin);
            this.Name = "UCFormTitle";
            this.Size = new System.Drawing.Size(416, 31);
            this.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.UCFrmTitle_MouseDoubleClick);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.UCFrmTitle_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.UCFrmTitle_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.UCFrmTitle_MouseUp);
            ((System.ComponentModel.ISupportInitialize)(this.picClose)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picMax)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picMin)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.PictureBox picClose;
        public System.Windows.Forms.PictureBox picMax;
        public System.Windows.Forms.PictureBox picMin;



    }
}
