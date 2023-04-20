using AzureMasterclass.Api.Dtos;
using AzureMasterclass.Domain;
using AzureMasterclass.Domain.Entities;
using AzureMasterclass.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace AzureMasterclass.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IAzureMasterclassDatabaseContext _databaseContext;
    private readonly IBlobStorageService _storageService;

    public BooksController(IAzureMasterclassDatabaseContext databaseContext, IBlobStorageService storageService )
    {
        _databaseContext = databaseContext;
        _storageService = storageService;
    }

    [HttpGet]
    public async Task<IReadOnlyCollection<BookDto>> GetAll()
    {
        var booksWithCoverName = await _databaseContext.Books.ToListAsync();
        var result = new List<BookDto>();
        foreach (var book in booksWithCoverName)
        {
            byte[]? cover = null;
            if (book.CoverBlobName != null)
            {
                cover = await _storageService.ReadBlobAsync(book.CoverBlobName!);    
            }
            
            result.Add(new BookDto(
                book.Id, 
                book.Name, 
                book.Description, 
                cover == null? null : Convert.ToBase64String(cover), 
                book.Isbn));
        }
        return result;
    }

    [HttpPost]
    public async Task<ActionResult> Add([FromBody]BookDto dto)
    {
        var bookExists = await _databaseContext.Books.AnyAsync(x => x.Id == dto.Id);
        if (!bookExists)
        {
            var coverBlobName = Guid.NewGuid().ToString();
            if (dto.Cover != null)
            {
                await _storageService.WriteBlobAsync(coverBlobName, Convert.FromBase64String(dto.Cover));
            }

            var newBook = new Book(dto.Name, dto.Description, dto.Cover == null ? null : coverBlobName);
            await _databaseContext.Books.AddAsync(newBook);
            await _databaseContext.SaveEntitiesAsync();
        }
        
        return Ok();
    }

    [HttpPut]
    public async Task<ActionResult> Update([FromBody] BookDto dto)
    {
        var bookToUpdate = await _databaseContext.Books.FirstOrDefaultAsync(x => x.Id == dto.Id);
        if (bookToUpdate == null)
        {
            return NotFound();
        }

        bookToUpdate.Name = dto.Name;
        bookToUpdate.Description = dto.Description;

        if (bookToUpdate.CoverBlobName != null)
        {
            await _storageService.DeleteBlobAsync(bookToUpdate.CoverBlobName);
        }

        if (dto.Cover != null)
        {
            var coverBlobName = Guid.NewGuid().ToString();
            await _storageService.WriteBlobAsync(coverBlobName, Convert.FromBase64String(dto.Cover));
            bookToUpdate.CoverBlobName = coverBlobName;
        }
        await _databaseContext.SaveEntitiesAsync();
        
        return Ok();
    }
    
    [HttpDelete]
    [Route("{id}")]
    public async Task<ActionResult> Delete([FromRoute]int id)
    {
        var bookToDelete = await _databaseContext.Books.FirstOrDefaultAsync(x => x.Id == id);
        if (bookToDelete != null)
        {
            if (bookToDelete.CoverBlobName != null)
            {
                await _storageService.DeleteBlobAsync(bookToDelete.CoverBlobName);
            }
            _databaseContext.Books.Remove(bookToDelete);
            await _databaseContext.SaveEntitiesAsync();
        }
        
        return Ok();
    }
}