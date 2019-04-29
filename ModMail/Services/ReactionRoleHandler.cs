using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using ModMail.Services.Models;
using Newtonsoft.Json;
using Serilog.Core;

namespace ModMail.Services
{
    public class ReactionRoleHandler
    {
        private readonly ConfigurationService _config;
        private readonly Logger _log;

        private List<ReactionRolePair> _reactionRoles;
        
        public ReactionRoleHandler(DiscordClient client, Logger log, ConfigurationService config)
        {
            _config = config;
            _log = log;
            
            client.MessageReactionAdded += OnReactionAdded;
        }

        public async Task InitializeAsync()
        {
            if (_config.GetConfig() == null) throw new ArgumentNullException(nameof(_config));
            
            var reactionPath = Path.Combine(_config.ConfigDirPath, "reactions.json");
            
            if (!File.Exists(reactionPath))
            {
                _reactionRoles = new List<ReactionRolePair>();
                var json = JsonConvert.SerializeObject(_reactionRoles, Formatting.Indented);
                await File.WriteAllTextAsync(reactionPath, json);
                
                _log.Information($"RctRole: Created new reaction roles file at {reactionPath}.");
            }
            else
            {
                var json = await File.ReadAllTextAsync(reactionPath);
                _reactionRoles = JsonConvert.DeserializeObject<List<ReactionRolePair>>(json);
                
                _log.Information($"RctRole: Loaded {_reactionRoles.Count} ReactionRolePairs.");
            }
        }

        private async Task OnReactionAdded(MessageReactionAddEventArgs args)
        {
            var emoteName = args.Emoji.ToString();
            var message = args.Message;
            
            // If the reactions on the message aren't tracked, return.
            if (_reactionRoles.All(r => r.Tracks(message.Id, emoteName) == false))
            {
                return;
            }
            // If the user is a bot, return.
            if (args.User.IsBot)
            {
                return;
            }
      
            var reactionRole = _reactionRoles.First(r => r.Tracks(message.Id, emoteName));
            var guild = args.Channel.Guild;
            var reactUser = await guild.GetMemberAsync(args.User.Id);
            var role = guild.GetRole(reactionRole.RoleId);

            if (role == null)
            {
                _log.Error($"RctRole: Guild has no role with id: {reactionRole.RoleId}!");
                return;
            }
            
            
            // Give the role if the user doesn't have it, else remove it.
            if (reactUser.Roles.Any(r => r.Id == reactionRole.RoleId))
            {
                await reactUser.RevokeRoleAsync(role);
                _log.Information($"RctRole: Removed role {role.Id} from {reactUser} ({reactUser.Id}).");
            }
            else
            {
                await reactUser.GrantRoleAsync(role);
                _log.Information($"RctRole: Added role {role.Id} to {reactUser} ({reactUser.Id}).");   
            }
        }

        public (bool, string) AddReactionRolePair(ReactionRolePair pair)
        {
            if (_reactionRoles.Any(r => r.Equals(pair)))
            {
                return (false, $"Cannot add {pair} as it already exists.");
            }
            
            _reactionRoles.Add(pair);
            _log.Information($"RctRole: Added new pair ({pair})");

            return (true, $"Successfully added new pair ({pair})");
        }

        public (bool, string) RemoveReactionRolePair(ReactionRolePair pair)
        {
            if (_reactionRoles.All(r => r.Equals(pair) == false))
            {
                return (false, $"Cannot find any pair {pair} to remove.");
            }

            var index = _reactionRoles.FindIndex(r => r.Equals(pair));
            _reactionRoles.RemoveAt(index);
            
            _log.Information($"RctRole: Removed existing pair ({pair})");
            return (true, $"Successfully removed existing pair ({pair})");
        }

        /// <summary>
        /// Save the reaction role pairs.
        /// </summary>
        /// <returns> bSuccess, CompletionMessage, ErrorException </returns>
        public async Task<(bool, string, Exception)> SaveReactionPairsAsync()
        {
            try
            {
                var reactionPath = Path.Combine(_config.ConfigDirPath, "reactions.json");
                var json = JsonConvert.SerializeObject(_reactionRoles, Formatting.Indented);
                await File.WriteAllTextAsync(reactionPath, json);

                return (true, $"Successfully saved reaction role pairs.", null);
            }
            catch (Exception e)
            {
                _log.Error("RctRole: Failed to save reaction role pair.", e);
                return (false, $"Failed to save reaction role pairs", e);
            }
        }
    }
}