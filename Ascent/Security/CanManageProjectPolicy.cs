using Ascent.Models;
using Microsoft.AspNetCore.Authorization;

namespace Ascent.Security;

public class CanManageProjectRequirement : IAuthorizationRequirement
{
}

public class CanManageProjectHandler : AuthorizationHandler<CanManageProjectRequirement, Project>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CanManageProjectRequirement requirement, Project project)
    {
        if (context.User.Claims.Any(c => c.Type == Constants.Claim.Write) ||
            context.User.Claims.Any(c => c.Type == Constants.Claim.Project && c.Value == project.Id.ToString()))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
