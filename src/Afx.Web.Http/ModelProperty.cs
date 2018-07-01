using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ComponentModel.DataAnnotations;

namespace Afx.Web.Http
{
    class ModelProperty
    {
        public string Name { get; set; }

        public PropertyInfo PropertyInfo { get; set; }

        public List<ValidationAttribute> ValidationList { get; set; }
    }
}
