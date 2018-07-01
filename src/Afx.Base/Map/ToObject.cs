using System;
using System.Collections.Generic;
using System.Text;

namespace Afx.Map
{
    /// <summary>
    /// ToObject
    /// </summary>
    public class ToObject : IToObject
    {
        [ThreadStatic]
        private List<object> currentToList;
        [ThreadStatic]
        private int num = 0;

        private Type m_stringType = typeof(string);
        /// <summary>
        /// m_objType
        /// </summary>
        protected Type m_objType;

        /// <summary>
        /// ToObject
        /// </summary>
        /// <param name="objType"></param>
        public ToObject(Type objType)
        {
            this.m_objType = objType;
        }
        /// <summary>
        /// IsTo
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected bool IsTo(object obj)
        {
            this.num++;
            if (this.m_objType.IsClass && this.m_objType != m_stringType)
            {
                if (this.currentToList == null || !this.currentToList.Contains(obj))
                {
                    if (this.currentToList == null) this.currentToList = new List<object>();
                    this.currentToList.Add(obj);

                    return true;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }
        /// <summary>
        /// Clear
        /// </summary>
        protected void Clear()
        {
            this.num--;
            if (this.num == 0)
            {
                if (this.currentToList != null) this.currentToList.Clear();
                this.currentToList = null;
            }
        }
        /// <summary>
        /// To
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public virtual object To(object obj)
        {
            return null;
        }
    }
}
