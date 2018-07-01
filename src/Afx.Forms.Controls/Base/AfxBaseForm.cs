using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace Afx.Forms.Controls
{
    public class AfxBaseForm : Form
    {
        public SynchronizationContext Sync { get; private set; }
        public int ManagedThreadId { get; private set; }
        public AfxBaseForm()
        {
            this.Sync = SynchronizationContext.Current;
            this.ManagedThreadId = Thread.CurrentThread.ManagedThreadId;
            this.SetStyle(ControlStyles.Opaque, false);
            this.SetStyle(
               ControlStyles.AllPaintingInWmPaint |
               ControlStyles.DoubleBuffer |
               ControlStyles.OptimizedDoubleBuffer |
               ControlStyles.ResizeRedraw |
               ControlStyles.Selectable |
               ControlStyles.ContainerControl |
               ControlStyles.UserPaint, true);
            this.UpdateStyles();
            this.IsCustomWindow = false;
        }

        protected virtual void ShowCenterScreen()
        {
            this.StartPosition = FormStartPosition.Manual;
            var rec = Screen.GetWorkingArea(this);
            this.Location = new Point((rec.Width - this.Width) / 2, (rec.Height - this.Height) / 2);
        }

        private ShowMsgForm showMsgForm = null;
        public virtual void ShowMessage(string msg)
        {
            bool temp = this.TopMost;
            if (showMsgForm == null || showMsgForm.IsDisposed)
                showMsgForm = new ShowMsgForm();
            showMsgForm.Msg = msg;
            showMsgForm.ShowDialog();
            this.TopMost = temp;
        }

        private ShowYesNoForm showYesNoForm = null;
        public virtual DialogResult ShowYesNo(string msg)
        {
            DialogResult result = DialogResult.None;
            bool temp = this.TopMost;
            if (showYesNoForm == null || showYesNoForm.IsDisposed)
                showYesNoForm = new ShowYesNoForm();
            showYesNoForm.Msg = msg;
            result = showYesNoForm.ShowDialog();
            this.TopMost = temp;

            return result;
        }

        private ShowYesNoCancelForm showYesNoCancelForm = null;
        public virtual DialogResult ShowYesNoCancel(string msg)
        {
            DialogResult result = DialogResult.None;
            bool temp = this.TopMost;
            if (showYesNoCancelForm == null || showYesNoCancelForm.IsDisposed)
                showYesNoCancelForm = new ShowYesNoCancelForm();
            showYesNoCancelForm.Msg = msg;
            result = showYesNoCancelForm.ShowDialog();
            this.TopMost = temp;

            return result;
        }
        
        #region 窗体圆角
        private void SetWindowRegion()
        {
            GraphicsPath FormPath = GetRoundedRectPath(this.ClientRectangle, 8);
            this.Region = new Region(FormPath);

        }
        private GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();

            // 左上角
            path.AddArc(rect.Right - radius, rect.Top - 1, radius, radius, 270, 90);

            // 右上角
            path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);

            // 右下角
            path.AddArc(rect.Left - 1, rect.Bottom - radius, radius, radius, 90, 90);

            // 左下角
            path.AddArc(rect.Left - 1, rect.Top - 1, radius, radius, 180, 90);
            path.CloseFigure();//闭合曲线
            return path;
        }
        #endregion

        protected override void OnPaint(PaintEventArgs e)
        {
            this.SuspendLayoutAll(this);
            if (this.FormBorderStyle == FormBorderStyle.None && this.FormBorderStyle == FormBorderStyle.None)
            {
                SetWindowRegion();
            }

            base.OnPaint(e);
            this.ResumeLayoutAll(this);
        }
        protected void SuspendLayoutAll(Control control)
        {
            control.SuspendLayout();
            foreach (Control c in control.Controls)
            {
                c.SuspendLayout();
                SuspendLayoutAll(c);
            }
        }

        protected void ResumeLayoutAll(Control control)
        {
            control.ResumeLayout();
            foreach (Control c in control.Controls)
            {
                c.ResumeLayout();
                ResumeLayoutAll(c);
            }
        }

        public bool IsCustomWindow { get; set; }

        protected override void WndProc(ref Message m)
        {
            if (this.FormBorderStyle == FormBorderStyle.None && this.IsCustomWindow)
            {
                switch (m.Msg)
                {
                    case (int)WindowsMessage.WM_NCPAINT:
                    case (int)WindowsMessage.WM_NCCALCSIZE:
                        break;
                    case (int)WindowsMessage.WM_NCACTIVATE:
                        if (m.WParam == (IntPtr)0)
                        {
                            m.Result = (IntPtr)1;
                        }
                        break;
                    case (int)WindowsMessage.WM_NCHITTEST:
                        this.WinResize(ref m);
                        break;
                    default:
                        base.WndProc(ref m);
                        break;
                }
            }
            else
            {
                base.WndProc(ref m);
            }

        }

        /// <summary>
        /// 拖动窗口大小
        /// </summary>
        /// <param name="m"></param>
        private void WinResize(ref Message m)
        {
            if (this.WindowState != FormWindowState.Maximized)
            {
                int wparam = m.LParam.ToInt32();

                Point point = new Point(
                    NativeMethods.LOWORD(wparam),
                    NativeMethods.HIWORD(wparam));
                point = PointToClient(point);

                // 分解当前鼠标的坐标

                int nPosX = (m.LParam.ToInt32() & 65535);
                int nPosY = (m.LParam.ToInt32() >> 16);
                if (nPosX >= this.Left + this.Width - 6 && nPosY >= this.Top + this.Height - 6)
                {
                    // 鼠标位置在窗体的右下角附近
                    m.Result = (IntPtr)WM_NCHITTEST.HTBOTTOMRIGHT;
                    return;
                }
                else if (nPosX >= this.Left + this.Width - 2)
                {
                    // 鼠标位置在窗体右侧
                    m.Result = (IntPtr)WM_NCHITTEST.HTRIGHT;
                    return;
                }
                else if (nPosY >= this.Top + this.Height - 2)
                {
                    // 鼠标位置在窗体下方
                    m.Result = (IntPtr)WM_NCHITTEST.HTBOTTOM;
                    return;
                }
                else if (nPosX <= this.Left + 2)
                {
                    // 鼠标位置在窗体左侧
                    m.Result = (IntPtr)WM_NCHITTEST.HTLEFT;
                    return;
                }
                else if (nPosY <= this.Top + 2)
                {
                    // 鼠标位置在窗体上侧
                    m.Result = (IntPtr)WM_NCHITTEST.HTTOP;
                    return;
                }
            }
        }
    }
}
