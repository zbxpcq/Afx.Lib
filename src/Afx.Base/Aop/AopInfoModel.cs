using System;
using System.Collections.Generic;
using System.Text;

namespace Afx.Aop
{
    /// <summary>
    /// AopInfoModel
    /// </summary>
    public sealed class AopInfoModel
    {
        /// <summary>
        /// ClassId
        /// </summary>
        public int ClassId { get; set; }
        /// <summary>
        /// GlobalList
        /// </summary>
        public List<IAop> GlobalList { get; set; }
        /// <summary>
        /// TypeList
        /// </summary>
        public List<IAop> TypeList { get; set; }
        /// <summary>
        /// TypeAttributes
        /// </summary>
        public List<AopAttribute> TypeAttributes { get; set; }
        /// <summary>
        /// MethodAttributes
        /// </summary>
        public List<AopAttribute> MethodAttributes { get; set; }
    }
}
