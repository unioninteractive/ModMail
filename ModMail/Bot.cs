using System;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Discord;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.DependencyInjection;
using ModMail.Commands;
using ModMail.Serilog.Sinks;
using ModMail.Services;
using ModMail.Services.Models;
using Serilog;
using Serilog.Core;
using TokenType = DSharpPlus.TokenType;

namespace ModMail
{
    public class Bot
    {
        private CommandsNextExtension _commands;
        private ConfigurationService _config;
        private DiscordClient _client;
        private IServiceProvider _services;
        
        public async Task StartAsync(string[] args)
        {
            if (args.Length < 1 || string.IsNullOrEmpty(args[0]))
            {
                Console.WriteLine("Please specify the config directory's path as the first command line argument.");
                Console.ReadLine();
                Environment.Exit(1);
            }

            // Create the configuration service.
            _config = new ConfigurationService();
            await _config.InitializeAsync(args[0]);

            var configData = _config.GetConfig();
            
            _client = new DiscordClient(new DiscordConfiguration
            {
                Token = Utils.CXGetEnvironmentVariable("MODMAILTOKEN"),
                TokenType = TokenType.Bot,
                LogLevel = LogLevel.Info,
                UseInternalLogHandler = false,
            });
            
            
            // Generate the bot's required services.
            _services = GetServices();
            
            // Inform people the bot is starting.
            _services.GetRequiredService<Logger>().Information("ModMail: Startup sequence initiated.");    
            
            // Initialize the services if needed.
            await InitializeServicesAsync();
            
            // Start the command service.
            _commands = StartCommandService();
            
            // Start the bot.
            await _client.ConnectAsync();
            
            // Halt execution of the method forever.
            await Task.Delay(-1);
        }

        private CommandsNextExtension StartCommandService()
        {
            var commands = _client.UseCommandsNext(new CommandsNextConfiguration()
            {
                Services =  _services,
                StringPrefixes = new[] {_config.GetConfig().Prefix},
                EnableDms = true,
                EnableMentionPrefix = true
            });

            commands.RegisterCommands(Assembly.GetExecutingAssembly());
            return commands;
        }


        /// <summary>
        /// Gets the required service for the bot.
        /// Add new services here, start them if necessary in Bot#InitializeServicesAsync()
        /// </summary>
        /// <returns> The service provider that contains all the services. </returns>
        private IServiceProvider GetServices()
        {
            var configData = _config.GetConfig();
            
            var provider = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_config)
                .AddSingleton(new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo.Console()
                    .WriteTo.DiscordWebhook(configData.LogWebhookId, configData.LogWebhookToken, configData.LogWebhookUsername, configData.LogWebhookAvatarUrl)
                    .CreateLogger())
                .AddSingleton<DiscordEventHandler>()
                .AddSingleton<ReactionRoleHandler>()
                .AddSingleton<EvaluationService>()
                .AddSingleton<MailService>()
                .AddSingleton<HttpClient>()
                .BuildServiceProvider();

            return provider;
        }

        private async Task InitializeServicesAsync()
        {
            // Hook discord events.
            _services.GetRequiredService<DiscordEventHandler>().HookEvents();
            // Start the reaction handler.
            await _services.GetRequiredService<ReactionRoleHandler>().InitializeAsync();
            // Start the mail service.
            _services.GetRequiredService<MailService>();
        }
    }
}