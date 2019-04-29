using System;
using Serilog;
using Serilog.Configuration;

namespace ModMail.Serilog.Sinks
{
    public static class ModMailSinkExtensions
    {
        public static LoggerConfiguration DiscordWebhook(
            this LoggerSinkConfiguration sinkConfiguration,
            ulong webhookId,
            string webhookToken,
            string webhookUsername = null,
            string webhookAvatarUrl = null)
        {
            if (webhookToken == null)
                throw new ArgumentNullException(nameof(webhookToken));

            return sinkConfiguration.Sink(new DiscordWebhookSink(webhookId, webhookToken, webhookUsername,
                webhookAvatarUrl));
        }
    }
}