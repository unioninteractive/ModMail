using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using ModMail.Preconditions;
using ModMail.Services;

namespace ModMail.Commands
{
    [Group("infractions")]
    [RequireStaff]
    public class InfractionCommands : BaseCommandModule
    {
        private readonly InfractionManager _infractions;

        public InfractionCommands(InfractionManager infractions)
        {
            _infractions = infractions;
        }

        [GroupCommand]
        public async Task GetUserInfractions(CommandContext ctx, DiscordUser user = null)
        {
            user = user ?? ctx.User;

            var query = _infractions.GetUserInfractions(user);

            if (!query.Successful)
            {
                await ctx.RespondAsync(embed: Embeds.Message($"Failed to get user infractions: {query.Message}"));
                return;
            }

            if (!query.Result.Any())
            {
                await ctx.RespondAsync(
                    embed: Embeds.Message($"{user.Username}#{user.Discriminator} has no infractions."));
            }
            else
            {
                await ctx.RespondAsync(embed: Embeds.UserInfractions(ctx, user, query.Result));
            }
            
        }
    }
}