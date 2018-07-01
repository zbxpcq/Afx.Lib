using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Afx.Forms.Controls
{
    public partial class ShowYesNoCancelForm : AfxBaseForm
    {
        public ShowYesNoCancelForm()
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
            this.DialogResult = System.Windows.Forms.DialogResult.Yes;
        }

        private void btnNo_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.No;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }
    }
}
