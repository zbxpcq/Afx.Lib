using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Afx.Forms.Controls
{
    public partial class AfxButton : Button
    {
        public AfxButton()
        {
            this.Font = new Font("微软雅黑", 8F, FontStyle.Regular, GraphicsUnit.Point, 134);
            this.Type = AfxButtonType.None;
        }

        private AfxButtonType type = AfxButtonType.None;
        public AfxButtonType Type
        {
            get { return this.type; }
            set
            {
                this.type = value;
                switch (type)
                {
                    case AfxButtonType.OK:
                        this.BackColor = Color.SteelBlue;
                        break;
                    case AfxButtonType.Yes:
                        this.BackColor = Color.DodgerBlue;
                        break;
                    case AfxButtonType.Cancel:
                        this.BackColor = Color.DarkSalmon;
                        break;
                    case AfxButtonType.No:
                        this.BackColor = Color.LightCoral;
                        break;
                    case AfxButtonType.Delete:
                        this.BackColor = Color.OrangeRed;
                        break;
                }

                if(type != AfxButtonType.None)
                {
                    this.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
                    this.ForeColor = Color.White;
                }

                this.UseVisualStyleBackColor = true;
            }
        }
    }
}
