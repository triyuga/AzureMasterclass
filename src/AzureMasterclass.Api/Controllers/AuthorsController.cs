using AzureMasterclass.Api.Dtos;
using AzureMasterclass.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AzureMasterclass.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AuthorsController : ControllerBase
{
    private readonly IAzureMasterclassDatabaseContext _databaseContext;

    public AuthorsController(IAzureMasterclassDatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }

    [HttpGet]
    public async Task<IReadOnlyCollection<AuthorDto>> GetAll()
    {
        return await _databaseContext.Authors
            .OrderBy(d => d.Id)
            .Select(d => new AuthorDto(d.Id, d.Name)).ToArrayAsync();
    }
}