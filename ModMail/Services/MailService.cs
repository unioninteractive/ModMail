using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using ModMail.Services.Models;
using Serilog.Core;

namespace ModMail.Services
{
    public class MailService
    {
        private readonly ConfigurationService _config;
        private readonly DiscordClient _client;
        private readonly HttpClient _httpClient;
        private readonly Logger _log;
        
        private readonly List<MailSession> _sessions;
        
        public MailService(ConfigurationService config, DiscordClient client, HttpClient httpClient, Logger log)
        {
            _config = config;
            _client = client;
            _httpClient = httpClient;
            _log = log;
            
            // Initialize the sessions.
            _sessions = new List<MailSession>();
            // When the bot finishes downloading guild data, grab existing mail channels and store them.
            _client.GuildDownloadCompleted += RecoverSessionsAsync;
            
            // Used to handle relaying DM messages.
            _client.MessageCreated += HandlePrivateMessageReceivedAsync;
            // Used to handle relaying ModMail channel messages.
            _client.MessageCreated += HandleModMailChannelMessageReceivedAsync;
        }

        /// <summary>
        /// Check if a user has an active session.
        /// </summary>
        /// <param name="userId"> The user to search for. </param>
        public bool HasSession(ulong userId)
        {
            return _sessions.Any(s => s.MailUserId == userId);
        }

        /// <summary>
        /// Get a user's session if they have one.
        /// </summary>
        /// <param name="userId"> The user to grab the session for. </param>
        /// <exception cref="InvalidOperationException"> Thrown if the user does not have any active session. </exception>
        public MailSession GetSession(ulong userId)
        {
            if (!HasSession(userId))
            {
                throw new InvalidOperationException($"User {userId} does not have an active session.");
            }

            return _sessions.First(s => s.MailUserId == userId);
        }

        /// <summary>
        /// Check if the specified channel ID is linked to any session.
        /// </summary>
        /// <param name="channelId"> The ID to check for. </param>
        public bool IsChannelLinkedToSession(ulong channelId)
        {
            return _sessions.Any(s => s.MailChannelId == channelId);
        }

        /// <summary>
        /// Get a mail session from a channel ID.
        /// </summary>
        /// <param name="channelId"> The channel ID for which to retrieve the mail session. </param>
        /// <returns> The session or null if none. </returns>
        public MailSession GetSessionFromChannel(ulong channelId)
        {
            if (!IsChannelLinkedToSession(channelId)) return null;

            return _sessions.First(s => s.MailChannelId == channelId);
        }
        
        /// <summary>
        /// Recover existing sessions with their existing channels if any.
        /// </summary>
        /// <exception cref="ArgumentNullException"> Thrown if the ConfigurationService hasn't finished loading yet. </exception>
        private async Task RecoverSessionsAsync(GuildDownloadCompletedEventArgs args)
        {         
            _log.Information("ModMail: Recovering sessions...");
            
            if (_config == null) throw new ArgumentNullException(nameof(_config));
            var config = _config.GetConfig();
            
            // Grab the main server for verification.
            var mainGuild = await _client.GetGuildAsync(config.MainGuildId);

            // Get the channels inside the ModMail category.
            var validChannels = mainGuild.Channels.Keys
                .Where(cat => cat == config.ModMailCategoryId)
                .Select(id => mainGuild.Channels[id])
                .SelectMany(cat => cat.Children);
            
            // Get the channels IDs that correspond to a modmail session and the user each belongs to.
            // A modmail session channel's name is the user ID (ulong) to which it belongs.
            var sessionIds = GetSessionIdentifiers(validChannels);
            
            // Construct sessions if they don't exist yet.
            // On bot startup, none will exist, but if the bot reconnects and fails to continue the previous session, 
            // the Ready event will fire again and call this method, meaning some sessions might still exist.
            foreach (var channelId in sessionIds.Keys)
            {
                // Continue if the session already exists.
                if (HasSession(sessionIds[channelId]))
                {
                    _log.Information($"ModMail: Found existing session for user {sessionIds[channelId]}.");
                    continue;
                }
                
                _log.Information($"ModMail: Creating new session for user {sessionIds[channelId]} with existing channel id {channelId}");
                await CreateSessionAsync(channelId, sessionIds[channelId]);
            }
        }
        
        /// <summary>
        /// Handle relaying private messages to their mail sessions.
        /// </summary>
        private async Task HandlePrivateMessageReceivedAsync(MessageCreateEventArgs args)
        {
            var message = args.Message;
            
            // Check if the message originated from a user.
            if (args.Author.IsBot) return;
            
            // Check if the message was received in a private context.
            if (!message.Channel.IsPrivate) return;

            // Check if the message starts with the bot's command prefix.
            // Ignore it if so.
            if (message.Content.StartsWith(_config.GetConfig().Prefix)) return;
            
            MailSession session = null;
            
            // If the user doesn't have a session, create it.
            if (!HasSession(args.Author.Id))
            {
                session = await CreateSessionAsync(args.Author.Id);
                var user = await (await _client.GetGuildAsync(_config.GetConfig().MainGuildId))
                    .GetMemberAsync(args.Author.Id);

                if (user != null) await user.SendMessageAsync("Thank you for contacting us, we will reply shortly!");
            }
            else // retrieve it
            {
                session = GetSession(args.Author.Id);
            }
            
            // Get the mail channel in order to relay the message there.
            var mailChannel = (await _client.GetGuildAsync(_config.GetConfig().MainGuildId))
                .GetChannel(session.MailChannelId);

            // Relay the message through the session's webhook to simulate a normal user.
            // However, attachments cannot be sent through by webhooks (only one, but D#+ doesn't support it yet anyway).
            if (!string.IsNullOrEmpty(message.Content))
                await BroadcastHookAsync(session, args.Author, args.Message);
            
            // Create the streams for each attachment if any and send them in bulk.
            if (message.Attachments.Any())
            {
                var fileStreams = new Dictionary<string, Stream>();

                foreach (var attachment in message.Attachments)
                {
                    var stream = await _httpClient.GetStreamAsync(attachment.Url);
                    fileStreams.Add(attachment.FileName, stream);
                }
                
                // Send the message and each attachments.
                await mailChannel.SendMultipleFilesAsync(fileStreams, "**Attachments received:**");
            }
        }
        
        /// <summary>
        /// Handle relaying messages from the mail channels to their private channels.
        /// </summary>
        private async Task HandleModMailChannelMessageReceivedAsync(MessageCreateEventArgs args)
        {
            var author = args.Author;
            // Ignore if the message was from a bot.
            if (author.IsBot) return;

            var session = _sessions.FirstOrDefault(s => s.MailChannelId == args.Channel.Id);
            // Ignore if the message wasn't sent in a channel linked to a MailSession.
            if (session == null) return;
            
            // Check if the message starts with the bot's command prefix.
            // Ignore it if so.
            if (args.Message.Content.StartsWith(_config.GetConfig().Prefix)) return;
            
            // Get the user from the main server.
            // This cannot be done otherwise.
            var mainGuild = await _client.GetGuildAsync(_config.GetConfig().MainGuildId);
            var user = await mainGuild.GetMemberAsync(session.MailUserId);
            
            // Relay the message, with attachments if any.
            var message = args.Message;

            if (message.Attachments.Any())
            {
                var fileStreams = new Dictionary<string, Stream>();

                foreach (var attachment in message.Attachments)
                {
                    var stream = await _httpClient.GetStreamAsync(attachment.Url);
                    fileStreams.Add(attachment.FileName, stream);
                }
                
                // Send the message and each attachments.
                await user.SendMultipleFilesAsync(fileStreams, $"**{author.Username}#{author.Discriminator}:** " 
                    + (string.IsNullOrEmpty(message.Content)
                    ? "**Attachments Received**"
                    : message.Content));
            }
            else
            {
                string formattedMessage = $"**{author.Username}#{author.Discriminator}:** {message.Content}";

                if (formattedMessage.Length > 2000)
                {
                    var channel = mainGuild.GetChannel(session.MailChannelId);
                    await channel.SendMessageAsync(
                        "**Warning: your message was not sent as it was too long. Please make it shorter.**");
                }
                else await user.SendMessageAsync(formattedMessage);
            }
        }
        
        private static async Task BroadcastHookAsync(MailSession session, DiscordUser author, DiscordMessage message)
        {
            var client = session.WebhookClient;
            
            // Update the hook's avatar and username.
            client.AvatarUrl = author.AvatarUrl;
            client.Username = author.Username;
            
            // Broadcast the message.
            await client.BroadcastMessageAsync(message.Content);
        }

        /// <summary>
        /// Get modmail sessions from a collection of channels.
        /// </summary>
        /// <param name="channels"></param>
        /// <returns> Dictionary(ulong channelId, ulong userId) </returns>
        private static Dictionary<ulong, ulong> GetSessionIdentifiers(IEnumerable<DiscordChannel> channels)
        {
            var dictionary = new Dictionary<ulong, ulong>();
            
            foreach (var channel in channels)
            {
                if (!ulong.TryParse(channel.Name, out var userId)) continue;
                
                dictionary.Add(channel.Id, userId);
            }

            return dictionary;
        }

        /// <summary>
        /// Create a session with a mail channel for a specified user.
        /// </summary>
        /// <param name="userId"> The user to create the session for. </param>
        /// <exception cref="InvalidOperationException"> Called if a session already exists for the specified user. </exception>
        public async Task<MailSession> CreateSessionAsync(ulong userId)
        {
            // Throw an error if a session already exists for the user.
            if (HasSession(userId))
            {
                _log.Error($"ModMail: Cannot create a session for {userId}: a session for this user already exists.");
                throw new InvalidOperationException($"User {userId} already has a session opened.");
            }
            
            // Create a channel for the modmail session.
            var channel = await CreateModMailChannelAsync(userId);
            // Create the webhook for the channel and its client.
            var webClient = await GetOrCreateWebhookAsync(channel);
            // Create the session itself.
            var session = new MailSession(channel.Id, userId, webClient);
            // Add it to the tracked sessions.
            _sessions.Add(session);

            return session;
        }
       
        /// <summary>
        /// Create a session from an existing channel.
        /// </summary>
        /// <param name="channelId"> The Discord ID of the existing channel to use. </param>
        /// <param name="userId"> The user to bind to that channel. </param>
        private async Task<MailSession> CreateSessionAsync(ulong channelId, ulong userId)
        {
            var mainGuild = await _client.GetGuildAsync(_config.GetConfig().MainGuildId);
            
            // Check if the channel does exist first.
            // If not, create it.
            if (mainGuild.Channels.Values.All(channel => channel.Id != channelId))
            {
                _log.Error($"ModMail: Attempted to create session from non existing channel {channelId}. A new channel will be created.");
                var channel = await CreateModMailChannelAsync(userId);
                channelId = channel.Id;
            }
            
            // Get the channel.
            var mChannel = mainGuild.GetChannel(channelId);
            // Get or create the webhook and webhook client for that session.
            var webClient = await GetOrCreateWebhookAsync(mChannel);
            // Create the session itself.
            var session = new MailSession(channelId, userId, webClient);
            // Add it to the tracked sessions.
            _sessions.Add(session);

            return session;
        }
        
        /// <summary>
        /// Get a channel's existing relay hook or create it. Then return a client for that hook.
        /// </summary>
        /// <param name="channel"> The channel to get or create the hook in. </param>
        private async Task<DiscordWebhookClient> GetOrCreateWebhookAsync(DiscordChannel channel)
        {
            DiscordWebhook hook;
            
            // Get the existing webhook if any and set it.
            var existingWebhooks = await channel.GetWebhooksAsync();
            if (existingWebhooks.Any())
            {
                hook = existingWebhooks.First();
            }
            else // Else create it.
            {
                hook = await channel.CreateWebhookAsync("Relay Hook");
            }
            
            // Create the client.
            var client = new DiscordWebhookClient();
            // Hook the hook har har.
            client.AddWebhook(hook);

            return client;
        }

        private async Task<DiscordChannel> CreateModMailChannelAsync(ulong userId)
        {
            // Get the main guild and the user.
            // The user will be null if it can't find him for some reason.
            var mainGuild = await _client.GetGuildAsync(_config.GetConfig().MainGuildId);
            var user = await mainGuild.GetMemberAsync(userId);
            var modMailCategory = mainGuild.GetChannel(_config.GetConfig().ModMailCategoryId);
            
            // Create the channel, place it in the ModMail category.
            var channel = await mainGuild.CreateTextChannelAsync(userId.ToString(), modMailCategory);
            // Set the topic with information about the user. 
            await channel.ModifyAsync(a =>
            {
                a.Topic = user != null
                    ? $"{user.Username}#{user.Discriminator} | {userId} | {user.Mention}"
                    : $"Cannot retrieve user data | {userId}";
            });
            
            _log.Information($"ModMail: Created new modmail channel with ID {channel.Id} for user {userId.ToString()}");
            return channel;
        }

        public async Task CloseMailSessionAsync(MailSession session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            
            await CloseMailSessionAsync(session.MailUserId);
        }

        public async Task CloseMailSessionAsync(ulong userId)
        {
            if (!HasSession(userId))
                throw new InvalidOperationException($"No mail sessions linked to user {userId} have been found.");
            
            // Get the session.
            var session = GetSession(userId);
            
            // Remove the session from the list of tracked sessions.
            _sessions.Remove(session);
            
            // Delete the channel from the Discord server.
            var channel = (await _client.GetGuildAsync(_config.GetConfig().MainGuildId))
                .GetChannel(session.MailChannelId);

            await channel.DeleteAsync("MailSession ended.");
            
            _log.Information($"ModMail: Closed session for user {session.MailUserId}.");
        }
    }
}