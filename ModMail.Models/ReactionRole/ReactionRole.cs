using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModMail.Models
{
    [Table("Discord.ReactionRoles")]
    public class ReactionRole : IEquatable<ReactionRole>
    {
        [Required]
        public ulong MessageId { get; private set; }
        
        [Key]
        [Required]
        public ulong RoleId { get; private set; }
        
        [Required]
        public string ReactionName { get; private set; }

        public ReactionRole(ulong messageId, string reactionName, ulong roleId)
        {
            MessageId = messageId;
            ReactionName = reactionName;
            RoleId = roleId;
        }

        public bool Equals(ReactionRole other)
        {
            return RoleId == other.RoleId;
        }

        public bool Tracks(ulong messageId, string reactionName)
        {
            return MessageId == messageId && ReactionName == reactionName;
        }

        public override string ToString()
        {
            return $"Message: {MessageId} Reaction: {ReactionName} Role: {RoleId}";
        }
    }
}