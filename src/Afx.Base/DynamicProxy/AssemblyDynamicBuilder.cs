using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;

namespace Afx.DynamicProxy
{
    /// <summary>
    /// AssemblyDynamicBuilder
    /// </summary>
    public class AssemblyDynamicBuilder
    {
        private static int globalIdentify = 0;
        /// <summary>
        /// globalIdentify
        /// </summary>
        /// <returns></returns>
        public static int GetGlobalId()
        {
            return Interlocked.Increment(ref globalIdentify);
        }

        private string m_moduleName;
        private ModuleBuilder m_moduleBuilder;
        private AssemblyBuilder m_assemblyBuilder;
#if NETCOREAPP || NETSTANDARD
        private AssemblyBuilderAccess m_assemblyBuilderAccess = AssemblyBuilderAccess.Run;
#else
        private AssemblyBuilderAccess m_assemblyBuilderAccess = AssemblyBuilderAccess.Run;
#endif
        /// <summary>
        /// AssemblyDynamicBuilder
        /// </summary>
        public AssemblyDynamicBuilder()
        {
            this.m_moduleName = ProxyUtil.MODULE_NAME + GetGlobalId();

            var assemblyName = new AssemblyName(this.m_moduleName);
            assemblyName.Version = new Version(1, 0, 0, 0);
#if NETCOREAPP || NETSTANDARD
            this.m_assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, this.m_assemblyBuilderAccess);
            this.m_moduleBuilder = this.m_assemblyBuilder.DefineDynamicModule(this.m_moduleName);
#else
            this.m_assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, this.m_assemblyBuilderAccess);
            if (this.m_assemblyBuilderAccess == AssemblyBuilderAccess.RunAndSave)
            {
                this.m_moduleBuilder = this.m_assemblyBuilder.DefineDynamicModule(this.m_moduleName, this.m_moduleName + ".dll");
            }
            else
            {
                this.m_moduleBuilder = this.m_assemblyBuilder.DefineDynamicModule(this.m_moduleName);
            }
#endif
        }

        /// <summary>
        /// GetDynamicName
        /// </summary>
        /// <param name="targetType"></param>
        /// <returns></returns>
        public string GetDynamicName(Type targetType)
        {
            if (targetType == null) throw new ArgumentNullException("targetType");
            string name = targetType.Name;
            int index = name.IndexOf('~');
            if (index > 0) name = name.Substring(0, index);
            return string.Format("{0}.{1}Proxy{2}", this.m_moduleName, name, GetGlobalId());
        }

        /// <summary>
        /// DefineType
        /// </summary>
        /// <param name="name"></param>
        /// <param name="attr"></param>
        /// <param name="parent"></param>
        /// <param name="interfaces"></param>
        /// <returns></returns>
        public TypeDynamicBuilder DefineType(string name, TypeAttributes attr, Type parent, Type[] interfaces)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            if (parent == null) throw new ArgumentNullException("parent");
            if (interfaces == null) throw new ArgumentNullException("interfaces");
            var typeBuilder = this.m_moduleBuilder.DefineType(name, attr, parent, interfaces);

            return new TypeDynamicBuilder(typeBuilder);
        }
    }
}
