using System;
using System.Collections.Generic;
using System.Text;

namespace Afx.Map
{
    /// <summary>
    /// map to model
    /// </summary>
    public interface IToObject
    {
        /// <summary>
        /// to model
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        object To(object obj);
    }
}
