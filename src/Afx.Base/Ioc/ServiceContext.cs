using System;
using System.Collections.Generic;
using System.Text;

namespace Afx.Ioc
{
    internal class ServiceContext
    {
        public Type ServiceType { get; private set; }

        private List<ObjectContext> objectList = new List<ObjectContext>();

        private object root = new object();

        internal ServiceContext(Type serviceType)
        {
            if (serviceType == null) throw new ArgumentNullException("serviceType");
            this.ServiceType = serviceType;
        }

        internal ServiceContext(Type serviceType, TargetContext targetInfo): this(serviceType)
        {
            if (targetInfo == null) throw new ArgumentNullException("targetInfo");
            objectList.Add(new ObjectContext(targetInfo));
        }

        internal ServiceContext(Type serviceType, FuncContext func) : this(serviceType)
        {
            if (func == null) throw new ArgumentNullException("func");
            objectList.Add(new ObjectContext(func));
        }

        internal ServiceContext(Type serviceType, object instance) : this(serviceType)
        {
            if (instance == null) throw new ArgumentNullException("instance");
            objectList.Add(new ObjectContext(instance));
        }

        public ObjectContext Add(TargetContext targetInfo)
        {
            if (targetInfo == null) throw new ArgumentNullException("targetInfo");
            ObjectContext objectContext = null;
            lock (this.root)
            {
                objectContext = this.objectList.Find(q => q.Mode == CreateMode.None && q.TargetInfo.TargetType == targetInfo.TargetType);
                if (objectContext == null)
                {
                    objectContext = new ObjectContext(targetInfo);
                    this.objectList.Add(objectContext);
                    this.objectList.TrimExcess();
                }
            }

            return objectContext;
        }

        public ObjectContext Add(FuncContext func)
        {
            if (func == null) throw new ArgumentNullException("func");
            ObjectContext objectContext = null;
            lock (this.root)
            {
                objectContext = this.objectList.Find(q => q.Mode == CreateMode.Method && q.Func.Method == func.Method && q.Func.Target == func.Target);
                if (objectContext == null)
                {
                    objectContext = new ObjectContext(func);
                    this.objectList.Add(objectContext);
                    this.objectList.TrimExcess();
                }
            }

            return objectContext;
        }

        public ObjectContext Add(object instance)
        {
            if (instance == null) throw new ArgumentNullException("instance");
            ObjectContext objectContext = null;
            lock (this.root)
            {
                objectContext = this.objectList.Find(q => q.Mode == CreateMode.Instance && q.Instance == instance);
                if (objectContext == null)
                {
                    objectContext = new ObjectContext(instance);
                    this.objectList.Add(objectContext);
                    this.objectList.TrimExcess();
                }
            }

            return objectContext;
        }

        public ObjectContext Get()
        {
            ObjectContext objectContext = null;
            lock (this.root)
            {
                if(this.objectList.Count > 0) objectContext = this.objectList[this.objectList.Count - 1];
            }

            return objectContext;
        }

        public ObjectContext GetByName(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            ObjectContext objectContext = null;
            lock (this.root)
            {
                objectContext = this.objectList.Find(q=>q.Name == name);
            }

            return objectContext;
        }

        public ObjectContext GetByKey(object key)
        {
            if (key == null) throw new ArgumentNullException("key");
            ObjectContext objectContext = null;
            lock (this.root)
            {
                objectContext = this.objectList.Find(q => key.Equals(q.Key));
            }

            return objectContext;
        }
    }
}
