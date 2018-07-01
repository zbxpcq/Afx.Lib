namespace Afx.Forms.Controls
{
    partial class UCPageFooter
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
            this.lb_PageCount = new System.Windows.Forms.Label();
            this.lbl_Last = new System.Windows.Forms.LinkLabel();
            this.lbl_Next = new System.Windows.Forms.LinkLabel();
            this.lbl_Previous = new System.Windows.Forms.LinkLabel();
            this.lbl_First = new System.Windows.Forms.LinkLabel();
            this.dropDownList_PageSize = new Afx.Forms.Controls.AfxDropDownList();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lb_PageCount
            // 
            this.lb_PageCount.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lb_PageCount.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lb_PageCount.Location = new System.Drawing.Point(318, 6);
            this.lb_PageCount.Name = "lb_PageCount";
            this.lb_PageCount.Size = new System.Drawing.Size(124, 17);
            this.lb_PageCount.TabIndex = 9;
            this.lb_PageCount.Text = "1/20 页";
            this.lb_PageCount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lbl_Last
            // 
            this.lbl_Last.ActiveLinkColor = System.Drawing.Color.Red;
            this.lbl_Last.AutoSize = true;
            this.lbl_Last.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbl_Last.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lbl_Last.Location = new System.Drawing.Point(155, 7);
            this.lbl_Last.Name = "lbl_Last";
            this.lbl_Last.Size = new System.Drawing.Size(36, 17);
            this.lbl_Last.TabIndex = 8;
            this.lbl_Last.TabStop = true;
            this.lbl_Last.Text = "末 页";
            this.lbl_Last.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lbl_Last_LinkClicked);
            // 
            // lbl_Next
            // 
            this.lbl_Next.ActiveLinkColor = System.Drawing.Color.Red;
            this.lbl_Next.AutoSize = true;
            this.lbl_Next.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbl_Next.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lbl_Next.Location = new System.Drawing.Point(104, 7);
            this.lbl_Next.Name = "lbl_Next";
            this.lbl_Next.Size = new System.Drawing.Size(44, 17);
            this.lbl_Next.TabIndex = 7;
            this.lbl_Next.TabStop = true;
            this.lbl_Next.Text = "下一页";
            this.lbl_Next.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lbl_Next_LinkClicked);
            // 
            // lbl_Previous
            // 
            this.lbl_Previous.ActiveLinkColor = System.Drawing.Color.Red;
            this.lbl_Previous.AutoSize = true;
            this.lbl_Previous.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbl_Previous.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lbl_Previous.Location = new System.Drawing.Point(53, 7);
            this.lbl_Previous.Name = "lbl_Previous";
            this.lbl_Previous.Size = new System.Drawing.Size(44, 17);
            this.lbl_Previous.TabIndex = 6;
            this.lbl_Previous.TabStop = true;
            this.lbl_Previous.Text = "上一页";
            this.lbl_Previous.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lbl_Previous_LinkClicked);
            // 
            // lbl_First
            // 
            this.lbl_First.ActiveLinkColor = System.Drawing.Color.Red;
            this.lbl_First.AutoSize = true;
            this.lbl_First.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbl_First.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lbl_First.Location = new System.Drawing.Point(9, 7);
            this.lbl_First.Name = "lbl_First";
            this.lbl_First.Size = new System.Drawing.Size(36, 17);
            this.lbl_First.TabIndex = 5;
            this.lbl_First.TabStop = true;
            this.lbl_First.Text = "首 页";
            this.lbl_First.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lbl_First_LinkClicked);
            // 
            // dropDownList_PageSize
            // 
            this.dropDownList_PageSize.DisplayMember = "Text";
            this.dropDownList_PageSize.Font = new System.Drawing.Font("微软雅黑", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.dropDownList_PageSize.FormattingEnabled = true;
            this.dropDownList_PageSize.Location = new System.Drawing.Point(239, 3);
            this.dropDownList_PageSize.Name = "dropDownList_PageSize";
            this.dropDownList_PageSize.Size = new System.Drawing.Size(48, 24);
            this.dropDownList_PageSize.TabIndex = 10;
            this.dropDownList_PageSize.ValueMember = "Value";
            this.dropDownList_PageSize.SelectedValueChanged += new System.EventHandler(this.dropDownList_PageSize_SelectedValueChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(203, 7);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(32, 17);
            this.label2.TabIndex = 11;
            this.label2.Text = "每页";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.Location = new System.Drawing.Point(292, 7);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(20, 17);
            this.label3.TabIndex = 12;
            this.label3.Text = "条";
            // 
            // UCPageFooter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.dropDownList_PageSize);
            this.Controls.Add(this.lb_PageCount);
            this.Controls.Add(this.lbl_Last);
            this.Controls.Add(this.lbl_Next);
            this.Controls.Add(this.lbl_Previous);
            this.Controls.Add(this.lbl_First);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "UCPageFooter";
            this.Size = new System.Drawing.Size(450, 30);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lb_PageCount;
        private System.Windows.Forms.LinkLabel lbl_Last;
        private System.Windows.Forms.LinkLabel lbl_Next;
        private System.Windows.Forms.LinkLabel lbl_Previous;
        private System.Windows.Forms.LinkLabel lbl_First;
        private AfxDropDownList dropDownList_PageSize;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
    }
}
