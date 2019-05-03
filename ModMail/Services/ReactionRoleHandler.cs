using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using ModMail.Models;
using Serilog.Core;

namespace ModMail.Services
{
    // TODO: Refactor results to use QueryResult.
    public class ReactionRoleHandler
    {
        private readonly ConfigurationService _config;
        private readonly Logger _log;

        private List<ReactionRole> _reactionRoles;
        
        public ReactionRoleHandler(DiscordClient client, Logger log, ConfigurationService config)
        {
            _config = config;
            _log = log;
            
            client.MessageReactionAdded += OnReactionAdded;
        }

        public void Initialize()
        {
            ModMailContext dbContext = null;

            try
            {
                // Create the database context and grab the reaction roles.
                dbContext = new ModMailContext();
                _reactionRoles = dbContext.ReactionRoles.ToList();

                _log.Information("RctRole: Successfully fetched reaction role pairs from the database.");
            }
            catch (Exception e)
            {
                _log.Fatal(e, "RctRole: An error has occured while fetching reaction role data. This service will not work!");
            }
            finally
            {
                dbContext?.Dispose();
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

        public async Task<(bool, string)> AddReactionRolePairAsync(ReactionRole pair)
        {
            ModMailContext dbContext = null;

            try
            {
                // Get the context.
                dbContext = new ModMailContext();

                // Fail if a reaction role pair with the same role id exists.
                if (dbContext.ReactionRoles.Any(r => r.RoleId == pair.RoleId))
                {
                    return (false, $"Cannot add {pair} as a pair with the same role ID already exists.");
                }

                // Add it and save the changes to the database.
                dbContext.ReactionRoles.Add(pair);
                await dbContext.SaveChangesAsync();

                // Update the reaction roles we have.
                _reactionRoles = dbContext.ReactionRoles.ToList();

                return (true, $"Successfully added new pair ({pair})");
            }
            catch (Exception e)
            {
                _log.Error(e, "RctRole: Failed to add a new reaction role pair to the database.");
                return (false, $"Failed to add a new reaction role pair to the database.");
            }
            finally
            {
                dbContext?.Dispose();
            }
        }

        
        public async Task<(bool, string)> RemoveReactionRolePairAsync(ReactionRole pair)
        {
            ModMailContext dbContext = null;

            try
            {
                // Get the context.
                dbContext = new ModMailContext();

                // Fail if a reaction role pair with the same role id doesn't exist.
                if (!dbContext.ReactionRoles.Any(r => r.Equals(pair)))
                {
                    return (false, $"Cannot remove {pair} as a pair with the same role ID doesn't exist.");
                }

                // Remove it from the database.
                dbContext.ReactionRoles.Remove(pair);
                
                // Save changes.
                await dbContext.SaveChangesAsync();

                return (true, $"Successfully removed pair ({pair})");
            }
            catch (Exception e)
            {
                _log.Error(e, "RctRole: Failed to remove existing reaction role pair from the database.");
                return (false, $"Failed to remove existing reaction role pair from the database.");
            }
            finally
            {
                dbContext?.Dispose();
            }
        }

        /// <summary>
        /// Reload the reaction roles from the database.
        /// Useful if the reaction roles pairs have been edited manually outside of the bot.
        /// </summary>
        public (bool, string) ReloadReactionRoles()
        {
            ModMailContext dbContext = null;

            try
            {
                dbContext = new ModMailContext();

                // Update the reaction role list.
                _reactionRoles = dbContext.ReactionRoles.ToList();

                _log.Information("RctRole: Updated reaction roles.");
                return (true, "Successfully reloaded reaction roles.");
            }
            catch (Exception e)
            {
                _log.Error(e, "RctRole: Failed to update reaction roles.");
                return (false, "Failed to realod reaction roles.");
            }
            finally
            {
                dbContext?.Dispose();
            }
        }

        /// <summary>
        /// Get the local reaction roles.
        /// </summary>
        /// <returns> The local reaction roles. </returns>
        public List<ReactionRole> GetReactionRoles()
        {
            var roles = new ReactionRole[_reactionRoles.Count];
            _reactionRoles.CopyTo(roles);
            return roles.ToList();
        }
    }
}