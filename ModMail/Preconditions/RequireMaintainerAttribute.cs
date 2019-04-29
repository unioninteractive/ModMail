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
    public class RequireMaintainerAttribute : CheckBaseAttribute
    {
        public override async Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            var config = ctx.Services.GetRequiredService<ConfigurationService>().GetConfig();
            var mainGuild = await ctx.Client.GetGuildAsync(config.MainGuildId);
            var user = await mainGuild.GetMemberAsync(ctx.User.Id);
            
            return user != null && user.Roles.Any(r => r.Id == config.MaintainerRoleId);
        }
    }
}