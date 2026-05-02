using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("portal_users");
        builder.HasKey(user => user.Id);
        builder.Property(user => user.Email).HasConversion(email => email.Value, value => new EmailAddress(value)).HasMaxLength(320);
        builder.Property(user => user.FullName).HasMaxLength(256).IsRequired();
        builder.Property(user => user.PasswordHash).HasMaxLength(512).IsRequired();
        builder.Property(user => user.Role).HasConversion<int>();
        builder.Property(user => user.IsActive).IsRequired();
        builder.Property(user => user.LastLoginAt);
        builder.Property(user => user.CreatedAt).IsRequired();
        builder.Property(user => user.UpdatedAt).IsRequired();
        builder.HasIndex(user => user.Email).IsUnique();
        builder.Ignore(user => user.RefreshTokens);
        builder.Ignore(user => user.Permissions);
        builder.Ignore(user => user.DomainEvents);
    }
}
