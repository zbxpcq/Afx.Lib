using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Http
{
    public interface IWsHandler : IDisposable
    {
        Task Invoke(HttpContext context);
    }
}
