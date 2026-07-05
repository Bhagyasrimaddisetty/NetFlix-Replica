using LibraryInventoryApi.Api.DTOs;
using LibraryInventoryApi.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryInventoryApi.Api.Controllers;

[ApiController]
[Route("api/loans")]
public class LoansController : ControllerBase
{
    private readonly ILibraryService _library;

    public LoansController(ILibraryService library)
    {
        _library = library;
    }

    [HttpPost("checkout")]
    public IActionResult Checkout([FromBody] CheckoutRequest request)
    {
        try
        {
            var loan = _library.CheckoutBook(request.BookId, request.MemberId);
            var book = _library.GetBook(loan.BookId);
            return CreatedAtAction(nameof(Checkout), ToResponse(loan, book.Title));
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{loanId:int}/return")]
    public IActionResult Return(int loanId)
    {
        try
        {
            var loan = _library.ReturnBook(loanId);
            var book = _library.GetBook(loan.BookId);
            return Ok(ToResponse(loan, book.Title));
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("overdue")]
    public IActionResult GetOverdue()
    {
        var overdue = _library.GetOverdueLoans()
            .Select(l => ToResponse(l, _library.GetBook(l.BookId).Title));
        return Ok(overdue);
    }

    [HttpGet("member/{memberId:int}")]
    public IActionResult GetActiveForMember(int memberId)
    {
        var loans = _library.GetActiveLoans(memberId)
            .Select(l => ToResponse(l, _library.GetBook(l.BookId).Title));
        return Ok(loans);
    }

    private LoanResponse ToResponse(Models.Loan loan, string bookTitle) => new(
        loan.Id, loan.BookId, bookTitle, loan.MemberId,
        loan.CheckoutDate, loan.DueDate, loan.ReturnDate,
        _library.CalculateLateFee(loan)
    );
}
