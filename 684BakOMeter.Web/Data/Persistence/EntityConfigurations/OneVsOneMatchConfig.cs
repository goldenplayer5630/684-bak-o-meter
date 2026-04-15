using _684BakOMeter.Web.Data.Persistence.Converters;
using _684BakOMeter.Web.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _684BakOMeter.Web.Data.Persistence.EntityConfigurations;

public class OneVsOneMatchConfig : IEntityTypeConfiguration<OneVsOneMatch>
{
    public void Configure(EntityTypeBuilder<OneVsOneMatch> builder)
    {
        builder.HasKey(m => m.Id);

        // ChugType enum persisted as its string name
        builder.Property(m => m.ChugType)
               .IsRequired()
               .HasConversion(EnumConverters.ToStringConverter<ChugType>())
               .HasMaxLength(50);

        // UTC timestamps
        builder.Property(m => m.CreatedAt)
               .IsRequired()
               .HasColumnType("timestamp with time zone");

        builder.Property(m => m.CompletedAt)
               .HasColumnType("timestamp with time zone");

        builder.Property(m => m.Notes)
               .HasMaxLength(500);

        // Player 1 (challenger) — restrict delete so the match record is preserved
        builder.HasOne(m => m.Player1)
               .WithMany(p => p.MatchesAsPlayer1)
               .HasForeignKey(m => m.Player1Id)
               .OnDelete(DeleteBehavior.Restrict);

        // Player 2 (challenged)
        builder.HasOne(m => m.Player2)
               .WithMany(p => p.MatchesAsPlayer2)
               .HasForeignKey(m => m.Player2Id)
               .OnDelete(DeleteBehavior.Restrict);

        // Winner (nullable)
        builder.HasOne(m => m.Winner)
               .WithMany(p => p.MatchesWon)
               .HasForeignKey(m => m.WinnerId)
               .OnDelete(DeleteBehavior.SetNull);

        // Player 1 attempt — one-to-one, FK lives here on the match
        builder.HasOne(m => m.Player1Attempt)
               .WithOne(ca => ca.OneVsOneMatchAsPlayer1)
               .HasForeignKey<OneVsOneMatch>(m => m.Player1AttemptId)
               .OnDelete(DeleteBehavior.SetNull);

        // Player 2 attempt — one-to-one, FK lives here on the match
        builder.HasOne(m => m.Player2Attempt)
               .WithOne(ca => ca.OneVsOneMatchAsPlayer2)
               .HasForeignKey<OneVsOneMatch>(m => m.Player2AttemptId)
               .OnDelete(DeleteBehavior.SetNull);

        // Useful indexes
        builder.HasIndex(m => m.ChugType);
        builder.HasIndex(m => m.CreatedAt);
        builder.HasIndex(m => new { m.Player1Id, m.Player2Id });
    }
}
