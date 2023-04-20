using AzureMasterclass.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureMasterclass.Api.EntityTypeConfigurations;

public class AuthorEntityTypeConfiguration: IEntityTypeConfiguration<Author>
{
    public void Configure(EntityTypeBuilder<Author> configuration)
    {
        configuration.ToTable(nameof(Author));
        configuration.HasKey(x => x.Id);
    }
}