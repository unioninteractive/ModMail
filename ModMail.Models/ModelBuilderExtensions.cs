using Microsoft.EntityFrameworkCore;

namespace ModMail.Models
{
    public static class ModelBuilderExtensions
    {
        public static ModelBuilder MapEnumerations(this ModelBuilder builder)
        {
            builder.ForNpgsqlHasEnum<InfractionType>();

            return builder;
        }
    }
}