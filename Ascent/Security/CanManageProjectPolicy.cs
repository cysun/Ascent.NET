using Microsoft.AspNetCore.Authorization;

namespace Ascent.Security;

public class CanManageProjectRequirement : IAuthorizationRequirement
{
}

public class CanManageProjectHandler : AuthorizationHandler<CanManageProjectRequirement, int>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CanManageProjectRequirement requirement, int projectId)
    {
        if (context.User.Claims.Any(c => c.Type == Constants.Claim.Write) ||
            context.User.Claims.Any(c => c.Type == Constants.Claim.Project && c.Value == projectId.ToString()))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
