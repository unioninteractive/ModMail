using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.DependencyInjection;
using ModMail.Services;

namespace ModMail.Preconditions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequireStaffAttribute : CheckBaseAttribute
    {
        public override async Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            var member = await ctx.Guild.GetMemberAsync(ctx.User.Id);
            var config = ctx.Services.GetRequiredService<ConfigurationService>().GetConfig();

            return member != null && member.Roles.Any(r => r.Id == config.StaffRoleId);
        }
    }
}