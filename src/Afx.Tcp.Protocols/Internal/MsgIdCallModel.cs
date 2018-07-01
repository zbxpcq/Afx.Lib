using System;
using System.Collections.Generic;
using System.Text;

namespace Afx.Tcp.Protocols
{
    internal class MsgIdCallModel
    {
        public int Id { get; set; }

        public MsgDataCall Call { get; set; }

        public object State { get; set; }
    }
}
