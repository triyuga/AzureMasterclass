using System.Reflection;
using AzureMasterclass.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AzureMasterclass.Domain;

public interface IAzureMasterclassDatabaseContext
{
    public DbSet<Book> Books { get; }
    public DbSet<Author> Authors { get; }

    Task SaveEntitiesAsync();
}

public class AzureMasterclassDatabaseContext : DbContext, IAzureMasterclassDatabaseContext
{
    public AzureMasterclassDatabaseContext(DbContextOptions<AzureMasterclassDatabaseContext> options) 
        : base(options)
    {
    }

    public DbSet<Book> Books { get; set; }
    public DbSet<Author> Authors { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("dbo");
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }

    public async Task SaveEntitiesAsync()
    {
        try
        {
            await base.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}