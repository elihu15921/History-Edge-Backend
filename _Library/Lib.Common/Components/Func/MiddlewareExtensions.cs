using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;

namespace Lib.Common.Components.Func
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseGCMiddleware(this IApplicationBuilder builder) => builder.UseMiddleware<GCMiddleware>();
    }
}
