using NpgsqlTypes;

namespace ModMail.Models
{
    public enum InfractionType
    {
        [PgName("Notice")]
        Notice,
        [PgName("Mute")]
        Mute,
        [PgName("Warn")]
        Warn,
        [PgName("Kick")]
        Kick,
        [PgName("Ban")]
        Ban
    }
}