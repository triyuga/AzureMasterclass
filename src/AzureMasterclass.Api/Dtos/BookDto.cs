namespace AzureMasterclass.Api.Dtos;

public record struct BookDto(int Id, string Name, string? Description, string? Cover, string? Isbn);
