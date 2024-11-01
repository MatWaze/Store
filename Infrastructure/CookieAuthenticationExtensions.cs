using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Linq.Expressions;

namespace Store.Infrastructure
{
    public static class CookieAuthenticationExtensions
    {
        // replace old handler with new which sets StatusCode to statusCode when the Path path is requested
        public static void DisableAuthForPath(
            this CookieAuthenticationEvents events,
            Expression<Func<CookieAuthenticationEvents,
                Func<RedirectContext<CookieAuthenticationOptions>,
                Task>>> expr,
            string path,
            int statusCode
        )
        {
            string propertyName = ((MemberExpression)expr.Body).Member.Name;

            Func<RedirectContext<CookieAuthenticationOptions>, Task>
                oldHandler = expr.Compile().Invoke(events);

            Func<RedirectContext<CookieAuthenticationOptions>, Task>
                newHandler = context =>
                {
                    if (context.Request.Path.StartsWithSegments(path))
                    {
                        context.Response.StatusCode = statusCode;
                    }
                    else
                    {
                        oldHandler(context);
                    }
                    return Task.CompletedTask;
                };

            typeof(CookieAuthenticationEvents).GetProperty(propertyName)?
                .SetValue(events, newHandler);
        }
    }
}
