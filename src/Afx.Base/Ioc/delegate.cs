using System;
using System.Collections.Generic;
using System.Text;

namespace Afx.Ioc
{
    public class OnGetContext : RegisterContext
    {
        public object[] Arguments { get; set; }

        public object Target { get; set; }
    }

    public delegate object OnGetCallback(OnGetContext context);
}
