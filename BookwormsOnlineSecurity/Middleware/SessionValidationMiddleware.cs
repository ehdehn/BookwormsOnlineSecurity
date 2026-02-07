using System;
using BookwormsOnlineSecurity.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookwormsOnlineSecurity.Middleware
{
    public class SessionValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public SessionValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var session = context.Session;
            var db = context.RequestServices.GetRequiredService<AuthDbContext>();

            // Determine user id from session or auth ticket
            var sessionUserId = session.GetString("UserId");
            var sessionId = session.GetString("SessionId");
            var principalUserId = context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            // If there's an authenticated principal but no server session, treat as invalid and sign out
            if (!string.IsNullOrEmpty(principalUserId) && string.IsNullOrEmpty(sessionUserId))
            {
                await SignOutAndRedirect(context, "/Login?multiple=1");
                return;
            }

            // Use the session user id if available, otherwise the principal user id
            var userId = sessionUserId ?? principalUserId;

            // Check session timeout (30 seconds)
            if (!string.IsNullOrEmpty(userId))
            {
                var loginTimeStr = session.GetString("LoginTime");
                if (DateTime.TryParse(loginTimeStr, out var loginTime))
                {
                    if (DateTime.UtcNow - loginTime > TimeSpan.FromSeconds(30))
                    {
                        await SignOutAndRedirect(context, "/Login?timeout=1");
                        return;
                    }
                }
            }

            // Single active session enforcement
            if (!string.IsNullOrEmpty(userId))
            {
                if (string.IsNullOrEmpty(sessionId))
                {
                    await SignOutAndRedirect(context, "/Login?multiple=1");
                    return;
                }

                var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null || string.IsNullOrEmpty(user.LastSessionId) || !string.Equals(user.LastSessionId, sessionId, StringComparison.Ordinal))
                {
                    await SignOutAndRedirect(context, "/Login?multiple=1");
                    return;
                }
            }

            await _next(context);
        }

        private static async Task SignOutAndRedirect(HttpContext context, string url)
        {
            // Clear server-side session
            context.Session.Clear();

            // Remove authentication cookie(s) and sign out
            try
            {
                await context.SignOutAsync(IdentityConstants.ApplicationScheme);
            }
            catch
            {
                // ignore
            }

            // Delete common Identity cookies to ensure browser drops them
            var cookieNames = new[]
            {
                ".AspNetCore.Identity.Application",
                ".AspNetCore.Identity.External",
                ".AspNetCore.Identity.TwoFactorRememberMe",
                ".AspNetCore.Identity.TwoFactorUserId",
                ".AspNetCore.Cookies"
            };

            foreach (var name in cookieNames)
            {
                if (context.Request.Cookies.ContainsKey(name))
                {
                    context.Response.Cookies.Delete(name);
                }
            }

            // Clear the request principal so subsequent rendering in the same request shows signed-out UI
            context.User = new ClaimsPrincipal(new ClaimsIdentity());

            context.Response.Redirect(url);
            await Task.CompletedTask;
        }
    }
}
