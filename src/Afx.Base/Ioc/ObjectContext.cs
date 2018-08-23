using System;
using System.Collections.Generic;
using System.Text;

namespace Afx.Ioc
{
    /// <summary>
    /// TService 实现信息
    /// </summary>
    public class ObjectContext
    {
        /// <summary>
        /// TService CreateMode
        /// </summary>
        public CreateMode Mode { get; private set; }

        /// <summary>
        /// TService Target Info (Mode=CreateMode.None)
        /// </summary>
        public TargetContext TargetInfo { get; private set; }

        /// <summary>
        /// TService Func Info (Mode=CreateMode.Method)
        /// </summary>
        public FuncContext Func { get; private set; }

        /// <summary>
        /// TService Instance (Mode=CreateMode.Instance)
        /// </summary>
        public object Instance { get; private set; }

        internal string Name { get;  set; }

        internal object Key { get; set; }

        internal bool EnableAop { get; set; }

        internal List<Type> AopTypeList { get; set; }

        internal ObjectContext(TargetContext targetInfo)
        {
            if (targetInfo == null) throw new ArgumentNullException("targetInfo");
            this.TargetInfo = targetInfo;
            this.Mode = CreateMode.None;
        }

        internal ObjectContext(FuncContext func)
        {
            if (func == null) throw new ArgumentNullException("func");
            this.Func = func;
            this.Mode = CreateMode.Method;
        }

        internal ObjectContext(object instance)
        {
            if (instance == null) throw new ArgumentNullException("instance");
            this.Instance = instance;
            this.Mode = CreateMode.Instance;
        }
    }
}
