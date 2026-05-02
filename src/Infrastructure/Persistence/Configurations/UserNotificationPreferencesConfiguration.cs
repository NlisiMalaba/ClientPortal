using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

internal sealed class UserNotificationPreferencesConfiguration : IEntityTypeConfiguration<UserNotificationPreferences>
{
    public void Configure(EntityTypeBuilder<UserNotificationPreferences> builder)
    {
        builder.ToTable("user_notification_preferences");
        builder.HasKey(preferences => preferences.Id);
        builder.Property(preferences => preferences.Frequency).HasConversion<int>();
        builder.HasIndex(preferences => preferences.UserId).IsUnique();
    }
}
