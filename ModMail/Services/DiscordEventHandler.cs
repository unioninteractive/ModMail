using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Serilog.Core;

namespace ModMail.Services
{
    public class DiscordEventHandler
    {
        private readonly DiscordClient _client;
        private readonly Logger _log;

        public DiscordEventHandler(DiscordClient client, Logger log)
        {
            _client = client;
            _log = log;
        }

        public void Log(object sender, DebugLogMessageEventArgs message)
        {
            var msg = $"{message.Application}: {message.Message}";
            
            switch (message.Level)
            {
                case LogLevel.Critical:
                    _log.Fatal(message.Exception, msg);
                    break;
                case LogLevel.Error:
                    _log.Error(message.Exception, msg);
                    break;
                case LogLevel.Warning:
                    _log.Warning(message.Exception, msg);
                    break;
                case LogLevel.Info:
                    _log.Information(message.Exception, msg);
                    break;
                case LogLevel.Debug:
                    _log.Debug(message.Exception, msg);
                    break;
                default:
                    _log.Warning(message.Exception, $"[???] {msg}");
                    break;
            }
        }

        private async Task OnReady(ReadyEventArgs e)
        {
            await _client.UpdateStatusAsync(new DiscordActivity("DM me to contact staff!", ActivityType.Playing));
        }
        
        public void HookEvents()
        {
            _client.DebugLogger.LogMessageReceived += Log;
            _client.Ready += OnReady;
        }
    }
}