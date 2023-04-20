using AzureMasterclass.Api.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AzureMasterclass.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    [HttpGet]
    public UserDto GetUser()
    {
        // TODO do this better?
        string email = User.FindFirst(ClaimTypes.Email)?.Value;
        string givenName = User.FindFirst(ClaimTypes.GivenName)?.Value;
        string surname = User.FindFirst(ClaimTypes.Surname)?.Value;
        return new UserDto(email, givenName, surname);
    }
}