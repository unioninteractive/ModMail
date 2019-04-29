using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using ModMail.Preconditions;
using ModMail.Services;

namespace ModMail.Commands
{
    [RequireStaff]
    [Group("mail")]
    public class MailCommands : BaseCommandModule
    {
        private readonly MailService _mail;

        public MailCommands(MailService mail)
        {
            _mail = mail;
        }

        [Command("create")]
        public async Task CreateModMailSession(CommandContext ctx, DiscordMember member)
        {
            if (_mail.HasSession(member.Id))
            {
                await ctx.RespondAsync("The specified user already has a mail session opened.");
                return;
            }

            await _mail.CreateSessionAsync(member.Id);
        }
        
        [Command("close")]
        public async Task CloseModMailSession(CommandContext ctx)
        {
            if (!_mail.IsChannelLinkedToSession(ctx.Channel.Id))
            {
                await ctx.RespondAsync("This channel isn't linked to any mail session.");
                return;
            }

            var session = _mail.GetSessionFromChannel(ctx.Channel.Id);
            await _mail.CloseMailSessionAsync(session);
        }
    }
}