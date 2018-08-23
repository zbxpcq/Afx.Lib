using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Afx.DynamicProxy
{
    /// <summary>
    /// 代理接口
    /// </summary>
    public interface IProxy
    {
        /// <summary>
        /// 设置代理TargetType
        /// </summary>
        /// <param name="type">Target Type</param>
        void SetTargetType(Type type);

        /// <summary>
        /// GetTargetType
        /// </summary>
        /// <returns></returns>
        Type GetTargetType();

        /// <summary>
        /// SetAopFunc
        /// </summary>
        /// <param name="funcs"></param>
        void SetAopFunc(Func<IAop>[] funcs);

        /// <summary>
        /// GetAopFunc
        /// </summary>
        /// <returns></returns>
        Func<IAop>[] GetAopFunc();
    }
}
