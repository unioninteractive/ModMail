using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using ModMail.Models;

namespace ModMail
{
    public static class Embeds
    {
        public static DiscordEmbed Message(string message) => Message(message, DiscordColor.White);

        public static DiscordEmbed Message(string message, DiscordColor color)
        {
            var builder = new DiscordEmbedBuilder()
                .WithDescription(Formatter.Bold(message))
                .WithColor(color);

            return builder.Build();
        }

        public static DiscordEmbed UserInfractions(CommandContext ctx, DiscordUser user, List<Infraction> infractions)
        {
            var countBuilder = new StringBuilder("This user has ");
            var values = Enum.GetValues(typeof(InfractionType)).Cast<InfractionType>().ToArray();

            // Get the amount of infractions for each type.
            for (int i = 0; i < values.Count(); i++)
            {
                var name = values[i].ToString().ToLower();
                var count = infractions.Count(x => x.Type == values[i]);

                if (count == 0 || count > 1) countBuilder.Append($"{count} {name}s");
                else countBuilder.Append($"{count} {name}");

                if (i + 2 < values.Count()) countBuilder.Append(", ");
                else if (i + 1 < values.Count()) countBuilder.Append(" and ");
            }
            
            var builder = new DiscordEmbedBuilder()
                .WithTitle($"{user.Username}#{user.Discriminator}'s infractions")
                .WithDescription(countBuilder.ToString())
                .WithColor(DiscordColor.Blurple)
                .WithFooter($"Requested by {ctx.User.Username}#{ctx.User.Discriminator}");
            
            // Add a field per infraction in the embed builder.
            foreach (var infraction in infractions)
            {
                builder.AddField(Formatter.Bold($"#{infraction.Id} - {infraction.Type} - {infraction.Timestamp:dd MMM yyyy}"),
                    infraction.Reason ?? "No message specified.");
            }

            return builder.Build();
        }
    }
}