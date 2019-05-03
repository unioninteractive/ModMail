using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModMail.Models
{
    [Table("Moderation.Infractions")]
    public class Infraction
    {
        public long Id { get; private set; }

        [Required]
        public InfractionType Type { get; private set; }
        
        [Required]
        [Column(TypeName = "timestamp with time zone")]
        public DateTimeOffset Timestamp { get; private set; }
        
        [Required]
        public ulong StaffId { get; private set; }
        
        [Required]
        public ulong UserId { get; private set; }
        
        public string Reason { get; private set; }

        public Infraction(InfractionType type, DateTimeOffset timestamp, ulong staffId, ulong userId,
            string reason = null)
        {
            Type = type;
            Timestamp = timestamp;
            StaffId = staffId;
            UserId = userId;
            Reason = reason;
        }
    }
}
