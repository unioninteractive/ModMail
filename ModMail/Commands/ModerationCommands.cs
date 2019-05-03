using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using ModMail.Models;
using ModMail.Preconditions;
using ModMail.Services;

namespace ModMail.Commands
{
    [RequireStaff]
    public class ModerationCommands : BaseCommandModule
    {
        private readonly InfractionManager _infractions;

        public ModerationCommands(InfractionManager infractions)
        {
            _infractions = infractions;
        }
        
        [Command("note")]
        [Aliases("notice", "comment")]
        public async Task AddNoteToUser(CommandContext ctx, DiscordUser user, [RemainingText] string note)
        {
            if (string.IsNullOrEmpty(note))
            {
                await ctx.RespondAsync(embed: Embeds.Message("The note cannot be empty.", DiscordColor.Red));
                return;
            }

            var infraction = new Infraction(InfractionType.Notice, DateTimeOffset.Now, ctx.User.Id, user.Id, note);
            var query = await _infractions.AddInfractionToUserAsync(user.Id, infraction);

            if (query.IsSuccessful)
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("✅"));
            else
                await ctx.RespondAsync(embed: Embeds.Message($"An error has occured while adding the infraction to the user: {query.Message}"));
        }
    }
}