using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Afx.Forms.Controls
{
    public partial class ShowMsgForm : AfxBaseForm
    {
        public ShowMsgForm()
        {
            InitializeComponent();
            this.ShowCenterScreen();
        }

        public string Msg
        {
            get { return this.lbMsg.Text; }
            set { this.lbMsg.Text = value; }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }
    }
}
