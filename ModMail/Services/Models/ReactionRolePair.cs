using System;
using System.ComponentModel;
using Newtonsoft.Json;

namespace ModMail.Services.Models
{
    public class ReactionRolePair : IEquatable<ReactionRolePair>
    {
        [JsonProperty]
        [DefaultValue(0)]
        public ulong MessageId { get; private set; }
        
        [JsonProperty] 
        [DefaultValue(0)]
        public ulong RoleId { get; private set; }
        
        [JsonProperty]
        [DefaultValue("")]
        public string ReactionName { get; private set; }

        public ReactionRolePair(ulong messageId, string reactionName, ulong roleId)
        {
            MessageId = messageId;
            ReactionName = reactionName;
            RoleId = roleId;
        }

        public bool Equals(ReactionRolePair other)
        {
            return this.MessageId == other.MessageId
                && this.ReactionName == other.ReactionName
                && this.RoleId == other.RoleId;
        }

        public bool Tracks(ulong messageId, string reactionName)
        {
            return this.MessageId == messageId && this.ReactionName == reactionName;
        }

        public override string ToString()
        {
            return $"M{MessageId} React{ReactionName} Role{RoleId}";
        }
    }
}