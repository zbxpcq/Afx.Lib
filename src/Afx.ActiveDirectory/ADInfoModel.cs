using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Afx.ActiveDirectory
{
    /// <summary>
    /// ADPropertyModel
    /// </summary>
    public class ADPropertyModel
    {
        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Value
        /// </summary>
        public string Value { get; set; }
    }
    /// <summary>
    /// ADPropertyCollection
    /// </summary>
    public class ADPropertyCollection : IEnumerable<ADPropertyModel>, IDisposable
    {
        private List<ADPropertyModel> list;
        /// <summary>
        /// ADPropertyCollection
        /// </summary>
        public ADPropertyCollection()
        {
            this.list = new List<ADPropertyModel>();
        }
        /// <summary>
        /// ADPropertyCollection
        /// </summary>
        /// <param name="capacity"></param>
        public ADPropertyCollection(int capacity)
        {
            this.list = new List<ADPropertyModel>(capacity);
        }
        /// <summary>
        /// Count
        /// </summary>
        public int Count
        {
            get { return this.list.Count; }
        }
        /// <summary>
        /// TrimExcess
        /// </summary>
        public void TrimExcess()
        {
            this.list.TrimExcess();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void Add(string name, string value)
        {
            if (!string.IsNullOrEmpty(name))
            {
                var m = this.list.Find(q => string.Compare(q.Name, name, true) == 0);
                if(m == null)
                {
                    m = new ADPropertyModel() { Name = name };
                    this.list.Add(m);
                }
                m.Value = value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string Get(string name)
        {
            string value = null;
            if (!string.IsNullOrEmpty(name))
            {
                var m = this.list.Find(q => string.Compare(q.Name, name, true) == 0);
                if (m != null)
                {
                    value = m.Value;
                }
            }

            return value;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool Remove(string name)
        {
            bool result = false;
            if (!string.IsNullOrEmpty(name))
            {
                result = this.list.RemoveAll(q => string.Compare(q.Name, name, true) == 0) > 0;
            }

            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool Contains(string name)
        {
            bool result = false;
            if (!string.IsNullOrEmpty(name))
            {
                result = this.list.FindIndex(q => string.Compare(q.Name, name, true) == 0) >= 0;
            }

            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        {
            this.list.Clear();
            this.list.TrimExcess();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string this[string name]
        {
            get { return this.Get(name); }
            set { this.Add(name, value); }
        }
        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            if (this.list != null)
            {
                this.list.Clear();
            }
            this.list = null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerator<ADPropertyModel> GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.list.GetEnumerator();
        }
    }

    /// <summary>
    /// AD域对象
    /// </summary>
    public class ADInfoModel : IDisposable
    {
        /// <summary>
        /// AD域对象的名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// AD域对象GUID
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// AD域对象路径
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// AD域对象架构类的名称
        /// </summary>
        public string SchemaClassName { get; set; }
         
        /// <summary>
        /// AD域对象属性
        /// </summary>
        public ADPropertyCollection Properties { get; internal set; }

        /// <summary>
        /// AD域对象 Children
        /// </summary>
        public List<ADInfoModel> Children { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            if (this.Properties != null) this.Properties.Clear();
            this.Properties = null;
            if (this.Children != null) this.Children.Clear();
            this.Children = null;
        }
    }
}
