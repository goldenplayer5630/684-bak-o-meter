using _684BakOMeter.Web.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _684BakOMeter.Web.Data.Persistence.EntityConfigurations;

public class AppSettingConfig : IEntityTypeConfiguration<AppSetting>
{
    public const string ApplicationModeKey = "ApplicationMode";

    public void Configure(EntityTypeBuilder<AppSetting> builder)
    {
        builder.HasKey(s => s.Key);

        builder.Property(s => s.Key)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(s => s.Value)
               .IsRequired()
               .HasMaxLength(200);

        // Seed the default mode so the row always exists after migration
        builder.HasData(new AppSetting
        {
            Key   = ApplicationModeKey,
            Value = nameof(ApplicationMode.Official),
        });
    }
}
