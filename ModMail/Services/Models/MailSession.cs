using System;
using DSharpPlus;

namespace ModMail.Services.Models
{
    public class MailSession : IEquatable<MailSession>
    {
        public MailSession(ulong mailChannelId, ulong mailUserId, DiscordWebhookClient webhookClient)
        {
            MailChannelId = mailChannelId;
            MailUserId = mailUserId;
            WebhookClient = webhookClient;
        }

        public ulong MailChannelId { get; private set; }
        
        public ulong MailUserId { get; private set; }
        
        public DiscordWebhookClient WebhookClient { get; private set; }

        public bool Equals(MailSession other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return MailUserId == other.MailUserId;
        }
    }
}