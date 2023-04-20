using AzureMasterclass.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureMasterclass.Api.EntityTypeConfigurations;

public class BookEntityTypeConfiguration: IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> configuration)
    {
        configuration.ToTable(nameof(Book));
        configuration.HasKey(x => x.Id);
        configuration.Property(x => x.Isbn).HasColumnName("ISBN");
    }
}