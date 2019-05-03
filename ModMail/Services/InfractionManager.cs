using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using ModMail.Models;
using ModMail.Utilities;
using Serilog.Core;

namespace ModMail.Services
{
    public class InfractionManager
    {
        private readonly Logger _log;

        public InfractionManager(Logger log)
        {
            _log = log;
        }


        /// <summary>
        /// Check if a user has an infraction.
        /// </summary>
        /// <param name="user"> The user to check against. </param>
        /// <returns> True if the user has one or more infractions. </returns>
        public QueryResult<bool> HasInfraction(DiscordUser user) => HasInfraction(user.Id);

        /// <summary>
        /// Check if a user has an infraction.
        /// </summary>
        /// <param name="userId"> The user to check against. </param>
        /// <returns> True if the user has one or more infractions. </returns>
        public QueryResult<bool> HasInfraction(ulong userId)
        {
            ModMailContext dbContext = null;

            try
            {
                dbContext = new ModMailContext();

                return QueryResult<bool>.FromSuccess(result: dbContext.Infractions.Any(i => i.UserId == userId));
            }
            catch (Exception e)
            {
                _log.Error(e, "Infractions: Failed to retrieve infractions from database.");
                return QueryResult<bool>.FromError(e.Message);
            }
            finally
            {
                dbContext?.Dispose();
            }
        }
        
        /// <summary>
        /// Get all of the infractions stored inside the bot's database.
        /// </summary>
        /// <returns> A list of all known infractions. </returns>
        public QueryResult<List<Infraction>> GetAllInfractions()
        {
            ModMailContext dbContext = null;

            try
            {
                dbContext = new ModMailContext();

                return QueryResult<List<Infraction>>.FromSuccess(result: dbContext.Infractions.ToList());
            }
            catch (Exception e)
            {
                _log.Error(e, "Infractions: Failed to retrieve all infractions from database.");
                return QueryResult<List<Infraction>>.FromError(e.Message);
            }
            finally
            {
                dbContext?.Dispose();
            }
        }

        /// <summary>
        /// Get the infractions committed by a specific user.
        /// </summary>
        /// <param name="user"> The DiscordUser to search for in the database. </param>
        /// <returns> The user's infractions if any. </returns>
        public QueryResult<List<Infraction>> GetUserInfractions(DiscordUser user) => GetUserInfractions(user.Id);
        
        /// <summary>
        /// Get the infractions committed by a specific user.
        /// </summary>
        /// <param name="id"> The Id of the user to search for. </param>
        /// <returns> The user's infractions if any. </returns>
        public QueryResult<List<Infraction>> GetUserInfractions(ulong id)
        {
            ModMailContext dbContext = null;

            try
            {
                dbContext = new ModMailContext();

                return QueryResult<List<Infraction>>.FromSuccess(result: 
                    dbContext.Infractions
                    .Where(i => i.UserId == id)
                    .ToList());
            }
            catch (Exception e)
            {
                _log.Error(e, $"Infractions: Failed to retrieve infractions for user {id}.");
                return QueryResult<List<Infraction>>.FromError(e.Message);
            }
            finally
            {
                dbContext?.Dispose();
            }
        }

        /// <summary>
        /// Add an infraction to a user.
        /// </summary>
        /// <param name="user"> The user to which we should add the infraction. </param>
        /// <param name="infraction"> The infraction to add. </param>
        /// <returns> True on success. </returns>
        public async Task<QueryResult> AddInfractionToUserAsync(DiscordUser user, Infraction infraction)
        {
            try
            {
                Assert.IsNotNull(user, $"{nameof(user)} is null.");
            }
            catch (AssertionException e)
            {
                _log.Error(e, "Infractions: an assertion has failed while adding an infraction to a user.");
                return QueryResult.FromError(e.Message);
            }
            
            return await AddInfractionToUserAsync(user.Id, infraction);
        }

        /// <summary>
        /// Add an infraction to a user.
        /// </summary>
        /// <param name="userId"> The Id of the user to which we should add the infraction. </param>
        /// <param name="infraction"> The infraction to add. </param>
        /// <returns> True on success. </returns>
        public async Task<QueryResult> AddInfractionToUserAsync(ulong userId, Infraction infraction)
        {
            ModMailContext dbContext = null;

            try
            {
                // Verifications before interacting with the db.
                Assert.IsNotNull(infraction, $"{nameof(infraction)} is null");
                Assert.IsTrue(userId == infraction.UserId, $"The specified infraction's UserID is not equal to the specified userId.");
                
                // Create the db context.
                dbContext = new ModMailContext();
                // Add the infraction.
                dbContext.Infractions.Add(infraction);
                // Save it to the database.
                await dbContext.SaveChangesAsync();

                return QueryResult.FromSuccess("Succesfully added infraction to user.");
            }
            catch (Exception e)
            {
                _log.Error(e, "Infractions: an error has occured while adding an infraction to a user.");
                return QueryResult.FromError(e.Message);
            }
            finally
            {
                // Dispose of the context.
                dbContext?.Dispose();
            }
        }

        public async Task<QueryResult> RemoveInfractionAsync(long infractionId)
        {
            ModMailContext dbContext = null;

            try
            {
                dbContext = new ModMailContext();

                // Verify that the database does contain an infraction with the specified ID.
                Assert.IsTrue(dbContext.Infractions.Any(i => i.Id == infractionId),
                    $"The database does not contain any infraction with id: {infractionId}");

                // Remove it from the database.
                var infraction = dbContext.Infractions.First(i => i.Id == infractionId);
                dbContext.Remove(infraction);

                // Commit the changes.
                await dbContext.SaveChangesAsync();
                
                return QueryResult.FromSuccess($"Removed infraction {infractionId}");
            }
            catch (Exception e)
            {
                _log.Error(e, $"Infractions: failed to remove infraction {infractionId}");
                return QueryResult.FromError(e.Message);
            }
            finally
            {
                dbContext?.Dispose();
            }
        }
    }
}