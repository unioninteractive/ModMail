using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using ModMail.Models;
using ModMail.Preconditions;

namespace ModMail.Commands
{
    [RequireStaff]
    public class InfractionCommands : BaseCommandModule
    {
        [Command("infractions")]
        public async Task GetInfractionsForUser(CommandContext ctx, DiscordUser user)
        {
            ModMailContext dbContext = null;
            
            try
            {
                dbContext = new ModMailContext();
                var builder = new StringBuilder();
                var infractions = dbContext.ModerationInfractions.Where(i => i.UserId == user.Id);

                if (!infractions.Any())
                {
                    await ctx.RespondAsync("User has no infractions.");
                    return;
                }
                
                foreach (var infraction in infractions)
                {
                    builder.Append($"{infraction.Type} | {infraction.Timestamp.ToString()}");
                    builder.AppendLine();
                }

                await ctx.RespondAsync(builder.ToString());
            }
            catch (Exception e)
            {
                await ctx.RespondAsync("Exception: " + e.Message);
            }
            finally
            {
                dbContext?.Dispose();
            }
        }

        [Command("note")]
        public async Task AddNoteToUser(CommandContext ctx, DiscordUser user, [RemainingText] string note = "")
        {
            if (string.IsNullOrEmpty(note))
            {
                await ctx.RespondAsync("The notice cannot be empty.");
                return;
            }
            
            var infraction = new Infraction(InfractionType.Notice, DateTimeOffset.Now, ctx.User.Id, user.Id, note);

            using (var dbContext = new ModMailContext())
            {
                dbContext.ModerationInfractions.Add(infraction);
                await dbContext.SaveChangesAsync();

                await ctx.RespondAsync("Added a note to user.");
            }
        }
    }
}