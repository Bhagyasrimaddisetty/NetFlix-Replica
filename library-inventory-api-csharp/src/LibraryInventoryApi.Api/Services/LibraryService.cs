using System.Collections.Concurrent;
using LibraryInventoryApi.Api.Models;

namespace LibraryInventoryApi.Api.Services;

public interface ILibraryService
{
    Book AddBook(string title, string author, string isbn, int totalCopies);
    IEnumerable<Book> GetAllBooks();
    Book GetBook(int bookId);
    Member AddMember(string name, string email);
    Loan CheckoutBook(int bookId, int memberId);
    Loan ReturnBook(int loanId);
    IEnumerable<Loan> GetActiveLoans(int memberId);
    IEnumerable<Loan> GetOverdueLoans();
    decimal CalculateLateFee(Loan loan, DateTime? asOf = null);
}

public class LibraryService : ILibraryService
{
    private readonly ConcurrentDictionary<int, Book> _books = new();
    private readonly ConcurrentDictionary<int, Member> _members = new();
    private readonly ConcurrentDictionary<int, Loan> _loans = new();

    private int _nextBookId = 1;
    private int _nextMemberId = 1;
    private int _nextLoanId = 1;

    public Book AddBook(string title, string author, string isbn, int totalCopies)
    {
        if (totalCopies < 1)
            throw new BusinessRuleException("A book must have at least 1 copy.");

        var book = new Book
        {
            Id = _nextBookId++,
            Title = title,
            Author = author,
            Isbn = isbn,
            TotalCopies = totalCopies,
            AvailableCopies = totalCopies
        };
        _books[book.Id] = book;
        return book;
    }

    public IEnumerable<Book> GetAllBooks() => _books.Values.OrderBy(b => b.Id);

    public Book GetBook(int bookId) =>
        _books.TryGetValue(bookId, out var book)
            ? book
            : throw new NotFoundException($"Book {bookId} not found.");

    public Member AddMember(string name, string email)
    {
        var member = new Member { Id = _nextMemberId++, Name = name, Email = email };
        _members[member.Id] = member;
        return member;
    }

    private Member GetMember(int memberId) =>
        _members.TryGetValue(memberId, out var member)
            ? member
            : throw new NotFoundException($"Member {memberId} not found.");

    public Loan CheckoutBook(int bookId, int memberId)
    {
        var book = GetBook(bookId);
        GetMember(memberId); // validates existence

        if (book.AvailableCopies < 1)
            throw new BusinessRuleException($"No available copies of '{book.Title}'.");

        var activeLoanCount = _loans.Values.Count(l => l.MemberId == memberId && !l.IsReturned);
        if (activeLoanCount >= Member.MaxActiveLoans)
            throw new BusinessRuleException(
                $"Member {memberId} has reached the maximum of {Member.MaxActiveLoans} active loans.");

        book.AvailableCopies--;

        var loan = new Loan
        {
            Id = _nextLoanId++,
            BookId = bookId,
            MemberId = memberId,
            CheckoutDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(Loan.LoanPeriodDays)
        };
        _loans[loan.Id] = loan;
        return loan;
    }

    public Loan ReturnBook(int loanId)
    {
        if (!_loans.TryGetValue(loanId, out var loan))
            throw new NotFoundException($"Loan {loanId} not found.");

        if (loan.IsReturned)
            throw new BusinessRuleException($"Loan {loanId} has already been returned.");

        loan.ReturnDate = DateTime.UtcNow;

        var book = GetBook(loan.BookId);
        book.AvailableCopies = Math.Min(book.AvailableCopies + 1, book.TotalCopies);

        return loan;
    }

    public IEnumerable<Loan> GetActiveLoans(int memberId) =>
        _loans.Values.Where(l => l.MemberId == memberId && !l.IsReturned).OrderBy(l => l.DueDate);

    public IEnumerable<Loan> GetOverdueLoans()
    {
        var now = DateTime.UtcNow;
        return _loans.Values.Where(l => !l.IsReturned && l.DueDate < now).OrderBy(l => l.DueDate);
    }

    public decimal CalculateLateFee(Loan loan, DateTime? asOf = null)
    {
        var referenceDate = loan.ReturnDate ?? asOf ?? DateTime.UtcNow;
        if (referenceDate <= loan.DueDate)
            return 0m;

        var daysLate = (int)Math.Ceiling((referenceDate - loan.DueDate).TotalDays);
        return daysLate * Loan.DailyLateFee;
    }
}
