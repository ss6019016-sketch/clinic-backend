using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using clinic.Services.Interfaces;

namespace clinic.Authorization
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequirePermissionAttribute : AuthorizeAttribute, IAsyncAuthorizationFilter
    {
        private readonly string _moduleName;
        private readonly string _action;

        public RequirePermissionAttribute(string moduleName, string action)
        {
            _moduleName = moduleName;
            _action = action;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (context.HttpContext.User?.Identity?.IsAuthenticated != true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var permissionService = context.HttpContext.RequestServices.GetService<IPermissionService>();
            if (permissionService == null)
            {
                context.Result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
                return;
            }

            var role = context.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            if (string.IsNullOrWhiteSpace(role))
            {
                context.Result = new ForbidResult();
                return;
            }

            var allowed = await permissionService.HasPermissionAsync(role, _moduleName, _action);
            if (!allowed)
            {
                context.Result = new ForbidResult();
                return;
            }
        }
    }
}
