using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Afx.Forms.Controls
{
    public partial class UCFormTitle : AfxBaseUserControl
    {
        public UCFormTitle()
        {
            InitializeComponent();
        }
        
        private void UCFrmTitle_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.OnWinFrmMax();
        }

        private Point mouseDownPoint;
        private bool isMouseDown = false;
        private bool isMouseMove = false;
        protected void UCFrmTitle_MouseDown(object sender, MouseEventArgs e)
        {
            Form frm = CurrentFrm;
            if (frm != null)
            {
                if (e.Button == MouseButtons.Left && frm.WindowState != FormWindowState.Maximized)
                {
                    this.mouseDownPoint = new Point(e.X, e.Y);
                    this.isMouseDown = true;
                }
            }
        }

        protected void UCFrmTitle_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && isMouseDown)
            {
                Form frm = CurrentFrm;
                 if (frm != null)
                 {
                     int x = frm.Location.X + e.X - mouseDownPoint.X;
                     int y = frm.Location.Y + e.Y - mouseDownPoint.Y;
                     frm.Location = new Point(x, y);
                 }
                isMouseMove = true;
            }
        }

        protected void UCFrmTitle_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.isMouseDown = false;
                if (this.isMouseMove)
                {
                    this.isMouseMove = false;
                    Form frm = CurrentFrm;
                    if (frm != null)
                    {
                        if (frm.Location.Y < 0)
                            frm.Location = new Point(frm.Location.X, 0);
                    }
                }
            }
        }

        public bool IsShowMinBox
        {
            get
            {
                return this.picMin.Visible;
            }
            set
            {
                this.picMin.Visible = value;
            }
        }

        public bool IsShowMaxBox
        {
            get
            {
                return this.picMax.Visible;
            }
            set
            {
                if (value == false)
                {
                    this.picMin.Location = this.picMax.Location;
                }
                this.picMax.Visible = value;
            }
        }

        public bool IsShowCloseBox
        {
            get
            {
                return this.picClose.Visible;
            }
            set
            {
                this.picClose.Visible = value;
            }
        }

        protected virtual void OnWinFrmMin()
        {
            Form frm = CurrentFrm;
            if (frm != null)
            {
                frm.WindowState = FormWindowState.Minimized;
            }
        }

        protected virtual void OnWinFrmMax()
        {
            if (this.IsShowMaxBox)
            {
                Form frm = CurrentFrm;
                if (frm != null)
                {
                    Rectangle rect = Screen.GetWorkingArea(this);
                    int h = frm.MaximumSize.Height;
                    int w = frm.MaximumSize.Width;
                    bool flag = false;
                    if (h == 0 || h > rect.Height)
                    {
                        h = rect.Height;
                        flag = true;
                    }
                    if (w == 0 || w > rect.Width)
                    {
                        w = rect.Width;
                        flag = true;
                    }
                    if (flag)
                    {
                        frm.MaximumSize = new Size(w, h);
                    }

                    switch (frm.WindowState)
                    {
                        case FormWindowState.Normal:
                            frm.WindowState = FormWindowState.Maximized;
                            picMax.Image = Properties.Resources.frmTitle_normal_normal;
                            break;
                        case FormWindowState.Maximized:
                            frm.WindowState = FormWindowState.Normal;
                            picMax.Image = Properties.Resources.frmTitle_max_normal;
                            break;
                    }
                }
            }
        }

        protected virtual void OnWinFrmClose()
        {
            Form frm = CurrentFrm;
            if (frm != null)
            {
                frm.Close();
                if (!frm.IsDisposed)
                    frm.Dispose();
                if (!this.IsDisposed)
                    this.Dispose();
            }
        }

        private void picMin_Click(object sender, EventArgs e)
        {
            this.OnWinFrmMin();
        }

        private void picMin_MouseEnter(object sender, EventArgs e)
        {
            this.picMin.Image = Properties.Resources.frmTitle_mini_down;
        }

        private void picMin_MouseLeave(object sender, EventArgs e)
        {
            this.picMin.Image = Properties.Resources.frmTitle_mini_normal;
        }

        private void picMax_Click(object sender, EventArgs e)
        {
            this.OnWinFrmMax();
        }

        private void picMax_MouseEnter(object sender, EventArgs e)
        {
            Form frm = CurrentFrm;
            if (frm != null)
            {
                switch (frm.WindowState)
                {
                    case FormWindowState.Normal:
                        picMax.Image = Properties.Resources.frmTitle_max_down;
                        break;
                    case FormWindowState.Maximized:
                        picMax.Image = Properties.Resources.frmTitle_normal_down;
                        break;
                }
            }
        }

        private void picMax_MouseLeave(object sender, EventArgs e)
        {
            Form frm = CurrentFrm;
            if (frm != null)
            {
                switch (frm.WindowState)
                {
                    case FormWindowState.Normal:
                        picMax.Image = Properties.Resources.frmTitle_max_normal;
                        break;
                    case FormWindowState.Maximized:
                        picMax.Image = Properties.Resources.frmTitle_normal_normal;
                        break;
                }
            }
        }

        private void picClose_Click(object sender, EventArgs e)
        {
            this.OnWinFrmClose();
        }

        private void picClose_MouseEnter(object sender, EventArgs e)
        {
            this.picClose.Image = Properties.Resources.frmTitle_close_down;
        }

        private void picClose_MouseLeave(object sender, EventArgs e)
        {
            this.picClose.Image = Properties.Resources.frmTitle_close_normal;
        }
    }
}
