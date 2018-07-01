using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Afx.Forms.Controls
{
    public class AfxDropDownItem
    {
        public string Text { get; set; }

        public object Value { get; set; }
    }

    public partial class AfxDropDownList : ComboBox
    {
        public AfxDropDownList()
        {
            this.Font = new Font("微软雅黑", 8F, FontStyle.Regular, GraphicsUnit.Point, 134);
            this.DisplayMember = "Text";
            this.ValueMember = "Value";
        }

        //{msg=0x100 (WM_KEYDOWN) hwnd=0x2c0f44 wparam=0x2e lparam=0x1530001 result=0x0}
        //{msg=0x102 (WM_CHAR) hwnd=0x1fd0de4 wparam=0x8 lparam=0xe0001 result=0x0}
        const int WM_KEYDOWN = 0x100;
        const int WM_CHAR = 0x102;
        const int WM_SYSKEYDOWN = 0x104;
        protected override bool ProcessKeyMessage(ref Message m)
        {
            if (m.Msg == WM_KEYDOWN || m.Msg == WM_CHAR || m.Msg == WM_SYSKEYDOWN)
            {
                return true;
            }

            return base.ProcessKeyMessage(ref m);
        }
    }
}
