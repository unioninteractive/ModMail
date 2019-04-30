using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql;

namespace ModMail.Models
{
    public class ModMailContext : DbContext
    {
        static ModMailContext() => MapGlobalEnums();
        
        public DbSet<Infraction> ModerationInfractions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {
            if (!builder.IsConfigured)
            {
                builder.UseNpgsql(Utils.CXGetEnvironmentVariable("DBCONNSTRING") ?? throw new ArgumentNullException("DBCONNSTRING is not set."));
            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Set the default schema to public.
            builder.HasDefaultSchema("public")
                .MapEnumerations();

            builder.Entity<Infraction>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("nextval('\"Infractions_ID_seq\"'::regclass)");
            });

            builder.HasSequence("Infractions_ID_seq");
        }

        private static void MapGlobalEnums()
        {
            NpgsqlConnection.GlobalTypeMapper.MapEnum<InfractionType>("InfractionType");
        }
    }
}
