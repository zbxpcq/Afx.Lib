using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Afx.Web.Http
{
    class ModelInfo
    {
        public string Name { get; set; }

        public Dictionary<string, ModelProperty> PropertyDic { get; set; }
    }
}
