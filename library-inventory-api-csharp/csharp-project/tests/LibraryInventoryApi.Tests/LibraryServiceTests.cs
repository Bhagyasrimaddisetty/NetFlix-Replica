using LibraryInventoryApi.Api.Models;
using LibraryInventoryApi.Api.Services;
using Xunit;

namespace LibraryInventoryApi.Tests;

public class LibraryServiceTests
{
    private static LibraryService NewService() => new();

    [Fact]
    public void AddBook_SetsAvailableCopiesEqualToTotal()
    {
        var svc = NewService();
        var book = svc.AddBook("Clean Code", "Robert Martin", "ISBN1", 3);

        Assert.Equal(3, book.TotalCopies);
        Assert.Equal(3, book.AvailableCopies);
    }

    [Fact]
    public void AddBook_ZeroCopies_ThrowsBusinessRuleException()
    {
        var svc = NewService();
        Assert.Throws<BusinessRuleException>(() => svc.AddBook("Bad Book", "Author", "ISBN2", 0));
    }

    [Fact]
    public void CheckoutBook_DecreasesAvailableCopies()
    {
        var svc = NewService();
        var book = svc.AddBook("Book A", "Author", "ISBN3", 2);
        var member = svc.AddMember("Alice", "alice@example.com");

        svc.CheckoutBook(book.Id, member.Id);

        var updated = svc.GetBook(book.Id);
        Assert.Equal(1, updated.AvailableCopies);
    }

    [Fact]
    public void CheckoutBook_NoAvailableCopies_ThrowsBusinessRuleException()
    {
        var svc = NewService();
        var book = svc.AddBook("Book B", "Author", "ISBN4", 1);
        var member = svc.AddMember("Bob", "bob@example.com");

        svc.CheckoutBook(book.Id, member.Id);

        Assert.Throws<BusinessRuleException>(() => svc.CheckoutBook(book.Id, member.Id));
    }

    [Fact]
    public void CheckoutBook_UnknownBook_ThrowsNotFoundException()
    {
        var svc = NewService();
        var member = svc.AddMember("Carol", "carol@example.com");

        Assert.Throws<NotFoundException>(() => svc.CheckoutBook(999, member.Id));
    }

    [Fact]
    public void CheckoutBook_MemberAtMaxActiveLoans_ThrowsBusinessRuleException()
    {
        var svc = NewService();
        var member = svc.AddMember("Dave", "dave@example.com");

        for (int i = 0; i < Member.MaxActiveLoans; i++)
        {
            var book = svc.AddBook($"Book {i}", "Author", $"ISBN-{i}", 1);
            svc.CheckoutBook(book.Id, member.Id);
        }

        var extraBook = svc.AddBook("Extra Book", "Author", "ISBN-EXTRA", 1);
        Assert.Throws<BusinessRuleException>(() => svc.CheckoutBook(extraBook.Id, member.Id));
    }

    [Fact]
    public void ReturnBook_IncreasesAvailableCopies()
    {
        var svc = NewService();
        var book = svc.AddBook("Book C", "Author", "ISBN5", 1);
        var member = svc.AddMember("Eve", "eve@example.com");
        var loan = svc.CheckoutBook(book.Id, member.Id);

        svc.ReturnBook(loan.Id);

        var updated = svc.GetBook(book.Id);
        Assert.Equal(1, updated.AvailableCopies);
    }

    [Fact]
    public void ReturnBook_AlreadyReturned_ThrowsBusinessRuleException()
    {
        var svc = NewService();
        var book = svc.AddBook("Book D", "Author", "ISBN6", 1);
        var member = svc.AddMember("Frank", "frank@example.com");
        var loan = svc.CheckoutBook(book.Id, member.Id);
        svc.ReturnBook(loan.Id);

        Assert.Throws<BusinessRuleException>(() => svc.ReturnBook(loan.Id));
    }

    [Fact]
    public void ReturnBook_UnknownLoan_ThrowsNotFoundException()
    {
        var svc = NewService();
        Assert.Throws<NotFoundException>(() => svc.ReturnBook(999));
    }

    [Fact]
    public void CalculateLateFee_NotYetDue_ReturnsZero()
    {
        var svc = NewService();
        var loan = new Loan
        {
            Id = 1,
            BookId = 1,
            MemberId = 1,
            CheckoutDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(5)
        };

        var fee = svc.CalculateLateFee(loan, DateTime.UtcNow);
        Assert.Equal(0m, fee);
    }

    [Fact]
    public void CalculateLateFee_ThreeDaysLate_ReturnsThreeTimesDailyFee()
    {
        var svc = NewService();
        var now = DateTime.UtcNow; // single reference point for both due date and "as of" check
        var dueDate = now.AddDays(-3);
        var loan = new Loan
        {
            Id = 1,
            BookId = 1,
            MemberId = 1,
            CheckoutDate = dueDate.AddDays(-Loan.LoanPeriodDays),
            DueDate = dueDate
        };

        var fee = svc.CalculateLateFee(loan, now);
        Assert.Equal(3 * Loan.DailyLateFee, fee);
    }

    [Fact]
    public void GetOverdueLoans_ReturnsOnlyPastDueUnreturnedLoans()
    {
        var svc = NewService();
        var book1 = svc.AddBook("Overdue Book", "Author", "ISBN7", 1);
        var book2 = svc.AddBook("OnTime Book", "Author", "ISBN8", 1);
        var member = svc.AddMember("Grace", "grace@example.com");

        var overdueLoan = svc.CheckoutBook(book1.Id, member.Id);
        overdueLoan.DueDate = DateTime.UtcNow.AddDays(-1); // simulate overdue

        svc.CheckoutBook(book2.Id, member.Id); // still within loan period

        var overdue = svc.GetOverdueLoans().ToList();

        Assert.Single(overdue);
        Assert.Equal(book1.Id, overdue[0].BookId);
    }

    [Fact]
    public void GetActiveLoans_ExcludesReturnedLoans()
    {
        var svc = NewService();
        var book1 = svc.AddBook("Active Book", "Author", "ISBN9", 1);
        var book2 = svc.AddBook("Returned Book", "Author", "ISBN10", 1);
        var member = svc.AddMember("Heidi", "heidi@example.com");

        var activeLoan = svc.CheckoutBook(book1.Id, member.Id);
        var returnedLoan = svc.CheckoutBook(book2.Id, member.Id);
        svc.ReturnBook(returnedLoan.Id);

        var active = svc.GetActiveLoans(member.Id).ToList();

        Assert.Single(active);
        Assert.Equal(activeLoan.Id, active[0].Id);
    }
}
