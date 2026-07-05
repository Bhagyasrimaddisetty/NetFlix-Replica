namespace LibraryInventoryApi.Api.Models;

public class Loan
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public int MemberId { get; set; }
    public DateTime CheckoutDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public bool IsReturned => ReturnDate.HasValue;
    public const int LoanPeriodDays = 14;
    public const decimal DailyLateFee = 5.0m; // currency-agnostic unit fee
}
