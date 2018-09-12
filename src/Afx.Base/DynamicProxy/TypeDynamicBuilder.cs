using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Afx.DynamicProxy
{
    /// <summary>
    /// TypeDynamicBuilder
    /// </summary>
    public class TypeDynamicBuilder
    {
        private TypeBuilder typeBuilder;
        private List<FieldBuilder> fieldList;

        /// <summary>
        /// TypeDynamicBuilder
        /// </summary>
        /// <param name="typeBuilder"></param>
        public TypeDynamicBuilder(TypeBuilder typeBuilder)
        {
            if (typeBuilder == null) throw new ArgumentNullException("typeBuilder");
            this.typeBuilder = typeBuilder;
            this.fieldList = new List<FieldBuilder>(3);
        }

        /// <summary>
        /// AddInterfaceImplementation
        /// </summary>
        /// <param name="interfaceType"></param>
        public void AddInterfaceImplementation(Type interfaceType)
        {
            if (interfaceType == null) throw new ArgumentNullException("interfaceType");
            if (!interfaceType.IsInterface) throw new ArgumentNullException(interfaceType.FullName + "不是接口！", "interfaceType");
            this.typeBuilder.AddInterfaceImplementation(interfaceType);
        }

        /// <summary>
        /// GetFieldBuilder
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public FieldBuilder GetFieldBuilder(string fieldName)
        {
            FieldBuilder fieldBuilder = this.fieldList.Find(q => q.Name == fieldName);

            return fieldBuilder;
        }

        /// <summary>
        /// DefineField
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="type"></param>
        /// <param name="attributes"></param>
        /// <returns></returns>
        public FieldBuilder DefineField(string fieldName, Type type, FieldAttributes attributes)
        {
            if (string.IsNullOrEmpty(fieldName)) throw new ArgumentNullException("fieldName");
            if (this.fieldList.Exists(q => q.Name == fieldName)) throw new ArgumentException(fieldName + " 不能重名！", "fieldName");
            if (type == null) throw new ArgumentNullException("type");

            var field = this.typeBuilder.DefineField(fieldName, type, attributes);
            this.fieldList.Add(field);

            return field;
        }

        /// <summary>
        /// DefineConstructor
        /// </summary>
        /// <param name="attributes"></param>
        /// <param name="callingConvention"></param>
        /// <param name="parameterTypes"></param>
        /// <returns></returns>
        public ILGenerator DefineConstructor(MethodAttributes attributes, CallingConventions callingConvention, Type[] parameterTypes)
        {
            if (parameterTypes == null) throw new ArgumentNullException("parameterTypes");
            ConstructorBuilder ctor = this.typeBuilder.DefineConstructor(attributes, callingConvention, parameterTypes);
            ILGenerator ctorIL = ctor.GetILGenerator();
            return ctorIL;
        }

        /// <summary>
        /// DefineMethod
        /// </summary>
        /// <param name="name"></param>
        /// <param name="attributes"></param>
        /// <param name="callingConvention"></param>
        /// <param name="returnType"></param>
        /// <param name="parameterTypes"></param>
        /// <returns></returns>
        public ILGenerator DefineMethod(string name, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            if (returnType == null) throw new ArgumentNullException("returnType");
            if (parameterTypes == null) throw new ArgumentNullException("parameterTypes");
            var methodBuilder = this.typeBuilder.DefineMethod(name, attributes, callingConvention, returnType, parameterTypes);
            ILGenerator il = methodBuilder.GetILGenerator();
            
            return il;
        }

        /// <summary>
        /// DefineMethod
        /// </summary>
        /// <param name="name"></param>
        /// <param name="attributes"></param>
        /// <param name="callingConvention"></param>
        /// <param name="returnType"></param>
        /// <param name="parameterTypes"></param>
        /// <returns></returns>
        public ILGenerator DefineMethod(string name, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, ParameterInfo[] parameterInfos)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            if (returnType == null) throw new ArgumentNullException("returnType");
            if (parameterInfos == null) throw new ArgumentNullException("parameterInfos");
            Type[] parameterTypes = ProxyUtil.GetType(parameterInfos);
            var methodBuilder = this.typeBuilder.DefineMethod(name, attributes, callingConvention, returnType, parameterTypes);
            for (int i = 0; i < parameterInfos.Length; i++)
            {
                var parameterInfo = parameterInfos[i];
                var parameterBuilder = methodBuilder.DefineParameter(parameterInfo.Position + 1, parameterInfo.Attributes, parameterInfo.Name);
                if ((parameterInfo.Attributes & ParameterAttributes.HasDefault) == ParameterAttributes.HasDefault)
                {
                    parameterBuilder.SetConstant(parameterInfo.DefaultValue);
                }
            }
            ILGenerator il = methodBuilder.GetILGenerator();

            return il;
        }

        /// <summary>
        /// CreateType
        /// </summary>
        /// <returns></returns>
        public Type CreateType()
        {
            Type type = null;
#if NETSTANDARD
            type = typeBuilder.CreateTypeInfo();
#else
            type = typeBuilder.CreateType();
#endif

            return type;
        }
    }
}
