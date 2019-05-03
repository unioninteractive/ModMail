using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using ModMail.Models;
using ModMail.Preconditions;
using ModMail.Services;
using Tababular;

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
            var reactionPair = new ReactionRole(messageId, emoteName, role.Id);
            var (result, message) = await  _reactionRole.AddReactionRolePairAsync(reactionPair);

            await ctx.RespondAsync(embed: Embeds.Message(message, result ? DiscordColor.Green : DiscordColor.Red));
        }

        [Command("list")]
        public async Task ListReactionPairs(CommandContext ctx)
        {
            try
            {
                var objects = new List<object>();
                var formatter = new TableFormatter();
            
                // Add each reaction role to the objects.
                foreach (var reaction in _reactionRole.GetReactionRoles())
                {
                    objects.Add(new { MessageID = reaction.MessageId, RoleID = reaction.RoleId, Reaction = reaction.ReactionName});
                }
            
                // Get the table.
                var table = formatter.FormatObjects(objects);
            
                // Send it.
                await ctx.RespondAsync(Formatter.BlockCode(table));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}