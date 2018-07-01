using System;
using System.Collections.Generic;
using System.Text;

namespace Afx.Aop
{
    class AopTypeModel
    {
        public List<Type> AopTypeList { get; set; }

        public Dictionary<string, List<Type>> MethodAopType { get; internal set; }

        internal List<Type> GetMethodAopTypeList(string method)
        {
            List<Type> list = null;
            if(this.MethodAopType != null && !this.MethodAopType.TryGetValue(method, out list))
            {
                list = new List<Type>(0);
            }

            return list ?? new List<Type>(0);
        }
    }
}
