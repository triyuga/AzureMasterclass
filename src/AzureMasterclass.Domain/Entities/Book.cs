namespace AzureMasterclass.Domain.Entities;

public class Book
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public int NumberOfPages { get; set; } 
    public string? Isbn { get; set; }
    public string? CoverBlobName { get; set; }
    
    private Book()
    {
        Id = 0;
        Name = string.Empty;
        NumberOfPages = 0;
    }

    public Book(string name, string? description, string? coverBlobName)
    {
        Name = name;
        Description = description;
        CoverBlobName = coverBlobName;
    }

    public void UpdateIsbn(string isbn)
    {
        Isbn = isbn;
    }
}