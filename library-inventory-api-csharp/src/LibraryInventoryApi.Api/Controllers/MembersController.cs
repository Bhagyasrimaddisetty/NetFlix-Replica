using LibraryInventoryApi.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryInventoryApi.Api.Controllers;

public record MemberCreateRequest(string Name, string Email);

[ApiController]
[Route("api/members")]
public class MembersController : ControllerBase
{
    private readonly ILibraryService _library;

    public MembersController(ILibraryService library)
    {
        _library = library;
    }

    [HttpPost]
    public IActionResult Create([FromBody] MemberCreateRequest request)
    {
        var member = _library.AddMember(request.Name, request.Email);
        return CreatedAtAction(nameof(Create), member);
    }
}
