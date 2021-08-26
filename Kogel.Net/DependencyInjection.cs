#if NETCOREAPP || NETSTANDARD2_0
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kogel.Net;
using Kogel.Net.Extension;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// 
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// 注入Http请求连接
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddKogelHttpClient(this IServiceCollection services)
        {
            services.AddScoped<IHttpClient, HttpClient>();
            return services;
        }
    }
}
#endif