using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PromoTest.Api.Domain;

namespace PromoTest.Api.Persistence.Configurations;

public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(x => x.Priority)
            .HasConversion<string>()
            .HasMaxLength(20);
        
        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property<uint>("xmin")
            .IsRowVersion();
    }
}