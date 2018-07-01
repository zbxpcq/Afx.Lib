using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Afx.ActiveDirectory
{
    /// <summary>
    /// AD域操作类
    /// </summary>
    public class ADDomain
    {
        private string _domain;
        private string _account;
        private string _password;

        /// <summary>
        /// 异常回调
        /// </summary>
        public Action<Exception> ErrorCall;
        private void OnErrorCall(Exception ex)
        {
            if(this.ErrorCall != null)
            {
                try { this.ErrorCall(ex); }
                catch { }
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="domain">域名</param>
        public ADDomain(string domain)
        {
            this._domain = domain;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="domain">域名</param>
        /// <param name="account">域账号</param>
        /// <param name="password">域密码</param>
        public ADDomain(string domain, string account, string password)
        {
            this._domain = domain;
            this.Set(account, password);
        }

        /// <summary>
        /// 设置账户密码
        /// </summary>
        /// <param name="account">域账号</param>
        /// <param name="password">域密码</param>
        public void Set(string account, string password)
        {
            this._account = account;
            this._password = password;
        }

        /// <summary>
        /// 使用默认账号密码登录AD域
        /// </summary>
        /// <returns>true：登录成功，false：登录失败</returns>
        public bool Login()
        {
            return this.Login(this._account, this._password, -1);
        }

        /// <summary>
        /// 使用默认账号密码登录AD域
        /// </summary>
        /// <param name="millisecondsTimeout">超时时间（毫秒）</param>
        /// <returns>true：登录成功，false：登录失败</returns>
        public bool Login(int millisecondsTimeout)
        {
            return this.Login(this._account, this._password, millisecondsTimeout);
        }
        
        /// <summary>
        /// 登录AD域
        /// </summary>
        /// <param name="account">域账号</param>
        /// <param name="password">域密码</param>
        /// <returns>true：登录成功，false：登录失败</returns>
        public bool Login(string account, string password)
        {
            return Login(account, password, -1);
        }

        private bool LoginTask(object obj)
        {
            object[] arr = obj as object[];
            string account = arr[0] as string;
            string password = arr[1] as string;
            bool result = false;
            if (string.IsNullOrEmpty(this._domain) || string.IsNullOrEmpty(account) || string.IsNullOrEmpty(password))
            {
                return result;
            }
            try
            {
                string path = string.Format("LDAP://{0}", this._domain);
                string domainaccount = string.Format("{0}\\{1}", this._domain, account);
                using (DirectoryEntry entry = new DirectoryEntry(path, domainaccount, password))
                {
                    entry.RefreshCache();
                    result = true;
                }
            }
            catch (Exception ex)
            {
                this.OnErrorCall(ex);
            }

            return result;
        }

        /// <summary>
        /// 登录AD域
        /// </summary>
        /// <param name="account">域账号</param>
        /// <param name="password">域密码</param>
        /// <param name="millisecondsTimeout">超时时间（毫秒）</param>
        /// <returns>true：登录成功，false：登录失败</returns>
        public bool Login(string account, string password, int millisecondsTimeout)
        {
            bool result = false;
            var task = Task.Factory.StartNew(new Func<object, bool>(this.LoginTask),
                new object[]{account, password});

            task.Wait(millisecondsTimeout);
            if(task.IsCompleted)
            {
                result = task.Result;
            }
            return result;
        }

        private bool ExistsTask(object obj)
        {
            string account = obj as string;
            bool result = false;
            if (string.IsNullOrEmpty(this._domain) || string.IsNullOrEmpty(this._account)
                || string.IsNullOrEmpty(this._password) || string.IsNullOrEmpty(account))
            {
                return result;
            }

            try
            {
                string path = string.Format("LDAP://{0}", this._domain);
                string domainaccount = string.Format("{0}\\{1}", this._domain, this._account);
                using (DirectoryEntry entry = new DirectoryEntry(path, domainaccount, this._password))
                {
                    using (DirectorySearcher search = new DirectorySearcher(entry))
                    {
                        search.Filter = "(&(&(objectCategory=person)(objectClass=user))(sAMAccountName=" + account + "))"; // LDAP 查询串
                        SearchResult searchResult = search.FindOne();
                        result = searchResult != null;
                    }
                }
            }
            catch (Exception ex)
            {
                this.OnErrorCall(ex);
            };

            return result;
        }

        /// <summary>
        /// 是否存在域账号
        /// </summary>
        /// <param name="account">域账号</param>
        /// <returns>true：存在，false：不存在</returns>
        public bool Exists(string account)
        {
            return this.Exists(account, -1);
        }

        /// <summary>
        /// 是否存在域账号
        /// </summary>
        /// <param name="account">域账号</param>
        /// <param name="millisecondsTimeout">超时时间（毫秒）</param>
        /// <returns>true：存在，false：不存在</returns>
        public bool Exists(string account, int millisecondsTimeout)
        {
            bool result = false;
            
            var task = Task.Factory.StartNew(new Func<object, bool>(this.ExistsTask), account);

            task.Wait(millisecondsTimeout);
            if (task.IsCompleted)
            {
                result = task.Result;
            }

            return result;
        }

        private List<string> GetUserFieldsTask(object obj)
        {
            int searchUserCount = (int)obj;
            List<string> list = null;
            if (string.IsNullOrEmpty(this._domain) || string.IsNullOrEmpty(this._account)
                || string.IsNullOrEmpty(this._password))
            {
                return list;
            }
            try
            {
                string path = string.Format("LDAP://{0}", this._domain);
                string domainaccount = string.Format("{0}\\{1}", this._domain, this._account);
                using (DirectoryEntry entry = new DirectoryEntry(path, domainaccount, this._password))
                {
                    using (DirectorySearcher search = new DirectorySearcher(entry))
                    {
                        search.PageSize = 0;
                        search.SizeLimit = searchUserCount > 0 ? searchUserCount : 50;
                        search.Asynchronous = true;
                        search.Filter = ("(&(objectCategory=person)(objectClass=user))");
                        list = new List<string>(70);
                        using (var all = search.FindAll())
                        {
                            foreach (SearchResult rearchResult in all)
                            {
                                foreach (DictionaryEntry kv in rearchResult.Properties)
                                {
                                    if (kv.Key != null)
                                    {
                                        string name = kv.Key.ToString();
                                        if (!string.IsNullOrEmpty(name) && !list.Contains(name, StringComparer.OrdinalIgnoreCase))
                                        {
                                            list.Add(name);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.OnErrorCall(ex);
            }


            return list;
        }

        /// <summary>
        /// 获取用户所有属性名称
        /// </summary>
        /// <returns>所有属性名称</returns>
        public List<string> GetUserFields()
        {
            return this.GetUserFields(-1, 50);
        }

        /// <summary>
        /// 获取用户所有属性名称
        /// </summary>
        /// <param name="millisecondsTimeout">超时时间（毫秒）</param>
        /// <returns>所有属性名称</returns>
        public List<string> GetUserFields(int millisecondsTimeout)
        {
            return this.GetUserFields(millisecondsTimeout, 50);
        }

        /// <summary>
        /// 获取用户所有属性名称
        /// </summary>
        /// <param name="millisecondsTimeout">超时时间（毫秒）</param>
        /// <param name="searchUserCount">需要搜索的用户数量</param>
        /// <returns>所有属性名称</returns>
        public List<string> GetUserFields(int millisecondsTimeout, int searchUserCount)
        {
            List<string> list = null;

            var task = Task.Factory.StartNew(new Func<object, List<string>>(this.GetUserFieldsTask), searchUserCount);

            task.Wait(millisecondsTimeout);
            if(task.IsCompleted)
            {
                list = task.Result;
            }

            return list ?? new List<string>(0);
        }

        private ADInfoModel Get(SearchResult searchResult)
        {
            ADInfoModel m = null;
            try
            {
                if (searchResult != null)
                {
                    m = new ADInfoModel();
                    m.Properties = new ADPropertyCollection(searchResult.Properties.Count);
                    foreach (DictionaryEntry kv in searchResult.Properties)
                    {
                        if (kv.Key != null && kv.Value is ResultPropertyValueCollection)
                        {
                            string name = kv.Key.ToString();
                            object obj = this.GetValue(kv.Value as ResultPropertyValueCollection);
                            string val = null;
                            if (string.Compare(name, "objectguid") == 0 && obj is byte[])
                            {
                                Guid guid = new Guid(obj as byte[]);
                                m.Guid = guid;
                                val = guid.ToString();
                            }
                            else
                            {
                                val = GetString(obj);
                            }

                            if (!string.IsNullOrEmpty(val))
                            {
                                m.Properties[name] = val;
                            }
                        }
                    }

                    m.Path = searchResult.Path;
                    if (m.Properties.Contains("name"))
                    {
                        m.Name = m.Properties["name"];
                    }
                    if (m.Properties.Contains("objectclass"))
                    {
                        m.SchemaClassName = m.Properties["objectclass"];
                    }
                }
            }
            catch
            {
                m = null;
            }

            return m;
        }

        private ADInfoModel GetUserInfoTask(object obj)
        {
            string account = obj as string;
            ADInfoModel result = null;
            if (string.IsNullOrEmpty(this._domain) || string.IsNullOrEmpty(this._account)
                || string.IsNullOrEmpty(this._password) || string.IsNullOrEmpty(account))
            {
                return result;
            }
            
            try
            {
                string path = string.Format("LDAP://{0}", this._domain);
                string domainaccount = string.Format("{0}\\{1}", this._domain, this._account);
                using (DirectoryEntry entry = new DirectoryEntry(path, domainaccount, this._password))
                {
                    using (DirectorySearcher search = new DirectorySearcher(entry))
                    {
                        search.Filter = "(&(&(objectCategory=person)(objectClass=user))(sAMAccountName=" + account + "))";
                        search.SearchScope = SearchScope.Subtree;
                        SearchResult searchResult = search.FindOne();
                        if (searchResult != null)
                        {
                            result = Get(searchResult);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.OnErrorCall(ex);
            }

            return result;
        }

        /// <summary>
        /// 获取指定域账号信息
        /// </summary>
        /// <param name="account">域账号</param>
        /// <returns>域账号信息</returns>
        public ADInfoModel GetUserInfo(string account)
        {
            return this.GetUserInfo(account, -1);
        }

        /// <summary>
        /// 获取指定域账号信息
        /// </summary>
        /// <param name="account">域账号</param>
        /// <param name="millisecondsTimeout">超时时间（毫秒）</param>
        /// <returns>域账号信息</returns>
        public ADInfoModel GetUserInfo(string account, int millisecondsTimeout)
        {
            ADInfoModel model = null;

            var task = Task.Factory.StartNew(new Func<object, ADInfoModel>(this.GetUserInfoTask), account);

            task.Wait(millisecondsTimeout);
            if(task.IsCompleted)
            {
                model = task.Result;
            }

            return model;
        }

        private List<ADInfoModel> SearchTask(object obj)
        {
            object[] arr = obj as object[];
            string searchPath = arr[0] as string;
            List<string> noSchemaClassName = arr[1] as List<string>;
            List<string> noName = arr[2] as List<string>;
            List<string> propertiesToLoad = arr[3] as List<string>;
            string filter = arr[4] as string;
            SearchScope searchScope = (SearchScope)arr[5];

            List<ADInfoModel> list = null;
            if (string.IsNullOrEmpty(this._domain) || string.IsNullOrEmpty(this._account)
                || string.IsNullOrEmpty(this._password))
            {
                return list;
            }

            try
            {
                string path = searchPath;
                if (string.IsNullOrEmpty(path))
                {
                    path = string.Format("LDAP://{0}", this._domain);
                }
                string domainaccount = string.Format("{0}\\{1}", this._domain, this._account);
                using (DirectoryEntry entry = new DirectoryEntry(path, domainaccount, this._password))
                {
                    using (DirectorySearcher search = new DirectorySearcher(entry))
                    {
                        search.SearchScope = searchScope;
                        search.Asynchronous = true;
                        search.SizeLimit = int.MaxValue;

                        if(!string.IsNullOrEmpty(filter))
                        {
                            search.Filter = filter;
                        }

                        if(propertiesToLoad != null && propertiesToLoad.Count > 0)
                        {
                            if (!propertiesToLoad.Contains("name", StringComparer.OrdinalIgnoreCase)) propertiesToLoad.Add("name");
                            if (!propertiesToLoad.Contains("objectclass", StringComparer.OrdinalIgnoreCase)) propertiesToLoad.Add("objectclass");
                            if (!propertiesToLoad.Contains("objectguid", StringComparer.OrdinalIgnoreCase)) propertiesToLoad.Add("objectguid");
                            if (!propertiesToLoad.Contains("adspath", StringComparer.OrdinalIgnoreCase)) propertiesToLoad.Add("adspath");
                            search.PropertiesToLoad.AddRange(propertiesToLoad.ToArray());
                        }

                        using (var all = search.FindAll())
                        {
                            list = new List<ADInfoModel>(all.Count);
                            foreach (SearchResult searchResult in all)
                            {
                                var info = Get(searchResult);
                                if (info != null)
                                {
                                    if ((noSchemaClassName == null || !noSchemaClassName.Contains(info.SchemaClassName, StringComparer.OrdinalIgnoreCase))
                                        && (noName == null || !noName.Contains(info.Name, StringComparer.OrdinalIgnoreCase)))
                                    {
                                        list.Add(info);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.OnErrorCall(ex);
            }

            return list;
        }

        /// <summary>
        /// 搜索AD域对象信息
        /// </summary>
        /// <param name="searchPath">搜索路径</param>
        /// <returns></returns>
        public List<ADInfoModel> Search(string searchPath)
        {
            return this.Search(searchPath, null, null, null, null, 1, -1);
        }

        /// <summary>
        /// 搜索AD域对象信息
        /// </summary>
        /// <param name="searchPath">搜索路径</param>
        /// <param name="noSchemaClassName">跳过的SchemaClassName</param>
        /// <param name="noName">跳过的AD域对象名称</param>
        /// <returns></returns>
        public List<ADInfoModel> Search(string searchPath,
            List<string> noSchemaClassName, List<string> noName)
        {
            return this.Search(searchPath, noSchemaClassName, noName, null, null, 1, -1);
        }

        /// <summary>
        /// 搜索AD域对象信息
        /// </summary>
        /// <param name="searchPath">搜索路径</param>
        /// <param name="noSchemaClassName">跳过的SchemaClassName</param>
        /// <param name="noName">跳过的AD域对象名称</param>
        /// <param name="propertiesToLoad">指示搜索过程中要检索的属性列表</param>
        /// <returns></returns>
        public List<ADInfoModel> Search(string searchPath,
            List<string> noSchemaClassName, List<string> noName,
            List<string> propertiesToLoad)
        {
            return this.Search(searchPath, noSchemaClassName, noName, propertiesToLoad, null, 1, -1);
        }

        /// <summary>
        /// 搜索AD域对象信息
        /// </summary>
        /// <param name="searchPath">搜索路径</param>
        /// <param name="noSchemaClassName">跳过的SchemaClassName</param>
        /// <param name="noName">跳过的AD域对象名称</param>
        /// <param name="propertiesToLoad">指示搜索过程中要检索的属性列表</param>
        /// <param name="filter">指示轻量目录访问协议 (LDAP) 格式筛选器字符串的值。如“(objectClass=user)”。 默认值为“(objectClass=*)”，它检索所有对象</param>
        /// <returns></returns>
        public List<ADInfoModel> Search(string searchPath,
            List<string> noSchemaClassName, List<string> noName,
            List<string> propertiesToLoad, string filter)
        {
            return this.Search(searchPath, noSchemaClassName, noName, propertiesToLoad, filter, 1, -1);
        }

        /// <summary>
        /// 搜索AD域对象信息
        /// </summary>
        /// <param name="searchPath">搜索路径</param>
        /// <param name="noSchemaClassName">跳过的SchemaClassName</param>
        /// <param name="noName">跳过的AD域对象名称</param>
        /// <param name="propertiesToLoad">指示搜索过程中要检索的属性列表</param>
        /// <param name="filter">指示轻量目录访问协议 (LDAP) 格式筛选器字符串的值。如“(objectClass=user)”。 默认值为“(objectClass=*)”，它检索所有对象</param>
        /// <param name="searchScope">0.将搜索限于基对象, 1.搜索基对象的直接子对象，但不搜索基对象, 2.搜索整个子树，包括基对象及其所有子对象</param>
        /// <returns></returns>
        public List<ADInfoModel> Search(string searchPath,
            List<string> noSchemaClassName, List<string> noName,
            List<string> propertiesToLoad, string filter,
            int searchScope)
        {
            return this.Search(searchPath, noSchemaClassName, noName, propertiesToLoad, filter, searchScope, -1);
        }
        
        /// <summary>
        /// 搜索AD域对象信息
        /// </summary>
        /// <param name="searchPath">搜索路径</param>
        /// <param name="noSchemaClassName">跳过的SchemaClassName</param>
        /// <param name="noName">跳过的AD域对象名称</param>
        /// <param name="propertiesToLoad">指示搜索过程中要检索的属性列表</param>
        /// <param name="filter">指示轻量目录访问协议 (LDAP) 格式筛选器字符串的值。如“(objectClass=user)”。 默认值为“(objectClass=*)”，它检索所有对象</param>
        /// <param name="searchScope">0.将搜索限于基对象, 1.搜索基对象的直接子对象，但不搜索基对象, 2.搜索整个子树，包括基对象及其所有子对象</param>
        /// <param name="millisecondsTimeout">超时时间（毫秒）</param>
        /// <returns></returns>
        public List<ADInfoModel> Search(string searchPath,
            List<string> noSchemaClassName, List<string> noName,
            List<string> propertiesToLoad, string filter,
             int searchScope, int millisecondsTimeout)
        {
            List<ADInfoModel> list = null;

            SearchScope temp = SearchScope.OneLevel;
            if (Enum.IsDefined(typeof(SearchScope), searchScope)) temp = (SearchScope)searchScope;
            var task = Task.Factory.StartNew(new Func<object, List<ADInfoModel>>(this.SearchTask),
                new object[] { searchPath, noSchemaClassName, noName, propertiesToLoad, filter, temp });

            task.Wait(millisecondsTimeout);
            if(task.IsCompleted)
            {
                list = task.Result;
            }

            return list ?? new List<ADInfoModel>(0);
        }

        private bool GetDic(Dictionary<string, string> dic, ResultPropertyCollection resultCollection)
        {
            bool result = false;
            try
            {
                if (dic != null && resultCollection != null && resultCollection.Count > 0)
                {
                    foreach (string name in resultCollection.PropertyNames)
                    {
                        var p = resultCollection[name];
                        object obj = GetValue(p);
                        string val = null;
                        if (string.Compare(name, "objectguid") == 0 && obj is byte[])
                        {
                            Guid guid = new Guid(obj as byte[]);
                            val = guid.ToString();
                        }
                        else
                        {
                            val = GetString(obj);
                        }
                        if (!string.IsNullOrEmpty(name))
                        {
                            dic[name] = val;
                        }
                    }
                }
            }
            catch { }

            return result;
        }

        private object GetValue(ResultPropertyValueCollection p)
        {
            object val = null;
            if(p!= null)
            {
                IEnumerator iEnu = p.GetEnumerator();
                while (iEnu.MoveNext())
                {
                    val = iEnu.Current;
                }
            }

            return val;
        }

        private string GetString(object obj)
        {
            string val = "";
            if (obj != null)
            {
                if (obj.GetType().IsArray)
                {
                    Array arr = obj as Array;
                    foreach (var item in arr)
                    {
                        if (item != null)
                        {
                            val = val + ";" + item.ToString();
                        }
                    }

                    if (val.Length > 0) val = val.Substring(1);
                }
                else
                {
                    val = obj.ToString();
                }
            }
            
            return val;
        }
     }
}
