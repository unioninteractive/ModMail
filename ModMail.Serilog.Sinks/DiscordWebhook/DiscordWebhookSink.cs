
// ReSharper disable CheckNamespace

using System;
using Discord.Webhook;
using Serilog.Core;
using Serilog.Events;

namespace ModMail.Serilog.Sinks
{
    public class DiscordWebhookSink : ILogEventSink
    {
        private readonly DiscordWebhookClient _client;
        private readonly IFormatProvider _formatProvider;
        private readonly string _webhookUsername, _webhookAvatarUrl;
        
        public DiscordWebhookSink(
            ulong webhookId, 
            string webhookToken,
            string webhookUsername = null,
            string webhookAvatarUrl = null)
        {
            _client = new DiscordWebhookClient(webhookId, webhookToken);
            _webhookUsername = webhookUsername;
            _webhookAvatarUrl = webhookAvatarUrl;
        }

        public void Emit(LogEvent logEvent)
        {
            var message = logEvent.RenderMessage();
            var time = FormatUtils.GetFormattedTime(logEvent.Timestamp);
            var level = FormatUtils.GetFormattedLevel(logEvent.Level);

            var finalMessage = $"[{time} {level}] {message}";

            if (logEvent.Exception != null)
            {
                finalMessage += Environment.NewLine
                     + $"Exception: {logEvent.Exception.Message}";

                if (logEvent.Exception is AggregateException aggregation)
                {
                    var index = 0;
                    foreach (var e in aggregation.InnerExceptions)
                    {
                        finalMessage += Environment.NewLine 
                                    + $"---> (Inner Exception #{index}) "
                                    + $"{e.GetType().FullName}: {e.Message}"
                                    + Environment.NewLine
                                    + e.StackTrace;

                        index++;
                    }
                }
            }

            // Remove '`' characters from the message if any.
            finalMessage = finalMessage.Replace("`", "");
            // Substring if the message is too long for Discord.
            if (finalMessage.Length > 1997)
            {
                finalMessage = finalMessage.Substring(0, 1997);
            }
            
            // Send the message to Discord
            _client.SendMessageAsync($"`{finalMessage}`", username: _webhookUsername, avatarUrl: _webhookAvatarUrl);
        }
    }
}