using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Protean
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class CallProtean
    {
        private readonly RequestDelegate _next;

        public CallProtean(RequestDelegate next)
        {
            _next = next;
        }

        public Task Invoke(HttpContext httpContext)
        {

   

            Cms oPcms = new Protean.Cms(httpContext);
            oPcms.InitializeVariables();
            oPcms.GetPageHTML();

            return _next(httpContext);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseCallProtean(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CallProtean>();
        }
    }
}
