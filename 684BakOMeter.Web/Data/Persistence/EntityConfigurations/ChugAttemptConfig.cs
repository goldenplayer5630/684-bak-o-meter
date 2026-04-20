using _684BakOMeter.Web.Data.Persistence.Converters;
using _684BakOMeter.Web.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _684BakOMeter.Web.Data.Persistence.EntityConfigurations;

public class ChugAttemptConfig : IEntityTypeConfiguration<ChugAttempt>
{
    public void Configure(EntityTypeBuilder<ChugAttempt> builder)
    {
        builder.HasKey(ca => ca.Id);

        // Many-to-one: ChugAttempt ? Player
        builder.HasOne(ca => ca.Player)
               .WithMany(p => p.Attempts)
               .HasForeignKey(ca => ca.PlayerId)
               .OnDelete(DeleteBehavior.Cascade);

        // UTC timestamps stored as PostgreSQL timestamptz
        builder.Property(ca => ca.StartedAt)
               .IsRequired()
               .HasColumnType("timestamp with time zone");

        builder.Property(ca => ca.EndedAt)
               .IsRequired()
               .HasColumnType("timestamp with time zone");

        builder.Property(ca => ca.DurationMs)
               .IsRequired();

        // IsHighScore is not stored — the repository computes it on retrieval
        // by finding the lowest DurationMs (ties broken by earliest Id) per player + chug type.
        builder.Ignore(ca => ca.IsHighScore);

        // ChugType enum persisted as its string name for readability in the database
        builder.Property(ca => ca.ChugType)
               .IsRequired()
               .HasConversion(EnumConverters.ToStringConverter<ChugType>())
               .HasMaxLength(50);

        builder.Property(ca => ca.Notes)
               .HasMaxLength(500);

        // The 1v1 FK columns live on OneVsOneMatch (one-to-one, FK on that side).
        // EF will resolve OneVsOneMatchAsPlayer1 / AsPlayer2 via the OneVsOneMatchConfig.
    }
}
