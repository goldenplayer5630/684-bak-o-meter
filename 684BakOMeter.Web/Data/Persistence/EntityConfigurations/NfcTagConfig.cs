using _684BakOMeter.Web.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _684BakOMeter.Web.Data.Persistence.EntityConfigurations;

public class NfcTagConfig : IEntityTypeConfiguration<NfcTag>
{
    public void Configure(EntityTypeBuilder<NfcTag> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Uid)
               .IsRequired()
               .HasMaxLength(100);

        // Each NFC tag UID must be globally unique
        builder.HasIndex(t => t.Uid)
               .IsUnique();

        builder.Property(t => t.CreatedAt)
               .IsRequired()
               .HasColumnType("timestamp with time zone");

        builder.Property(t => t.IsActive)
               .IsRequired()
               .HasDefaultValue(true);

        builder.Property(t => t.Label)
               .HasMaxLength(100);

        // Many-to-one: NfcTag → Player
        builder.HasOne(t => t.Player)
               .WithMany(p => p.NfcTags)
               .HasForeignKey(t => t.PlayerId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
