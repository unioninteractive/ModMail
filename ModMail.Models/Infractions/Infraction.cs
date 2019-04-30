using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModMail.Models
{
    [Table("Moderation.Infractions")]
    public class Infraction
    {
        public long Id { get; set; }

        [Required]
        public InfractionType Type { get; set; }
        
        [Required]
        [Column(TypeName = "timestamp with time zone")]
        public DateTimeOffset Timestamp { get; set; }
        
        [Required]
        public ulong StaffId { get; set; }
        
        [Required]
        public ulong UserId { get; set; }
        
        public string Message { get; set; }

        public Infraction(InfractionType type, DateTimeOffset timestamp, ulong staffId, ulong userId,
            string message = null)
        {
            Type = type;
            Timestamp = timestamp;
            StaffId = staffId;
            UserId = userId;
            Message = message;
        }
    }
}
