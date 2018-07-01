using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Afx.Forms.Controls
{
    public class AfxBaseUserControl : UserControl
    {
        public SynchronizationContext Sync { get; private set; }
        public int ManagedThreadId { get; private set; }
        public AfxBaseUserControl()
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

        private Form _currentFrm = null;
        protected Form CurrentFrm
        {
            get
            {
                if (_currentFrm == null)
                    _currentFrm = this.FindForm();

                return _currentFrm;
            }
        }
    }
}
