using System;
using System.Collections.Generic;
using System.Text;

namespace Afx.Ioc
{
    public interface IRegisterContext
    {
        IContainer Container { get; }

        Type ServiceType { get; }

        ObjectContext Context { get; }

        IRegisterContext SetName(string name);

        IRegisterContext SetKey(object key);

        IRegisterContext EnableAop(bool enable);

        IRegisterContext AopType(Type type);
    }

    public class RegisterContext : IRegisterContext
    {
        internal protected RegisterContext() { }

        public virtual IContainer Container { get; internal set; }

        public virtual Type ServiceType { get; internal set; }

        public virtual ObjectContext Context { get; internal set; }

        public virtual IRegisterContext AopType(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");
            if(!typeof(Afx.DynamicProxy.IAop).IsAssignableFrom(type)) throw new ArgumentException(type.FullName + " 不是 IAop！","type");
            this.CheckAop();
            if (this.Context.AopTypeList == null) this.Context.AopTypeList = new List<Type>();
            if (!this.Context.AopTypeList.Contains(type)) this.Context.AopTypeList.Add(type);
            this.Context.AopTypeList.TrimExcess();

            return this;
        }

        public virtual IRegisterContext EnableAop(bool enable)
        {
            this.CheckAop();
            this.Context.EnableAop = enable;

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

        public virtual IRegisterContext SetKey(object key)
        {
            if (key == null) throw new ArgumentNullException("key");
            this.Context.Key = key;

            return this;
        }

        public virtual IRegisterContext SetName(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            this.Context.Name = name;

            return this;
        }
    }
}
