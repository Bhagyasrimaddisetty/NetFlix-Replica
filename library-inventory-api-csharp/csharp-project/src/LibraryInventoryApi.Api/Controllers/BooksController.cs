using LibraryInventoryApi.Api.DTOs;
using LibraryInventoryApi.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryInventoryApi.Api.Controllers;

[ApiController]
[Route("api/books")]
public class BooksController : ControllerBase
{
    private readonly ILibraryService _library;

    public BooksController(ILibraryService library)
    {
        _library = library;
    }

    [HttpGet]
    public IActionResult GetAll() => Ok(_library.GetAllBooks());

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        try
        {
            return Ok(_library.GetBook(id));
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpPost]
    public IActionResult Create([FromBody] BookCreateRequest request)
    {
        try
        {
            var book = _library.AddBook(request.Title, request.Author, request.Isbn, request.TotalCopies);
            return CreatedAtAction(nameof(GetById), new { id = book.Id }, book);
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
