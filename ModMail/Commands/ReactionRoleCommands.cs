using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using ModMail.Preconditions;
using ModMail.Services;
using ModMail.Services.Models;

namespace ModMail.Commands
{
    [Group("reactions")]
    [RequireMaintainer]
    public class ReactionRoleCommands : BaseCommandModule
    {
        private readonly ReactionRoleHandler _reactionRole;

        public ReactionRoleCommands(ReactionRoleHandler reactRole)
        {
            _reactionRole = reactRole;
        }
        
        [Command("addpair")]
        public async Task AddReactionPair(CommandContext ctx, ulong messageId, string emoteName, DiscordRole role)
        {
            var reactionPair = new ReactionRolePair(messageId, emoteName, role.Id);
            var (result, message) = _reactionRole.AddReactionRolePair(reactionPair);

            await ctx.RespondAsync(message);
        }

        [Command("save")]
        public async Task SaveReactionPairs(CommandContext ctx)
        {
            var (result, message, exception) = await _reactionRole.SaveReactionPairsAsync();

            await ctx.RespondAsync($"`{message}`");
        }
    }
}