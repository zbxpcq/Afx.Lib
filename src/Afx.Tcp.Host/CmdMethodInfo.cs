using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace  Afx.Tcp.Host
{
    class CmdMethodInfo
    {
        public int Cmd { get; set; }

        public Type Type { get; set; }

        public MethodInfo Method { get; set; }

        public Type ParameterType { get; set; }

        public List<Type> AuthTypeList { get; set; }

        public bool NoAuth { get; set; }
    }
}
