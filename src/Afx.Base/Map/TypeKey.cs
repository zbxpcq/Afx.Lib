using System;
using System.Collections.Generic;
using System.Security;
using System.Text;

namespace Afx.Map
{
    /// <summary>
    /// TypeKey
    /// </summary>
    class TypeKey
    {
        /// <summary>
        /// FromType
        /// </summary>
        public Type FromType { get; set; }

        /// <summary>
        /// ToType
        /// </summary>
        public Type ToType { get; set; }

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if(obj != null && (obj is TypeKey))
            {
                var tk = obj as TypeKey;
                if(tk.FromType == this.FromType && tk.ToType == this.ToType)
                {
                    return true;
                }
            }
            
            return false;
        }

        /// <summary>
        /// GetHashCode
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            string s = string.Format("{0}_To_{1}",
                (this.FromType != null ? this.FromType.FullName : ""),
                (this.ToType != null ? this.ToType.FullName : ""));

            return s.GetHashCode();
        }
    }

    class TypeKeyComparer : IEqualityComparer<TypeKey>
    {

        public bool Equals(TypeKey x, TypeKey y)
        {
            if (x == null && y == null)
                return true;

            if(x != null && y != null)
            {
                return x.Equals(y);
            }

            return false;
        }

        public int GetHashCode(TypeKey obj)
        {
            if (obj == null) return 0;

            return obj.GetHashCode();
        }
    }
}
