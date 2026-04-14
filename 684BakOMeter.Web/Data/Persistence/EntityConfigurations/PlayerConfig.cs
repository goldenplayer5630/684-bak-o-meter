using _684BakOMeter.Web.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _684BakOMeter.Web.Data.Persistence.EntityConfigurations;

public class PlayerConfig : IEntityTypeConfiguration<Player>
{
    public void Configure(EntityTypeBuilder<Player> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
               .IsRequired()
               .HasMaxLength(100);

        // Enforce unique player names (stored normalized lower-case)
        builder.HasIndex(p => p.Name)
               .IsUnique();
    }
}
