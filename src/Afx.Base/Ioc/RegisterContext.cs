using System;
using System.Collections.Generic;
using System.Text;

using Afx.DynamicProxy;

namespace Afx.Ioc
{
    /// <summary>
    /// TService注册信息
    /// </summary>
    public interface IRegisterContext
    {
        /// <summary>
        /// Container
        /// </summary>
        IContainer Container { get; }

        /// <summary>
        /// TService Type
        /// </summary>
        Type ServiceType { get; }
        /// <summary>
        /// TService ObjectContext
        /// </summary>
        ObjectContext Context { get; }

        /// <summary>
        /// 设置命名
        /// </summary>
        /// <param name="name">命名</param>
        /// <returns>this</returns>
        IRegisterContext Name(string name);

        /// <summary>
        /// 设置key
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>this</returns>
        IRegisterContext Key(object key);

        /// <summary>
        /// 开启关闭aop
        /// </summary>
        /// <param name="enabled">enable</param>
        /// <returns>this</returns>
        IRegisterContext Aop(bool enabled);

        /// <summary>
        /// 添加 IAop Type
        /// </summary>
        /// <param name="type">IAop Type</param>
        /// <returns>this</returns>
        IRegisterContext Aop(Type type);

        /// <summary>
        /// 添加 IAop Type
        /// </summary>
        /// <typeparam name="TAop"></typeparam>
        /// <returns>this</returns>
        IRegisterContext Aop<TAop>() where TAop: IAop;
    }

    /// <summary>
    /// TService注册信息
    /// </summary>
    public class RegisterContext : IRegisterContext
    {
        internal protected RegisterContext() { }
        /// <summary>
        /// Container
        /// </summary>
        public virtual IContainer Container { get; internal set; }
        /// <summary>
        /// TService Type
        /// </summary>
        public virtual Type ServiceType { get; internal set; }
        /// <summary>
        /// TService ObjectContext
        /// </summary>
        public virtual ObjectContext Context { get; internal set; }

        /// <summary>
        /// 添加 IAop Type
        /// </summary>
        /// <typeparam name="TAop"></typeparam>
        /// <returns>this</returns>
        public virtual IRegisterContext Aop<TAop>() where TAop : IAop
        {
            return this.Aop(typeof(TAop));
        }

        /// <summary>
        /// 添加 IAop Type
        /// </summary>
        /// <param name="type">IAop Type</param>
        /// <returns>this</returns>
        public virtual IRegisterContext Aop(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");
            if(!typeof(IAop).IsAssignableFrom(type)) throw new ArgumentException(type.FullName + " 不是 Afx.DynamicProxy.IAop！", "type");
            this.CheckAop();
            if (this.Context.AopTypeList == null) this.Context.AopTypeList = new List<Type>();
            if (!this.Context.AopTypeList.Contains(type)) this.Context.AopTypeList.Add(type);
            this.Context.AopTypeList.TrimExcess();

            return this;
        }
        /// <summary>
        /// 开启关闭aop
        /// </summary>
        /// <param name="enabled">enable</param>
        /// <returns>this</returns>
        public virtual IRegisterContext Aop(bool enabled)
        {
            this.CheckAop();
            this.Context.EnabledAop = enabled;
            if(enabled)
            {
                var container = this.Container as Container;
                switch(this.Context.Mode)
                {
                    case CreateMode.None:
                        container.proxyGenerator.RegisterClassProxy(this.Context.TargetInfo.TargetType);
                        break;
                    case CreateMode.Instance:
                    case CreateMode.Method:
                        if(this.ServiceType.IsInterface)
                            container.proxyGenerator.RegisterClassProxy(this.ServiceType);
                        break;
                }

            }

            return this;
        }

        private void CheckAop()
        {
            if(this.Context.Mode != CreateMode.None)
            {
                if (!this.ServiceType.IsInterface)
                    throw new ArgumentException(this.Context.Mode + "方式 TService 必须是 interface！", "TService");
            }
        }
        /// <summary>
        /// 设置key
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>this</returns>
        public virtual IRegisterContext Key(object key)
        {
            if (key == null) throw new ArgumentNullException("key");
            this.Context.Key = key;

            return this;
        }
        /// <summary>
        /// 设置命名
        /// </summary>
        /// <param name="name">命名</param>
        /// <returns>this</returns>
        public virtual IRegisterContext Name(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            this.Context.Name = name;

            return this;
        }
    }
}
